using UnityEngine;
using UnityEditor;
using System.IO;

namespace RotaryHeart.Lib.UniNotes
{
    public class PreferencesWindow
    {
        #region GUIContent
        static GUIContent notesSaveContent = new GUIContent("Notes Save Location", "Path where all notes data will be saved.");

        static GUIContent previewContent = new GUIContent("View Notes on Selection", "Should the Notes be visible on the scene view when the object is highlighted");

        static GUIContent anchorContent = new GUIContent("Position", "Scene view Notes position");
        static GUIContent childContent = new GUIContent("Show Children Notes", "Should the children Notes be visible on the scene too");
        static GUIContent sizeContent = new GUIContent("Scene Preview Size", "How big the scene Notes preview should be");

        static GUIContent btnVisibleContent = new GUIContent("Enabled", "Enable or disable the button on the scene view");
        static GUIContent btnAnchorContent = new GUIContent("Position", "Scene view button position");
        static GUIContent btnSizeContent = new GUIContent("Size", "How big the button should be");

        static GUIContent anEnabledContent = new GUIContent("Enabled", "Enable or disable the advanced notes");
        static GUIContent anWidthContent = new GUIContent("Width", "This is the width of the note area");
        static GUIContent anProjectNoteSizeContent = new GUIContent("Size", "How big the notes will be drawn");
        #endregion

        // Have we loaded the prefs yet
        private static bool prefsLoaded = false;

        private static string notesPath = "";

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
        private static float anProjectNoteSize = 16;

        // Add preferences section named "My Preferences" to the Preferences Window
        [PreferenceItem("UniNotes")]
        public static void PreferencesGUI()
        {
            //Change this to save the data on the ProjectSettings, there all the UniNotes settings(including the heirarchy and project noes) will be saved
            // Load the preferences
            if (!prefsLoaded)
            {
                notesPath = Constants.NotesPath;

                viewOnSelection = Constants.SceneNotesEnabled;
                anchor = Constants.SceneNotesAnchor;
                showChildren = Constants.ShowChildren;
                size = Constants.SceneNotesSize;

                btnActive = Constants.SceneNotesButtonEnabled;
                btnAnchor = Constants.SceneNotesButtonAnchor;
                btnSize = Constants.SceneNotesButtonSize;

                anHierarchyEnabled = Constants.HierarchyNotesEnabled;
                anHierarchyWidth = Constants.HierarchyNotesWidth;

                anProjectEnabled = Constants.ProjectNotesEnabled;
                anProjectWidth = Constants.ProjectNotesWidth;
                anProjectNoteSize = Constants.ProjectNotesSize;

                prefsLoaded = true;
            }

            Divider.EditorGUILayout.Divider(notesSaveContent);
            EditorGUILayout.BeginHorizontal();
            GUI.enabled = false;
            EditorGUILayout.LabelField(new GUIContent(notesPath, notesPath), EditorStyles.textField);
            GUI.enabled = true;
            if (GUILayout.Button("...", GUILayout.Width(30)))
            {
                string path = EditorUtility.OpenFolderPanel("Select Path", Constants.NotesPath, "");

                if (!string.IsNullOrEmpty(path))
                {
                    if (Directory.Exists(Constants.NotesPath))
                    {
                        DirectoryCopy(Constants.NotesPath, path, true);
                        Directory.Delete(Constants.NotesPath, true);
                    }

                    Constants.NotesPath = notesPath = path;
                }
            }
            EditorGUILayout.EndHorizontal();

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
            anProjectNoteSize = EditorGUILayout.FloatField(anProjectNoteSizeContent, anProjectNoteSize);
            GUI.enabled = true;

            // Save the preferences
            if (GUI.changed)
            {
                Constants.NotesPath = notesPath;
                Constants.SceneNotesEnabled = viewOnSelection;
                Constants.SceneNotesAnchor = anchor;
                Constants.ShowChildren = showChildren;
                Constants.SceneNotesSize = size;

                Constants.SceneNotesButtonEnabled = btnActive;
                Constants.SceneNotesButtonAnchor = btnAnchor;
                Constants.SceneNotesButtonSize = btnSize;

                Constants.HierarchyNotesEnabled = anHierarchyEnabled;
                Constants.HierarchyNotesWidth = anHierarchyWidth;

                Constants.ProjectNotesEnabled = anProjectEnabled;
                Constants.ProjectNotesWidth = anProjectWidth;
                Constants.ProjectNotesSize = anProjectNoteSize;
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Restore Default"))
            {
                Constants.RestoreDefaults();

                prefsLoaded = false;
            }
        }
        private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the source directory does not exist, throw an exception.
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory does not exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }


            // Get the file contents of the directory to copy.
            FileInfo[] files = dir.GetFiles();

            foreach (FileInfo file in files)
            {
                // Create the path to the new copy of the file.
                string temppath = Path.Combine(destDirName, file.Name);

                // Copy the file.
                file.CopyTo(temppath, false);
            }

            // If copySubDirs is true, copy the subdirectories.
            if (copySubDirs)
            {

                foreach (DirectoryInfo subdir in dirs)
                {
                    // Create the subdirectory.
                    string temppath = Path.Combine(destDirName, subdir.Name);

                    // Copy the subdirectories.
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }
    }
}