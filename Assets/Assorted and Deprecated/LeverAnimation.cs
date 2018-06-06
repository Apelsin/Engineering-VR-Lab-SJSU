using UnityEngine;

namespace CVRLabSJSU
{
    public class LeverAnimation : MonoBehaviour
    {
        public void pressLever()
        {
            GetComponent<Animation>().Play();
            Debug.Log("PRESS BUTTON");
        }
    }
}