using UnityEngine;

namespace CVRLabSJSU
{
    [RequireComponent(typeof(Animator))]
    public class AnimatorHelper : MonoBehaviour
    {
        public void SetBoolTrue(string name)
        {
            GetComponent<Animator>().SetBool(name, true);
        }

        public void SetBoolFalse(string name)
        {
            GetComponent<Animator>().SetBool(name, false);
        }
    }
}