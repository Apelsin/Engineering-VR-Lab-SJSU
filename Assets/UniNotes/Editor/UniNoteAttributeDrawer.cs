using UnityEditor;
using UnityEngine;

namespace RotaryHeart.Lib.UniNotes
{
    /// <summary>
    /// Class used to draw the property
    /// </summary>
    [CustomPropertyDrawer(typeof(UniNoteAttribute))]
    public class UniNoteAttributeDrawer : PropertyDrawer
    {
        Rect inputRect;

        GUIStyle textStyle = new GUIStyle(EditorStyles.label);

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            UniNoteAttribute noteAttribute = attribute as UniNoteAttribute;

            string noteSettingId = noteAttribute.noteSettingId;
            string note = noteAttribute.note;

            label.text = string.IsNullOrEmpty(note) ? " " : note;

            var indentedRect = EditorGUI.IndentedRect(inputRect);
            float height = textStyle.CalcHeight(label, indentedRect.width - (!string.IsNullOrEmpty(noteSettingId) ? indentedRect.x : 0));

            return height + EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            UniNoteAttribute noteAttribute = attribute as UniNoteAttribute;

            string noteSettingId = noteAttribute.noteSettingId;
            string note = noteAttribute.note;

            GUIContent def = new GUIContent(label);
            label.text = string.IsNullOrEmpty(note) ? " " : note;

            inputRect = new Rect(position);

            textStyle.richText = true;
            textStyle.wordWrap = true;

            var indentedRect = EditorGUI.IndentedRect(inputRect);

            //Draw the note
            inputRect.height = textStyle.CalcHeight(label, indentedRect.width - indentedRect.x);
            indentedRect.height = inputRect.height;

            //Special check to see if the message type is not none
            if (!string.IsNullOrEmpty(noteSettingId))
            {
                UniNotesSettings.NoteSetting setting;
                //Found a value, remove the hint
                AdvancedNoteDrawer.Settings.FindSetting(noteSettingId, out setting);

                if (setting != null)
                {
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
            }
            else
            {
                textStyle.onActive.textColor = EditorStyles.label.onActive.textColor;
                textStyle.normal.textColor = EditorStyles.label.normal.textColor;
            }

            EditorGUI.SelectableLabel(inputRect, note, textStyle);

            position.y = inputRect.yMax;
            EditorGUI.PropertyField(position, property, def, true);
        }
    }
}