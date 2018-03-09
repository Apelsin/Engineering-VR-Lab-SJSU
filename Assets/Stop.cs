using UnityEngine;

public class Stop : MonoBehaviour
{
    // Use this for initialization
    private void Start()
    {
        this.GetComponent<ParticleSystem>().Stop();
    }

    // Update is called once per frame
    private void Update()
    {
    }
}