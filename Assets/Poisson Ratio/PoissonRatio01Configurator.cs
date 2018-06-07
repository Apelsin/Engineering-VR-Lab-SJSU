using cakeslice;
using CVRLabSJSU;
using UnityEngine;

public class PoissonRatio01Configurator : MonoBehaviour
{
    private void Start()
    {
        //yield return new WaitForSeconds(1f);
        var main_camera_object = GameObject.FindGameObjectWithTag("MainCamera");
        //var main_camera = main_camera_object.GetComponent<Camera>();
        foreach (var fc in FindObjectsOfType<LookAtCamera>())
            fc.CameraTransform = main_camera_object.transform;

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