using System;
using UnityEngine;
using UnityEngine.Events;

namespace CVRLabSJSU
{
    public class TriggerEvents : MonoBehaviour
    {
        [Serializable]
        public class Event : UnityEvent<Collider> { }

        [SerializeField]
        private Event TriggerEnter;

        [SerializeField]
        private Event TriggerExit;

        [SerializeField]
        private Event TriggerStay;

        public void AddTriggerEnterHandler(UnityAction<Collider> handler)
        {
            TriggerEnter.AddListener(handler);
        }

        public void RemoveTriggerEnterHandler(UnityAction<Collider> handler)
        {
            TriggerEnter.RemoveListener(handler);
        }

        public void AddTriggerExitHandler(UnityAction<Collider> handler)
        {
            TriggerExit.AddListener(handler);
        }

        public void RemoveTriggerExitHandler(UnityAction<Collider> handler)
        {
            TriggerExit.RemoveListener(handler);
        }

        public void AddTriggerStayHandler(UnityAction<Collider> handler)
        {
            TriggerStay.AddListener(handler);
        }

        public void RemoveTriggerStayHandler(UnityAction<Collider> handler)
        {
            TriggerStay.RemoveListener(handler);
        }

        private void OnTriggerEnter(Collider collider)
        {
            TriggerEnter.Invoke(collider);
        }

        private void OnTriggerExit(Collider collider)
        {
            TriggerExit.Invoke(collider);
        }

        private void OnTriggerStay(Collider collider)
        {
            TriggerStay.Invoke(collider);
        }
    }
}