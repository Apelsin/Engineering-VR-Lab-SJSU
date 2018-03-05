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
        main_manu.NaviageChapter.AddListener((chapter_name) =>
        {
            manager.SetAnimatorTrigger("Exit Main Menu");
            manager.SetAnimatorTrigger("To " + chapter_name);
        });
    }
}