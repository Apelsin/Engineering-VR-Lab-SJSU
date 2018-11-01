using UnityEngine;

namespace RotaryHeart.Lib.UniNotes
{
    public class UniNoteAttribute : PropertyAttribute
    {
        public string noteSettingId = "";
        public string note = "Your note here";

        public UniNoteAttribute(string noteSettingId, string note)
        {
            this.noteSettingId = noteSettingId;
            this.note = note;
        }

        public UniNoteAttribute() { }
    }
}