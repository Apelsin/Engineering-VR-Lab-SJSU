using cakeslice;
using CVRLabSJSU;
using UnityEngine;

public class PointerOutlineConfigurator : MonoBehaviour
{
    private void Start()
    {
        var pointer_menu_manager = FindObjectOfType<PointerMenuManager>();

        pointer_menu_manager.MenuAdded.AddListener(HandleMenuAdded);
        pointer_menu_manager.MenuRemoved.AddListener(HandleMenuRemoved);
    }

    private void HandleMenuAdded(object sender, PointerMenuManager.PointerMenuEventArgs args)
    {
        args.RaycastHit.transform.gameObject.AddComponent<Outline>();
    }

    private void HandleMenuRemoved(object sender, PointerMenuManager.PointerMenuEventArgs args)
    {
        var outline = args.RaycastHit.transform.gameObject.GetComponent<Outline>();
        if (outline)
            Destroy(outline);
    }
}