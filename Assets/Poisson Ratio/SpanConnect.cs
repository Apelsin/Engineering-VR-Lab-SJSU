using UnityEngine;

[ExecuteInEditMode]
public class SpanConnect : MonoBehaviour
{
    public Transform A;
    public Transform B;
    [Range(0f, 1f)]
    public float CenterBalance = 0.5f;

    [Range(0f, 1f)]
    public float Length = 0.5f;

    [Range(-1f, 1f)]
    public float LengthBalance = 0f;

    public Vector3 BasisRotation;

    //[Range(0f, 4f)]
    //public float LateralScale = 1f;

    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        var center = Vector3.LerpUnclamped(A.position, B.position, CenterBalance);
        var direction = (B.position - A.position).normalized;
        Quaternion rotation;
        if (direction != Vector3.zero)
        {
            rotation = Quaternion.LookRotation(direction) * Quaternion.Euler(BasisRotation);
            transform.rotation = rotation;
        }
        //var length = /*LateralScale * */ Vector3.Distance(A.position, B.position);
        var local_scale = new Vector3(Length, Length, Length);
        var local_direction = transform.parent.InverseTransformDirection(direction);
        transform.position = center + transform.parent.TransformVector(local_direction * LengthBalance * Length);
        //transform.localPosition = ;
        //transform.localScale = local_scale;
    }
}