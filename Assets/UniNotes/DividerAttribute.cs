using UnityEngine;

namespace RotaryHeart.Lib.UniNotes
{
    /// <summary>
    /// Adds a divider on the inspector
    /// </summary>
    public class DividerAttribute : PropertyAttribute
    {
        private string m_header;
        private string m_subtitle;

        public string Header
        {
            get { return m_header; }
        }

        public string Subtitle
        {
            get { return m_subtitle; }
        }

        public DividerAttribute(string header, string subtitle)
        {
            m_header = header;
            m_subtitle = subtitle;
        }

        public DividerAttribute(string header)
        {
            m_header = header;
            m_subtitle = "";
        }

        public DividerAttribute()
        {
            m_header = "";
            m_subtitle = "";
        }
    }
}