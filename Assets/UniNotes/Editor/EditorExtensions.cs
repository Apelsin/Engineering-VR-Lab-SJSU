using UnityEngine;
using UnityEditor;
using RotaryHeart.Lib.ProjectPreferences;
using System.Reflection;
using UnityEditor.SceneManagement;

namespace RotaryHeart.Lib.UniNotes
{
    public class EditorExtensions
    {
        public enum Anchor
        {
            Top, TopRight, Right, BottomRight, Bottom, BottomLeft, Left, TopLeft
        }

        //Used to draw rects with color
        private static readonly Texture2D backgroundTexture = Texture2D.whiteTexture;
        private static readonly GUIStyle textureStyle = new GUIStyle { normal = new GUIStyleState { background = backgroundTexture } };

        static long lastMilliseconds;
        static PropertyInfo inspectorModeInfo = typeof(SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// Draws a rect with a solid color
        /// </summary>
        /// <param name="position">Position to draw the rect</param>
        /// <param name="color">Color to draw the rect</param>
        /// <param name="content">Content, if any</param>
        public static void DrawRect(Rect position, Color color, GUIContent content = null)
        {
            var backgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = color;
            GUI.Box(position, content ?? GUIContent.none, textureStyle);
            GUI.backgroundColor = backgroundColor;
        }

        /// <summary>
        /// Included option on right clicking the component so that the note can be edited
        /// </summary>
        [MenuItem("CONTEXT/UniNoteComponent/Edit Note")]
        private static void EditComment(MenuCommand menuCommand)
        {
            var comment = menuCommand.context as UniNoteComponent;

            comment.myNote.editable = !comment.myNote.editable;
        }

        /// <summary>
        /// Option to add a note on the hierarchy window
        /// </summary>
        [MenuItem("GameObject/UniNotes/Add Note", false, 1)]
        static void HierarchyUniNotes()
        {
            var temp = System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond;

            //Added to prevent it from being called multiple times when multiple objects are selected
            if ((temp - lastMilliseconds) < 150)
                return;

            foreach (var obj in Selection.objects)
            {
                GameObject go = obj as GameObject;

                string id = GetFileId(obj).ToString();

                if (id.Equals("0"))
                {
                    if (EditorUtility.DisplayDialog("UniNotes Warning", "The scene needs to be saved before adding notes to a GameObject. Do you want to save it now?", "Yes", "No"))
                    {
                        EditorSceneManager.SaveScene(go.scene);
                        HierarchyUniNotes();
                        break;
                    }
                    else
                    {
                        //Didn't save
                        break;
                    }
                }
                else if (!id.Equals("-1"))
                {
                    UniNotesSettings.UniNoteData data;

                    if (ProjectPrefs.HasKey("UniNotes_Hierarchy:" + go.scene.path, id.ToString()))
                    {
                        data = JsonUtility.FromJson<UniNotesSettings.UniNoteData>(ProjectPrefs.GetString("UniNotes_Hierarchy:" + go.scene.path, id.ToString()));
                    }
                    else
                    {
                        data = new UniNotesSettings.UniNoteData();
                        data.notes = new System.Collections.Generic.List<UniNotesSettings.UniNoteData.Note>();
                    }

                    data.expandedIndex = data.notes.Count;
                    data.notes.Add(new UniNotesSettings.UniNoteData.Note() { id = "-1", text = "<- Select Note" });

                    ProjectPrefs.SetString("UniNotes_Hierarchy:" + go.scene.path, id, JsonUtility.ToJson(data));
                }
            }

            lastMilliseconds = temp;
        }

        /// <summary>
        /// Option to add a not on the project window
        /// </summary>
        [MenuItem("Assets/UniNotes/Add Note", false, 1)]
        static void ProjectUniNotes()
        {
            foreach (var id in Selection.assetGUIDs)
            {
                UniNotesSettings.UniNoteData data;

                if (ProjectPrefs.HasKey("UniNotes_Project", id.ToString()))
                {
                    data = JsonUtility.FromJson<UniNotesSettings.UniNoteData>(ProjectPrefs.GetString("UniNotes_Project", id.ToString()));
                }
                else
                {
                    data = new UniNotesSettings.UniNoteData();
                    data.notes = new System.Collections.Generic.List<UniNotesSettings.UniNoteData.Note>();
                }

                data.expandedIndex = data.notes.Count;
                data.notes.Add(new UniNotesSettings.UniNoteData.Note() { id = "-1", text = "<- Select Note" });

                ProjectPrefs.SetString("UniNotes_Project", id, JsonUtility.ToJson(data));
            }
        }

        /// <summary>
        /// Returns the FileId of an Object
        /// </summary>
        /// <param name="obj">Object to check</param>
        /// <returns>FileId value</returns>
        public static long GetFileId(Object obj)
        {
            if (obj == null)
                return -1;

            SerializedObject serializedObject = new SerializedObject(obj);
            inspectorModeInfo.SetValue(serializedObject, InspectorMode.Debug, null);

            SerializedProperty localIdProp = serializedObject.FindProperty("m_LocalIdentfierInFile");   //note the misspelling!

            return localIdProp.longValue;
        }
    }
}