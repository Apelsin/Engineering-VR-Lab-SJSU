using System.Collections.Generic;
using UnityEngine;

namespace CVRLabSJSU
{
    public class ComboBox : MonoBehaviour, ISerializationCallbackReceiver
    {
        public RectTransform RectTransform;
        public CanvasGroup CanvasGroup;

        [SerializeField]
        private float _ItemRowHeight = 48;

        private float _ItemRowHeight__cached;

        public float ItemRowHeight
        {
            get { return _ItemRowHeight; }
            set
            {
                if (_ItemRowHeight__cached != value)
                {
                    _ItemRowHeight = value;
                    _ItemRowHeight__cached = value;
                    OnUpdateBoundsAndViz();
                }
            }
        }

        [SerializeField]
        [Range(0f, 1f)]
        private float _Time;

        private float _Time__cached; // Double-backing field because Unity...

        public float Time
        {
            get { return _Time; }
            set
            {
                if (_Time__cached != value)
                {
                    _Time = value;
                    _Time__cached = value;
                    OnUpdateBoundsAndViz();
                }
            }
        }

        public AnimationCurve AlphaCurve = AnimationCurve.EaseInOut(0f, 0f, 0.2f, 1f);

        [SerializeField]
        private List<ComboItem> ItemsList;

        public IEnumerable<ComboItem> Items
        {
            get { return ItemsList; }
        }

        [SerializeField]
        private ComboItem[] AllItems;

        public void ClearItems()
        {
            ItemsList.Clear();
            //OnUpdateText();
        }

        public ComboItem AddItem(string text_str)
        {
            var idx = ItemsList.Count;
            var item = AllItems[idx];
            item.Text.text = text_str;
            ItemsList.Add(item);
            //OnUpdateText();
            return item;
        }

        private void OnUpdateAnimatedParameters()
        {
            // Round-trip animated parameters
            Time = Time;
            ItemRowHeight = ItemRowHeight;
        }

        private void Start()
        {
            // Initialize animated parameters
            _Time__cached = _Time;
            _ItemRowHeight__cached = _ItemRowHeight;
        }

        private void Update()
        {
            OnUpdateAnimatedParameters();
        }

        private void OnUpdateBoundsAndViz()
        {
            var size = RectTransform.sizeDelta;
            size.y = ItemRowHeight * ItemsList.Count * Time;
            RectTransform.sizeDelta = size;
            CanvasGroup.alpha = AlphaCurve.Evaluate(Time);
            CanvasGroup.blocksRaycasts = Time > 0f;
        }

        private void OnUpdateText()
        {
            //int i = 0;
            //foreach(var item in Items)
            //{
            //    ItemTextComponents[i].text = item.Text;
            //    i++;
            //}
        }

        public void OnBeforeSerialize()
        {
            if (RectTransform == null)
                RectTransform = GetComponent<RectTransform>();

            if (CanvasGroup == null)
                CanvasGroup = GetComponent<CanvasGroup>();

            // Round trip
            OnUpdateAnimatedParameters();
            OnUpdateBoundsAndViz();
            OnUpdateText();
        }

        public void OnAfterDeserialize()
        {
        }
    }
}