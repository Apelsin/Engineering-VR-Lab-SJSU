using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CVRLabSJSU
{
    public class GraphTrace : MonoBehaviour
    {
        public class TraceUpdateEventArgs : EventArgs
        {
            public Vector2 NormalizedCoordinates;
            public Vector2 LCDCoordinates;
        }

        [Serializable]
        public class TraceUpdateEvent : UnityEvent<object, TraceUpdateEventArgs> { }

        [SerializeField]
        private TraceUpdateEvent _TraceUpdated;

        public TraceUpdateEvent TraceUpdated => _TraceUpdated;

        public RectTransform Cursor;

        public RectTransform PlotArea;
        public PlotTraceReadout Readout;

        public Image TraceLineX;
        public Image TraceLineY;

        private void Start()
        {
        }

        private static Vector2 RectConstrainPoint(Rect area, Vector2 point, Rect offset = default(Rect))
        {
            area.xMin -= offset.xMin - offset.x;
            area.xMax -= offset.xMax - offset.x;
            // IDK why this one is different, but this works
            area.yMin += offset.yMax - offset.y;
            area.yMax += offset.yMin - offset.y;
            return Vector2.Max(area.min, Vector2.Min(point, area.max));
        }

        private void Update()
        {
            var cursor_world_position = Cursor.position;
            var cursor_relative_position = PlotArea.InverseTransformPoint(cursor_world_position);
            var plot_rect = PlotArea.rect;
            var normalized_position = new Vector3()
            {
                x = Mathf.InverseLerp(plot_rect.xMin, plot_rect.xMax, cursor_relative_position.x),
                y = Mathf.InverseLerp(plot_rect.yMin, plot_rect.yMax, cursor_relative_position.y),
                z = cursor_relative_position.z
            };

            // TODO: decouple GraphTrace and PlotTraceReadout, TraceLineX/Y, etc. (use events + handlers?)

            var cursor_in_graph_area = plot_rect.Contains(cursor_relative_position);

            if (cursor_in_graph_area)
                OnTraceUpdated(cursor_relative_position, normalized_position);

            if (Readout.gameObject.activeInHierarchy)
                Readout.SetVisible(cursor_in_graph_area);

            // Don't allow the readout rect to escape the allowed area
            var allowed_area = PlotArea.rect;
            var readout_rect = Readout.RectTransform.rect;

            var cursor_rel_pos_constrained = RectConstrainPoint(allowed_area, cursor_relative_position);
            var readout_rel_pos_constrained = RectConstrainPoint(allowed_area, cursor_relative_position, readout_rect);

            var cursor_world_pos_constrained = PlotArea.TransformPoint(cursor_rel_pos_constrained);
            var readout_world_pos_constrained = PlotArea.TransformPoint(readout_rel_pos_constrained);

            Readout.RectTransform.position = readout_world_pos_constrained;

            // Hack: use the readout alpha value for the trace lines
            var color_x = TraceLineX.color;
            var color_y = TraceLineY.color;
            color_x.a = Readout.CanvasGroup.alpha;
            color_y.a = Readout.CanvasGroup.alpha;
            TraceLineX.color = color_x;
            TraceLineY.color = color_y;

            var pos_x = TraceLineX.rectTransform.localPosition;
            var pos_y = TraceLineY.rectTransform.localPosition;
            pos_x.x = cursor_rel_pos_constrained.x;
            pos_y.y = cursor_rel_pos_constrained.y;
            TraceLineX.rectTransform.localPosition = pos_x;
            TraceLineY.rectTransform.localPosition = pos_y;
        }

        public void OnTraceUpdated(Vector2 lcd_coordinates, Vector2 normalized_coordinates)
        {
            TraceUpdated.Invoke(this, new TraceUpdateEventArgs()
            {
                LCDCoordinates = lcd_coordinates,
                NormalizedCoordinates = normalized_coordinates
            });
        }

        //private static bool RectContainedInArea(RectTransform area_transform, RectTransform r)
        //{
        //    var area = area_transform.rect;
        //    var a = new Vector2(r.rect.xMin, r.rect.yMin);
        //    var b = new Vector2(r.rect.xMax, r.rect.yMin);
        //    var c = new Vector2(r.rect.xMin, r.rect.yMax);
        //    var d = new Vector2(r.rect.xMax, r.rect.yMax);
        //    a = r.TransformPoint(a);
        //    b = r.TransformPoint(b);
        //    c = r.TransformPoint(c);
        //    d = r.TransformPoint(d);
        //    a = area_transform.InverseTransformPoint(a);
        //    b = area_transform.InverseTransformPoint(b);
        //    c = area_transform.InverseTransformPoint(c);
        //    d = area_transform.InverseTransformPoint(d);
        //    Func<Vector2, bool> f = area.Contains;
        //    return f(a) && f(b) && f(c) && f(d);
        //}
    }
}