using CVRLabSJSU;
using RoaringFangs.ASM;
using System.Collections;
using UnityEngine;
using VRTK;

public class MainMenuConfigurator : MonoBehaviour
{
    [Range(0f, 1f)]
    public float FadeTime = 0.5f;
    [Range(0f, 1f)]
    public float FadeWaitTime = 0.5f;
    public Color FadeColor = new Color(0.2f, 0.6f, 1.0f, 1.0f);
    private static readonly Color Transparent = new Color(1.0f, 1.0f, 1.0f, 0.0f);
    private IEnumerator NavigateChapter(SceneStateManager manager, string chapter_name)
    {
        var sdk = VRTK_SDKManager.instance.GetHeadsetSDK();
        sdk.HeadsetFade(FadeColor, FadeTime);
        yield return new WaitForSeconds(FadeTime);
        manager.SetAnimatorTrigger("To Limbo");
        manager.SetAnimatorTrigger("To " + chapter_name);
        yield return new WaitForSeconds(FadeWaitTime);
        sdk.HeadsetFade(Transparent, FadeTime);
        yield return new WaitForSeconds(FadeTime);
    }

    private void Start()
    {
        // Assuming this is the main menu, can be changed later
        var game_controller_object = GameObject.FindGameObjectWithTag("GameController");
        var manager = game_controller_object?.GetComponent<SceneStateManager>();
        var main_manu = FindObjectOfType<PanelManagerUI>();
        main_manu.NavigateChapter.AddListener((chapter_name) =>
        {
            manager.StartCoroutine(NavigateChapter(manager, chapter_name));
        });

        var panel_object = GameObject.FindGameObjectWithTag("Chapter Select Panel");
        var panel_animator = panel_object.GetComponent<Animator>();
        if (panel_animator)
        {
            panel_animator.SetBool("Presented", true);
        }
    }
}