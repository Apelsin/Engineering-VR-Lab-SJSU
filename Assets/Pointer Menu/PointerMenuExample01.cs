using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRTK;

namespace CVRLabSJSU
{
    [RequireComponent(typeof(DestinationMarkerEventReceiver))]
    [RequireComponent(typeof(VRTK_InteractableObject))]
    public class TouchMenuExample01 : MonoBehaviour
    {
        private DestinationMarkerEventReceiver DestinationEvents;
        private VRTK_InteractableObject InteractableObject;

        private struct PointerMenuData
        {
            public PointerContextMenu Menu;
            public LayerMask OriginalIgnoreMask;
        }

        public GameObject MenuPrefab;
        private Dictionary<VRTK_Pointer, PointerMenuData> PointerMenus = new Dictionary<VRTK_Pointer, PointerMenuData>();

        private Dictionary<VRTK_Pointer, Vector3> Destinations = new Dictionary<VRTK_Pointer, Vector3>();

        public LayerMask MenuLayerIgnoreMask;

        public void HandleDestinationMarkerEnter(object sender, DestinationMarkerEventArgs args)
        {
            // This feels hacky as shit but whatever
            var pointer = (sender as Component).GetComponent<VRTK_Pointer>();
            Destinations[pointer] = args.destinationPosition;
        }

        public void HandleUse(object sender, InteractableObjectEventArgs args)
        {
            //Debug.Log(MenuPosition);
            var pointer = args.interactingObject.GetComponent<VRTK_Pointer>();
            if (!PointerMenus.ContainsKey(pointer))
            {
                var menu_object = Instantiate(MenuPrefab);
                var menu = menu_object.GetComponent<PointerContextMenu>();
                var pointer_layers_to_ignore = pointer.pointerRenderer.layersToIgnore;
                var target_position = Destinations[pointer];
                menu.TargetPosition = target_position;
                menu.MainCameraTransform = GameObject.FindGameObjectWithTag("MainCamera").transform;
                PointerMenus[pointer] = new PointerMenuData()
                {
                    Menu = menu,
                    OriginalIgnoreMask = pointer_layers_to_ignore
                };
                pointer.pointerRenderer.layersToIgnore = MenuLayerIgnoreMask;
            }
        }

        public void OnEnable()
        {
            DestinationEvents = GetComponent<DestinationMarkerEventReceiver>();
            InteractableObject = GetComponent<VRTK_InteractableObject>();
            DestinationEvents.DestinationMarkerEnter.AddListener(HandleDestinationMarkerEnter);
            InteractableObject.InteractableObjectUsed += HandleUse;
        }

        public void OnDisable()
        {
            DestinationEvents.DestinationMarkerEnter.RemoveListener(HandleDestinationMarkerEnter);
            InteractableObject.InteractableObjectUsed -= HandleUse;
            DestinationEvents = null;
            InteractableObject = null;
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