using UnityEngine;

[ExecuteInEditMode]
public class PieMenuButtonPositioner : MonoBehaviour
{
#if UNITY_EDITOR
    [Range(-180f, 180f)]
    public float Twist = 0f;
    private void OnValidate()
    {
        var index = 0;
        var number_of_children = transform.childCount;
        foreach(Transform button_transform in transform)
        {
            float t = (float)index / (float)number_of_children;
            button_transform.localRotation = Quaternion.AngleAxis(360f * -t, Vector3.forward) * Quaternion.AngleAxis(Twist, Vector3.up);
            index++;
        }
    }
#endif
}