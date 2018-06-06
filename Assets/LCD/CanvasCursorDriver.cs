using UnityEngine;
using UnityEngine.EventSystems;

namespace CVRLabSJSU
{
    public class CanvasCursorDriver : MonoBehaviour, ISerializationCallbackReceiver
    {
        public Camera Camera;
        public RectTransform CursorArea;
        public CanvasCursor Cursor;

        public void OnAfterDeserialize()
        {
        }

        public void OnBeforeSerialize()
        {
        }

        // TODO: abstract this into an input class
        // (not the UnityEngine.Input static class!!!)
        private Vector3 GetMousePosition()
        {
            return Input.mousePosition;
        }

        private void Start()
        {
        }

        private void Update()
        {
            var mouse_position = GetMousePosition();
            OnSetCursorPosition(mouse_position);
        }

        public void HandlePointerEnter(BaseEventData base_data)
        {
            var e = (PointerEventData)base_data;
            OnSetCursorPosition(e.position);
        }

        public void OnSetCursorPosition(Vector3 cursor_position)
        {
            Vector3 world_position;
            bool cursor_in_plane = RectTransformUtility.ScreenPointToWorldPointInRectangle(
                CursorArea,
                cursor_position,
                Camera,
                out world_position);

            if (cursor_in_plane)
            {
                var cursor_in_rect = RectTransformUtility.RectangleContainsScreenPoint(
                    CursorArea,
                    cursor_position,
                    Camera);
                if (cursor_in_rect)
                    Cursor.RectTransform.position = world_position;
            }
        }
    }
}