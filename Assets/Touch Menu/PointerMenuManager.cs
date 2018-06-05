using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VRTK;

public class PointerMenuManager : MonoBehaviour
{
    private const int UI_LAYER_MASK = 32;
    public GameObject MenuPrefab;
    public LayerMask MenuLayerIgnoreMask = ~UI_LAYER_MASK;

    private DestinationMarkerEventReceiver DestinationEvents;
    private VRTK_InteractableObject InteractableObject;

    private struct PointerMenuData
    {
        public PointerContextMenu Menu;
        public LayerMask OriginalIgnoreMask;
    }

    private Dictionary<VRTK_Pointer, PointerMenuData> PointerMenus = new Dictionary<VRTK_Pointer, PointerMenuData>();
    private Dictionary<VRTK_Pointer, Vector3> Destinations = new Dictionary<VRTK_Pointer, Vector3>();

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
            var target_position = Destinations[pointer];
            pointer_context_menu.TargetPosition = target_position;
            pointer_context_menu.MainCameraTransform = GameObject.FindGameObjectWithTag("MainCamera").transform;
            PointerMenus[pointer] = new PointerMenuData()
            {
                Menu = pointer_context_menu,
                OriginalIgnoreMask = pointer_layers_to_ignore
            };
            pointer.pointerRenderer.layersToIgnore = MenuLayerIgnoreMask;
            var map = button_behavior_manager?.GetButtonBehaviorsMap();
            // Here is where the magic happens
            pointer_context_menu.SetMenuButtons(managed_pointer_menu.Buttons, map);
        }
    }

    public void UpdatePointerDestination(VRTK_Pointer pointer, Vector3 destination_point)
    {
        Destinations[pointer] = destination_point;
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