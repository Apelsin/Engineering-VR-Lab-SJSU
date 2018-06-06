using UnityEngine;
using UnityEngine.UI;

public class PoissonDemoCanvas01 : MonoBehaviour
{
    public Text PressureReadout;
    public float PressureScale = 1f;
    public string PressureUnits;

    public Text PoissonRatioReadout;

    public RectTransform PoissonGraphTransform;
    public RectTransform PoissonGraphMarkerTransform;
    public RectTransform SlopeLine;

    [Range(1f, 5f)]
    public float PoissonGraphZoomAmount = 1f;

    public Transform Reference;

    public Transform ReferenceTensileMarker;
    public Transform ReferenceLateralMarker;

    public Transform SpecimenTensileMarker;
    public Transform SpecimenLateralMarker;

    public Collider ReferenceCollider;

    public Text EpsilonTReadout;
    public Text EpsilonLReadout;

    public bool DebugViz;

    /// <summary>
    /// Gets the tensile and lateral displacement (delta) values based on
    /// reference and specimen tensile marker transforms
    /// </summary>
    private Vector2 GetSpecimenDeltas()
    {
        var diff_t = SpecimenTensileMarker.position - ReferenceTensileMarker.position;
        var diff_l = SpecimenLateralMarker.position - ReferenceLateralMarker.position;

        float d_t = ReferenceTensileMarker.InverseTransformDirection(diff_t).z;
        float d_l = ReferenceLateralMarker.InverseTransformDirection(diff_l).x;

        // Tensile, lateral
        return new Vector2(d_t, d_l);
    }

    /// <summary>
    /// Gets the width and height of the reference model using its collider
    /// and a reference transform
    /// </summary>
    public Vector2 GetReferenceLengths()
    {
        var top = ReferenceCollider.ClosestPoint(Reference.position + 100f * Reference.forward);
        var bottom = ReferenceCollider.ClosestPoint(Reference.position - 100f * Reference.forward);
        var right = ReferenceCollider.ClosestPoint(Reference.position + 100f * Reference.right);
        var left = ReferenceCollider.ClosestPoint(Reference.position - 100f * Reference.right);

        if (DebugViz)
        {
            Debug.DrawLine(top, bottom, Color.magenta);
            Debug.DrawLine(left, right, Color.cyan);
        }

        // Tensile, lateral
        return new Vector2((top - bottom).magnitude, (right - left).magnitude);
    }

    public void OnPressureSliderValueChanged(float value)
    {
        var pressure = PressureScale * value;
        PressureReadout.text = $"{pressure:0} {PressureUnits}";
    }

    private void Start()
    {
    }

    private static float SquareRadius(float angle_in_radians)
    {
        float sec = 1f / Mathf.Cos(angle_in_radians);
        float csc = 1f / Mathf.Sin(angle_in_radians);
        float abs_sec = Mathf.Abs(sec);
        float abs_csc = Mathf.Abs(csc);
        return Mathf.Min(abs_sec, abs_csc);
    }

    // TODO: move to separate component
    private static void SetSlopeLine(RectTransform transform, Vector2 graph_size_delta, float dy, float dx)
    {
        var angle_in_radians = Mathf.Atan2(dy, dx); // In Radians
        var angle_in_degrees = angle_in_radians * Mathf.Rad2Deg;
        transform.localRotation = Quaternion.AngleAxis(angle_in_degrees, Vector3.forward);
        var size = transform.sizeDelta;

        // Assuming square graph for now!!!
        // TODO: rectangular graph size
        // TODO: remove hard-coded coefficient
        size.x = 0.9f * graph_size_delta.x * SquareRadius(angle_in_radians);

        transform.sizeDelta = size;
    }

    private static void SetPoissonReadout(Text readout, float value)
    {
        readout.text = $"{value:0.00}";
    }

    private void Update()
    {
        if (PoissonGraphTransform && PoissonGraphMarkerTransform)
        {
            var graph_size_delta = PoissonGraphTransform.sizeDelta;
            var deltas = GetSpecimenDeltas();
            var lengths = GetReferenceLengths();
            var e_t = deltas.x / lengths.x; // Tensile epsilon
            var e_l = -deltas.y / lengths.y; // Lateral epsilon (note the negative sign)

            PoissonGraphMarkerTransform.localPosition =
                PoissonGraphZoomAmount * new Vector2(e_t * graph_size_delta.x, e_l * graph_size_delta.y);

            EpsilonTReadout.text = $"{e_t:0.00}";
            EpsilonLReadout.text = $"{e_l:0.00}";

            if (e_t != 0f)
            {
                if (SlopeLine)
                    SetSlopeLine(SlopeLine, graph_size_delta, e_l, e_t);

                if (PoissonRatioReadout)
                    SetPoissonReadout(PoissonRatioReadout, -e_l / e_t);
            }
        }
    }
}