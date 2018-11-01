using RotaryHeart.Lib.ProjectPreferences;
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace RotaryHeart.Lib.UniNotes
{
    /// <summary>
    /// Class used to draw the note on the top part of the GameObject inspector
    /// </summary>
    [CustomEditor(typeof(GameObject))]
    public class NoteInjector : Editor
    {
        //Code based of https://forum.unity.com/threads/extending-instead-of-replacing-built-in-inspectors.407612/

        //Unity's built-in editor
        Editor defaultEditor;
        //The components
        UniNoteComponent[] uniNotes;
        UniNoteComponent[] childNotes;

        //Flag to ensure that the system was drawn before trying to apply any changes (prevents error on the editor)
        int drawn = 0;
        //The type of the built-in inspector
        Type defaultEditorType = Type.GetType("UnityEditor.GameObjectInspector, UnityEditor");
        GUIContent foldoutContent;
        GUIContent helpContent;
        GUIContent contextContent;

        void OnEnable()
        {
            //When this inspector is created, also create the built-in inspector
            defaultEditor = CreateEditor(targets, defaultEditorType);
            //Get the components
            uniNotes = (target as GameObject).GetComponents<UniNoteComponent>();
            childNotes = (target as GameObject).GetComponentsInChildren<UniNoteComponent>(true);

            //Built-in editor enable method
            MethodInfo enableMethod = defaultEditorType.GetMethod("OnEnable", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            //Call OnEnable if it finds it
            if (enableMethod != null)
            {
                enableMethod.Invoke(defaultEditor, null);
            }

            if (uniNotes != null)
            {
                foldoutContent = new GUIContent("   Uni Note", EditorGUIUtility.FindTexture("d_UnityEditor.ConsoleWindow"));
                helpContent = EditorGUIUtility.IconContent("_Help");
                contextContent = EditorGUIUtility.IconContent("d__Popup");
            }
        }

        void OnDisable()
        {
            //Destroy the created editor to avoid memory leakage.
            //MethodInfo disableMethod = defaultEditorType.GetMethod("OnDisable", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            //Call OnDisable if it finds it
            //if (disableMethod != null)
            //{
            //    disableMethod.Invoke(defaultEditor, null);
            //}

            // OnDisable happens to be called by destructor of defaultEditor
            DestroyImmediate(defaultEditor);
        }

        void OnSceneGUI()
        {
            //Check if we should draw the scene UI notes
            if (!Constants.SceneNotesEnabled || childNotes == null || childNotes.Length == 0)
                return;

            Handles.BeginGUI();
            //Draw the UI for the scene
            SceneNoteDrawer.DrawUI(childNotes);
            Handles.EndGUI();
        }

        protected override void OnHeaderGUI()
        {
            //This is added so that the header UI for the GameObject is drawn correctly
            defaultEditor.DrawHeader();
        }

        public override void OnInspectorGUI()
        {
            //For GameObject editor this is not required, included in case that the editor type is changed
            //defaultEditor.OnInspectorGUI();

            //This will only be called if we have UniNoteComponent attached
            if (uniNotes == null)
                return;

            //Iterate all the components found
            for (int i = 0; i < uniNotes.Length; i++)
            {
                UniNoteComponent uniNote = uniNotes[i];

                if (uniNote.myNote == null)
                    continue;

                Event current = Event.current;

                //Be sure that we are on layout event type the first time we are drawing
                if (drawn < uniNotes.Length && current.type != EventType.Layout)
                    return;
                else if (drawn <= uniNotes.Length)
                    drawn++;

                //Draw a line to separate the component
                Rect lineRect = EditorGUILayout.GetControlRect(true, 1);

                //Adjust the position only on 2018.2+ versions
#if UNITY_2018_2_OR_NEWER
                lineRect.y -= 7;
#endif

                lineRect.x = 0;
                lineRect.width += 20;
                EditorExtensions.DrawRect(lineRect, Color.white * 0.5f);

                //Default icon size
                Vector2 defaultSize = EditorGUIUtility.GetIconSize();
                //Resize the icon so that is more visible
                EditorGUIUtility.SetIconSize(Vector2.one * 20);

                //Rect used to draw the fouldout
                Rect foldoutRect = EditorGUILayout.GetControlRect();

                //Adjust the position only on 2018.2+ versions
#if UNITY_2018_2_OR_NEWER
                foldoutRect.y -= 7;
#endif

                //Remove the extra space that the help and context buttons use
                foldoutRect.width -= 38;
                //We are using a serialized property to save the isExpanded value from the foldout
                SerializedProperty name = serializedObject.FindProperty("m_Name");

                GUIStyle foldoutStyle = EditorStyles.foldout;
                foldoutStyle.fontStyle = FontStyle.Bold;

                //Draw the foldout
                name.isExpanded = EditorGUI.Foldout(foldoutRect, name.isExpanded, foldoutContent, true, foldoutStyle);

                //Restore the icon size
                EditorGUIUtility.SetIconSize(defaultSize);

                //Rect used to draw the help and context buttons
                Rect rect = new Rect(foldoutRect.width + 20, foldoutRect.y, 20, 20);

                EditorGUI.LabelField(rect, helpContent);

                //Help clicked
                if (rect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
                {
                    Event.current.Use();
                    Application.OpenURL("https://www.rotaryheart.com/Wiki/UniNotes.html");
                }

                rect.x += 18;

                bool clicked = false;

                EditorGUI.LabelField(rect, contextContent);

                //Context clicked
                if (rect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
                {
                    Event.current.Use();
                    clicked = true;
                }

                //Add a context menu when right clicking the foldout or when the context button is clicked so that the comment can be edited
                if ((foldoutRect.Contains(current.mousePosition) && current.type == EventType.ContextClick) || clicked)
                {
                    int index = i;

                    GenericMenu menu = new GenericMenu();

                    menu.AddItem(new GUIContent("Reset"), false, OnResetClicked, index);

                    menu.AddSeparator("");

                    menu.AddItem(new GUIContent("Remove Note"), false, OnRemoveClicked, index);

                    menu.AddSeparator("");

                    if (uniNote.myNote.editable)
                        menu.AddItem(new GUIContent("Disable Note Edit"), false, OnEditClicked, index);
                    else
                        menu.AddItem(new GUIContent("Enable Note Edit"), false, OnEditClicked, index);

                    menu.AddSeparator("");

                    menu.AddItem(new GUIContent("Change Note/None"), string.IsNullOrEmpty(uniNote.myNote.noteSettingId), () =>
                    {
                        Undo.RecordObject(uniNote, "Changed note id");
                        uniNote.myNote.noteSettingId = string.Empty;
                    });

                    foreach (var note in AdvancedNoteDrawer.Settings.notes)
                    {
                        menu.AddItem(new GUIContent("Change Note/" + note.noteName), uniNote.myNote.noteSettingId.Equals(note.noteId), () =>
                        {
                            Undo.RecordObject(uniNote, "Changed note id");
                            uniNote.myNote.noteSettingId = note.noteId;
                        });
                    }

                    menu.ShowAsContext();

                    current.Use();
                }

                //Foldout is expanded, draw the note
                if (name.isExpanded)
                {
                    string input = uniNote.myNote.note;

                    GUIStyle textStyle = new GUIStyle(EditorStyles.label);
                    textStyle.wordWrap = true;
                    textStyle.richText = true;

                    //If the note is on edit mode
                    if (uniNote.myNote.editable)
                    {
                        //Dropdown for selecting icon type
                        //var messageType = (UniNoteIconType)EditorGUILayout.EnumPopup("Message Icon Type", uniNote.myNote.messageType);
                        input = EditorGUILayout.TextArea(input, EditorStyles.textArea);

                        //Save changes
                        if (!input.Equals(uniNote.myNote.note))
                        {
                            Undo.RecordObject(uniNote, "Modify Note Text");
                            uniNote.myNote.note = input;
                        }
                    }
                    else
                    {
                        Rect position = EditorGUILayout.BeginHorizontal();

                        //Special check to see if the message type is not none
                        if (!string.IsNullOrEmpty(uniNote.myNote.noteSettingId))
                        {
                            UniNotesSettings.NoteSetting setting;
                            //Found a value, remove the hint
                            AdvancedNoteDrawer.Settings.FindSetting(uniNote.myNote.noteSettingId, out setting);
                            GUIContent content;

                            position.x += 30;
                            position.width -= 30;
                            EditorExtensions.DrawRect(position, setting.backgroundColor);
                            //Draw the icon
                            if (setting.icon != null)
                            {
                                content = new GUIContent(setting.icon);
                                EditorGUILayout.LabelField(content);
                            }
                            else
                            {
                                Debug.unityLogger.logEnabled = false;
                                content = EditorGUIUtility.IconContent(setting.unityIcon);
                                Debug.unityLogger.logEnabled = true;

                                EditorGUILayout.LabelField(content, GUILayout.Width(30));
                            }

                            textStyle.onActive.textColor = textStyle.normal.textColor = setting.textColor;
                        }
                        else
                        {
                            textStyle.onActive.textColor = EditorStyles.label.onActive.textColor;
                            textStyle.normal.textColor = EditorStyles.label.normal.textColor;
                        }

                        float size = textStyle.CalcHeight(new GUIContent(input), Screen.width - 90);
                        position.height = size;
                        GUI.enabled = false;
                        EditorGUILayout.SelectableLabel("", textStyle, GUILayout.Height(size), GUILayout.Width(Screen.width - 90));
                        GUI.enabled = true;
                        EditorGUI.SelectableLabel(position, input, textStyle);
                        EditorGUILayout.EndHorizontal();
                    }

#if UNITY_2018_2_OR_NEWER
                    EditorGUILayout.GetControlRect(true, 7);
#endif

                }
            }
        }

        /// <summary>
        /// Called when the ContextMenu Reset option is clicked
        /// </summary>
        void OnResetClicked(object index)
        {
            uniNotes[(int)index].myNote = new UniNote();
        }

        /// <summary>
        /// Called when the ContextMenu Remove option is clicked
        /// </summary>
        void OnRemoveClicked(object index)
        {
            DestroyImmediate(uniNotes[(int)index]);
        }

        /// <summary>
        /// Called when the ContextMenu Edit option is clicked
        /// </summary>
        void OnEditClicked(object index)
        {
            Undo.RecordObject(uniNotes[(int)index], "Enable Note Edit");
            uniNotes[(int)index].myNote.editable = !uniNotes[(int)index].myNote.editable;
        }

    }
}
