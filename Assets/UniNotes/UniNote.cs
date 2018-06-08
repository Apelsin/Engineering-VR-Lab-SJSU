using System;

namespace RotaryHeart.Lib.UniNotes
{
    /// <summary>
    /// Note class
    /// </summary>
    [Serializable]
    public class UniNote
    {
        public string noteSettingId;
        public string note = "Your comment";
        public bool editable = false;
    }
}
