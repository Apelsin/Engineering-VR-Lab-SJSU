using CVRLabSJSU;
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
                // Lever press handler
                lever.Pressed.AddListener(() =>
                {
                    // If the tester's grabber is not busy
                    if (!tester.GrabberIsBusy)
                    {
                        // If the tester's grabber is reset
                        if (tester.GrabberIsReset)
                        {
                            // If there is a specimen being clamped
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

                                // For both graphers/displays
                                foreach (var grapher in new[] { single_grapher, multi_grapher })
                                {
                                    // Skip missing graphers
                                    if (grapher == null)
                                        continue;

                                    grapher.Curve = specimen_properties.NormalizedStressStrain;

                                    grapher.LineColor = specimen_properties.CurveColor;

                                    // Set the curve bounds for all graphers (necessary for drawing the curve correctly)
                                    grapher.CurveBounds = new Rect(0f, 0f, max_strain, max_stress);

                                    grapher.Period = 0.1f; // TODO
                                    grapher.Graph();
                                }

                                // For single grapher, assign labels for specific points on the curve
                                // TODO: use tags or something more robust than GetComponentIn*
                                var tensile_graph_controller = single_grapher
                                    .GetComponentInParent<Canvas>()
                                    .GetComponentInChildren<TensileGraphController>();
                                
                                if (tensile_graph_controller)
                                {
                                    // TODO: struct for these properties???
                                    tensile_graph_controller.YieldStrength = specimen_properties.YieldStrength;
                                    tensile_graph_controller.UltimateTensileStrength = specimen_properties.UltimateTensileStrength;
                                    tensile_graph_controller.FracturePoint = specimen_properties.FracturePoint;
                                }
                                else
                                    Debug.LogWarning("TensileGraphController is missing.");

                                // For multi/comparison grapher, assign one label for the newly-added curve
                                // TODO: use tags or something more robust than GetComponentIn*
                                var tensile_graph_id_controller = multi_grapher
                                    .GetComponentInParent<Canvas>()
                                    .GetComponentInChildren<TensileGraphIdentificationController>();
                                if (tensile_graph_controller)
                                {
                                    // Parse the material type
                                    tensile_graph_id_controller.SetCurrentSpecimenType(specimen_properties.MaterialType);
                                }
                                else
                                    Debug.LogWarning("TensileGraphItendificationController is missing.");

                                tester.OnBeginTensileTest();
                            }
                            else
                                Debug.LogWarning("Tester needs a clamped specimen to operate");
                        }
                        else // If the grabber is not reset
                        {
                            // Reset just the single grapher
                            foreach (var grapher in new[] { single_grapher /*, multi_grapher */ })
                            {
                                grapher.Period = 0.01f; // TODO
                                grapher.Clear();
                            }
                            // Reset the tester (grabber)
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