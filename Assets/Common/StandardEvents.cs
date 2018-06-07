using UnityEngine;
using UnityEngine.Events;

namespace CVRLabSJSU
{
    public class StandardEvents : MonoBehaviour
    {
        [SerializeField]
        private UnityEvent _OnEnable;

        public UnityEvent OnEnableEvent => _OnEnable;

        private void OnEnable()
        {
            OnEnableEvent.Invoke();
        }

        [SerializeField]
        private UnityEvent _OnDisable;

        public UnityEvent OnDisableEvent => _OnDisable;

        private void OnDisable()
        {
            OnDisableEvent.Invoke();
        }

        [SerializeField]
        private UnityEvent _Awake;

        public UnityEvent AwakeEvent => _Awake;

        private void Awake()
        {
            AwakeEvent.Invoke();
        }

        [SerializeField]
        private UnityEvent _Start;

        public UnityEvent StartEvent => _Start;

        private void Start()
        {
            StartEvent.Invoke();
        }
    }
}