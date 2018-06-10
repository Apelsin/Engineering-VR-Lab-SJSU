using UnityEngine;
using UnityEngine.UI;

namespace CVRLabSJSU
{
    public class ComboItem : MonoBehaviour, ISerializationCallbackReceiver
    {
        [SerializeField]
        private Text _Text;

        public Text Text
        {
            get { return _Text; }
            private set { _Text = value; }
        }

        [SerializeField]
        private Button _Button;

        public Button Button
        {
            get { return _Button; }
            private set { _Button = value; }
        }

        [SerializeField]
        private bool _IsSelected;

        public bool IsSelected
        {
            get { return _IsSelected; }
            set
            {
                _IsSelected = value;
                // BUG: When IsSelected is called from OnBeforeSerialize, Button is null...for some reason...
                if (Button)
                {
                    var colors = Button.colors;
                    colors.normalColor = IsSelected ? SelectedColor : DefaultColor;
                    Button.colors = colors;
                }
            }
        }

        public Color DefaultColor = new Color(0.2f, 0.2f, 0.2f);
        public Color SelectedColor = new Color(0.1f, 0.3f, 1.0f);

        public void OnBeforeSerialize()
        {
            Button = GetComponent<Button>();
            Text = GetComponent<Text>();

            // Round-trip
            IsSelected = IsSelected;
        }

        public void OnAfterDeserialize()
        {
        }
    }
}