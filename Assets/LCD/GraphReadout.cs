using UnityEngine;

namespace CVRLabSJSU
{
    public class GraphReadout : MonoBehaviour
    {
        public CurveGrapher Grapher;
        public PlotTraceReadout Readout;

        [Range(0.0001f, 0.01f)]
        public float SlopeDt = 0.001f;

        // TODO: Decouple GraphReadout and GraphTrace
        public void HandleTraceUpdated(object sender, GraphTrace.TraceUpdateEventArgs args)
        {
            var x0 = Mathf.Lerp(Grapher.CurveBounds.xMin, Grapher.CurveBounds.xMax, args.NormalizedCoordinates.x);
            var x1 = Mathf.Lerp(Grapher.CurveBounds.xMin, Grapher.CurveBounds.xMax, args.NormalizedCoordinates.x + SlopeDt);
            var dx = x1 - x0;
            var y = Mathf.Lerp(Grapher.CurveBounds.yMin, Grapher.CurveBounds.yMax, args.NormalizedCoordinates.y);
            var fx0 = Grapher.EvaluateCurveBounded(x0);
            var fx1 = Grapher.EvaluateCurveBounded(x1);
            var slope = (fx1 - fx0) / dx;
            Readout.SetTraceText(x0, y, fx0, slope);
        }
    }
}