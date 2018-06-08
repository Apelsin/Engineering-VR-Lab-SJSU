using System.Collections;
using UnityEditor;
using UnityEngine;

namespace RotaryHeart.Lib.UniNotes
{
    public class HoverWindow : EditorWindow
    {
        static HoverWindow reference;

        UniNotesSettings.NoteSetting setting;
        UniNotesSettings.UniNoteData.Note note;
        UniNotesSettings.UniNoteData data;
        bool done = false;
        GUIStyle style;
        Vector2 scrollPosition;
        Rect dimension = new Rect();
        Rect scrollContent = new Rect();
        string section;
        string id;

        public static void Initialize(UniNotesSettings.NoteSetting setting, UniNotesSettings.UniNoteData.Note note, UniNotesSettings.UniNoteData data, string section, string id)
        {
            if (reference == null)
            {
                reference = CreateInstance<HoverWindow>();

                reference.style = new GUIStyle(EditorStyles.textArea);

                reference.style.richText = true;
                reference.style.normal.background = null;

                reference.dimension.x = 0;
                reference.dimension.y = 0;

                reference.dimension.width = 300;
                reference.dimension.height = 150;

                float textHeight = reference.style.CalcHeight(new GUIContent(note.text), reference.dimension.width - 20);

                reference.scrollContent = new Rect(0, 0, reference.dimension.width - 20, textHeight < 150 ? 150 : textHeight);

                reference.ShowAsDropDown(reference.dimension, new Vector2(reference.dimension.width, reference.dimension.height));
            }

            reference.section = section;
            reference.id = id;
            reference.setting = setting;
            reference.note = note;
            reference.data = data;
        }

        private void OnGUI()
        {
            if (!done)
            {
                Rect newPosition = new Rect(Event.current.mousePosition.x - dimension.width / 2, Event.current.mousePosition.y + dimension.height - 6, dimension.width, dimension.height);

                float offsetX = (newPosition.x + newPosition.width) - (Screen.currentResolution.width - 40);
                float offsetY = (newPosition.y + newPosition.height) - (Screen.currentResolution.height - 40);

                if (offsetX > 0)
                {
                    newPosition.x -= offsetX;
                }
                if (offsetY > 0)
                {
                    newPosition.y -= offsetY;
                }

                position = newPosition;

                done = true;
            }

            EditorExtensions.DrawRect(dimension, setting.backgroundColor);

            scrollPosition = GUI.BeginScrollView(dimension, scrollPosition, scrollContent, false, true);

            EditorGUI.BeginChangeCheck();
            style.normal.textColor = setting.textColor;
            note.text = EditorGUI.TextArea(scrollContent, note.text, style);

            //If the note type or text is changed saved the values
            if (!EditorGUI.EndChangeCheck())
            {
                ProjectPreferences.ProjectPrefs.SetString(section, id, JsonUtility.ToJson(data));
            }

            GUI.EndScrollView();
        }

        private void OnDestroy()
        {
            reference = null;
        }

        public static void CloseMe()
        {
            if (reference != null)
            {
                reference.Close();
            }
        }
    }
}