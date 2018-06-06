using UnityEngine;

namespace CVRLabSJSU
{
    public class TeleportPointer : MonoBehaviour, ISerializationCallbackReceiver
    {
        public SteamVR_TrackedObject TrackedObject;
        public GameObject TeleportReticlePrefab;
        public Vector3 ReticleOffset;
        public LayerMask LayerMask;

        public bool HandleTeleport = true;

        public Transform CameraRig;
        public Transform EyesTransform;

        private GameObject TeleportReticle;

        private SteamVR_Controller.Device Controller
        {
            get { return SteamVR_Controller.Input((int)TrackedObject.index); }
        }

        private void Update()
        {
            bool press = Controller.GetPress(SteamVR_Controller.ButtonMask.Touchpad);
            bool press_up = Controller.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad);
            var p = TrackedObject.transform.position;
            var fwd = transform.forward;
            RaycastHit hit;
            if ((press || press_up) && Physics.Raycast(p, fwd, out hit, 100, LayerMask))
            {
                if (press_up)
                {
                    if (HandleTeleport)
                        Teleport(hit.point);
                    TeleportReticle.SetActive(false);
                }
                else
                {
                    TeleportReticle.SetActive(true);
                    TeleportReticle.transform.position = hit.point + ReticleOffset;
                }
            }
            else
            {
                TeleportReticle.SetActive(false);
            }
        }

        private void Teleport(Vector3 point)
        {
            var camera_to_eyes = EyesTransform.position - CameraRig.position;
            camera_to_eyes.y = 0f; // TODO
            CameraRig.position = point - camera_to_eyes;
        }

        private void Start()
        {
            TeleportReticle = Instantiate(TeleportReticlePrefab, transform, true);
            TeleportReticle.transform.SetParent(null, true);
        }

        public void OnBeforeSerialize()
        {
            TrackedObject = TrackedObject ?? GetComponent<SteamVR_TrackedObject>();
        }

        public void OnAfterDeserialize()
        {
        }
    }
}