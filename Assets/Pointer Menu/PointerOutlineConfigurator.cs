using cakeslice;
using CVRLabSJSU;
using System.Collections.Generic;
using UnityEngine;

public class PointerOutlineConfigurator : MonoBehaviour
{
    private Dictionary<GameObject, Outline> ManagedOutlines = new Dictionary<GameObject, Outline>();
    private void Start()
    {
        var pointer_menu_manager = FindObjectOfType<PointerMenuManager>();
        if (!pointer_menu_manager)
        {
            Debug.LogWarning("PointerMenuManager not found.");
            return;
        }
        pointer_menu_manager.MenuAdded.AddListener(HandleMenuAdded);
        pointer_menu_manager.MenuRemoved.AddListener(HandleMenuRemoved);
    }

    private void HandleMenuAdded(object sender, PointerMenuManager.PointerMenuEventArgs args)
    {
        var game_object = args.RaycastHit.transform.gameObject;
        var outline = game_object.AddComponent<Outline>();
        ManagedOutlines[game_object] = outline;
    }

    private void HandleMenuRemoved(object sender, PointerMenuManager.PointerMenuEventArgs args)
    {
        var game_object = args.RaycastHit.transform.gameObject;
        Outline outline;
        if (ManagedOutlines.TryGetValue(game_object, out outline))
            Destroy(outline);
    }
}