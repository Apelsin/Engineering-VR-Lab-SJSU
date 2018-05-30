using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VRTK;

[RequireComponent(typeof(DestinationMarkerEventReceiver))]
[RequireComponent(typeof(VRTK_InteractableObject))]
public class BasicTouchMenu : MonoBehaviour
{
    public GameObject MenuPrefab;
    public LayerMask MenuLayerIgnoreMask;
    public MenuButtons Template;

    public ButtonInfo[] Buttons;

    private DestinationMarkerEventReceiver DestinationEvents;
    private VRTK_InteractableObject InteractableObject;

    private struct PointerMenuData
    {
        public PointerContextMenu Menu;
        public LayerMask OriginalIgnoreMask;
    }

    private Dictionary<VRTK_Pointer, PointerMenuData> PointerMenus = new Dictionary<VRTK_Pointer, PointerMenuData>();
    private Dictionary<VRTK_Pointer, Vector3> Destinations = new Dictionary<VRTK_Pointer, Vector3>();

    private void HandleDestinationMarkerEnter(object sender, DestinationMarkerEventArgs args)
    {
        var pointer = (sender as Component).GetComponent<VRTK_Pointer>();
        Destinations[pointer] = args.destinationPosition;
    }

    private void HandleUse(object sender, InteractableObjectEventArgs args)
    {
        var button_behavior_manager =
            GameObject.FindGameObjectWithTag("Behavior Manager")?
            .GetComponentInChildren<ButtonBehaviorManager>();
        if (!button_behavior_manager)
            throw new Exception("Could not find button behavior manager.");
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
            var map = button_behavior_manager.GetButtonBehaviorsMap();
            SetMenuButtons(menu, Buttons, map);
        }
    }

    private static void SetMenuButtons(
        PointerContextMenu menu,
        ButtonInfo[] button_infos,
        IDictionary<string, ManagedButtonBehavior> behaviors)
    {
        // Set menu buttons and behaviors
        var buttons = menu.Buttons;
        foreach (var button in buttons)
            button.onClick.RemoveAllListeners();
        var number_of_buttons = Mathf.Min(button_infos.Length, buttons.Length);
        int i;
        for (i = 0; i < number_of_buttons; i++)
        {
            var info = button_infos[i];
            var button_component = buttons[i];
            button_component.GetComponentInChildren<Text>().text = info.Text;
            if (info.IsTerminal)
            {
                ManagedButtonBehavior behavior;
                if (behaviors.TryGetValue(info.Id, out behavior))
                {
                    button_component.onClick.AddListener(behavior.Clicked.Invoke);
                    button_component.interactable = behavior.IsEnabled;
                }
            }
            else
            {
                button_component.onClick.AddListener(() =>
                {
                    SetMenuButtons(menu, info.Children, behaviors);
                    menu.Pulse();
                });
                button_component.interactable = true;
            }
        }
        for (; i < buttons.Length; i++)
        {
            var button_component = buttons[i];
            button_component.GetComponentInChildren<Text>().text = String.Empty;
            button_component.interactable = false;
        }
    }

    public void Start()
    {
        if (Template)
            Buttons = Template.Buttons.ToArray();
        else
            Debug.LogWarning("Menu has no template.");
        if (Buttons.Length == 0)
            Debug.LogWarning("Menu has no buttons.");
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