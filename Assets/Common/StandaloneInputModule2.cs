using UnityEngine;
using UnityEngine.EventSystems;

namespace CVRLabSJSU
{
    public class StandaloneInputModule2 : StandaloneInputModule
    {
        public bool AllowProcessMouseEvent = true;

        public override void Process()
        {
            bool usedEvent = SendUpdateEventToSelectedObject();

            if (eventSystem.sendNavigationEvents)
            {
                if (!usedEvent)
                    usedEvent |= SendMoveEventToSelectedObject();

                if (!usedEvent)
                    SendSubmitEventToSelectedObject();
            }

            // touch needs to take precedence because of the mouse emulation layer
            if (!ProcessTouchEvents() && input.mousePresent && AllowProcessMouseEvent)
                ProcessMouseEvent();
        }

        private bool ProcessTouchEvents()
        {
            for (int i = 0; i < input.touchCount; ++i)
            {
                Touch touch = input.GetTouch(i);

                if (touch.type == TouchType.Indirect)
                    continue;

                bool released;
                bool pressed;
                var pointer = GetTouchPointerEventData(touch, out pressed, out released);

                ProcessTouchPress(pointer, pressed, released);

                if (!released)
                {
                    ProcessMove(pointer);
                    ProcessDrag(pointer);
                }
                else
                    RemovePointerData(pointer);
            }
            return input.touchCount > 0;
        }

        private static void Execute(IPointerHoverHandler handler, BaseEventData eventData)
        {
            var pointer_event_data = ExecuteEvents.ValidateEventData<PointerEventData>(eventData);
            handler.OnPointerHover(pointer_event_data);
        }

        public static ExecuteEvents.EventFunction<IPointerHoverHandler> PointerHoverHandler => Execute;

        protected override void ProcessMove(PointerEventData pointerEvent)
        {
            base.ProcessMove(pointerEvent);
            var targetGO = (Cursor.lockState == CursorLockMode.Locked ? null : pointerEvent.pointerCurrentRaycast.gameObject);
            HandlePointerHover(pointerEvent, targetGO);
        }

        protected void HandlePointerHover(PointerEventData currentPointerData, GameObject newEnterTarget)
        {
            currentPointerData.pointerEnter = newEnterTarget;
            var hovered = currentPointerData.hovered;
            for (int i = 0; i < hovered.Count; i++)
                ExecuteEvents.Execute(hovered[i], currentPointerData, PointerHoverHandler);
        }
    }
}