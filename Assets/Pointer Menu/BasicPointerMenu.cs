using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRTK;

namespace CVRLabSJSU
{
    [RequireComponent(typeof(DestinationMarkerEventReceiver))]
    [RequireComponent(typeof(VRTK_InteractableObject))]
    public class BasicPointerMenu : PointerMenuBase
    {
        private const int UI_LAYER_MASK = 32;
        public GameObject MenuPrefab;
        public LayerMask MenuLayerIgnoreMask = ~UI_LAYER_MASK;
        public MenuButtons Template;

        public List<ButtonInfo> Buttons;

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

        private Dictionary<VRTK_Pointer, PointerMenuInfo> PointerMenus = new Dictionary<VRTK_Pointer, PointerMenuInfo>();
        private Dictionary<VRTK_Pointer, PointerDestinationInfo> Destinations = new Dictionary<VRTK_Pointer, PointerDestinationInfo>();

        protected override void OnUse(VRTK_Pointer pointer)
        {
            var button_behavior_manager =
                GameObject.FindGameObjectWithTag("Behavior Manager")?
                .GetComponentInChildren<ButtonBehaviorManager>();
            if (!button_behavior_manager)
                Debug.LogWarning("Could not find button behavior manager.");
            if (!PointerMenus.ContainsKey(pointer))
            {
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
                pointer_context_menu.OnSetManagedMenuButtons(Buttons, map);
            }
        }

        protected override void OnDestinationMarkerEnter(VRTK_Pointer pointer, Vector3 destination_position, RaycastHit raycast_hit)
        {
            Destinations[pointer] = new PointerDestinationInfo()
            {
                DestinationPoint = destination_position,
                RaycastHit = raycast_hit
            };
        }

        public void Start()
        {
            if (Template)
                Buttons = Template.Buttons.ToList();
            else
                Debug.LogWarning("Menu has no template.");
            if (Buttons.Count == 0)
                Debug.LogWarning("Menu has no buttons.");
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
                    PointerMenus.Remove(pointer);
                    menu_data.Menu.RequestDestroy();
                    pointer.pointerRenderer.layersToIgnore = menu_data.OriginalIgnoreMask;
                }
            }
        }
    }
}