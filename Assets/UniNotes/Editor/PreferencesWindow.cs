using UnityEngine;
using RotaryHeart.Lib.ProjectPreferences;
using UnityEditor;

namespace RotaryHeart.Lib.UniNotes
{
    public class PreferencesWindow
    {
        #region GUIContent
        static GUIContent previewContent = new GUIContent("View Notes on Selection", "Should the Notes be visible on the scene view when the object is highlighted");

        static GUIContent anchorContent = new GUIContent("Position", "Scene view Notes position");
        static GUIContent childContent = new GUIContent("Show Children Notes", "Should the children Notes be visible on the scene too");
        static GUIContent sizeContent = new GUIContent("Scene Preview Size", "How big the scene Notes preview should be");

        static GUIContent btnVisibleContent = new GUIContent("Enabled", "Enable or disable the button on the scene view");
        static GUIContent btnAnchorContent = new GUIContent("Position", "Scene view button position");
        static GUIContent btnSizeContent = new GUIContent("Size", "How big the button should be");

        static GUIContent anEnabledContent = new GUIContent("Enabled", "Enable or disable the advanced notes");
        static GUIContent anWidthContent = new GUIContent("Width", "How big the notes will be drawn");
        #endregion

        // Have we loaded the prefs yet
        private static bool prefsLoaded = false;

        private static bool viewOnSelection = false;
        private static EditorExtensions.Anchor anchor = EditorExtensions.Anchor.Bottom;
        private static bool showChildren = false;
        private static Vector2 size = new Vector2(400, 100);

        private static bool btnActive = true;
        private static EditorExtensions.Anchor btnAnchor = EditorExtensions.Anchor.Bottom;
        private static Vector2 btnSize = new Vector2(400, 100);

        private static bool anHierarchyEnabled = true;
        private static float anHierarchyWidth = 90;

        private static bool anProjectEnabled = true;
        private static float anProjectWidth = 90;

        // Add preferences section named "My Preferences" to the Preferences Window
        [PreferenceItem("UniNotes")]
        public static void PreferencesGUI()
        {
            //Change this to save the data on the ProjectSettings, there all the UniNotes settings(including the heirarchy and project noes) will be saved
            // Load the preferences
            if (!prefsLoaded)
            {
                viewOnSelection = ProjectPrefs.GetBool(Constants.SECTION, Constants.SCENE_NOTES_ENABLED, false);
                anchor = (EditorExtensions.Anchor)ProjectPrefs.GetInt(Constants.SECTION, Constants.SCENE_NOTES_ANCHOR, 4);
                showChildren = ProjectPrefs.GetBool(Constants.SECTION, Constants.SCENE_NOTES_VIEW_CHILDREN, false);
                size.x = ProjectPrefs.GetFloat(Constants.SECTION, Constants.SCENE_NOTES_WIDTH, 400);
                size.y = ProjectPrefs.GetFloat(Constants.SECTION, Constants.SCENE_NOTES_HEIGHT, 100);

                btnActive = ProjectPrefs.GetBool(Constants.SECTION, Constants.SCENE_NOTES_BTN_ENABLED, false);
                btnAnchor = (EditorExtensions.Anchor)ProjectPrefs.GetInt(Constants.SECTION, Constants.SCENE_NOTES_BTN_ANCHOR, 5);
                btnSize.x = ProjectPrefs.GetFloat(Constants.SECTION, Constants.SCENE_NOTES_BTN_WIDTH, 30);
                btnSize.y = ProjectPrefs.GetFloat(Constants.SECTION, Constants.SCENE_NOTES_BTN_HEIGHT, 30);

                anHierarchyEnabled = ProjectPrefs.GetBool(Constants.SECTION, Constants.ADVANCED_NOTES_HIERARCHY_ENABLED, true);
                anHierarchyWidth = ProjectPrefs.GetFloat(Constants.SECTION, Constants.ADVANCED_NOTES_HIERARCHY_WIDTH, 90);

                anProjectEnabled = ProjectPrefs.GetBool(Constants.SECTION, Constants.ADVANCED_NOTES_PROJECT_ENABLED, true);
                anProjectWidth = ProjectPrefs.GetFloat(Constants.SECTION, Constants.ADVANCED_NOTES_PROJECT_WIDTH, 90);
                prefsLoaded = true;
            }

            // Project Preferences GUI
            Divider.EditorGUILayout.Divider("Scene Notes");
            viewOnSelection = EditorGUILayout.Toggle(previewContent, viewOnSelection);

            GUI.enabled = viewOnSelection;

            Divider.EditorGUILayout.Divider("", "Notes");
            anchor = (EditorExtensions.Anchor)EditorGUILayout.EnumPopup(anchorContent, anchor);
            showChildren = EditorGUILayout.Toggle(childContent, showChildren);
            size = EditorGUILayout.Vector2Field(sizeContent, size);

            Divider.EditorGUILayout.Divider(GUIContent.none, new GUIContent("Hide/Show Button"));
            btnActive = EditorGUILayout.Toggle(btnVisibleContent, btnActive);

            if (viewOnSelection)
                GUI.enabled = btnActive;

            btnAnchor = (EditorExtensions.Anchor)EditorGUILayout.EnumPopup(btnAnchorContent, btnAnchor);
            btnSize = EditorGUILayout.Vector2Field(btnSizeContent, btnSize);
            GUI.enabled = true;

            Divider.EditorGUILayout.Divider("Advanced Notes");
            Divider.EditorGUILayout.Divider("", "Hierarchy Window");
            anHierarchyEnabled = EditorGUILayout.Toggle(anEnabledContent, anHierarchyEnabled);

            GUI.enabled = anHierarchyEnabled;
            anHierarchyWidth = EditorGUILayout.FloatField(anWidthContent, anHierarchyWidth);
            GUI.enabled = true;

            Divider.EditorGUILayout.Divider("", "Project Window");
            anProjectEnabled = EditorGUILayout.Toggle(anEnabledContent, anProjectEnabled);

            GUI.enabled = anProjectEnabled;
            anProjectWidth = EditorGUILayout.FloatField(anWidthContent, anProjectWidth);
            GUI.enabled = true;

            // Save the preferences
            if (GUI.changed)
            {
                ProjectPrefs.SetBool(Constants.SECTION, Constants.SCENE_NOTES_ENABLED, viewOnSelection);
                ProjectPrefs.SetInt(Constants.SECTION, Constants.SCENE_NOTES_ANCHOR, (int)anchor);
                ProjectPrefs.SetBool(Constants.SECTION, Constants.SCENE_NOTES_VIEW_CHILDREN, showChildren);
                ProjectPrefs.SetFloat(Constants.SECTION, Constants.SCENE_NOTES_WIDTH, size.x);
                ProjectPrefs.SetFloat(Constants.SECTION, Constants.SCENE_NOTES_HEIGHT, size.y);

                ProjectPrefs.SetBool(Constants.SECTION, Constants.SCENE_NOTES_BTN_ENABLED, btnActive);
                ProjectPrefs.SetInt(Constants.SECTION, Constants.SCENE_NOTES_BTN_ANCHOR, (int)btnAnchor);
                ProjectPrefs.SetFloat(Constants.SECTION, Constants.SCENE_NOTES_BTN_WIDTH, btnSize.x);
                ProjectPrefs.SetFloat(Constants.SECTION, Constants.SCENE_NOTES_BTN_HEIGHT, btnSize.y);

                ProjectPrefs.SetBool(Constants.SECTION, Constants.ADVANCED_NOTES_HIERARCHY_ENABLED, anHierarchyEnabled);
                ProjectPrefs.SetFloat(Constants.SECTION, Constants.ADVANCED_NOTES_HIERARCHY_WIDTH, anHierarchyWidth);

                ProjectPrefs.SetBool(Constants.SECTION, Constants.ADVANCED_NOTES_PROJECT_ENABLED, anProjectEnabled);
                ProjectPrefs.SetFloat(Constants.SECTION, Constants.ADVANCED_NOTES_PROJECT_WIDTH, anProjectWidth);
            }
        }
    }
}