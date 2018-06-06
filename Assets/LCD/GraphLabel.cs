using UnityEngine;
using UnityEngine.UI;

namespace CVRLabSJSU
{
    public class GraphLabel : MonoBehaviour
    {
        [SerializeField]
        private Text _Text;

        public Text Text => _Text;

        [SerializeField]
        private Text _SecondaryText;

        public Text SecondaryText => _SecondaryText;
    }
}