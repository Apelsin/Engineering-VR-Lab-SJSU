using UnityEngine;

namespace CVRLabSJSU
{
    public class ButtonAnimation : MonoBehaviour
    {
        public void pressButton()
        {
            GetComponent<Animation>().Play();
            Debug.Log("PRESS BUTTON");
        }
    }
}