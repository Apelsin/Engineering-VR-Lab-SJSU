using CVRLabSJSU;
using UnityEngine;

namespace CVRLabSJSU
{
    public class MouseInputDisabler : MonoBehaviour
    {
        private StandaloneInputModule2 StandaloneInputModule;
        void OnEnable()
        {
            // Disable mouse input
            StandaloneInputModule = FindObjectOfType<StandaloneInputModule2>();
            if (StandaloneInputModule)
                StandaloneInputModule.AllowProcessMouseEvent = false;
            else
                Debug.LogWarning("Could not disable mouse input because StandaloneInputModule2 is missing.");
        }

        void OnDisable()
        {
            if(StandaloneInputModule)
                StandaloneInputModule.AllowProcessMouseEvent = true;
            else
                Debug.LogWarning("Could not enable mouse input because StandaloneInputModule2 is missing.");
        }
    }
}