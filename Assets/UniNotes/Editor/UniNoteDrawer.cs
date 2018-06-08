using UnityEditor;
using UnityEngine;

namespace RotaryHeart.Lib.UniNotes
{
    /// <summary>
    /// Class used to draw the property
    /// </summary>
    [CanEditMultipleObjects]
    [CustomPropertyDrawer(typeof(UniNote))]
    public class UniNoteDrawer : PropertyDrawer
    {
        Rect inputRect;

        GUIStyle textStyle = new GUIStyle(EditorStyles.label);

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty Note = property.FindPropertyRelative("note");
            SerializedProperty Editable = property.FindPropertyRelative("editable");

            label.text = string.IsNullOrEmpty(Note.stringValue) ? " " : Note.stringValue;

            textStyle.richText = true;
            textStyle.wordWrap = true;

            if (Editable.boolValue)
            {
                textStyle.richText = false;
            }

            float height = textStyle.CalcHeight(label, inputRect.width);

            return height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty noteSettingId = property.FindPropertyRelative("noteSettingId");
            SerializedProperty Note = property.FindPropertyRelative("note");
            SerializedProperty Editable = property.FindPropertyRelative("editable");

            string input = Note.stringValue;
            label.text = string.IsNullOrEmpty(input) ? " " : input;

            inputRect = new Rect(position);

            //If we are hovering the icon show the window
            if (inputRect.Contains(Event.current.mousePosition))
            {
                //Icon click, show context menu
                if (Event.current.type == EventType.ContextClick)
                {
                    HoverWindow.CloseMe();
                    GenericMenu menu = new GenericMenu();

                    menu.AddItem(Editable.boolValue ? new GUIContent("Disable Note Edit") : new GUIContent("Enable Note Edit"), false, () =>
                    {
                        Editable.boolValue = !Editable.boolValue;
                        property.serializedObject.ApplyModifiedProperties();
                    });

                    menu.AddSeparator("");

                    menu.AddItem(new GUIContent("Change Note/None"), string.IsNullOrEmpty(noteSettingId.stringValue), () =>
                    {
                        noteSettingId.stringValue = string.Empty;
                        property.serializedObject.ApplyModifiedProperties();
                    });

                    foreach (var note in AdvancedNoteDrawer.Settings.notes)
                    {
                        menu.AddItem(new GUIContent("Change Note/" + note.noteName), noteSettingId.stringValue.Equals(note.noteId), () =>
                        {
                            noteSettingId.stringValue = note.noteId;
                            property.serializedObject.ApplyModifiedProperties();
                        });
                    }

                    menu.ShowAsContext();

                    Event.current.Use();
                }
            }

            textStyle.richText = true;
            textStyle.wordWrap = true;

            //If the note is on edit mode
            if (Editable.boolValue)
            {
                textStyle.richText = false;

                //Dropdown for selecting icon type
                inputRect.height = textStyle.CalcHeight(label, inputRect.width);

                Note.stringValue = EditorGUI.TextArea(inputRect, input, EditorStyles.textArea);
            }
            else
            {
                //Special check to see if the message type is not none
                if (!string.IsNullOrEmpty(noteSettingId.stringValue))
                {
                    UniNotesSettings.NoteSetting setting;
                    //Found a value, remove the hint
                    AdvancedNoteDrawer.Settings.FindSetting(noteSettingId.stringValue, out setting);

                    //Draw the icon
                    if (setting.icon != null)
                    {
                        GUI.Label(inputRect, setting.icon);
                    }
                    else
                    {
                        Debug.unityLogger.logEnabled = false;
                        GUIContent content = EditorGUIUtility.IconContent(setting.unityIcon);
                        Debug.unityLogger.logEnabled = true;

                        Vector2 iconSize = EditorGUIUtility.GetIconSize();
                        EditorGUIUtility.SetIconSize(Vector2.one * 20);

                        inputRect.y -= 2.5f;

                        //Draw the icon
                        EditorGUI.LabelField(inputRect, content);

                        inputRect.y += 2.5f;

                        //Restore icon size
                        EditorGUIUtility.SetIconSize(iconSize);
                    }

                    inputRect.x += 30;
                    inputRect.width -= 30;
                    EditorExtensions.DrawRect(inputRect, setting.backgroundColor);

                    textStyle.onActive.textColor = textStyle.normal.textColor = setting.textColor;
                }
                else
                {
                    textStyle.onActive.textColor = EditorStyles.label.onActive.textColor;
                    textStyle.normal.textColor = EditorStyles.label.normal.textColor;
                }

                //Draw the note
                inputRect.height = textStyle.CalcHeight(label, inputRect.width);

                EditorGUI.SelectableLabel(inputRect, input, textStyle);
            }
        }
    }
}