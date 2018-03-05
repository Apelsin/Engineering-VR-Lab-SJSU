using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PanelManagerUI : MonoBehaviour {

    [Serializable]
    public class NavigateChapterEvent : UnityEvent<string> { }

    public NavigateChapterEvent NaviageChapter;

    public GameObject chaptersPanel;


	// Use this for initialization
	void Start () {
        chaptersPanel.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void ToggleSettingsPanel()
    {

        bool active;
        active = chaptersPanel.activeSelf == true ? false : true;
        chaptersPanel.SetActive(active);
    }

    public void HideSettingsPanel()
    {
        chaptersPanel.SetActive(false);
    }

    public void OnNavigateChapter(string chapter_name)
    {
        NaviageChapter.Invoke(chapter_name);
    }
}

