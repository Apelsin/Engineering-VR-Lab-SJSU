using UnityEngine;
using UnityEngine.UI;

namespace CVRLabSJSU
{
    public class LCDRendererImage : MonoBehaviour
    {
        public GameObject LCDRendererPrefab;

        [SerializeField]
        private RawImage _Image;

        public RawImage Image
        {
            get { return _Image; }
            private set { _Image = value; }
        }

        public GameObject Target;

        private void Start()
        {
            var lcd_renderer_game_object = Instantiate(LCDRendererPrefab);
            lcd_renderer_game_object.transform.SetParent(transform);
            var lcd_renderer = lcd_renderer_game_object?.GetComponent<LCDRenderer>();

            // TODO: Don't hardcode this
            var lcd_canvas = lcd_renderer?.GetComponentInChildren<Canvas>();
            Target.transform.SetParent(lcd_canvas.transform, false);

            // Create instance of camera target texture
            var render_texture = new RenderTexture(lcd_renderer.Camera.targetTexture);
            lcd_renderer.Camera.targetTexture = render_texture;

            Image.texture = render_texture;
        }
    }
}