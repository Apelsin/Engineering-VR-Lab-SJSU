using RoaringFangs.ASM;
using UnityEngine;

public class MainMenuConfigurator : MonoBehaviour
{
    private void Start()
    {
        // Assuming this is the main menu, can be changed later
        var game_controller_object = GameObject.FindGameObjectWithTag("GameController");
        var manager = game_controller_object?.GetComponent<SceneStateManager>();
        var main_manu = FindObjectOfType<PanelManagerUI>();
        main_manu.NavigateChapter.AddListener((chapter_name) =>
        {
            manager.SetAnimatorTrigger("To Limbo");
            manager.SetAnimatorTrigger("To " + chapter_name);
        });

        var panel_object = GameObject.FindGameObjectWithTag("Chapter Select Panel");
        var panel_animator = panel_object.GetComponent<Animator>();
        if (panel_animator)
        {
            panel_animator.SetBool("Presented", true);
        }
    }
}