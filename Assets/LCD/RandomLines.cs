using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CVRLabSJSU
{
    [RequireComponent(typeof(LinePlotter))]
    public class RandomLines : MonoBehaviour, ISerializationCallbackReceiver
    {
        [SerializeField]
        private LinePlotter _LinePlotter;

        public LinePlotter LinePlotter
        {
            get { return _LinePlotter; }
            private set { _LinePlotter = value; }
        }

        private Transform PointA;
        private Transform PointB;

        private IEnumerator Start()
        {
            for (; ; )
            {
                yield return new WaitUntil(() => enabled);
                var rect_transform = transform.GetComponent<RectTransform>();
                var size = rect_transform.rect;
                var point_position = new Vector3(Random.Range(size.xMin, size.xMax), Random.Range(size.yMin, size.yMax), 0f);
                CanvasLineSegment segment;
                if (LinePlotter.AddPoint(point_position, out segment))
                {
                    var color = Random.ColorHSV();
                    segment.GetComponent<Image>().color = color;
                    segment.PointA.GetComponent<Image>().color = color;
                    segment.PointB.GetComponent<Image>().color = color;
                    yield return new WaitForSeconds(0.05f);
                }
            }
        }

        public void OnBeforeSerialize()
        {
            LinePlotter = LinePlotter ?? GetComponent<LinePlotter>();
        }

        public void OnAfterDeserialize()
        {
        }
    }
}