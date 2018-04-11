using UnityEngine;

public class ChapterConfigurator : MonoBehaviour
{
    private void Start()
    {
        var panel_object = GameObject.FindGameObjectWithTag("Chapter Select Panel");
        var panel_animator = panel_object.GetComponent<Animator>();
        if(panel_animator)
        {
            panel_animator.SetBool("Presented", false);
        }
    }
}