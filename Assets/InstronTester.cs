using System.Collections.Generic;
using UnityEngine;

public class InstronTester : MonoBehaviour
{
    public string CurrentTestMaterialType;
    public List<string> TestMaterialTypes;
    public Animator GrabberAnimator;

    public void OnBeginTensileTest()
    {
        GrabberAnimator.SetTrigger("Start");
    }

    public void OnResetTensileTest()
    {
        GrabberAnimator.SetTrigger("Reset");
    }

    private void Start()
    {
        var type_id = TestMaterialTypes.IndexOf(CurrentTestMaterialType);
        GrabberAnimator.SetInteger("Test Material Type", type_id);
    }

    private void Update()
    {
    }
}