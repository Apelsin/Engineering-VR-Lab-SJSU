using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using RotaryHeart.Lib.ProjectPreferences;
using System.Linq;

namespace RotaryHeart.Lib.UniNotes
{
    [CustomEditor(typeof(UniNotesSettings))]
    public class UniNotesSettingsEditor : UnityEditor.Editor
    {
        private Malee.Editor.ReorderableList reorderableList;
        private SerializedProperty notes;

        private void OnEnable()
        {
            notes = serializedObject.FindProperty("notes");

            reorderableList = new Malee.Editor.ReorderableList(notes, true, true, true);

            //Listeners to draw events
            reorderableList.drawHeaderCallback += DrawHeader;
            reorderableList.drawElementCallback += DrawElement;

            //Listeners to remove add
            reorderableList.onAddCallback += AddItem;
            reorderableList.onRemoveCallback += RemoveItem;
        }

        private void OnDisable()
        {
            // Make sure we don't get memory leaks
            reorderableList.drawHeaderCallback -= DrawHeader;
            reorderableList.drawElementCallback -= DrawElement;

            reorderableList.onAddCallback -= AddItem;
            reorderableList.onRemoveCallback -= RemoveItem;
        }

        /// <summary>
        /// Draws the header of the list
        /// </summary>
        private void DrawHeader(Rect rect, GUIContent label)
        {
            GUI.Label(rect, "Notes Settings");
        }

        /// <summary>
        /// Draws one element of the list
        /// </summary>
        private void DrawElement(Rect rect, SerializedProperty element, GUIContent label, bool selected, bool focused)
        {
            EditorGUI.BeginChangeCheck();
            EditorExtensions.DrawRect(new Rect(rect.x - 12, rect.y, rect.width + 12, rect.height), Color.white * 0.4f);
            EditorGUI.PropertyField(rect, element);

            //If something changed, save the value
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(target);
                serializedObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Called when adding a new item
        /// </summary>
        private void AddItem(Malee.Editor.ReorderableList list)
        {
            notes.arraySize++;

            //This is added to be sure that the new entry is empty not a clone of the last one
            SerializedProperty newEntry = notes.GetArrayElementAtIndex(notes.arraySize - 1);
            newEntry.isExpanded = true;
            newEntry.FindPropertyRelative("noteId").stringValue = System.Guid.NewGuid().ToString("N");
            newEntry.FindPropertyRelative("noteName").stringValue = "";
            newEntry.FindPropertyRelative("unityIcon").stringValue = "";
            newEntry.FindPropertyRelative("icon").objectReferenceValue = null;
            newEntry.FindPropertyRelative("backgroundColor").colorValue = Color.white;
            newEntry.FindPropertyRelative("textColor").colorValue = Color.white;

            EditorUtility.SetDirty(target);
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Called when removing an item
        /// </summary>
        private void RemoveItem(Malee.Editor.ReorderableList list)
        {
            EditorCoroutines.EditorCoroutines.StartCoroutine(DeleteReferences(notes.GetArrayElementAtIndex(list.Index).FindPropertyRelative("noteId").stringValue), this);

            notes.DeleteArrayElementAtIndex(list.Index);

            EditorUtility.SetDirty(target);
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draws the list
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            //Draw the note
            EditorGUILayout.PropertyField(serializedObject.FindProperty("uniNote"));

            //Draw the actual list in the inspector
            reorderableList.DoLayoutList();
        }

        /// <summary>
        /// Updates all the notes reference to point to the new id
        /// </summary>
        /// <param name="newId">New setting id</param>
        /// <param name="oldId">Old setting id</param>
        public static IEnumerator UpdateReferences(string newId, string oldId)
        {
            float index = 0;
            float count;

            string[] projectPrefSections = ProjectPrefs.GetSections();
            string[] projectKeys;
            List<string> hierarchySections = new List<string>();

            //Is there any project note?
            if (projectPrefSections.Contains("UniNotes_Project"))
                projectKeys = ProjectPrefs.GetKeys("UniNotes_Project");
            else
                projectKeys = new string[0];

            //Add all the scene notes
            foreach (var section in projectPrefSections)
            {
                if (section.StartsWith("UniNotes_Hierarchy:"))
                {
                    hierarchySections.AddRange(ProjectPrefs.GetKeys(section));
                }
            }

            count = projectKeys.Length;
            count += hierarchySections.Count;

            count--;

            //Check project notes references
            for (int i = 0; i < projectKeys.Length; i++)
            {
                EditorUtility.DisplayProgressBar("Analizing Project Notes", projectKeys[i], index / count);

                //Get the note
                string value = ProjectPrefs.GetString("UniNotes_Project", projectKeys[i]);
                UniNotesSettings.UniNoteData data = JsonUtility.FromJson<UniNotesSettings.UniNoteData>(value);
                bool changed = false;

                foreach (var note in data.notes)
                {
                    //If the id is the same as the old one, update it
                    if (note.id.Equals(oldId))
                    {
                        note.id = newId;
                        EditorUtility.DisplayProgressBar("Analizing Project Notes", "Updating Reference", index / count);
                        changed = true;
                    }
                }

                if (changed)
                {
                    ProjectPrefs.SetString("UniNotes_Project", projectKeys[i], JsonUtility.ToJson(data));
                }

                index++;
                yield return new WaitForSeconds(0.001f);
            }

            //Check hierarchy notes references
            for (int i = 0; i < hierarchySections.Count; i++)
            {
                EditorUtility.DisplayProgressBar("Analizing Hierarchy Notes", hierarchySections[i], index / count);
                string[] hierarchyKeys = ProjectPrefs.GetKeys(hierarchySections[i]);

                //Special check in case that no key is found on the scene
                if (hierarchyKeys == null)
                    continue;

                //Get this scene keys
                for (int l = 0; l < hierarchyKeys.Length; l++)
                {
                    //Get the note
                    string value = ProjectPrefs.GetString(hierarchySections[i], hierarchyKeys[l]);
                    UniNotesSettings.UniNoteData data = JsonUtility.FromJson<UniNotesSettings.UniNoteData>(value);
                    bool changed = false;

                    foreach (var note in data.notes)
                    {
                        //If the id is the same as the old one, update it
                        if (note.id.Equals(oldId))
                        {
                            note.id = newId;
                            EditorUtility.DisplayProgressBar("Analizing Hierarchy Notes", "Updating Reference", l / hierarchyKeys.Length);
                            changed = true;
                        }
                    }

                    if (changed)
                    {
                        ProjectPrefs.SetString(hierarchySections[i], hierarchyKeys[l], JsonUtility.ToJson(data));
                    }
                }

                index++;
                yield return new WaitForSeconds(0.001f);
            }

            EditorUtility.ClearProgressBar();
        }

        /// <summary>
        /// Delete all the notes that are pointing to the removed setting
        /// </summary>
        /// <param name="id">Setting id to be removed</param>
        IEnumerator DeleteReferences(string id)
        {
            float index = 0;
            float count;

            string[] projectPrefSections = ProjectPrefs.GetSections();
            string[] projectKeys;
            List<string> hierarchySections = new List<string>();

            //Is there any project note?
            if (projectPrefSections.Contains("UniNotes_Project"))
                projectKeys = ProjectPrefs.GetKeys("UniNotes_Project");
            else
                projectKeys = new string[0];

            //Add all the scene notes
            foreach (var section in projectPrefSections)
            {
                if (section.StartsWith("UniNotes_Hierarchy:"))
                {
                    hierarchySections.AddRange(ProjectPrefs.GetKeys(section));
                }
            }

            count = projectKeys.Length;
            count += hierarchySections.Count;

            count--;

            //Check project notes references
            for (int i = 0; i < projectKeys.Length; i++)
            {
                EditorUtility.DisplayProgressBar("Analizing Project Notes", projectKeys[i], index / count);

                //Get the note
                string value = ProjectPrefs.GetString("UniNotes_Project", projectKeys[i]);
                UniNotesSettings.UniNoteData data = JsonUtility.FromJson<UniNotesSettings.UniNoteData>(value);
                bool removed = false;

                for (int noteIndex = data.notes.Count - 1; noteIndex >= 0; noteIndex--)
                {
                    //If the id is matches, delete it
                    if (data.notes[noteIndex].id.Equals(id))
                    {
                        EditorUtility.DisplayProgressBar("Analizing Project Notes", "Removing Reference", index / count);
                        data.notes.RemoveAt(noteIndex);
                        removed = true;
                    }
                }

                if (removed)
                {
                    if (data.notes.Count == 0)
                        ProjectPrefs.RemoveKey("UniNotes_Project", projectKeys[i]);
                    else
                        ProjectPrefs.SetString("UniNotes_Project", projectKeys[i], JsonUtility.ToJson(data));
                }

                index++;
                yield return new WaitForSeconds(0.001f);
            }

            //Check hierarchy notes references
            for (int i = 0; i < hierarchySections.Count; i++)
            {
                EditorUtility.DisplayProgressBar("Analizing Hierarchy Notes", hierarchySections[i], index / count);
                string[] hierarchyKeys = ProjectPrefs.GetKeys(hierarchySections[i]);

                //Special check in case that no key is found on the scene
                if (hierarchyKeys == null)
                    continue;

                //Get this scene keys
                for (int l = 0; l < hierarchyKeys.Length; l++)
                {
                    //Get the note
                    string value = ProjectPrefs.GetString(hierarchySections[i], hierarchyKeys[l]);
                    UniNotesSettings.UniNoteData data = JsonUtility.FromJson<UniNotesSettings.UniNoteData>(value);
                    bool removed = false;

                    for (int noteIndex = data.notes.Count - 1; noteIndex >= 0; noteIndex--)
                    {
                        //If the id is matches, delete it
                        if (data.notes[noteIndex].id.Equals(id))
                        {
                            EditorUtility.DisplayProgressBar("Analizing Hierarchy Notes", "Removing Reference", l / hierarchyKeys.Length);
                            data.notes.RemoveAt(noteIndex);
                            removed = true;
                        }
                    }

                    if (removed)
                    {
                        if (data.notes.Count == 0)
                            ProjectPrefs.RemoveKey(hierarchySections[i], hierarchyKeys[l]);
                        else
                            ProjectPrefs.SetString(hierarchySections[i], hierarchyKeys[l], JsonUtility.ToJson(data));
                    }

                }

                index++;
                yield return new WaitForSeconds(0.001f);
            }

            EditorUtility.ClearProgressBar();
        }
    }
}
