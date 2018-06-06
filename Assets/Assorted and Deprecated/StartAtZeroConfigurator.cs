using UnityEngine;

namespace CVRLabSJSU
{
    public class StartAtZeroConfigurator : MonoBehaviour
    {
        public Vector3 StartOffset;

        private void Start()
        {
            var rig = GameObject.FindGameObjectWithTag("Camera Rig");
            var eyes = GameObject.FindGameObjectWithTag("MainCamera");
            var rig_xf = rig.transform;
            var eyes_xf = eyes.transform;
            var rig_to_eyes = eyes_xf.position - rig_xf.position;
            rig_to_eyes.y = 0f; // TODO
            rig.transform.position = StartOffset - rig_to_eyes;
        }

        private void Update()
        {
        }
    }
}