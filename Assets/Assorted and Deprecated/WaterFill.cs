using System.Collections;
using UnityEngine;

namespace CVRLabSJSU
{
    public class WaterFill : MonoBehaviour
    {
        public Object waterSource;
        public ParticleSystem waterSystem;

        // Use this for initialization
        private void Start()
        {
            StartCoroutine(fill());
        }

        // Update is called once per frame
        private void Update()
        {
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<FluidHolderScript>())
            {
                if (waterSystem.emissionRate >= 5)
                    StartCoroutine(fill());
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.GetComponent<FluidHolderScript>())
            {
                StopAllCoroutines();
            }
        }

        private IEnumerator fill()
        {
            while (true)
            {
                //Debug.Log("COROUTINE");
                GameObject.Instantiate(waterSource, this.transform.position, Quaternion.identity);

                yield return new WaitForSeconds(0.5f / waterSystem.emissionRate);
            }
        }
    }
}