using RoaringFangs.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CurveGrapher : MonoBehaviour, ISerializationCallbackReceiver
{
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

    private HashSet<Transform> Points = new HashSet<Transform>();

    private IEnumerator CurrentOperation;

    private IEnumerator Start()
    {
        for (; ; )
        {
            if(CurrentOperation != null)
                while (CurrentOperation.MoveNext())
                    yield return CurrentOperation.Current;
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerable Test()
    {
        for (; ; )
        {
            var plot = PlotCurve(
                Plotter,
                Curve,
                Period,
                ZOffset,
                LineColor,
                GraphRectTransform.rect,
                MaxSegmentLength,
                MaxIterations,
                MaxNumberOfPoints);
            foreach (var _ in plot)
                yield return _;
            foreach (var _ in EraseCurve(Plotter, Period))
                yield return _;
        }
    }

    static IEnumerator Concat(params IEnumerator[] enumerators)
    {
        for(int i = 0; i < enumerators.Length; i++)
        {
            var enumerator = enumerators[i];
            while (enumerator.MoveNext())
                yield return enumerator.Current;
        }
    }

    private HashSet<GameObject> XAxisTexts = new HashSet<GameObject>();
    private HashSet<GameObject> YAxisTexts = new HashSet<GameObject>();

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
            text.GetComponent<Text>().text = $"{x:0.00}";
        }
        for (int i = 0; i < y_count; i++)
        {
            var t = (float)i / (float)(y_count - 1);
            var y = Mathf.Lerp(y_min, y_max, t);
            var text = YAxisTextPool.Cycle();
            YAxisTexts.Add(text);
            text.transform.localPosition = new Vector2(0f, t * y_size);
            text.GetComponent<Text>().text = $"{y:0.00}";
        }
    }

    public void Graph()
    {
        //var clear = EraseCurve(Plotter, 0f).GetEnumerator();
        Plotter.Break();
        var plot = PlotCurve(
            Plotter,
            Curve,
            Period,
            ZOffset,
            LineColor,
            GraphRectTransform.rect,
            MaxSegmentLength,
            MaxIterations,
            MaxNumberOfPoints)
            .GetEnumerator();
        //StartCoroutine(plot.GetEnumerator());
        CurrentOperation = plot;// Concat(clear, plot);
        SetMajorTickMarkNumbers(CurveBounds, NumberOfMajorTickMarks);
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
    private static void EquidistantStep(
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

                if (period > 0)
                    yield return new WaitForSeconds(period);
            }

            EquidistantStep(curve.Evaluate, x, y, max_segment_length, max_iterations, out x, out y);
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