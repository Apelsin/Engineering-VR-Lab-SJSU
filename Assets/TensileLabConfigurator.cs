using UnityEngine;

public class TensileLabConfigurator : MonoBehaviour
{

    //private void HandleStartButtonTriggerEnter(Collider other)
    //{
        

    //        //Debug.Log("Button Pressed!");
    //        //buttonSound.Play();
    //        //buttonSound.Play(44100);
    //        //pressLeverAnimation.pressLever();

    //        //if (grabberInputReady)
    //        //{
    //        //    //Move Grabber Up
    //        //    if (grabberMovingUp)
    //        //    {
    //        //        grabberMovingUp = false;
    //        //    }
    //        //    else
    //        //    {
    //        //        grabberMovingUp = true;
    //        //    }

    //        //    Countdown((int)respawnTime);
    //        //}
    //    }
    //}
    private void Start()
    {
        var lever = FindObjectOfType<LeverPressed>();
        var lever_trigger_events = lever?.GetComponent<TriggerEvents>();
        if (lever_trigger_events)
            lever_trigger_events.AddTriggerEnterHandler((other) =>
            {
                if (other.gameObject.tag == "leftHand" || other.gameObject.tag == "rightHand")
                {
                    GameObject graph = Instantiate(Resources.Load("LineGraph", typeof(GameObject))) as GameObject;
                    Debug.Log("Instantiated LineGraph");
                    lever.OnPress();
                }
            });
        else
            Debug.LogError("Could not find lever trigger events component");
    }
}