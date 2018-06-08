using UnityEngine;
using UnityEditor;
using RotaryHeart.Lib.ProjectPreferences;

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

        //Finds and returns the reference to the settings SO
        public static UniNotesSettings Settings
        {
            get
            {
                if (m_settings == null)
                {
                    // search for all UniNotesSettings type asset
                    string[] guids = AssetDatabase.FindAssets("t:UniNotesSettings");

                    if (guids.Length > 0)
                    {
                        string settingsPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                        m_settings = AssetDatabase.LoadAssetAtPath<UniNotesSettings>(settingsPath);
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
            //Hierarchy GUI
            EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyGUI;
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;

            //Project GUI
            EditorApplication.projectWindowItemOnGUI -= OnProjectGUI;
            EditorApplication.projectWindowItemOnGUI += OnProjectGUI;
        }

        static void OnHierarchyGUI(int instanceID, Rect selectionRect)
        {
            //If the setting file is not found or we don't want to draw the hierarchy notes
            if (Settings == null || !ProjectPrefs.GetBool(Constants.SECTION, Constants.ADVANCED_NOTES_HIERARCHY_ENABLED, true))
            {
                return;
            }

            //Get the GO that is going to be drawn
            GameObject go = (GameObject)EditorUtility.InstanceIDToObject(instanceID);

            //Get the width stored on the settings
            float width = ProjectPrefs.GetFloat(Constants.SECTION, Constants.ADVANCED_NOTES_HIERARCHY_WIDTH, 90);

            //Added .x at the end to be sure that they are on the correct position even if they are indented, the +14 is for the scroll bar
            selectionRect.x = selectionRect.width - (width - selectionRect.x + 14);

            //Calculate the file id of this object
            string settingId = EditorExtensions.GetFileId(go).ToString();

            //Get the current object stored on the ProjectPrefs, default to -2 if it doesn't find any. If the GO is null the scene object is being drawn, default it to -2 too
            string value = go == null ? "-2" : ProjectPrefs.GetString("UniNotes_Hierarchy:" + go.scene.path, settingId, "-2");

            //Only draw the dragger on the scene and on a gamobject with notes
            if (go == null || !value.Equals("-2"))
            {
                width = DrawDragger(selectionRect, width, true);
            }

            //This object doesn't have any note
            if (value.Equals("-2"))
            {
                return;
            }

            //Change the rect width to the correct width
            selectionRect.width = width;

            //Draw the note
            DrawNote("UniNotes_Hierarchy:" + go.scene.path, settingId, selectionRect, value);
        }

        static void OnProjectGUI(string guid, Rect selectionRect)
        {
            //If the setting file is not found or we don't want to draw the project notes
            if (Settings == null || !ProjectPrefs.GetBool(Constants.SECTION, Constants.ADVANCED_NOTES_PROJECT_ENABLED, true))
            {
                return;
            }

            //Get the current object stored on the ProjectPrefs, default to -2 if it doesn't find any
            string value = ProjectPrefs.GetString("UniNotes_Project", guid, "-2");

            //This object doesn't have any note
            if (value.Equals("-2"))
            {
                return;
            }

            //Get the width stored on the settings
            float width = ProjectPrefs.GetFloat(Constants.SECTION, Constants.ADVANCED_NOTES_PROJECT_WIDTH, 90);

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
            selectionRect.height = EditorGUIUtility.singleLineHeight;

            //Change the rect width to the correct width
            selectionRect.width = width;

            //Draw the note
            DrawNote("UniNotes_Project", guid, selectionRect, value);
        }

        /// <summary>
        /// Draws a note on the given rect
        /// </summary>
        /// <param name="section">ProjectPrefs section that has the note data</param>
        /// <param name="id">Note id</param>
        /// <param name="rect">Note position</param>
        /// <param name="value">Data saved on ProjectPrefs</param>
        static void DrawNote(string section, string id, Rect rect, string value)
        {
            UniNotesSettings.UniNoteData data = JsonUtility.FromJson<UniNotesSettings.UniNoteData>(value);

            if (data == null)
            {
                Debug.LogError("Data not saved correctly for: " + id + " in section: " + section);
                return;
            }

            //Iterate through all the notes available for this element
            for (int i = data.notes.Count - 1; i >= 0; i--)
            {
                int index = i;

                //If a note is expanded skip all but the expanded one
                if (data.expandedIndex != -1 && index != data.expandedIndex)
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
                        ProjectPrefs.RemoveKey(section, id);
                        return;
                    }
                }

                #region Draw Note
                Rect iconRect;

                //If a note is expanded
                if (data.expandedIndex != -1)
                {
                    //Draw the rectangle color
                    EditorExtensions.DrawRect(new Rect(rect.x + 20, rect.y, rect.width - 10, rect.height), setting.backgroundColor);

                    iconRect = new Rect(rect.x + 5, rect.y + 1, 16, rect.height);

                    //Draw the icon
                    if (setting.icon != null)
                    {
                        GUI.Label(iconRect, setting.icon);
                    }
                    else
                    {
                        Debug.unityLogger.logEnabled = false;
                        Vector2 size = EditorGUIUtility.GetIconSize();
                        EditorGUIUtility.SetIconSize(Vector2.one * 12);
                        GUIContent content = EditorGUIUtility.IconContent(setting.unityIcon);
                        Debug.unityLogger.logEnabled = true;

                        //An icon is required, draw the default one
                        if (content.image == null)
                        {
                            EditorGUIUtility.SetIconSize(Vector2.one * 16);
                            content = EditorGUIUtility.IconContent("d_UnityEditor.ConsoleWindow");
                        }

                        GUI.Label(iconRect, content);
                        EditorGUIUtility.SetIconSize(size);
                    }

                    GUIStyle style = new GUIStyle(GUI.skin.label);
                    style.normal.textColor = setting.textColor;

                    //Draw the text field
                    Color prevColor = GUI.color;
                    GUI.color = setting.textColor;
                    EditorGUI.LabelField(new Rect(rect.x + 20, rect.y, rect.width - 10, rect.height), textFieldVal, style);
                    GUI.color = prevColor;
                }
                else
                {
                    iconRect = new Rect(rect.x + rect.width - 9 - (rect.height * index), rect.y + 1, rect.height, rect.height);

                    //If the drawer is above any icon, skip it
                    if (rect.x > iconRect.x)
                        continue;

                    //Draw the icon
                    if (setting.icon != null)
                    {
                        GUI.Label(iconRect, setting.icon);
                    }
                    else
                    {
                        Debug.unityLogger.logEnabled = false;
                        Vector2 size = EditorGUIUtility.GetIconSize();
                        EditorGUIUtility.SetIconSize(Vector2.one * 12);
                        GUIContent content = EditorGUIUtility.IconContent(setting.unityIcon);
                        Debug.unityLogger.logEnabled = true;

                        //An icon is required, draw the default one
                        if (content.image == null)
                        {
                            EditorGUIUtility.SetIconSize(Vector2.one * 16);
                            content = EditorGUIUtility.IconContent("d_UnityEditor.ConsoleWindow");
                        }

                        GUI.Label(iconRect, content);
                        EditorGUIUtility.SetIconSize(size);
                    }
                }

                #endregion

                #region Icon context menu logic
                EditorGUIUtility.AddCursorRect(iconRect, MouseCursor.Link);

                //If we are hovering the icon show the window
                if (iconRect.Contains(Event.current.mousePosition))
                {
                    //Icon click, show context menu
                    if (Event.current.type == EventType.MouseDown)
                    {
                        HoverWindow.CloseMe();
                        GenericMenu menu = new GenericMenu();

                        menu.AddItem(new GUIContent("Read Note"), false, () =>
                        {
                            HoverWindow.Initialize(setting, data.notes[index], data, section, id);
                        });

                        menu.AddSeparator("");

                        menu.AddItem(new GUIContent(data.expandedIndex == -1 ? "Expand" : "Collapse"), false, () =>
                        {
                            data.expandedIndex = data.expandedIndex == -1 ? index : -1;
                            ProjectPrefs.SetString(section, id, JsonUtility.ToJson(data));
                        });
                        menu.AddItem(new GUIContent("Delete"), false, () =>
                        {
                            if (data.notes.Count == 1)
                            {
                                ProjectPrefs.RemoveKey(section, id);
                            }
                            else
                            {
                                data.notes.RemoveAt(index);
                                data.expandedIndex = -1;
                                ProjectPrefs.SetString(section, id, JsonUtility.ToJson(data));
                            }
                        });

                        menu.AddSeparator("");

                        foreach (var note in Settings.notes)
                        {
                            menu.AddItem(new GUIContent("Change Note/" + note.noteName), data.notes.Exists(x => x.id.Equals(note.noteId)), () =>
                            {
                                data.notes[index].id = note.noteId;
                                if (data.notes[index].text.Equals("<- Select Note"))
                                {
                                    data.notes[index].text = "";
                                }
                                ProjectPrefs.SetString(section, id, JsonUtility.ToJson(data));
                            });
                        }

                        menu.ShowAsContext();

                        Event.current.Use();
                    }
                }
                #endregion
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
                width = Mathf.Clamp(width - current.delta.x, 10, rect.width - 20);

                //Save the data
                if (isHierarchy)
                    ProjectPrefs.SetFloat(Constants.SECTION, Constants.ADVANCED_NOTES_HIERARCHY_WIDTH, width);
                else
                    ProjectPrefs.SetFloat(Constants.SECTION, Constants.ADVANCED_NOTES_PROJECT_WIDTH, width);
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
    }
}