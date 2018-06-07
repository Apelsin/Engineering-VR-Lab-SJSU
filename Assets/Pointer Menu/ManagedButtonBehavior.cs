using System;
using UnityEngine.Events;

namespace CVRLabSJSU
{
    [Serializable]
    public struct ManagedButtonBehavior
    {
        public Func<bool> EnabledCallback;
        public UnityEvent Clicked;
        public bool IsEnabled { get { return EnabledCallback == null || EnabledCallback(); } }
    }
}