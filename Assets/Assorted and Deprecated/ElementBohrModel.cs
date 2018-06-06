using System.Collections;
using UnityEngine;

namespace CVRLabSJSU
{
    public class ElementBohrModel : MonoBehaviour
    {
        public GameObject element;
        private bool elementSpawned = false;
        private float respawnTime = 30.0f;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "leftHand" || other.gameObject.tag == "rightHand")
                // 2
                if (!elementSpawned)
                {
                    Instantiate(element, transform.position + new Vector3(6, 0, -6), transform.rotation);

                    elementSpawned = true;
                    StartCoroutine("Countdown", 10);
                }
        }

        //Allow Element to spawn after countdown
        private IEnumerator Countdown(int time)
        {
            while (time >= 0)
            {
                Debug.Log(time--);
                yield return new WaitForSeconds(1);
            }
            elementSpawned = false;
            Debug.Log("CountDown Complete: Can Spawn Element again");
        }
    }
}