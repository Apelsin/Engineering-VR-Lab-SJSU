using RoaringFangs.Utility;
using System.Linq;
using UnityEngine;

namespace CVRLabSJSU
{
    public class LinePlotter : MonoBehaviour, ISerializationCallbackReceiver
    {
        [SerializeField]
        private MonoBehaviour VertexPoolBehavior;

        public IGameObjectPool VertexPool
        {
            get { return (IGameObjectPool)VertexPoolBehavior; }
            set { VertexPoolBehavior = (MonoBehaviour)value; }
        }

        [SerializeField]
        private MonoBehaviour LineSegmentPoolBehavior;

        public IGameObjectPool LineSegmentPool
        {
            get { return (IGameObjectPool)LineSegmentPoolBehavior; }
            set { LineSegmentPoolBehavior = (MonoBehaviour)value; }
        }

        private Transform PointA;
        private Transform PointB;

        public void Break()
        {
            PointB = null;
        }

        public bool AddPoint(Vector3 local_position, out CanvasLineSegment segment)
        {
            PointA = PointB;
            PointB = VertexPool.Cycle().transform;
            PointB.localPosition = local_position;
            if (PointA && PointB)
            {
                segment = LineSegmentPool.Cycle().GetComponent<CanvasLineSegment>();
                segment.PointA = PointA;
                segment.PointB = PointB;
                return true;
            }
            else
            {
                segment = null;
                return false;
            }
        }

        public bool RemoveVertex(Transform point)
        {
            // TODO: optimize this
            var segments = LineSegmentPoolBehavior
                .GetComponentsInChildren<CanvasLineSegment>()
                .Where(s => s.PointA == point || s.PointB == point);
            VertexPool.Return(point.gameObject);
            if (segments.Any())
            {
                foreach (var segment in segments)
                    LineSegmentPool.Return(segment.gameObject);
                return true;
            }
            return false;
        }

        public virtual void OnBeforeSerialize()
        {
            VertexPool = VertexPool;
            LineSegmentPool = LineSegmentPool;
        }

        public virtual void OnAfterDeserialize()
        {
        }
    }
}