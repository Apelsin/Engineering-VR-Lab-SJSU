using UnityEngine;

public class TeleportPointer : MonoBehaviour, ISerializationCallbackReceiver
{
    public GameObject TeleportReticlePrefab;
    public SteamVR_TrackedObject TrackedObject;
    public Transform CameraRig;
    public Transform HeadTransform;
    public Vector3 TeleportReticleOffset;
    public LayerMask TeleportMask;

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
        if ((press || press_up) && Physics.Raycast(p, fwd, out hit, 100, TeleportMask))
        {
            if (press_up)
            {
                Teleport(hit.point);
            }
            else
            {
                TeleportReticle.SetActive(true);
                TeleportReticle.transform.position = hit.point + TeleportReticleOffset;
            }
        }
        else
        {
            TeleportReticle.SetActive(false);
        }
    }

    private void Teleport(Vector3 point)
    {
        TeleportReticle.SetActive(false);
        Vector3 difference = CameraRig.position - HeadTransform.position;
        difference.y = 0;
        CameraRig.position = point + difference;
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