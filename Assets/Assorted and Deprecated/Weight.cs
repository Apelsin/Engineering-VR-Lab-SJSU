using UnityEngine;

namespace CVRLabSJSU
{
    public class Weight : MonoBehaviour
    {
        public TextMesh weightText;
        private string displayString;
        private float weight;

        // Use this for initialization
        private void Start()
        {
        }

        // Update is called once per frame
        private void Update()
        {
        }

        private void OnTriggerEnter(Collider other)
        {
            float objWeight = other.GetComponent<ObjWeight>().weight;
            weight += objWeight;
            UpdateString();
        }

        private void OnTriggerExit(Collider other)
        {
            float objWeight = other.GetComponent<ObjWeight>().weight;
            weight -= objWeight;
            UpdateString();
        }

        private void UpdateString()
        {
            displayString = "" + weight;
            weightText.text = displayString;
        }
    }
}