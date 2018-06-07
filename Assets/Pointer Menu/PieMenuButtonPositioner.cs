using UnityEngine;
using UnityEngine.UI;

namespace CVRLabSJSU
{
    [ExecuteInEditMode]
    public class PieMenuButtonPositioner : MonoBehaviour
    {
#if UNITY_EDITOR
        [Range(-180f, 180f)]
        public float StartingAngle = 0f;
        [Range(-180f, 180f)]
        public float Twist = 0f;
        private void OnValidate()
        {
            var index = 0;
            var number_of_children = transform.childCount;
            foreach (Transform button_transform in transform)
            {
                float t = (float)index / (float)number_of_children;
                float angle = 360f * -t + StartingAngle;
                button_transform.localRotation = Quaternion.AngleAxis(angle, Vector3.forward) * Quaternion.AngleAxis(Twist, Vector3.up);
                // Align text with this tranform
                var text = button_transform.GetComponentInChildren<Text>();
                text.transform.rotation = transform.rotation;
                index++;
            }
        }
#endif
    }
}