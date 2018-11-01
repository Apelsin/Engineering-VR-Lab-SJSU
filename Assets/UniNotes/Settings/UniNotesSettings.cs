using System.Collections.Generic;
using UnityEngine;

namespace RotaryHeart.Lib.UniNotes
{
    [CreateAssetMenu(fileName = "UniNotesSettings.asset", menuName = "Uni Notes Settings")]
    public class UniNotesSettings : ScriptableObject
    {
#if UNITY_EDITOR
#pragma warning disable 0414 //Suppress value not used warning
        [SerializeField]
        private UniNote uniNote = new UniNote() { note = "Here you can modify or create all the avilable notes that can be used on the hierarchy and project windows. Usefull for big teams working on a project." };
#pragma warning restore 0414 //Restore value not used warning
#endif

        public List<NoteSetting> notes = new List<NoteSetting>();

        /// <summary>
        /// Setting class, used only for hierarchy and project notes
        /// </summary>
        [System.Serializable]
        public class NoteSetting
        {
            public string noteId = "";
            public string noteName = "";
            public Color backgroundColor;
            public Color textColor = Color.white;
            public Texture icon;
            public string unityIcon = "";
        }

        /// <summary>
        /// Class used for hierarchy and project notes data stored
        /// </summary>
        public class UniNoteData
        {
            [SerializeField]
            public int expandedIndex;
            [SerializeField]
            public List<Note> notes;

            //Properties used by the yaml serialization
            public int ExpandedIndex { get { return expandedIndex; } set { expandedIndex = value; } }
            public List<Note> Notes { get { return notes; } set { notes = value; } }

            [System.Serializable]
            public class Note
            {
                public string id;
                public string text;
                public List<string> urls = new List<string>();

                //Properties used by the yaml serialization
                public string Id { get { return id; } set { id = value; } }
                public string Text { get { return text; } set { text = value; } }
                public List<string> Urls { get { return urls; } set { urls = value; } }
            }
        }

        /// <summary>
        /// Finds the correct setting for the given id
        /// </summary>
        /// <param name="id">Id to check</param>
        /// <param name="setting">If the setting if ound it will contain the setting reference; otherwise, null</param>
        /// <returns>The index of the setting</returns>
        public int FindSetting(string id, out NoteSetting setting)
        {
            setting = null;

            int index = 0;
            foreach (var note in notes)
            {
                if (note.noteId.Equals(id))
                {
                    setting = note;
                    return index;
                }
                index++;
            }

            return -1;
        }
    }
}
