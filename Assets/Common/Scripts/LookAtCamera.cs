using UnityEngine;

namespace CVRLabSJSU
{
    public class LookAtCamera : MonoBehaviour
    {
        public enum LookAtMethod
        {
            Directional,
            Rotational,
        }

        public Transform CameraTransform;

        public LookAtMethod Method;

        public bool Roll = true;
        public bool Pitch = true;
        public bool Yaw = true;

        public Vector3 TargetOffset;

        public bool AutomaticallyFindCamera = false;

        private void OnEnable()
        {
            if (AutomaticallyFindCamera)
                CameraTransform = GameObject.FindGameObjectWithTag("MainCamera")?.transform;
        }

        private static Quaternion GetDirectionalLookAtRotation(
            Transform source,
            Transform target,
            bool allow_roll,
            bool allow_pitch,
            bool allow_yaw,
            Vector3 target_offset)
        {
            var target_position = target.TransformPoint(target_offset);
            var forward = source.position - target_position;
            if (!allow_pitch)
                forward.y = 0f;
            forward.Normalize();
            //right = transform.right;
            var up = allow_roll ? target.up : Vector3.up;
            return Quaternion.LookRotation(forward, up);
        }

        private static Quaternion GetRotationalLookAtRotation(
            Transform source,
            Transform target,
            bool allow_roll,
            bool allow_pitch,
            bool allow_yaw,
            Vector3 target_offset)
        {
            var forward = target.forward;
            if (!allow_pitch)
            {
                forward.y = 0f;
                forward.Normalize();
            }
            var right = target.right;
            if (!allow_roll)
            {
                right.y = 0f;
                right.Normalize();
            }
            var up = Vector3.Cross(forward, right);
            return Quaternion.LookRotation(forward, up);
        }

        private void Update()
        {
            switch (Method)
            {
                case LookAtMethod.Directional:
                    transform.rotation = GetDirectionalLookAtRotation(
                        transform,
                        CameraTransform,
                        Roll,
                        Pitch,
                        Yaw,
                        TargetOffset);
                    break;

                case LookAtMethod.Rotational:
                    transform.rotation = GetDirectionalLookAtRotation(
                        transform,
                        CameraTransform,
                        Roll,
                        Pitch,
                        Yaw,
                        TargetOffset);
                    break;

                default:
                    break;
            }
        }
    }
}