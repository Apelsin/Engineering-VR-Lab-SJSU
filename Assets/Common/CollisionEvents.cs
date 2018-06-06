using System;
using UnityEngine;
using UnityEngine.Events;

namespace CVRLabSJSU
{
    public class CollisionEvents : MonoBehaviour
    {
        [Serializable]
        public class Event : UnityEvent<Collision> { }

        [SerializeField]
        private Event CollisionEnter;

        [SerializeField]
        private Event CollisionExit;

        [SerializeField]
        private Event CollisionStay;

        public void AddCollisionEnterHandler(UnityAction<Collision> handler)
        {
            CollisionEnter.AddListener(handler);
        }

        public void RemoveCollisionEnterHandler(UnityAction<Collision> handler)
        {
            CollisionEnter.RemoveListener(handler);
        }

        public void AddCollisionExitHandler(UnityAction<Collision> handler)
        {
            CollisionExit.AddListener(handler);
        }

        public void RemoveCollisionExitHandler(UnityAction<Collision> handler)
        {
            CollisionExit.RemoveListener(handler);
        }

        public void AddCollisionStayHandler(UnityAction<Collision> handler)
        {
            CollisionStay.AddListener(handler);
        }

        public void RemoveCollisionStayHandler(UnityAction<Collision> handler)
        {
            CollisionStay.RemoveListener(handler);
        }

        private void OnCollisionEnter(Collision collision)
        {
            CollisionEnter.Invoke(collision);
        }

        private void OnCollisionExit(Collision collision)
        {
            CollisionExit.Invoke(collision);
        }

        private void OnCollisionStay(Collision collision)
        {
            CollisionStay.Invoke(collision);
        }
    }
}