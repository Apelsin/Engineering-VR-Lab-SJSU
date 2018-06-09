using UnityEngine;
using UnityEngine.EventSystems;

namespace CVRLabSJSU
{
    [RequireComponent(typeof(RectTransform))]
    public class CanvasCursorDriver :
        MonoBehaviour,
        ISerializationCallbackReceiver,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        public Camera Camera;
        public RectTransform RectTransform;
        public CanvasCursor Cursor;

        private bool CursorInArea;

        private Vector3 GetMousePosition()
        {
            // TODO: abstract this into an input class
            // (i.e. not the UnityEngine.Input static class)
            return Input.mousePosition;
        }

        private void Update()
        {
            if (CursorInArea)
            {
                var mouse_position = GetMousePosition();
                OnSetCursorPosition(mouse_position);
            }
        }

        public void OnSetCursorPosition(Vector3 cursor_position)
        {
            Vector3 world_position;
            bool cursor_in_plane = RectTransformUtility.ScreenPointToWorldPointInRectangle(
                RectTransform,
                cursor_position,
                Camera,
                out world_position);

            if (cursor_in_plane)
            {
                var cursor_in_rect = RectTransformUtility.RectangleContainsScreenPoint(
                    RectTransform,
                    cursor_position,
                    Camera);
                if (cursor_in_rect)
                    Cursor.RectTransform.position = world_position;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            CursorInArea = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            CursorInArea = false;
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