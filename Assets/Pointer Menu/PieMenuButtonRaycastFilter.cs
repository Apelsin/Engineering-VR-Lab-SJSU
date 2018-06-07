using UnityEngine;
using UnityEngine.UI;

namespace CVRLabSJSU
{
    public class PieMenuButtonRaycastFilter : MonoBehaviour, ICanvasRaycastFilter, ISerializationCallbackReceiver
    {
        public RectTransform RectTransform;
        public Image Image;
        public float AlphaCutoff;

        // Adapted from https://coeurdecode.com/game%20development/2015/10/18/non-rectangular-ui-buttons-in-unity/
        public bool IsRaycastLocationValid(Vector2 screen_point, Camera event_camera)
        {
            Vector2 rect_point;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                RectTransform, screen_point, event_camera, out rect_point);

            Vector2 norm_point = (rect_point - RectTransform.rect.min);
            norm_point.x /= RectTransform.rect.width;
            norm_point.y /= RectTransform.rect.height;

            // Read pixel color at normalized hit point
            var texture = Image.sprite.texture;
            var color = texture.GetPixel((int)(norm_point.x * texture.width), (int)(norm_point.y * texture.height));

            // Keep hits on pixels above minimum alpha
            return color.a > AlphaCutoff;
        }

        public void OnAfterDeserialize()
        {
        }

        public void OnBeforeSerialize()
        {
            if (RectTransform == null)
                RectTransform = GetComponent<RectTransform>();
            if (Image == null)
                Image = GetComponent<Image>();
        }
    }
}