using UnityEngine;

namespace CVRLabSJSU
{
    public class CanvasCursor : MonoBehaviour, ISerializationCallbackReceiver
    {
        [SerializeField]
        private RectTransform _RectTransform;

        public RectTransform RectTransform
        {
            get { return _RectTransform; }
            private set { _RectTransform = value; }
        }

        public void OnAfterDeserialize()
        {
        }

        public void OnBeforeSerialize()
        {
            if (RectTransform == null)
                RectTransform = GetComponent<RectTransform>();
        }

        private void Start()
        {
        }

        private void Update()
        {
        }
    }
}