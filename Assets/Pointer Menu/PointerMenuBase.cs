using UnityEngine;
using VRTK;

namespace CVRLabSJSU
{
    public abstract class PointerMenuBase : MonoBehaviour
    {
        protected DestinationMarkerEventReceiver DestinationEvents;
        protected VRTK_InteractableObject InteractableObject;

        protected abstract void OnUse(VRTK_Pointer pointer);

        protected abstract void OnDestinationMarkerEnter(
            VRTK_Pointer pointer,
            Vector3 destination_position,
            RaycastHit raycast_hit = default(RaycastHit));

        private void HandleDestinationMarkerEnter(object sender, DestinationMarkerEventArgs args)
        {
            var pointer = (sender as Component)?.GetComponent<VRTK_Pointer>();
            OnDestinationMarkerEnter(pointer, args.destinationPosition, args.raycastHit);
        }

        private void HandleUse(object sender, InteractableObjectEventArgs args)
        {
            var pointer = args.interactingObject?.GetComponent<VRTK_Pointer>();
            OnUse(pointer);
        }

        protected virtual void OnEnable()
        {
            DestinationEvents = GetComponent<DestinationMarkerEventReceiver>();
            InteractableObject = GetComponent<VRTK_InteractableObject>();
            DestinationEvents.DestinationMarkerEnter.AddListener(HandleDestinationMarkerEnter);
            InteractableObject.InteractableObjectUsed += HandleUse;
            if (!InteractableObject.isUsable)
                Debug.LogWarning("InteractableObject.isUsable is false (should be true).");
            if (!InteractableObject.pointerActivatesUseAction)
                Debug.LogWarning("InteractableObject.pointerActivatesUseAction is false (should be true).");
        }

        protected virtual void OnDisable()
        {
            DestinationEvents.DestinationMarkerEnter.RemoveListener(HandleDestinationMarkerEnter);
            InteractableObject.InteractableObjectUsed -= HandleUse;
            DestinationEvents = null;
            InteractableObject = null;
        }
    }
}