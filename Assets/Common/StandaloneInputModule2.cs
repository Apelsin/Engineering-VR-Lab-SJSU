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

            if (AllowProcessMouseEvent)
                ProcessMouseEvent();
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