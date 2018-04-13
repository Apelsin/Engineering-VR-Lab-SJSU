using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TensileLabConfigurator : MonoBehaviour
{
    private HashSet<GameObject> GrabbedObjects = new HashSet<GameObject>();

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

    private void OnSubscribeToGrabber(ControllerGrabObject grabber)
    {
        // Ensure listeners are added once (and only once)
        grabber.Grabbed.RemoveListener(HandleGrabbed);
        grabber.Grabbed.AddListener(HandleGrabbed);
        grabber.Released.RemoveListener(HandleReleased);
        grabber.Released.AddListener(HandleReleased);
    }

    private void HandleControllerGrabObjectStarted(object sender, ControllerGrabObject.StartEventArgs e)
    {
        var grabber = (ControllerGrabObject)sender;
        OnSubscribeToGrabber(grabber);
    }

    private void Start()
    {
        var tester = FindObjectOfType<InstronTester>();
        var lever = FindObjectOfType<LeverPressed>();

        var single_grapher = GameObject
            .FindGameObjectWithTag("Single Test Tensile Graph")
            ?.GetComponent<CurveGrapher>();

        var multi_grapher = GameObject
            .FindGameObjectWithTag("Multi Test Tensile Graph")
            ?.GetComponent<CurveGrapher>();

        //var grapher = FindObjectOfType<CurveGrapher>();

        {
            if (lever)
            {
                lever.Pressed.AddListener(() =>
                {
                    if (!tester.GrabberIsBusy)
                    {
                        if (tester.GrabberIsReset)
                        {
                            var clamped_specimen = tester.ClampedSpecimen;
                            if (clamped_specimen)
                            {
                                // *Inhale*...
                                var specimen_properties = clamped_specimen?.GetComponent<TTSpecimenProperties>();
                                float max_strain = specimen_properties.MaxStrain;
                                float max_stress = specimen_properties.MaxStress;

                                // Only set the graph bounds for the single grapher
                                single_grapher.GraphBounds = new Rect(0f, 0f, max_strain, max_stress);

                                // Only clear the single grapher
                                single_grapher.ClearImmediately();

                                foreach (var grapher in new[] { single_grapher, multi_grapher })
                                {
                                    // Skip missing graphers
                                    if (grapher == null)
                                        continue;

                                    var tensile_labeler = grapher.GetComponent<TensileGraphController>();
                                    grapher.Curve = specimen_properties.NormalizedStressStrain;

                                    grapher.LineColor = specimen_properties.CurveColor;

                                    // Set the curve bounds for all graphers (necessary for drawing the curve correctly)
                                    grapher.CurveBounds = new Rect(0f, 0f, max_strain, max_stress);

                                    if (tensile_labeler)
                                    {
                                        // TODO: struct for these properties???
                                        tensile_labeler.YieldStrength = specimen_properties.YieldStrength;
                                        tensile_labeler.UltimateTensileStrength = specimen_properties.UltimateTensileStrength;
                                        tensile_labeler.FracturePoint = specimen_properties.FracturePoint;
                                    }

                                    grapher.Period = 0.1f; // TODO
                                    grapher.Graph();
                                }
                                tester.OnBeginTensileTest();
                            }
                            else
                                Debug.LogWarning("Tester needs a clamped specimen to operate");
                        }
                        else
                        {
                            foreach (var grapher in new[] { single_grapher /*, multi_grapher */ })
                            {
                                grapher.Period = 0.01f; // TODO
                                grapher.Clear();
                            }
                            tester.OnResetTensileTest();
                        }
                    }
                    else
                        Debug.LogError("Cannot activate tester because it is already in operation.");
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

        // Subscribe to handle new controller instances
        ControllerGrabObject.Started += HandleControllerGrabObjectStarted;

        // Handle existing grabbers
        foreach (var grabber in FindObjectsOfType<ControllerGrabObject>())
            OnSubscribeToGrabber(grabber);
    }

    private void OnDestroy()
    {
        ControllerGrabObject.Started -= HandleControllerGrabObjectStarted;
    }
}