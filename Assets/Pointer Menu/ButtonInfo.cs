using System;
using System.Linq;
using UnityEngine.Events;

namespace CVRLabSJSU
{
    [Serializable]
    public struct ButtonInfo
    {
        public string Id;
        public string Text;
        public object Data;
        public ButtonInfo[] Children;
        public bool IsTerminal { get { return !(Children?.Length > 0); } }
    }
}