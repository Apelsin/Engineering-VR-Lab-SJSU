using UnityEngine;
using UnityEngine.EventSystems;

namespace CVRLabSJSU
{
    [RequireComponent(typeof(RectTransform))]
    public class CanvasCursorDriver :
        MonoBehaviour,
        ISerializationCallbackReceiver,
        IPointerHoverHandler
    {
        [SerializeField]
        private Camera _Camera;

        private Camera _CachedCamera;

        public Camera Camera
        {
            get
            {
                if (_Camera != null)
                    return _Camera;
                if (_CachedCamera != null)
                    return _CachedCamera;
                return _CachedCamera = GameObject.FindGameObjectWithTag("MainCamera")?.GetComponent<Camera>();
            }
            set
            {
                _Camera = value;
            }
        }

        public RectTransform RectTransform;
        public CanvasCursor Cursor;

        private void Update()
        {
        }

        protected void OnSetCursorScreenPosition(Vector2 screen_position)
        {
            Vector3 world_position;
            bool cursor_in_plane = RectTransformUtility.ScreenPointToWorldPointInRectangle(
                RectTransform,
                screen_position,
                Camera,
                out world_position);

            if (cursor_in_plane)
            {
                var cursor_in_rect = RectTransformUtility.RectangleContainsScreenPoint(
                    RectTransform,
                    screen_position,
                    Camera);
                if (cursor_in_rect)
                    OnSetCursorPosition(world_position);
            }
        }

        protected void OnSetCursorPosition(Vector3 world_position)
        {
            Cursor.RectTransform.position = world_position;
        }

        public void OnPointerHover(PointerEventData eventData)
        {
            // Optimization: only calculate the world position from screen position
            // if the world position is not already defined.
            // Ideally, the result should be the same.
            var world_position = eventData.pointerCurrentRaycast.worldPosition;
            if (world_position == Vector3.zero)
                OnSetCursorScreenPosition(eventData.position);
            else
                OnSetCursorPosition(world_position);
        }

        public void OnBeforeSerialize()
        {
            if (RectTransform == null)
                RectTransform = GetComponent<RectTransform>();
        }

        public void OnAfterDeserialize()
        {
        }
    }
}