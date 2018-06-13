using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace CVRLabSJSU
{
    [Serializable]
    public struct ManagedButtonBehavior
    {
        public Func<bool> EnabledCallback;

        [FormerlySerializedAs("Clicked")]
        [SerializeField]
        private PointerContextMenu.ClickEvent _Clicked;

        public event UnityAction<object, PointerContextMenu.ClickEventArgs> Clicked
        {
            add { _Clicked.AddListener(value); }
            remove { _Clicked.RemoveListener(value); }
        }

        public void OnClicked(Button button, SingleButtonInfo info)
        {
            var args = new PointerContextMenu.ClickEventArgs()
            {
                Button = button,
                Info = info
            };
            _Clicked.Invoke(this, args);
        }

        public bool IsEnabled { get { return EnabledCallback == null || EnabledCallback(); } }
    }
}