using UnityEngine;
using VRTK;

namespace CVRLabSJSU
{
    [RequireComponent(typeof(VRTK_DestinationMarker))]
    public class DestinationMarkerInteractor : MonoBehaviour
    {
        private VRTK_DestinationMarker _DestinationMarker;

        void OnEnable()
        {
            _DestinationMarker = GetComponent<VRTK_DestinationMarker>();
            _DestinationMarker.DestinationMarkerEnter += _DestinationMarker_DestinationMarkerEnter;
            _DestinationMarker.DestinationMarkerExit += _DestinationMarker_DestinationMarkerExit;
            _DestinationMarker.DestinationMarkerSet += _DestinationMarker_DestinationMarkerSet;
        }

        void OnDisable()
        {
            _DestinationMarker.DestinationMarkerEnter -= _DestinationMarker_DestinationMarkerEnter;
            _DestinationMarker.DestinationMarkerExit -= _DestinationMarker_DestinationMarkerExit;
            _DestinationMarker.DestinationMarkerSet -= _DestinationMarker_DestinationMarkerSet;
        }

        private void _DestinationMarker_DestinationMarkerEnter(object sender, DestinationMarkerEventArgs args)
        {
            var receiver = args.target?.GetComponent<DestinationMarkerEventReceiver>();
            receiver?.OnDestinationMarkerEnter(sender, args);
        }

        private void _DestinationMarker_DestinationMarkerExit(object sender, DestinationMarkerEventArgs args)
        {
            var receiver = args.target?.GetComponent<DestinationMarkerEventReceiver>();
            receiver?.OnDestinationMarkerExit(sender, args);
        }

        private void _DestinationMarker_DestinationMarkerSet(object sender, DestinationMarkerEventArgs args)
        {
            var receiver = args.target?.GetComponent<DestinationMarkerEventReceiver>();
            receiver?.OnDestinationMarkerSet(sender, args);
        }
    }
}