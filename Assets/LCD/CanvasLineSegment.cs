using UnityEngine;

namespace CVRLabSJSU
{
    [ExecuteInEditMode]
    public class CanvasLineSegment : MonoBehaviour, ISerializationCallbackReceiver
    {
        [SerializeField]
        private RectTransform _RectTransform;

        public RectTransform RectTransform
        {
            get { return _RectTransform; }
            private set { _RectTransform = value; }
        }

        [SerializeField]
        private Transform _PointA;

        public Transform PointA
        {
            get { return _PointA; }
            set
            {
                _PointA = value;
                OnPositionChange();
            }
        }

        [SerializeField]
        private Transform _PointB;

        public Transform PointB
        {
            get { return _PointB; }
            set
            {
                _PointB = value;
                OnPositionChange();
            }
        }

        [SerializeField]
        [Range(0f, 1f)]
        private float _CenterBalance = 0.5f;

        public float CenterBalance
        {
            get { return _CenterBalance; }
            set
            {
                _CenterBalance = value;
                OnPositionChange();
            }
        }

        [SerializeField]
        [Range(0f, 360f)]
        private float _Angle = 0f;

        public float Angle
        {
            get { return _Angle; }
            set
            {
                _Angle = value;
                OnPositionChange();
            }
        }

        [SerializeField]
        private float _Width = 5f;

        public float Width
        {
            get { return _Width; }
            set
            {
                _Width = value;
                SetWidth(value);
            }
        }

        private void SetWidth(float width)
        {
            var size = RectTransform.sizeDelta;
            size.y = width;
            RectTransform.sizeDelta = size;
        }

        private void Connect(Transform a, Transform b, float center_balance, float angle)
        {
            Connect(a.position, b.position, center_balance, angle);
        }

        private void Connect(Vector3 a, Vector3 b, float center_balance, float angle)
        {
            var local_a = transform.parent.InverseTransformPoint(a);
            var local_b = transform.parent.InverseTransformPoint(b);

            var local_position = Vector3.Lerp(local_a, local_b, center_balance);

            var right = (local_b - local_a).normalized;
            if (right == Vector3.zero)
                right = Vector3.right;

            // Incoming tricky rotational math
            var forward = Vector3.forward;
            var up = Vector3.Cross(right, forward);

            var local_rotation = Quaternion.LookRotation(right, up) * Quaternion.Euler(0f, 90f, 0f);

            var size = RectTransform.sizeDelta;
            var length = Vector3.Distance(local_a, local_b);
            size.x = length;

            RectTransform.localPosition = local_position;
            RectTransform.sizeDelta = size;
            RectTransform.localRotation = local_rotation;
        }

        private void Start()
        {
            OnPositionChange();
            SetWidth(Width);
        }

        private Vector3? PointA_cached_position;
        private Vector3? PointB_cached_position;

        private void LateUpdate()
        {
            if (PointA != null && PointB != null)
            {
                if (!PointA_cached_position.HasValue ||
                    !PointB_cached_position.HasValue ||
                    PointA_cached_position != PointA.position ||
                    PointB_cached_position != PointB.position)
                {
                    Connect(PointA.position, PointB.position, CenterBalance, Angle);
                    PointA_cached_position = PointA.position;
                    PointB_cached_position = PointB.position;
                }
            }
        }

        private void OnPositionChange()
        {
            if (PointA != null && PointB != null)
                Connect(PointA, PointB, CenterBalance, Angle);
        }

        public void OnBeforeSerialize()
        {
            if (RectTransform == null)
                RectTransform = GetComponent<RectTransform>();

            PointA = PointA;
            PointB = PointB;
            Width = Width;
        }

        public void OnAfterDeserialize()
        {
        }
    }
}