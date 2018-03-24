using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TensileLabConfigurator : MonoBehaviour
{
    private HashSet<GameObject> GrabbedObjects = new HashSet<GameObject>();

    private void HandleControllerGrabObjectStarted(object sender, ControllerGrabObject.StartEventArgs e)
    {
        var grabber = (ControllerGrabObject)sender;
        grabber.Grabbed.AddListener(HandleGrabbed);
        grabber.Released.AddListener(HandleReleased);
    }

    private void HandleGrabbed(object sender, ControllerGrabObject.GrabEventArgs args)
    {
        GrabbedObjects.Add(args.GameObject);
        // TODO: cache this, etc.
        var testers = FindObjectsOfType<InstronTester>();
        foreach (var tester in testers)
        {
            if (tester.ClampedSpecimen == args.GameObject)
                tester.ClampedSpecimen = null;
        }

        var rigidbody = args.GameObject.GetComponent<Rigidbody>();
        if (rigidbody)
            rigidbody.detectCollisions = false;
    }

    private void HandleReleased(object sender, ControllerGrabObject.GrabEventArgs args)
    {
        GrabbedObjects.Remove(args.GameObject);

        var rigidbody = args.GameObject.GetComponent<Rigidbody>();
        if (rigidbody)
            rigidbody.detectCollisions = true;
    }

    private void Start()
    {
        var tester = FindObjectOfType<InstronTester>();
        var lever = FindObjectOfType<LeverPressed>();
        var grapher = FindObjectOfType<CurveGrapher>();

        {
            if (lever)
            {
                lever.Pressed.AddListener(() =>
                {
                    var clamped_specimen = tester?.ClampedSpecimen;
                    if (clamped_specimen)
                    {
                        if (!tester.GrabberIsBusy)
                        {
                            if (tester.GrabberIsReset)
                            {
                                var specimen_properties = clamped_specimen?.GetComponent<TTSpecimenProperties>();
                                grapher.Curve = specimen_properties.NormalizedStressStrain;
                                float max_strain = specimen_properties.MaxStrain;
                                float max_stress = specimen_properties.MaxStress;
                                grapher.CurveBounds = new Rect(0f, 0f, max_strain, max_stress);
                                grapher.ClearImmediately();
                                grapher.Period = 0.1f; // TODO
                                grapher.Graph();
                            }
                            else
                            {
                                grapher.Period = 0.01f; // TODO
                                grapher.Clear();
                            }
                            tester.GrabberAnimator.SetTrigger("Toggle");
                        }
                        else
                            Debug.LogError("Cannot activate tester because it is already in operation.");
                    }
                    else
                        Debug.LogWarning("Tester needs a clamped specimen to operate");
                });
            }
            else
                Debug.LogError("Could not find lever component");
        }

        {
            var lever_trigger_events = lever?.GetComponent<TriggerEvents>();
            if (lever_trigger_events)
                lever_trigger_events.AddTriggerEnterHandler((other) =>
                {
                    if (other.gameObject.tag == "leftHand" || other.gameObject.tag == "rightHand")
                    {
                        lever.OnPress();
                    }
                });
            else
                Debug.LogError("Could not find lever trigger events component");
        }

        {
            var tester_collision_events = tester?.GetComponentsInChildren<CollisionEvents>();
            if (tester_collision_events.Any())
                foreach (var ce in tester_collision_events)
                {
                    ce.AddCollisionEnterHandler((collision) =>
                    {
                        if (collision.gameObject.tag == "material")
                        {
                            var specimen = collision.gameObject;
                        // Only assign the specimen if the user lets go of it (not still grabbing it)
                        if (!GrabbedObjects.Contains(specimen))
                            {
                                var specimen_properties = specimen?.GetComponent<TTSpecimenProperties>();
                                if (specimen_properties)
                                    tester.ClampedSpecimen = specimen;
                                else
                                    Debug.LogWarning("Object lacks specimen properties, ignoring (TTSpecimenProperties)");
                            }
                        }
                    });
                }
            else
                Debug.LogError("Could not find tensile tester collision events component");
        }

        ControllerGrabObject.Started += HandleControllerGrabObjectStarted;
    }

    private void OnDestroy()
    {
        ControllerGrabObject.Started -= HandleControllerGrabObjectStarted;
    }
}