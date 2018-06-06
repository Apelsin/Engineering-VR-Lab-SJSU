using System;
using UnityEngine;
using UnityEngine.Events;

namespace CVRLabSJSU
{
    public class ControllerGrabObject : MonoBehaviour, ISerializationCallbackReceiver
    {
        public class StartEventArgs : EventArgs { }

        public static event EventHandler<StartEventArgs> Started;

        public class GrabEventArgs : EventArgs
        {
            public GameObject GameObject;
            public SteamVR_Controller.Device ControllerDevice;
        }

        [Serializable]
        public class GrabEvent : UnityEvent<object, GrabEventArgs> { }

        public SteamVR_TrackedObject TrackedObject;

        [SerializeField]
        private GrabEvent _Grabbed;

        public GrabEvent Grabbed => _Grabbed;

        [SerializeField]
        private GrabEvent _Released;

        public GrabEvent Released => _Released;

        private SteamVR_Controller.Device Controller
        {
            get { return SteamVR_Controller.Input((int)TrackedObject.index); }
        }

        private GameObject CollidingObject;
        private GameObject HeldObject;
        //private Grabbed HeldObjectGrabMarker;

        private void SetCollidingObject(Collider col)
        {
            if (CollidingObject || !col.GetComponent<Rigidbody>())
            {
                return;
            }
            CollidingObject = col.gameObject;
        }

        // When the trigger collider enters another, this sets up the other collider as a potential grab target.
        public void OnTriggerEnter(Collider other)
        {
            SetCollidingObject(other);
        }

        // Similar to trigger, but different because it ensures that the target is set when the player holds a controller over an object for a while. Without this, the collision may fail or become buggy.
        public void OnTriggerStay(Collider other)
        {
            SetCollidingObject(other);
        }

        // When the collider exits an object, abandoning an ungrabbed target, this code removes its target by setting it to null.
        public void OnTriggerExit(Collider other)
        {
            if (!CollidingObject)
            {
                return;
            }

            CollidingObject = null;
        }

        private void GrabObject()
        {
            // Move the GameObject inside the player’s hand and remove it from the CollidingObject variable.
            HeldObject = CollidingObject;
            // Add the grabbed marker to the held object
            //HeldObjectGrabMarker = HeldObject.AddComponent<Grabbed>();
            CollidingObject = null;
            // Add a new joint that connects the controller to the object using the AddFixedJoint() method below.
            var joint = AddFixedJoint();
            joint.connectedBody = HeldObject.GetComponent<Rigidbody>();
            Grabbed.Invoke(this, new GrabEventArgs()
            {
                ControllerDevice = Controller,
                GameObject = HeldObject
            });
        }

        // Make a new fixed joint, add it to the controller, and then set it up so it doesn’t break easily. Finally, you return it.
        private FixedJoint AddFixedJoint()
        {
            FixedJoint fx = gameObject.AddComponent<FixedJoint>();
            fx.breakForce = 20000;
            fx.breakTorque = 20000;
            return fx;
        }

        private void ReleaseObject()
        {
            // Make sure there’s a fixed joint attached to the controller.
            if (GetComponent<FixedJoint>())
            {
                // Remove the connection to the object held by the joint and destroy the joint.
                GetComponent<FixedJoint>().connectedBody = null;
                Destroy(GetComponent<FixedJoint>());
                // Add the speed and rotation of the controller when the player releases the object, so the result is a realistic arc.
                var held_rigidbody = HeldObject.GetComponent<Rigidbody>();
                held_rigidbody.velocity = Controller.velocity;
                held_rigidbody.angularVelocity = Controller.angularVelocity;
            }
            // Destroy the grabbed marker on the held object
            //Destroy(HeldObjectGrabMarker);
            //HeldObjectGrabMarker = null;
            // Remove the reference to the formerly attached object.
            var was_held = HeldObject;
            HeldObject = null;
            Released.Invoke(this, new GrabEventArgs()
            {
                ControllerDevice = Controller,
                GameObject = was_held
            });
        }

        private void Start()
        {
            Started?.Invoke(this, new StartEventArgs());
        }

        // Update is called once per frame
        private void Update()
        {
            // When the player squeezes the trigger and there’s a potential grab target, this grabs it.
            if (Controller.GetHairTriggerDown())
            {
                if (CollidingObject)
                {
                    GrabObject();
                }
            }

            // If the player releases the trigger and there’s an object attached to the controller, this releases it.
            if (Controller.GetHairTriggerUp())
            {
                if (HeldObject)
                {
                    ReleaseObject();
                }
            }
        }

        public void OnBeforeSerialize()
        {
            TrackedObject = TrackedObject ?? GetComponent<SteamVR_TrackedObject>();
        }

        public void OnAfterDeserialize()
        {
        }
    }
}