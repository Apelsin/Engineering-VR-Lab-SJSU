using UnityEngine;
using UnityEngine.Events;

namespace CVRLabSJSU
{
    public class TransformChildrenChangedEvents : MonoBehaviour
    {
        [SerializeField]
        private UnityEvent _TransformChildrenChanged;

        public UnityEvent TransformChildrenChanged => _TransformChildrenChanged;

        [SerializeField]
        private UnityEvent _TransformHasChanged;

        public UnityEvent TransformHasChanged => _TransformHasChanged;

        private void OnTransformChildrenChanged()
        {
            TransformChildrenChanged.Invoke();
        }

        private void Update()
        {
            if (transform.hasChanged)
            {
                TransformHasChanged.Invoke();
                transform.hasChanged = false;
            }
        }
    }
}