using UnityEngine.EventSystems;

namespace CVRLabSJSU
{
    public interface IPointerHoverHandler : IEventSystemHandler
    {
        //
        // Parameters:
        //   eventData:
        //     Current event data.
        void OnPointerHover(PointerEventData eventData);
    }
}