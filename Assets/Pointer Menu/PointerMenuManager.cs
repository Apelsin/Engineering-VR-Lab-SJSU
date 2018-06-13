using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using VRTK;

namespace CVRLabSJSU
{
    public class PointerMenuManager : MonoBehaviour
    {
        public struct PointerMenuEventArgs
        {
            public PointerContextMenu Menu;
            public Vector3 DestinationPoint;
            public RaycastHit RaycastHit;
            public VRTK_Pointer Pointer;
        }

        [Serializable]
        public class PointerMenuEvent : UnityEvent<object, PointerMenuEventArgs> { }

        private struct PointerMenuInfo
        {
            public PointerContextMenu Menu;
            public LayerMask OriginalIgnoreMask;
        }

        private struct PointerDestinationInfo
        {
            public Vector3 DestinationPoint;
            public RaycastHit RaycastHit;
        }

        private const int UI_LAYER_MASK = 32;
        public GameObject MenuPrefab;
        public LayerMask MenuLayerIgnoreMask = ~UI_LAYER_MASK;

        [SerializeField]
        private PointerMenuEvent _MenuAdded;
        public PointerMenuEvent MenuAdded => _MenuAdded;

        [SerializeField]
        private PointerMenuEvent _MenuRemoved;
        public PointerMenuEvent MenuRemoved => _MenuRemoved;

        private DestinationMarkerEventReceiver DestinationEvents;
        private VRTK_InteractableObject InteractableObject;

        private Dictionary<VRTK_Pointer, PointerMenuInfo> PointerMenus = new Dictionary<VRTK_Pointer, PointerMenuInfo>();
        private Dictionary<VRTK_Pointer, PointerDestinationInfo> Destinations = new Dictionary<VRTK_Pointer, PointerDestinationInfo>();

        public void OnUseMenu(ManagedPointerMenu managed_pointer_menu, VRTK_Pointer pointer)
        {
            if (!PointerMenus.ContainsKey(pointer))
            {
                var button_behavior_manager = FindObjectOfType<ButtonBehaviorManager>();
                if (!button_behavior_manager)
                    Debug.LogWarning("Could not find button behavior manager.");

                var menu_object = Instantiate(MenuPrefab);
                var pointer_context_menu = menu_object.GetComponent<PointerContextMenu>();
                var pointer_layers_to_ignore = pointer.pointerRenderer.layersToIgnore;
                var pointer_destination_info = Destinations[pointer];
                pointer_context_menu.TargetPosition = pointer_destination_info.DestinationPoint;
                pointer_context_menu.MainCameraTransform = GameObject.FindGameObjectWithTag("MainCamera").transform;
                PointerMenus[pointer] = new PointerMenuInfo()
                {
                    Menu = pointer_context_menu,
                    OriginalIgnoreMask = pointer_layers_to_ignore
                };
                pointer.pointerRenderer.layersToIgnore = MenuLayerIgnoreMask;
                var map = button_behavior_manager?.GetButtonBehaviorsMap();
                // Here is where the magic happens
                pointer_context_menu.OnSetManagedMenuButtons(managed_pointer_menu.Buttons, map);
                // Magic happens here
                MenuAdded.Invoke(this, new PointerMenuEventArgs()
                {
                    Menu = pointer_context_menu,
                    Pointer = pointer,
                    DestinationPoint = pointer_destination_info.DestinationPoint,
                    RaycastHit = pointer_destination_info.RaycastHit
                });
            }
        }

        public void UpdatePointer(VRTK_Pointer pointer, Vector3 destination_point, RaycastHit raycast_hit)
        {
            Destinations[pointer] = new PointerDestinationInfo()
            {
                DestinationPoint = destination_point,
                RaycastHit = raycast_hit,
            };
        }

        private void Update()
        {
            foreach (var kvp in PointerMenus.ToArray())
            {
                var pointer = kvp.Key;
                var menu_data = kvp.Value;
                // If the controller button is released, remove the menu
                if (!pointer.controller.IsButtonPressed(pointer.activationButton))
                {
                    var pointer_destination_info = Destinations[pointer];
                    MenuRemoved.Invoke(this, new PointerMenuEventArgs()
                    {
                        Menu = menu_data.Menu,
                        Pointer = pointer,
                        DestinationPoint = pointer_destination_info.DestinationPoint,
                        RaycastHit = pointer_destination_info.RaycastHit
                    });
                    PointerMenus.Remove(pointer);
                    menu_data.Menu.RequestDestroy();
                    pointer.pointerRenderer.layersToIgnore = menu_data.OriginalIgnoreMask;
                }
            }
        }
    }
}