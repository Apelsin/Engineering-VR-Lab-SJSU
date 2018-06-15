using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

namespace CVRLabSJSU
{
    // TODO: Turn ButtonInfo into a class (not struct) and use ButtonInfoSurrogate for serializing
    [Serializable]
    public struct ButtonInfo
    {
        public string Id;
        public string Class;
        public string Text;
        public object Data;
        public bool Checked;
        public List<ButtonInfo> Children;
        public bool IsTerminal { get { return !(Children?.Count > 0); } }
    }

    [Serializable]
    public struct SingleButtonInfo
    {
        public string Id;
        public string Class;
        public string Text;
        public object Data;
        public bool Checked;
        public bool IsTerminal;

        public static explicit operator SingleButtonInfo(ButtonInfo info)
        {
            return new SingleButtonInfo()
            {
                Id = info.Id,
                Class = info.Class,
                Text = info.Text,
                Data = info.Data,
                Checked = info.Checked,
                IsTerminal = info.IsTerminal,
            };
        }
    }
}