using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using VRTK;

namespace CVRLabSJSU
{
    [RequireComponent(typeof(DestinationMarkerEventReceiver))]
    [RequireComponent(typeof(VRTK_InteractableObject))]
    public class ManagedPointerMenu : PointerMenuBase
    {
        private PointerMenuManager _CachedManager;

        private PointerMenuManager CachedManager
        {
            get
            {
                if (_CachedManager == null)
                {
                    var manager = FindObjectOfType<PointerMenuManager>();
                    if (manager == null)
                        throw new ArgumentNullException("PointerMenuManager not found.");
                    _CachedManager = manager;
                }
                return _CachedManager;
            }
        }

        [SerializeField]
        private PointerMenuManager _Manager;

        public PointerMenuManager Manager
        {
            get { return _Manager ?? CachedManager; }
            set { _Manager = value; }
        }

        [FormerlySerializedAs("Template")]
        [SerializeField]
        private MenuButtons _Template;

        public MenuButtons Template
        {
            get { return _Template; }
            set
            {
                _Template = value ? Instantiate(value) : default(MenuButtons);
                RefreshButtons();
            }
        }

        [SerializeField]
        private MenuButtons _SharedTemplate;

        public MenuButtons SharedTemplate
        {
            get { return _SharedTemplate; }
            set
            {
                _SharedTemplate = value;
                RefreshButtons();
            }
        }

        private void RefreshButtons()
        {
            if (SharedTemplate)
                _Buttons = SharedTemplate.Buttons;
            else if (Template)
                _Buttons = Template.Buttons;
            else
                _Buttons = default(List<ButtonInfo>);
        }

        [SerializeField]
        private List<ButtonInfo> _Buttons;

        public List<ButtonInfo> Buttons
        {
            get { return _Buttons; }
        }

        //private static IEnumerable<ButtonInfo> GetAllButtons(ButtonInfo[] buttons)
        //{
        //    foreach (var button in buttons)
        //    {
        //        yield return button;
        //        foreach (var child in GetAllButtons(button.Children))
        //            yield return child;
        //    }
        //}

        //private IEnumerable<ButtonInfo> AllButtons
        //{
        //    get
        //    {
        //        return GetAllButtons(_Buttons);
        //    }
        //}

        // TODO
        //private Dictionary<string, ButtonInfo> ButtonsDictionary;

        [SerializeField]
        protected PointerMenuManager.PointerMenuEvent _MenuAddedCallback;

        public event UnityAction<object, PointerMenuManager.PointerMenuEventArgs> MenuAddedCallback
        {
            add { _MenuAddedCallback.AddListener(value); }
            remove { _MenuAddedCallback.RemoveListener(value); }
        }

        [SerializeField]
        protected PointerMenuManager.PointerMenuEvent _MenuRemovedCallback;

        public event UnityAction<object, PointerMenuManager.PointerMenuEventArgs> MenuRemovedCallback
        {
            add { _MenuRemovedCallback.AddListener(value); }
            remove { _MenuRemovedCallback.RemoveListener(value); }
        }

        protected override void OnUse(VRTK_Pointer pointer)
        {
            Manager.MenuAdded.AddListener(HandleManagerMenuAdded);
            Manager.OnUseMenu(this, pointer); // Invokes Manager.MenuAdded
            Manager.MenuAdded.RemoveListener(HandleManagerMenuAdded);
        }

        public void SetButtonChecked(string id, bool @checked)
        {
            CheckButton(ref _Buttons, id, @checked);
        }

        // TODO: improve memory efficiency of this function and ButtonInfo in general
        // Probably use a backing hashtable for Button array
        protected static bool CheckButton(ref List<ButtonInfo> buttons, string id, bool @checked)
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                var button = buttons[i];
                if (button.Children != null && button.Children.Count > 0)
                {
                    if (CheckButton(ref button.Children, id, @checked))
                    {
                        buttons[i] = button; // Replace button info value in array
                        return true;
                    }
                }
                if (!String.IsNullOrEmpty(button.Id) && button.Id == id)
                {
                    button.Checked = @checked;
                    buttons[i] = button; // Replace button info value in array
                    return true;
                }
            }
            return false;
        }

        protected static bool IsButtonChecked(ref List<ButtonInfo> buttons, string id, out bool @checked)
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                var button = buttons[i];
                if (button.Id == id)
                {
                    @checked = button.Checked;
                    return true;
                }
                if (button.Children.Count > 0 && IsButtonChecked(ref button.Children, id, out @checked))
                    return true;
            }
            @checked = false;
            return false;
        }

        public bool IsButtonChecked(string id)
        {
            bool @checked;
            IsButtonChecked(ref _Buttons, id, out @checked);
            return @checked;
        }

        public void ClearCheckedButtons(bool @checked = false)
        {
            ClearCheckedButtons(ref _Buttons, @checked);
        }

        protected static void ClearCheckedButtons(ref List<ButtonInfo> buttons, bool @checked = false)
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                var button = buttons[i];
                button.Checked = @checked;
                if (button.Children != null && button.Children.Count > 0)
                    ClearCheckedButtons(ref button.Children, @checked);
                buttons[i] = button; // Replace button info value in array
            }
        }

        private void HandleManagerMenuAdded(object sender, PointerMenuManager.PointerMenuEventArgs args)
        {
            var manager = (PointerMenuManager)sender;
            _MenuAddedCallback.Invoke(this, args);

            // "Self-removing" handler
            UnityAction<object, PointerMenuManager.PointerMenuEventArgs> menu_removed_handler = null;
            menu_removed_handler = (sender2, args2) =>
            {
                HandleManagerMenuRemoved(sender2, args2);
                manager.MenuRemoved.RemoveListener(menu_removed_handler);
            };
            manager.MenuRemoved.AddListener(menu_removed_handler);
        }

        private void HandleManagerMenuRemoved(object sender, PointerMenuManager.PointerMenuEventArgs args)
        {
            _MenuRemovedCallback.Invoke(this, args);
        }

        protected override void OnDestinationMarkerEnter(VRTK_Pointer pointer, Vector3 destination_position, RaycastHit raycast_hit)
        {
            Manager.UpdatePointer(pointer, destination_position, raycast_hit);
        }

        public void Awake()
        {
            // Instantiate a copy of the template so that changes to Buttons
            // are not written to the asset database in editor mode
            if (SharedTemplate)
                SharedTemplate = SharedTemplate; // Round Trip
            else if (Template)
                Template = Template; // Round trip
            else
                Debug.LogWarning("Menu has no template.");
            if (Buttons.Count == 0)
                Debug.LogWarning("Menu has no buttons.");
        }

        public void Start()
        {
            // TODO
            //ButtonsDictionary = AllButtons.ToDictionary(b => b.Id, b => b);
        }
    }
}