using UnityEngine;

namespace CVRLabSJSU
{
    public class DebugLogHelper : MonoBehaviour
    {
        public void DebugLog(string message)
        {
            Debug.Log(message);
        }

        public void DebugLog(float value)
        {
            Debug.Log(value);
        }
    }
}