using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PointerContextMenu : MonoBehaviour
{
    public Animator Animator;
    public Transform MainCameraTransform;
    public Vector3 TargetPosition;
    public Button[] Buttons;
    private void OnUpdate(bool apply_scale)
    {
        var offset = (TargetPosition - MainCameraTransform.position);
        transform.position = TargetPosition;
        transform.rotation = Quaternion.LookRotation(offset.normalized, Vector3.up);
        if (apply_scale)
        {
            var scale = offset.magnitude;
            transform.localScale = new Vector3(scale, scale, scale);
        }
    }
    private void Start()
    {
        if (MainCameraTransform == null)
            return;
        OnUpdate(true);
    }
    private void Update()
    {
        if (MainCameraTransform == null)
            return;
        OnUpdate(false);
    }

    private void Pulse()
    {
        Animator.SetTrigger("Pulse");
    }

    public void SetMenuButtons(
        ButtonInfo[] button_infos,
        IDictionary<string, ManagedButtonBehavior> behaviors)
    {
        // Set menu buttons and behaviors
        foreach (var button in Buttons)
            button.onClick.RemoveAllListeners();
        var number_of_buttons = Mathf.Min(button_infos.Length, Buttons.Length);
        int i;
        for (i = 0; i < number_of_buttons; i++)
        {
            var info = button_infos[i];
            var button_component = Buttons[i];
            button_component.GetComponentInChildren<Text>().text = info.Text;
            bool interactable = !String.IsNullOrEmpty(info.Id);
            if (info.IsTerminal)
            {
                if (behaviors != null)
                {
                    ManagedButtonBehavior behavior;
                    if (behaviors.TryGetValue(info.Id, out behavior))
                    {
                        button_component.onClick.AddListener(behavior.Clicked.Invoke);
                        interactable &= behavior.IsEnabled;
                    }
                }
                else
                    Debug.LogWarning("Behaviors mapping is null.");
            }
            else
            {
                button_component.onClick.AddListener(() =>
                {
                    SetMenuButtons(info.Children, behaviors);
                    Pulse();
                });
            }
            button_component.interactable = interactable;
        }
        for (; i < Buttons.Length; i++)
        {
            var button_component = Buttons[i];
            button_component.GetComponentInChildren<Text>().text = String.Empty;
            button_component.interactable = false;
        }
    }

    public void RequestDestroy()
    {
        Animator.SetTrigger("RequestDestroy");
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}