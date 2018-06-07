using System;
using UnityEngine;
using UnityEngine.Events;
using VRTK;

namespace CVRLabSJSU
{
    public class DestinationMarkerEventReceiver : MonoBehaviour
    {
        [Serializable]
        public class DestinationMarkerEvent : UnityEvent<object, DestinationMarkerEventArgs> { }

        [SerializeField]
        private DestinationMarkerEvent _DestinationMarkerEnter;

        public DestinationMarkerEvent DestinationMarkerEnter => _DestinationMarkerEnter;

        [SerializeField]
        private DestinationMarkerEvent _DestinationMarkerExit;

        public DestinationMarkerEvent DestinationMarkerExit => _DestinationMarkerExit;

        [SerializeField]
        private DestinationMarkerEvent _DestinationMarkerSet;

        public DestinationMarkerEvent DestinationMarkerSet => _DestinationMarkerSet;

        public void OnDestinationMarkerEnter(object sender, DestinationMarkerEventArgs args)
        {
            _DestinationMarkerEnter.Invoke(sender, args);
        }

        public void OnDestinationMarkerExit(object sender, DestinationMarkerEventArgs args)
        {
            _DestinationMarkerExit.Invoke(sender, args);
        }

        public void OnDestinationMarkerSet(object sender, DestinationMarkerEventArgs args)
        {
            _DestinationMarkerSet.Invoke(sender, args);
        }
    }
}