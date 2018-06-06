using UnityEngine;
using UnityEngine.UI;

namespace CVRLabSJSU
{
    public class PlotTraceReadout : MonoBehaviour, ISerializationCallbackReceiver
    {
        public RectTransform RectTransform;
        public CanvasGroup CanvasGroup;
        public Animator Animator;
        public Text ReadoutText;

        public void OnAfterDeserialize()
        {
        }

        public void OnBeforeSerialize()
        {
            if (Animator == null)
                Animator = GetComponent<Animator>();
            if (RectTransform == null)
                RectTransform = GetComponent<RectTransform>();
            if (CanvasGroup == null)
                CanvasGroup = GetComponent<CanvasGroup>();
        }

        public void SetVisible(bool visible)
        {
            Animator.SetBool("Visible", visible);
        }

        public void SetTraceText(float x, float y, float fx, float slope)
        {
            string str = $"x: {x:0.00}\ny: {y:0.00}\nf(x): {fx:0.00}\nslope: {slope:0.00}";
            ReadoutText.text = str;
        }
    }
}