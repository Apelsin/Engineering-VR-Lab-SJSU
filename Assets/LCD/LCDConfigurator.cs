using UnityEngine;

namespace CVRLabSJSU
{
    public class LCDConfigurator : MonoBehaviour
    {
        private void Start()
        {
            var cursor_drivers = FindObjectsOfType<CanvasCursorDriver>();

            foreach (var cursor_driver in cursor_drivers)
                cursor_driver.Camera = Camera.main;
        }
    }
}