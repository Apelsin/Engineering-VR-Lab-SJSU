using System;
using UnityEngine;
using UnityEngine.Events;

namespace CVRLabSJSU
{
    public class PanelManagerUI : MonoBehaviour
    {
        [Serializable]
        public class NavigateChapterEvent : UnityEvent<string> { }

        public NavigateChapterEvent NavigateChapter;

        public GameObject chaptersPanel;

        // Use this for initialization
        private void Start()
        {
            chaptersPanel.SetActive(false);
        }

        // Update is called once per frame
        private void Update()
        {
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
            NavigateChapter.Invoke(chapter_name);
        }

        public void OnQuit()
        {
            Application.Quit();
        }
    }
}