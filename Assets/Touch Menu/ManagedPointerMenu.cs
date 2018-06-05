using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VRTK;

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

    public MenuButtons Template;
    public ButtonInfo[] Buttons;

    protected override void OnUse(VRTK_Pointer pointer)
    {
        Manager.OnUseMenu(this, pointer);
    }

    protected override void OnDestinationMarkerEnter(VRTK_Pointer pointer, Vector3 destination_position)
    {
        Manager.UpdatePointerDestination(pointer, destination_position);
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
}