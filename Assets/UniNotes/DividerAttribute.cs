using UnityEngine;

namespace RotaryHeart.Lib.UniNotes
{
    /// <summary>
    /// Adds a divider on the inspector
    /// </summary>
    public class DividerAttribute : PropertyAttribute
    {
        public string Header { get; private set; }
        public string Subtitle { get; private set; }

        public DividerAttribute(string header, string subtitle)
        {
            Header = header;
            Subtitle = subtitle;
        }

        public DividerAttribute(string header)
        {
            Header = header;
            Subtitle = "";
        }

        public DividerAttribute()
        {
            Header = "";
            Subtitle = "";
        }
    }
}