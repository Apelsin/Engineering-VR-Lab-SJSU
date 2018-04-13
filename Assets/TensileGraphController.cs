using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TensileGraphController : MonoBehaviour
{
    public string YieldStrengthLabelText = "Yield Strength";
    public float YieldStrength = 0.25f;
    public string UltimateTensileStrengthLabelText = "Ult. Tensile Strength";
    public float UltimateTensileStrength = 0.5f;
    public string FracturePointLabelText = "Fracture";
    public float FracturePoint = 0.75f;

    private static bool InRange(float value, float lo, float hi)
    {
        return value >= lo && value <= hi;
    }

    public void HandlePointAdded(object sender, CurveGrapher.PointAddedEventArgs args)
    {
        // Technically speaking, this is all plainly horrible :))))
        // TODO: make this not bad code

        var area = args.Area ?? new Rect(0f, 0f, 1f, 1f);

        var grapher = (CurveGrapher)sender;

        var t1 = args.Segment.PointA.transform;
        var t2 = args.Segment.PointB.transform;

        var p1 = t1.localPosition;
        var p2 = t2.localPosition;

        var labels = new List<string>();
        if (InRange(Mathf.Lerp(area.xMin, area.xMax, YieldStrength), p1.x, p2.x))
            labels.Add(YieldStrengthLabelText);

        if (InRange(Mathf.Lerp(area.xMin, area.xMax, UltimateTensileStrength), p1.x, p2.x))
            labels.Add(UltimateTensileStrengthLabelText);

        if (InRange(Mathf.Lerp(area.xMin, area.xMax, FracturePoint), p1.x, p2.x))
        {
            labels.Add(FracturePointLabelText);
            grapher.Cancel();
        }

        if (labels.Any())
        {
            string label = string.Join(" ", labels.ToArray());
            grapher.AddLabel(label).transform.position = t2.position;
        }
    }
}