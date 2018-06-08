using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace RotaryHeart.Lib.UniNotes
{
    [CustomPropertyDrawer(typeof(UniNotesSettings.NoteSetting))]
    public class UniNotesSettingDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return (EditorGUIUtility.singleLineHeight) * (property.isExpanded ? 9f : 1);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty noteName = property.FindPropertyRelative("noteName");

            property.isExpanded = EditorGUI.Foldout(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), property.isExpanded, noteName.stringValue, true);

            if (property.isExpanded)
            {
                SerializedProperty noteId = property.FindPropertyRelative("noteId");
                SerializedProperty unityIcon = property.FindPropertyRelative("unityIcon");
                SerializedProperty textureIcon = property.FindPropertyRelative("icon");
                SerializedProperty backgroundColor = property.FindPropertyRelative("backgroundColor");
                SerializedProperty textColor = property.FindPropertyRelative("textColor");

                Rect pos = position;
                pos.height = EditorGUIUtility.singleLineHeight;

                pos.x += 10;
                pos.width -= 15;
                pos.y += EditorGUIUtility.singleLineHeight;

                //Draw the setting name field
                EditorGUI.BeginChangeCheck();
                noteName.stringValue = EditorGUI.TextField(pos, "Setting Name", noteName.stringValue);

                if (EditorGUI.EndChangeCheck())
                {
                    AdvancedNoteDrawer.ResetSettings();
                }

                if (string.IsNullOrEmpty(noteId.stringValue))
                {
                    noteId.stringValue = Guid.NewGuid().ToString("N");
                }

                EditorGUI.BeginChangeCheck();
                pos.y += pos.height + 5;
                string tempNoteId = EditorGUI.DelayedTextField(pos, "Unique ID", noteId.stringValue);

                if (EditorGUI.EndChangeCheck())
                {
                    if (!tempNoteId.Equals(noteId.stringValue))
                    {
                        EditorCoroutines.EditorCoroutines.StartCoroutine(UniNotesSettingsEditor.UpdateReferences(tempNoteId, noteId.stringValue), this);
                        noteId.stringValue = tempNoteId;
                    }
                }

                //What kind of icon are we going to be using
                pos.y += pos.height + 5;
                unityIcon.isExpanded = EditorGUI.Toggle(pos, "Use custom icons", unityIcon.isExpanded);

                pos.y += pos.height + 5;
                if (!unityIcon.isExpanded)
                {
                    //We are using builtin icons
                    property.FindPropertyRelative("icon").objectReferenceValue = null;
                    pos.width -= 20;
                    unityIcon.stringValue = EditorGUI.TextField(pos, "Built-In Icon", unityIcon.stringValue);

                    //Disable logger while drawing this incase that the icon is not found
                    Debug.unityLogger.logEnabled = false;
                    GUIContent content = EditorGUIUtility.IconContent(unityIcon.stringValue);
                    Debug.unityLogger.logEnabled = true;

                    EditorGUI.LabelField(new Rect(pos.x + pos.width, pos.y, 60, pos.height), content);
                    pos.width += 20;
                }
                else
                {
                    //Using custom texture
                    unityIcon.stringValue = "";
                    EditorGUI.PropertyField(pos, textureIcon, new GUIContent("Texture Icon"));
                }

                //Draw colors section
                pos.y += pos.height + 5;
                backgroundColor.colorValue = EditorGUI.ColorField(pos, "Background", backgroundColor.colorValue);

                pos.y += pos.height + 5;
                textColor.colorValue = EditorGUI.ColorField(pos, "Text", textColor.colorValue);
            }
        }
    }
}