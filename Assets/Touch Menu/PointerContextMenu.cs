using UnityEngine;
using UnityEngine.UI;

public class PointerContextMenu : MonoBehaviour
{
    public Animator Animator;
    public Transform MainCameraTransform;
    public Vector3 TargetPosition;
    public Button[] Buttons;
    private void OnUpdate(bool apply_scale)
    {
        var offset = (TargetPosition - MainCameraTransform.position);
        transform.position = TargetPosition;
        transform.rotation = Quaternion.LookRotation(offset.normalized, Vector3.up);
        if (apply_scale)
        {
            var scale = offset.magnitude;
            transform.localScale = new Vector3(scale, scale, scale);
        }
    }
    private void Start()
    {
        if (MainCameraTransform == null)
            return;
        OnUpdate(true);
    }
    private void Update()
    {
        if (MainCameraTransform == null)
            return;
        OnUpdate(false);
    }

    public void Pulse()
    {
        Animator.SetTrigger("Pulse");
    }

    public void RequestDestroy()
    {
        Animator.SetTrigger("RequestDestroy");
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}