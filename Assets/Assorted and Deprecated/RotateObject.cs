using UnityEngine;

namespace CVRLabSJSU
{
    public class RotateObject : MonoBehaviour
    {
        // Use this for initialization
        private void Start()
        {
        }

        // Update is called once per frame
        private void Update()
        {
            // ...also rotate around the World's Y axis
            transform.Rotate(new Vector3(0, 16, 0) * Time.deltaTime, Space.World);
        }
    }
}