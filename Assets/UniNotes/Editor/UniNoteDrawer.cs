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
        GUIStyle textStyle = new GUIStyle(EditorStyles.label);

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty positionProp = property.FindPropertyRelative("position");
            return positionProp.rectValue.height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty noteSettingId = property.FindPropertyRelative("noteSettingId");
            SerializedProperty Note = property.FindPropertyRelative("note");
            SerializedProperty Editable = property.FindPropertyRelative("editable");
            SerializedProperty positionProp = property.FindPropertyRelative("position");

            string input = Note.stringValue;
            label.text = string.IsNullOrEmpty(input) ? " " : input;

            Rect inputRect = new Rect(position);

            textStyle.richText = true;
            textStyle.wordWrap = true;

            var indentedRect = EditorGUI.IndentedRect(inputRect);

            //If we are hovering the icon show the window
            if (indentedRect.Contains(Event.current.mousePosition))
            {
                //Icon click, show context menu
                if (Event.current.type == EventType.ContextClick)
                {
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

            //If the note is on edit mode
            if (Editable.boolValue)
            {
                textStyle.richText = false;

                //Dropdown for selecting icon type
                inputRect.height = textStyle.CalcHeight(label, indentedRect.width);

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

                    GUIContent content;

                    //Get the icon image
                    if (setting.icon != null)
                    {
                        content = new GUIContent(setting.icon);
                    }
                    else
                    {
                        Debug.unityLogger.logEnabled = false;
                        content = EditorGUIUtility.IconContent(setting.unityIcon);
                        Debug.unityLogger.logEnabled = true;
                    }

                    //Draw the icon
                    Vector2 iconSize = EditorGUIUtility.GetIconSize();
                    EditorGUIUtility.SetIconSize(Vector2.one * 20);

                    //Draw the icon
                    EditorGUI.LabelField(inputRect, content);

                    //Restore icon size
                    EditorGUIUtility.SetIconSize(iconSize);

                    EditorExtensions.DrawRect(indentedRect, setting.backgroundColor);

                    textStyle.onActive.textColor = textStyle.normal.textColor = setting.textColor;

                    inputRect.x += 24;
                    inputRect.width -= 24;
                }
                else
                {
                    textStyle.onActive.textColor = EditorStyles.label.onActive.textColor;
                    textStyle.normal.textColor = EditorStyles.label.normal.textColor;
                }

                //Draw the note
                inputRect.height = textStyle.CalcHeight(label, indentedRect.width - indentedRect.x);

                EditorGUI.SelectableLabel(inputRect, input, textStyle);
            }

            positionProp.rectValue = inputRect;
        }
    }
}