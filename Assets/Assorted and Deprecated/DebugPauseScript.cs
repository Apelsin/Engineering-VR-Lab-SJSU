using UnityEngine;

namespace CVRLabSJSU
{
    public class DebugPauseScript : MonoBehaviour
    {
        private SteamVR_TrackedObject trackedObj;

        private void Awake()
        {
            trackedObj = GetComponent<SteamVR_TrackedObject>();
        }

        // Use this for initialization
        private void Start()
        {
        }

        // Update is called once per frame
        private void Update()
        {
            var device = SteamVR_Controller.Input((int)trackedObj.index);
            if (device.GetTouchDown(SteamVR_Controller.ButtonMask.Grip))
            {
                Debug.Break();
            }
        }
    }
}