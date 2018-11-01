using UnityEngine;
using UnityEditor;
using RotaryHeart.Lib.YamlDotNet.Serialization;
using System.IO;
using System.Collections.Generic;

namespace RotaryHeart.Lib.UniNotes
{
    /// <summary>
    /// Handles the advanced drawing, heirarchy and project window
    /// </summary>
    [InitializeOnLoad]
    public class AdvancedNoteDrawer
    {
        //Color of the dragger line
        static Color draggerColor = EditorGUIUtility.isProSkin ? new Color32(194, 194, 194, 255) : new Color32(56, 56, 56, 255);
        //Dragging flag
        static bool dragging = false;

        //Reference to the settings SO
        static UniNotesSettings m_settings = null;
        //List of all the available settings name, used for the dropdown
        static string[] m_elements;

        static Dictionary<string, UniNotesSettings.UniNoteData> parsedNotes = new Dictionary<string, UniNotesSettings.UniNoteData>();

        //Finds and returns the reference to the settings SO
        public static UniNotesSettings Settings
        {
            get
            {
                if (m_settings == null)
                {
                    // search for all UniNotesSettings type asset
                    string[] guids = AssetDatabase.FindAssets("t:UniNotesSettings");

                    foreach (var guid in guids)
                    {
                        string path = AssetDatabase.GUIDToAssetPath(guid);
                        string parentPath = System.IO.Path.GetDirectoryName(path);

                        if (parentPath.EndsWith("Settings"))
                        {
                            parentPath = System.IO.Path.GetDirectoryName(parentPath);

                            if (parentPath.EndsWith("UniNotes"))
                            {
                                m_settings = AssetDatabase.LoadAssetAtPath<UniNotesSettings>(path);
                            }
                        }
                    }

                    if (m_settings == null)
                    {
                        Debug.LogError("Couldn't find any UniNotesSettings asset.");
                    }
                }

                return m_settings;
            }
        }

        public static string[] AvailableSettings
        {
            get
            {
                //If the current count doesn't match with the count on the SO
                if (m_elements == null || m_elements.Length != Settings.notes.Count)
                {
                    int count = Settings.notes.Count;
                    m_elements = new string[count];

                    for (int i = 0; i < count; i++)
                    {
                        m_elements[i] = Settings.notes[i].noteName;
                    }
                }

                return m_elements;
            }
        }

        static AdvancedNoteDrawer()
        {
            if (!Constants.NotifyAboutUpdate)
            {
                UniNoteImporter.CheckImport();
                Constants.NotifyAboutUpdate = true;
            }

            //Hierarchy GUI
            EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyGUI;
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;

            //Project GUI
            EditorApplication.projectWindowItemOnGUI -= OnProjectGUI;
            EditorApplication.projectWindowItemOnGUI += OnProjectGUI;

            //Used to detect if any note component is attached to this scene
            UnityEditor.SceneManagement.EditorSceneManager.sceneOpened -= EditorSceneManager_sceneOpened;
            UnityEditor.SceneManagement.EditorSceneManager.sceneOpened += EditorSceneManager_sceneOpened;
        }

        private static void EditorSceneManager_sceneOpened(UnityEngine.SceneManagement.Scene scene, UnityEditor.SceneManagement.OpenSceneMode mode)
        {
            //Get the scene path using the GUID
            string scenePath = Path.Combine(Path.Combine(Constants.NotesPath, "Hierarchy"), AssetDatabase.AssetPathToGUID(scene.path));

            //If a component is found on the scene add it to the ini file
            if (GameObject.FindObjectOfType<UniNoteComponent>() != null)
            {
                Directory.CreateDirectory(scenePath);
            }
            //If no component is found and there are no hierarchy notes, remove it
            else if (Directory.Exists(scenePath) && Directory.GetFiles(scenePath, "*.UniNote", SearchOption.TopDirectoryOnly).Length == 0)
            {
                Directory.Delete(scenePath);
            }
        }

        static void OnHierarchyGUI(int instanceID, Rect selectionRect)
        {
            //If the setting file is not found or we don't want to draw the hierarchy notes
            if (Settings == null || !Constants.HierarchyNotesEnabled)
            {
                return;
            }

            //Get the GO that is going to be drawn
            GameObject go = (GameObject)EditorUtility.InstanceIDToObject(instanceID);

            //Get the scene path using the GUID
            string scenePath = Path.Combine(Path.Combine(Constants.NotesPath, "Hierarchy"), go == null ? "" : AssetDatabase.AssetPathToGUID(go.scene.path));

            //Calculate the file id of this object
            string settingId = EditorExtensions.GetFileId(go).ToString();

            //Current note path
            string filePath = Path.Combine(scenePath, settingId + ".UniNote");

            //Get the width stored on the settings
            float width = Constants.HierarchyNotesWidth;

            //Added .x at the end to be sure that they are on the correct position even if they are indented, the +14 is for the scroll bar
            selectionRect.x = selectionRect.width - (width - selectionRect.x + 14);

            //Only draw the dragger on the scene and on a gamobject with notes
            if (go == null || File.Exists(filePath))
            {
                width = DrawDragger(selectionRect, width, true);
            }

            //Check if this GO note file exists
            if (!File.Exists(filePath))
                return;

            //Change the rect width to the correct width
            selectionRect.width = width;

            //Draw the note
            DrawNote("UniNotes_Hierarchy:" + go.scene.path, settingId, selectionRect, filePath);
        }

        static void OnProjectGUI(string guid, Rect selectionRect)
        {
            //If the setting file is not found or we don't want to draw the project notes
            if (Settings == null || !Constants.ProjectNotesEnabled || string.IsNullOrEmpty(guid))
            {
                return;
            }

            //Get the path using the GUID
            string projectPath = Path.Combine(Constants.NotesPath, "Project");
            //Current note path
            string filePath = Path.Combine(projectPath, guid + ".UniNote");

            //This object doesn't have any note
            if (!File.Exists(filePath))
            {
                return;
            }

            //Get the width stored on the settings
            float width = Constants.ProjectNotesWidth;

            //If the project window is on 2 column and the assets are not on list
            if (selectionRect.height > 16)
            {
                //Limit the width to be the same as the asset height
                if (width > selectionRect.height)
                    width = selectionRect.height;

                //Adjust the x value
                selectionRect.x = selectionRect.width - (width - selectionRect.x);
            }
            else
            {
                //Added rect.x + 14 at the end to be sure that they are on the correct position even if they are indented (14 is for the scroll bar)
                selectionRect.x = selectionRect.width - (width - selectionRect.x + 14);

                width = DrawDragger(selectionRect, width, false);
            }

            //Be sure that the height is set to single line
            selectionRect.height = Constants.ProjectNotesSize;

            //Change the rect width to the correct width
            selectionRect.width = width;

            //Draw the note
            DrawNote("UniNotes_Project", guid, selectionRect, filePath);
        }

        /// <summary>
        /// Draws a note on the given rect
        /// </summary>
        /// <param name="section">ProjectPrefs section that has the note data</param>
        /// <param name="id">Note id</param>
        /// <param name="rect">Note position</param>
        /// <param name="filePath">Path to the note</param>
        static void DrawNote(string section, string id, Rect rect, string filePath)
        {
            UniNotesSettings.UniNoteData data = null;

            if (!parsedNotes.TryGetValue(filePath, out data))
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    Deserializer deserializer = new Deserializer();
                    data = deserializer.Deserialize<UniNotesSettings.UniNoteData>(reader);
                }

                parsedNotes.Add(filePath, data);
            }

            if (data == null)
            {
                return;
            }

            //Iterate through all the notes available for this element
            for (int i = data.notes.Count - 1; i >= 0; i--)
            {
                int index = i;

                //If a note is expanded skip all but the expanded one
                if (data.expandedIndex != -1 && index != (data.expandedIndex - 1))
                    continue;

                //Get the current note added and the custom name
                string settingVal = data.notes[index].id;
                string textFieldVal = data.notes[index].text;

                UniNotesSettings.NoteSetting setting;

                //No value is stored, create a new one to be used
                if (settingVal.Equals("-1"))
                {
                    setting = new UniNotesSettings.NoteSetting() { backgroundColor = Color.black * 0, textColor = Color.white, icon = null };
                }
                else
                {
                    //Found a value, remove the hint
                    Settings.FindSetting(settingVal, out setting);

                    //Fail safe, if the setting is not found, delete the note from this object
                    if (setting == null)
                    {
                        if (data.notes.Count == 1)
                        {
                            File.Delete(filePath);

                            return;
                        }
                        else
                        {
                            using (StreamWriter writer = new StreamWriter(filePath))
                            {
                                data.notes.RemoveAt(index);

                                Serializer serializer = new Serializer();
                                serializer.Serialize(writer, data);
                            }
                        }

                        return;
                    }
                }

                #region Draw Note
                Rect iconRect;

                //If a note is expanded
                if (data.expandedIndex != -1)
                {
                    //Draw the rectangle color
                    Rect bgRect = new Rect(rect.x + rect.height, rect.y, rect.width - rect.height / 2, EditorGUIUtility.singleLineHeight);
                    EditorExtensions.DrawRect(bgRect, setting.backgroundColor);

                    iconRect = new Rect(rect.x, rect.y - (rect.height / 2) + (EditorGUIUtility.singleLineHeight / 2), rect.height, rect.height);

                    GUIStyle style = new GUIStyle(GUI.skin.label);
                    style.normal.textColor = setting.textColor;

                    //Draw the text field
                    Color prevColor = GUI.color;
                    GUI.color = setting.textColor;
                    EditorGUI.LabelField(bgRect, textFieldVal, style);
                    GUI.color = prevColor;
                }
                else
                {
                    iconRect = new Rect(rect.x + (rect.width + rect.height / 2) - (rect.height * (index + 1)), rect.y - (rect.height / 2) + (EditorGUIUtility.singleLineHeight / 2), rect.height, rect.height);

                    //If the drawer is above any icon, skip it
                    if (rect.x > iconRect.x)
                        continue;
                }

                Vector2 size = EditorGUIUtility.GetIconSize();
                EditorGUIUtility.SetIconSize(Vector2.one * (rect.height - 4));

                //Draw the icon
                if (setting.icon != null)
                {
                    GUI.Label(iconRect, setting.icon);
                }
                else
                {
                    Debug.unityLogger.logEnabled = false;
                    GUIContent content = EditorGUIUtility.IconContent(setting.unityIcon);
                    Debug.unityLogger.logEnabled = true;

                    //An icon is required, draw the default one
                    if (content.image == null)
                    {
                        EditorGUIUtility.SetIconSize(Vector2.one * 16);
                        content = EditorGUIUtility.IconContent("d_UnityEditor.ConsoleWindow");
                    }

                    GUI.Label(iconRect, content);
                }

                EditorGUIUtility.SetIconSize(size);

                #endregion

                EditorGUIUtility.AddCursorRect(iconRect, MouseCursor.Link);

                //If we are hovering the icon show the window
                if (iconRect.Contains(Event.current.mousePosition))
                {
                    //Icon click, show context menu
                    if (Event.current.type == EventType.MouseDown)
                    {
                        HoverWindow.Initialize(filePath, index);
                        Event.current.Use();
                    }
                }
            }
        }

        /// <summary>
        /// Draws the dragger
        /// </summary>
        /// <param name="rect">Position</param>
        /// <param name="width">CUrrent width</param>
        /// <param name="isHierarchy">Flag to know where to save the width</param>
        /// <returns></returns>
        static float DrawDragger(Rect rect, float width, bool isHierarchy)
        {
            Rect dragger = new Rect(rect);

            dragger.x -= 4;

            dragger.width = 2;

            //Draws the rect
            EditorExtensions.DrawRect(dragger, draggerColor * (dragging ? 1 : .65f));

            //Offset used for dragging
            dragger.xMin -= 8;
            dragger.xMax += 8;

            Event current = Event.current;

            //Changes the cursor if it's above the dragger
            EditorGUIUtility.AddCursorRect(dragger, MouseCursor.ResizeHorizontal);

            //Dragging logic
            if (dragging && current.type == EventType.MouseDrag)
            {
                current.Use();
                //Clamp used to prevent moving out of the window dimensions
                width = Mathf.Clamp(width - current.delta.x, Constants.ProjectNotesSize / 2, rect.width - 20);

                //Save the data
                if (isHierarchy)
                    Constants.HierarchyNotesWidth = width;
                else
                    Constants.ProjectNotesWidth = width;
            }

            //Start dragging
            if (dragger.Contains(current.mousePosition) && current.type == EventType.MouseDrag)
            {
                current.Use();
                dragging = true;
            }

            //Stop dragging
            if (dragging && current.type == EventType.MouseUp)
            {
                current.Use();
                dragging = false;
            }

            return width;
        }

        public static void ResetSettings()
        {
            m_elements = null;
        }

        public static void UpdateNote(string path, UniNotesSettings.UniNoteData data)
        {
            parsedNotes[path] = data;
        }
    }
}