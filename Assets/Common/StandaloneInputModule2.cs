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
    }
}