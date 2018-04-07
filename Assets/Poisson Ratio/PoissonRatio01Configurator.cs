using System.Collections;
using UnityEngine;

public class PoissonRatio01Configurator : MonoBehaviour
{
    void Start()
    {
        //yield return new WaitForSeconds(1f);
        var main_camera_object = GameObject.FindGameObjectWithTag("MainCamera");
        //var main_camera = main_camera_object.GetComponent<Camera>();
        foreach (var fc in FindObjectsOfType<LookAtCamera>())
            fc.CameraTransform = main_camera_object.transform;
    }
}