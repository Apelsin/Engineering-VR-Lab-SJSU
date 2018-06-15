using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CVRLabSJSU
{
    public class PointerContextMenu : MonoBehaviour
    {
        public struct ClickEventArgs
        {
            public Button Button;
            public SingleButtonInfo Info;
        }

        [Serializable]
        public class ClickEvent : UnityEvent<object, ClickEventArgs> { }

        public struct NavigationEventArgs
        {
            public ButtonInfo Parent;
        }

        [Serializable]
        public class NavigationEvent : UnityEvent<object, NavigationEventArgs> { }

        public Animator Animator;
        public Transform MainCameraTransform;
        public Vector3 TargetPosition;
        public bool AutoRotate = true;

        public Color CheckedColor = new Color(0.1f, 0.5f, 1.0f);

        public Button[] Buttons;

        private Vector3 RightVector;

        [SerializeField]
        private ClickEvent _ButtonClick;

        public event UnityAction<object, ClickEventArgs> ButtonClick
        {
            add { _ButtonClick.AddListener(value); }
            remove { _ButtonClick.RemoveListener(value); }
        }

        [SerializeField]
        private NavigationEvent _Navigate;

        public event UnityAction<object, NavigationEventArgs> Navigate
        {
            add { _Navigate.AddListener(value); }
            remove { _Navigate.RemoveListener(value); }
        }

        private void OnUpdate(bool apply_scale)
        {
            var offset = (TargetPosition - MainCameraTransform.position);
            var forward = offset.normalized;
            var distance = offset.magnitude;
            Vector3 up;
            if (AutoRotate)
            {
                up = Vector3.up;
            }
            else
            {
                Vector3.OrthoNormalize(ref forward, ref RightVector);
                up = Vector3.Cross(forward, RightVector);
            }
            transform.position = TargetPosition;
            transform.rotation = Quaternion.LookRotation(forward, up);
            if (apply_scale)
            {
                var scale = distance;
                transform.localScale = new Vector3(scale, scale, scale);
            }
        }

        private void Start()
        {
            if (MainCameraTransform == null)
                return;
            RightVector = MainCameraTransform.right;
            RightVector.y = 0f;
            OnUpdate(true);
        }

        private void Update()
        {
            if (MainCameraTransform == null)
                return;
            OnUpdate(false);
        }

        private void Pulse()
        {
            Animator.SetTrigger("Pulse");
        }

        protected void ClearButtonEventHandlers()
        {
        }

        public void OnSetManagedMenuButtons(
            List<ButtonInfo> button_infos,
            IDictionary<string, ManagedButtonBehavior> behaviors)
        {
            // Set menu buttons and behaviors
            foreach (var button in Buttons)
            {
                button.onClick.RemoveAllListeners();
            }
            var number_of_buttons = Mathf.Min(button_infos.Count, Buttons.Length);
            int i;
            if (behaviors == null)
                Debug.LogWarning("Behaviors mapping is null.");
            for (i = 0; i < number_of_buttons; i++)
            {
                var info = button_infos[i];
                var button_component = Buttons[i];
                button_component.GetComponentInChildren<Text>().text = info.Text;
                bool interactable = !String.IsNullOrEmpty(info.Id);
                if (info.IsTerminal)
                {
                    if (behaviors != null)
                    {
                        ManagedButtonBehavior behavior;
                        if (behaviors.TryGetValue(info.Id, out behavior))
                        {
                            // Add managed behavior handler
                            button_component.onClick.AddListener(
                                () => behavior.OnClicked(button_component, (SingleButtonInfo)info));

                            // If behavior is not enabled, then interactable is false
                            interactable &= behavior.IsEnabled;
                        }
                    }

                    // Add ButtonClick handler
                    var args = new ClickEventArgs() { Button = button_component, Info = (SingleButtonInfo)info };
                    button_component.onClick.AddListener(() => _ButtonClick.Invoke(this, args));
                }
                else
                {
                    // Add Set + Navigate handler
                    button_component.onClick.AddListener(() =>
                    {
                        OnSetManagedMenuButtons(info.Children, behaviors);
                        _Navigate.Invoke(this, new NavigationEventArgs() { Parent = info });
                        Pulse();
                    });
                }
                button_component.interactable = interactable;
            }
            for (; i < Buttons.Length; i++)
            {
                var button_component = Buttons[i];
                button_component.GetComponentInChildren<Text>().text = String.Empty;
                button_component.interactable = false;
            }
        }

        public void RequestDestroy()
        {
            Animator.SetTrigger("RequestDestroy");
        }

        public void Destroy()
        {
            Destroy(gameObject);
        }
    }
}