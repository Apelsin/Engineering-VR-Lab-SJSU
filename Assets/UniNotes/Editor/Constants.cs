using UnityEngine;
using RotaryHeart.Lib.ProjectPreferences;

namespace RotaryHeart.Lib.UniNotes
{
    public sealed class Constants
    {
        private static string DEF_NOTES_PATH;
        private const bool DEF_SCENE_NOTES_ENABLED = true;
        private const bool DEF_SCENE_NOTES_VIEW_CHILDREN = true;
        private const bool DEF_SCENE_NOTES_BTN_ENABLED = true;
        private const bool DEF_ADVANCED_NOTES_HIERARCHY_ENABLED = true;
        private const bool DEF_ADVANCED_NOTES_PROJECT_ENABLED = true;
        private const bool DEF_SCENE_NOTES_HIDDEN = true;
        private const bool DEF_NOTIFIED_UPDATE = true;
        private const EditorExtensions.Anchor DEF_SCENE_NOTES_ANCHOR = EditorExtensions.Anchor.Bottom;
        private const EditorExtensions.Anchor DEF_SCENE_NOTES_BTN_ANCHOR = EditorExtensions.Anchor.BottomLeft;
        private static Vector2 DEF_SCENE_NOTES_SIZE = new Vector2(400, 100);
        private static Vector2 DEF_SCENE_NOTES_BTN_SIZE = new Vector2(32, 32);
        private const float DEF_ADVANCED_NOTES_HIERARCHY_WIDTH = 90;
        private const float DEF_ADVANCED_NOTES_PROJECT_WIDTH = 90;
        private const float DEF_ADVANCED_NOTES_PROJECT_SIZE = 16;

        public static readonly string ID_SECTION = "UniNotes_Settings";
        public static readonly string ID_NOTES_PATH = "notesPath";
        public static readonly string ID_SCENE_NOTES_ENABLED = "sn_enabled";
        public static readonly string ID_SCENE_NOTES_ANCHOR = "sn_anchor";
        public static readonly string ID_SCENE_NOTES_VIEW_CHILDREN = "sn_children";
        public static readonly string ID_SCENE_NOTES_WIDTH = "sn_width";
        public static readonly string ID_SCENE_NOTES_HEIGHT = "sn_height";
        public static readonly string ID_SCENE_NOTES_HIDDEN = "sn_hidden";
        public static readonly string ID_SCENE_NOTES_BTN_ENABLED = "sn_btn_Active";
        public static readonly string ID_SCENE_NOTES_BTN_ANCHOR = "sn_btn_Anchor";
        public static readonly string ID_SCENE_NOTES_BTN_WIDTH = "sn_btn_Width";
        public static readonly string ID_SCENE_NOTES_BTN_HEIGHT = "sn_btn_Height";
        public static readonly string ID_ADVANCED_NOTES_HIERARCHY_ENABLED = "an_hierarchyEnabled";
        public static readonly string ID_ADVANCED_NOTES_HIERARCHY_WIDTH = "an_hierarchyWidth";
        public static readonly string ID_ADVANCED_NOTES_PROJECT_ENABLED = "an_projectEnabled";
        public static readonly string ID_ADVANCED_NOTES_PROJECT_WIDTH = "an_projectWidth";
        public static readonly string ID_ADVANCED_NOTES_PROJECT_SIZE = "an_projectNoteSize";
        public static readonly string ID_NOTIFIED_UPDATE = "NotifyUpdate";

        public static string NotesPath
        {
            get
            {
                if (string.IsNullOrEmpty(DEF_NOTES_PATH))
                    DEF_NOTES_PATH = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Application.dataPath), "UniNotes");

                return ProjectPrefs.GetString(ID_SECTION, ID_NOTES_PATH, DEF_NOTES_PATH);
            }
            set
            {
                ProjectPrefs.SetString(ID_SECTION, ID_NOTES_PATH, value);
            }
        }
        public static bool SceneNotesEnabled
        {
            get
            {
                return ProjectPrefs.GetBool(ID_SECTION, ID_SCENE_NOTES_ENABLED, DEF_SCENE_NOTES_ENABLED);
            }
            set
            {
                ProjectPrefs.SetBool(ID_SECTION, ID_SCENE_NOTES_ENABLED, value);
            }
        }
        public static bool ShowChildren
        {
            get
            {
                return ProjectPrefs.GetBool(ID_SECTION, ID_SCENE_NOTES_VIEW_CHILDREN, DEF_SCENE_NOTES_VIEW_CHILDREN);
            }
            set
            {
                ProjectPrefs.SetBool(ID_SECTION, ID_SCENE_NOTES_VIEW_CHILDREN, value);
            }
        }
        public static bool SceneNotesButtonEnabled
        {
            get
            {
                return ProjectPrefs.GetBool(ID_SECTION, ID_SCENE_NOTES_BTN_ENABLED, DEF_SCENE_NOTES_BTN_ENABLED);
            }
            set
            {
                ProjectPrefs.SetBool(ID_SECTION, ID_SCENE_NOTES_BTN_ENABLED, value);
            }
        }
        public static bool HierarchyNotesEnabled
        {
            get
            {
                return ProjectPrefs.GetBool(ID_SECTION, ID_ADVANCED_NOTES_HIERARCHY_ENABLED, DEF_ADVANCED_NOTES_HIERARCHY_ENABLED);
            }
            set
            {
                ProjectPrefs.SetBool(ID_SECTION, ID_ADVANCED_NOTES_HIERARCHY_ENABLED, value);
            }
        }
        public static bool ProjectNotesEnabled
        {
            get
            {
                return ProjectPrefs.GetBool(ID_SECTION, ID_ADVANCED_NOTES_PROJECT_ENABLED, DEF_ADVANCED_NOTES_PROJECT_ENABLED);
            }
            set
            {
                ProjectPrefs.SetBool(ID_SECTION, ID_ADVANCED_NOTES_PROJECT_ENABLED, value);
            }
        }
        public static bool SceneNotesHidden
        {
            get
            {
                return ProjectPrefs.GetBool(ID_SECTION, ID_SCENE_NOTES_HIDDEN, DEF_SCENE_NOTES_HIDDEN);
            }
            set
            {
                ProjectPrefs.SetBool(ID_SECTION, ID_SCENE_NOTES_HIDDEN, value);
            }
        }
        public static bool NotifyAboutUpdate
        {
            get
            {
                return ProjectPrefs.GetBool(ID_SECTION, ID_NOTIFIED_UPDATE, DEF_NOTIFIED_UPDATE);
            }
            set
            {
                ProjectPrefs.SetBool(ID_SECTION, ID_NOTIFIED_UPDATE, value);
            }
        }
        public static EditorExtensions.Anchor SceneNotesAnchor
        {
            get
            {
                return (EditorExtensions.Anchor)ProjectPrefs.GetInt(ID_SECTION, ID_SCENE_NOTES_ANCHOR, (int)DEF_SCENE_NOTES_ANCHOR);
            }
            set
            {
                ProjectPrefs.SetInt(ID_SECTION, ID_SCENE_NOTES_ANCHOR, (int)value);
            }
        }
        public static EditorExtensions.Anchor SceneNotesButtonAnchor
        {
            get
            {
                return (EditorExtensions.Anchor)ProjectPrefs.GetInt(ID_SECTION, ID_SCENE_NOTES_BTN_ANCHOR, (int)DEF_SCENE_NOTES_BTN_ANCHOR);
            }
            set
            {
                ProjectPrefs.SetInt(ID_SECTION, ID_SCENE_NOTES_BTN_ANCHOR, (int)value);
            }
        }
        public static Vector2 SceneNotesSize
        {
            get
            {
                float x = ProjectPrefs.GetFloat(ID_SECTION, ID_SCENE_NOTES_WIDTH, DEF_SCENE_NOTES_SIZE.x);
                float y = ProjectPrefs.GetFloat(ID_SECTION, ID_SCENE_NOTES_HEIGHT, DEF_SCENE_NOTES_SIZE.y);

                return new Vector2(x, y);
            }
            set
            {
                ProjectPrefs.SetFloat(ID_SECTION, ID_SCENE_NOTES_WIDTH, value.x);
                ProjectPrefs.SetFloat(ID_SECTION, ID_SCENE_NOTES_HEIGHT, value.y);
            }
        }
        public static Vector2 SceneNotesButtonSize
        {
            get
            {
                float x = ProjectPrefs.GetFloat(ID_SECTION, ID_SCENE_NOTES_BTN_WIDTH, DEF_SCENE_NOTES_BTN_SIZE.x);
                float y = ProjectPrefs.GetFloat(ID_SECTION, ID_SCENE_NOTES_BTN_HEIGHT, DEF_SCENE_NOTES_BTN_SIZE.y);

                return new Vector2(x, y);
            }
            set
            {
                ProjectPrefs.SetFloat(ID_SECTION, ID_SCENE_NOTES_BTN_WIDTH, value.x);
                ProjectPrefs.SetFloat(ID_SECTION, ID_SCENE_NOTES_BTN_HEIGHT, value.y);
            }
        }
        public static float HierarchyNotesWidth
        {
            get
            {
                return ProjectPrefs.GetFloat(ID_SECTION, ID_ADVANCED_NOTES_HIERARCHY_WIDTH, DEF_ADVANCED_NOTES_HIERARCHY_WIDTH);
            }
            set
            {
                ProjectPrefs.SetFloat(ID_SECTION, ID_ADVANCED_NOTES_HIERARCHY_WIDTH, value);
            }
        }
        public static float ProjectNotesWidth
        {
            get
            {
                return ProjectPrefs.GetFloat(ID_SECTION, ID_ADVANCED_NOTES_PROJECT_WIDTH, DEF_ADVANCED_NOTES_PROJECT_WIDTH);
            }
            set
            {
                ProjectPrefs.SetFloat(ID_SECTION, ID_ADVANCED_NOTES_PROJECT_WIDTH, value);
            }
        }
        public static float ProjectNotesSize
        {
            get
            {
                return ProjectPrefs.GetFloat(ID_SECTION, ID_ADVANCED_NOTES_PROJECT_SIZE, DEF_ADVANCED_NOTES_PROJECT_SIZE);
            }
            set
            {
                ProjectPrefs.SetFloat(ID_SECTION, ID_ADVANCED_NOTES_PROJECT_SIZE, value);
            }
        }

        public static void RestoreDefaults()
        {
            NotesPath = DEF_NOTES_PATH;
            SceneNotesEnabled = DEF_SCENE_NOTES_ENABLED;
            ShowChildren = DEF_SCENE_NOTES_VIEW_CHILDREN;
            SceneNotesButtonEnabled = DEF_SCENE_NOTES_BTN_ENABLED;
            HierarchyNotesEnabled = DEF_ADVANCED_NOTES_HIERARCHY_ENABLED;
            ProjectNotesEnabled = DEF_ADVANCED_NOTES_PROJECT_ENABLED;
            SceneNotesHidden = DEF_SCENE_NOTES_HIDDEN;
            NotifyAboutUpdate = DEF_NOTIFIED_UPDATE;
            SceneNotesAnchor = DEF_SCENE_NOTES_ANCHOR;
            SceneNotesButtonAnchor = DEF_SCENE_NOTES_BTN_ANCHOR;
            SceneNotesSize = DEF_SCENE_NOTES_SIZE;
            SceneNotesButtonSize = DEF_SCENE_NOTES_BTN_SIZE;
            HierarchyNotesWidth = DEF_ADVANCED_NOTES_HIERARCHY_WIDTH;
            ProjectNotesWidth = DEF_ADVANCED_NOTES_PROJECT_WIDTH;
            ProjectNotesSize = DEF_ADVANCED_NOTES_PROJECT_SIZE;
        }
    }
}