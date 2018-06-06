using RoaringFangs.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CVRLabSJSU
{
    public class CurveGrapher : MonoBehaviour, ISerializationCallbackReceiver
    {
        public class PointAddedEventArgs : EventArgs
        {
            public Vector3 Position;
            public CanvasLineSegment Segment;
            public Rect? Area;
        }

        [Serializable]
        public class PointAddedEvent : UnityEvent<object, PointAddedEventArgs> { }

        public LinePlotter Plotter;

        [SerializeField]
        [Range(0f, 1f)]
        private float _Period = 0.05f;

        public float Period { get { return _Period; } set { _Period = value; } }

        private float ZOffset = 0f;

        [SerializeField]
        private Color _LineColor = Color.black;

        public Color LineColor
        {
            get { return _LineColor; }
            set { _LineColor = value; }
        }

        public RectTransform GraphRectTransform;

        [SerializeField]
        private AnimationCurve _Curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        public AnimationCurve Curve { get { return _Curve; } set { _Curve = value; } }

        [SerializeField]
        private Rect _CurveBounds = new Rect(0f, 0f, 1f, 1f);

        public Rect CurveBounds { get { return _CurveBounds; } set { _CurveBounds = value; } }

        [SerializeField]
        private Rect _GraphBounds = new Rect(0f, 0f, 1f, 1f);

        public Rect GraphBounds { get { return _GraphBounds; } set { _GraphBounds = value; } }

        [Header("Evaluator Settings")]
        [SerializeField]
        [Range(0.001f, 1.0f)]
        private float _MaxSegmentLength = 0.01f;

        public float MaxSegmentLength
        {
            get { return _MaxSegmentLength; }
            set { _MaxSegmentLength = value; }
        }

        [SerializeField]
        [Range(1, 8)]
        private int _MaxIterations = 4;

        public int MaxIterations
        {
            get { return _MaxIterations; }
            set { _MaxIterations = value; }
        }

        [SerializeField]
        [Range(2, 266)]
        private int _MaxNumberOfPoints = 128;

        public int MaxNumberOfPoints
        {
            get { return _MaxNumberOfPoints; }
            set { _MaxNumberOfPoints = value; }
        }

        public RectTransform XAxisTickMarkRT;

        [SerializeField]
        private MonoBehaviour _XAxisTextPoolBehaviour;

        public IGameObjectPool XAxisTextPool
        {
            get { return (IGameObjectPool)_XAxisTextPoolBehaviour; }
            set { _XAxisTextPoolBehaviour = (MonoBehaviour)value; }
        }

        public RectTransform YAxisTickMarkRT;

        [SerializeField]
        private MonoBehaviour _YAxisTextPoolBehaviour;

        public IGameObjectPool YAxisTextPool
        {
            get { return (IGameObjectPool)_YAxisTextPoolBehaviour; }
            set { _YAxisTextPoolBehaviour = (MonoBehaviour)value; }
        }

        public Vector2Int NumberOfMajorTickMarks;

        public bool XAxisIsPercentage = false;
        public string XAxisNumberFormat = "{0}";
        public bool YAxisIsPercentage = false;
        public string YAxisNumberFormat = "{0}";

        [SerializeField]
        private MonoBehaviour _LabelPoolBehavior;

        public IGameObjectPool LabelPool
        {
            get { return (IGameObjectPool)_LabelPoolBehavior; }
            set { _LabelPoolBehavior = (MonoBehaviour)value; }
        }

        [SerializeField]
        private PointAddedEvent _PointAdded;

        public PointAddedEvent PointAdded => _PointAdded;

        private HashSet<Transform> Points = new HashSet<Transform>();

        private bool CancelCurrentOperation = false;
        private IEnumerator CurrentOperation;

        private IEnumerator Start()
        {
            for (; ; )
            {
                if (CurrentOperation != null)
                    while (CurrentOperation.MoveNext())
                    {
                        // This is probably a bad way to do this
                        if (!CancelCurrentOperation)
                        {
                            yield return CurrentOperation.Current;
                        }
                        else
                        {
                            CurrentOperation = null;
                            CancelCurrentOperation = false;
                            break;
                        }
                    }
                yield return new WaitForEndOfFrame();
            }
        }

        private static IEnumerator Concat(params IEnumerator[] enumerators)
        {
            for (int i = 0; i < enumerators.Length; i++)
            {
                var enumerator = enumerators[i];
                while (enumerator.MoveNext())
                    yield return enumerator.Current;
            }
        }

        private HashSet<GameObject> XAxisTexts = new HashSet<GameObject>();
        private HashSet<GameObject> YAxisTexts = new HashSet<GameObject>();
        private HashSet<GameObject> LabelObjects = new HashSet<GameObject>();

        public void SetMajorTickMarkNumbers(Rect bounds, Vector2Int counts)
        {
            SetMajorTickMarkNumbers(bounds.xMin, bounds.yMin, bounds.xMax, bounds.yMax, counts.x, counts.y);
        }

        public void SetMajorTickMarkNumbers(
            float x_min,
            float y_min,
            float x_max,
            float y_max,
            int x_count,
            int y_count)
        {
            var x_size = XAxisTickMarkRT.rect.width;
            var y_size = YAxisTickMarkRT.rect.height;

            foreach (var text in XAxisTexts)
                XAxisTextPool.Return(text);
            XAxisTexts.Clear();

            foreach (var text in YAxisTexts)
                YAxisTextPool.Return(text);
            YAxisTexts.Clear();

            // TODO: DRY this out
            for (int i = 0; i < x_count; i++)
            {
                var t = (float)i / (float)(x_count - 1);
                var x = Mathf.Lerp(x_min, x_max, t);
                var text = XAxisTextPool.Cycle();
                XAxisTexts.Add(text);
                text.transform.localPosition = new Vector2(t * x_size, 0f);
                string str = string.Format(XAxisNumberFormat, XAxisIsPercentage ? 100f * x : x);
                text.GetComponent<Text>().text = str;
            }
            for (int i = 0; i < y_count; i++)
            {
                var t = (float)i / (float)(y_count - 1);
                var y = Mathf.Lerp(y_min, y_max, t);
                var text = YAxisTextPool.Cycle();
                YAxisTexts.Add(text);
                text.transform.localPosition = new Vector2(0f, t * y_size);
                string str = string.Format(YAxisNumberFormat, YAxisIsPercentage ? 100f * y : y);
                text.GetComponent<Text>().text = str;
            }
        }

        private static float InverseLerpUnclamped(float a, float b, float c)
        {
            var n = c - a;
            var d = b - a;
            if (d == 0)
                return 0f;
            return n / d;
        }

        private static Rect GetNormalizedRect(Rect R, Rect r)
        {
            return new Rect()
            {
                xMin = InverseLerpUnclamped(R.xMin, R.xMax, r.xMin),
                xMax = InverseLerpUnclamped(R.xMin, R.xMax, r.xMax),
                yMin = InverseLerpUnclamped(R.yMin, R.yMax, r.yMin),
                yMax = InverseLerpUnclamped(R.yMin, R.yMax, r.yMax),
            };
        }

        private static Rect ScaleRect(Rect R, Rect r)
        {
            return new Rect()
            {
                xMin = Mathf.LerpUnclamped(R.xMin, R.xMax, r.xMin),
                xMax = Mathf.LerpUnclamped(R.xMin, R.xMax, r.xMax),
                yMin = Mathf.LerpUnclamped(R.yMin, R.yMax, r.yMin),
                yMax = Mathf.LerpUnclamped(R.yMin, R.yMax, r.yMax),
            };
        }

        public void Graph()
        {
            var graph_rect = GraphRectTransform.rect;
            var normalized_rect = GetNormalizedRect(GraphBounds, CurveBounds);
            var area_rect = ScaleRect(graph_rect, normalized_rect);

            //var clear = EraseCurve(Plotter, 0f).GetEnumerator();
            Plotter.Break();
            var plot = PlotCurve(
                Plotter,
                Curve,
                Period,
                ZOffset,
                LineColor,
                area_rect,
                MaxSegmentLength,
                MaxIterations,
                MaxNumberOfPoints)
                .GetEnumerator();
            //StartCoroutine(plot.GetEnumerator());
            CurrentOperation = plot;// Concat(clear, plot);
            SetMajorTickMarkNumbers(GraphBounds, NumberOfMajorTickMarks);
        }

        public void Cancel()
        {
            // TODO: is this a good idea...?
            CancelCurrentOperation = true;
        }

        public void Clear()
        {
            //StartCoroutine(EraseCurve(Plotter, Period).GetEnumerator());
            Plotter.Break();
            CurrentOperation = EraseCurve(Plotter, Period).GetEnumerator();
        }

        public void ClearImmediately()
        {
            //StartCoroutine(EraseCurve(Plotter, 0f).GetEnumerator());
            Plotter.Break();
            foreach (var _ in EraseCurve(Plotter, 0f)) ;
        }

        private static float Distance(float x0, float y0, float x1, float y1)
        {
            var dx = x1 - x0;
            var dy = y1 - y0;
            var dxdx = dx * dx;
            var dydy = dy * dy;
            return Mathf.Sqrt(dxdx + dydy);
        }

        private static bool EquidistantStep(
            Func<float, float> f,
            float x0,
            float y0,
            float max_segment_length,
            int max_iterations,
            out float x1,
            out float y1)
        {
            var d = max_segment_length;
            x1 = x0 + d;
            y1 = f(x1);
            for (int i = 0; i < max_iterations; i++)
            {
                d *= 0.5f;
                var distance = Distance(x0, y0, x1, y1);
                if (distance > max_segment_length)
                    x1 -= d;
                else
                    x1 += d;
                y1 = f(x1);
            }
            return x0 != x1 && y0 != y1;
        }

        public GraphLabel AddLabel(string label_text, string sublabel_text = "", Color? color = null)
        {
            var label_object = LabelPool.Cycle();
            var label = label_object.GetComponent<GraphLabel>();
            label.Text.text = label_text;
            label.SecondaryText.text = sublabel_text;
            if (color.HasValue)
            {
                label.Text.color = color.Value;
                label.SecondaryText.color = color.Value;
            }
            LabelObjects.Add(label_object);
            return label;
        }

        //public void SetLabelText(GameObject label_object, string label_text, string sublabel_text)
        //{
        //    //label.GetComponentInChildren<Text>().text = label_text;
        //    var label = label_object.GetComponent<GraphLabel>();
        //    label.Text.text = label_text;
        //    label.SecondaryText.text = sublabel_text; // TODO
        //}

        public void RemoveLabel(GameObject label_object)
        {
            throw new NotImplementedException();
        }

        private void OnPointAdded(Vector3 position, CanvasLineSegment segment, Rect? area)
        {
            PointAdded.Invoke(this, new PointAddedEventArgs()
            {
                Position = position,
                Segment = segment,
                Area = area
            });
        }

        private IEnumerable PlotCurve(
            LinePlotter plotter,
            AnimationCurve curve,
            float period,
            float z_offset,
            Color line_color,
            Rect? area,
            float max_segment_length,
            int max_iterations,
            int max_number_of_points)
        {
            if (plotter == null)
                throw new ArgumentNullException("plotter", "Plotter must be set before plotting a curve.");

            //plotter.Break();
            float x = 0f;
            float y = curve.Evaluate(x);
            for (int i = 0; i <= max_number_of_points; i++)
            {
                Vector3 position;
                if (area.HasValue)
                {
                    position = new Vector3()
                    {
                        x = Mathf.Lerp(area.Value.xMin, area.Value.xMax, x),
                        y = Mathf.Lerp(area.Value.yMin, area.Value.yMax, y),
                        z = z_offset
                    };
                }
                else
                {
                    position = new Vector3(x, y, z_offset);
                }
                CanvasLineSegment segment;
                if (plotter.AddPoint(position, out segment))
                {
                    Points.Add(segment.PointA);
                    Points.Add(segment.PointB);

                    TrySetColor(line_color, segment.gameObject, segment.PointA.gameObject, segment.PointB.gameObject);

                    OnPointAdded(position, segment, area);

                    if (period > 0)
                        yield return new WaitForSeconds(period);
                }

                bool next_point = EquidistantStep(curve.Evaluate, x, y, max_segment_length, max_iterations, out x, out y);
                if (!next_point)
                    break;
            }
            //plotter.Break();
        }

        private static bool TrySetColor(Color color, params GameObject[] game_objects)
        {
            var images = game_objects
                .Select(g => g.GetComponent<Image>())
                .Where(i => i != null)
                .ToArray();
            foreach (var image in images)
                image.color = color;
            return game_objects.Length == images.Length;
        }

        private IEnumerable EraseCurve(LinePlotter plotter, float period)
        {
            if (plotter == null)
                throw new ArgumentNullException("plotter", "Plotter must be set before erasing a curve.");
            //plotter.Break();
            // TODO: RemoveLabel etc.
            foreach (var label in LabelObjects)
                LabelPool.Return(label);
            LabelObjects.Clear();

            foreach (var point in Points.ToArray())
            {
                plotter.RemoveVertex(point);
                Points.Remove(point);
                if (period > 0)
                    yield return new WaitForSeconds(period);
            }
        }

        public float EvaluateCurveBounded(float x)
        {
            float t = Mathf.InverseLerp(CurveBounds.xMin, CurveBounds.xMax, x);
            float ft = Curve.Evaluate(t);
            return Mathf.Lerp(CurveBounds.yMin, CurveBounds.yMax, ft);
        }

        private void Update()
        {
        }

        public void OnBeforeSerialize()
        {
            // Round-trip
            XAxisTextPool = XAxisTextPool;
            YAxisTextPool = YAxisTextPool;
        }

        public void OnAfterDeserialize()
        {
        }
    }
}