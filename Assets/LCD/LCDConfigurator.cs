using UnityEngine;

public class LCDConfigurator : MonoBehaviour
{
    //public LCDRenderer LCDRendererPrefab;

    private void Start()
    {
        var graphers = FindObjectsOfType<CurveGrapher>();
        foreach (var grapher in graphers)
            grapher.Graph();

        //var lcd_images = FindObjectsOfType<LCDRendererImage>();

        //foreach (var lcd_image in lcd_images)
        //{
        //    var lcd_renderer = Instantiate(LCDRendererPrefab);
        //    var lcd_canvas = lcd_renderer?.GetComponentInChildren<Canvas>();
        //    // Create instance of camera target texture
        //    var render_texture = new RenderTexture(lcd_renderer.Camera.targetTexture);
        //    lcd_renderer.Camera.targetTexture = render_texture;
        //    var target = lcd_image.Target;
        //    target.transform.SetParent(lcd_canvas.transform, false);
        //    lcd_image.Image.texture = render_texture;
        //}
    }
}