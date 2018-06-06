using System.Collections;
using UnityEngine;

namespace CVRLabSJSU
{
    public class BunsenHeat : MonoBehaviour
    {
        public ParticleSystem bunsen;
        public Object fireBurn;
        public Object smoke;
        public float heatRate = 0.1f;

        private AudioSource audio;
        public AudioClip steelAudio;
        public AudioClip paperAudio;

        private bool audioPlayed = false;
        private bool paperCoroutine = false;
        private bool boilCoroutine = false;

        // Use this for initialization
        private void Start()
        {
            audio = GetComponent<AudioSource>();
        }

        // Update is called once per frame
        private void Update()
        {
        }

        private void OnTriggerStay(Collider other)
        {
            if (other.gameObject.GetComponent<FluidHolderScript>())
            {
                FluidHolderScript fluid = other.gameObject.GetComponent<FluidHolderScript>();
                fluid.solution.temperature += (bunsen.startSpeed * heatRate);

                if (fluid.solution.temperature > 200)
                {
                    fluid.solution.temperature = 200;
                }

                if (fluid.solution.getAmount() >= fluid.maxAmount / 4 && fluid.solution.temperature > 100)
                {
                    Debug.Log("BOILING");
                    if (!boilCoroutine)
                        StartCoroutine(BoilWater(other));
                }
            }
            else if (other.gameObject.GetComponent<SteelFire>())
            {
                if (bunsen.startSpeed >= 5)
                {
                    if (!audioPlayed)
                    {
                        audio.clip = steelAudio;
                        audio.Play();
                        audioPlayed = true;
                    }
                    other.gameObject.GetComponent<SteelFire>().Explode();
                }
            }
            else if (other.tag == "Paper")
            {
                if (!paperCoroutine)
                    StartCoroutine(BurnPaper(other));
            }
        }

        private IEnumerator BurnPaper(Collider other)
        {
            paperCoroutine = true;

            audio.clip = paperAudio;
            audio.Play();

            ((GameObject)(GameObject.Instantiate(fireBurn, other.transform.position, Quaternion.identity))).transform.parent = other.transform;

            yield return new WaitForSeconds(3.0f);

            if (other.GetComponent<Joint>())
            {
                other.GetComponent<Joint>().connectedBody.GetComponent<GrabScriptVive>().Disconnect();
            }

            GameObject.Destroy(other.gameObject);

            paperCoroutine = false;
        }

        private IEnumerator BoilWater(Collider other)
        {
            boilCoroutine = true;

            Vector3 spawnPos = other.transform.position + new Vector3(0, 2, 0);

            GameObject smokeObj = (GameObject)(GameObject.Instantiate(smoke, spawnPos, Quaternion.identity));

            smokeObj.transform.parent = other.transform;

            while (other.GetComponent<FluidHolderScript>().solution.getAmount() > 0)
            {
                other.GetComponent<FluidHolderScript>().changeLevel(-1f);
                yield return new WaitForSeconds(0.1f);
            }

            GameObject.Destroy(smokeObj);

            boilCoroutine = false;
        }
    }
}