using RotaryHeart.Lib.YamlDotNet.Serialization;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

namespace RotaryHeart.Lib.UniNotes
{
    public class HoverWindow : EditorWindow
    {
        static HoverWindow reference;
        const float windowWidth = 300;
        const float windowHeight = 150;
        const string defaultUrl = "Rotary Heart | http://rotaryheart.com";

        UniNotesSettings.NoteSetting setting;
        UniNotesSettings.UniNoteData data;
        bool initialized = false;
        bool isOnEdit = false;
        Vector2 scrollPosition;
        Rect dimension = new Rect();
        Rect scrollContent = new Rect();
        Rect scrollViewRect = new Rect();
        string filePath;
        int noteIndex;
        int urlEdit = -1;
        float height = windowHeight - ((EditorGUIUtility.singleLineHeight + 2) * 2);

        /// <summary>
        /// Define & cache our custom styles for faster access and easier interchangeability
        /// </summary>
        #region Custom Styles
        static Dictionary<string, GUIStyle> styles = null;
        static Dictionary<string, GUIStyle> Styles
        {
            get
            {
                if (styles == null)
                {
                    RectOffset emptyRectOffset = new RectOffset();
                    styles = new Dictionary<string, GUIStyle>();

                    GUIStyle toolbarWithoutPadding = new GUIStyle(EditorStyles.toolbar);
                    toolbarWithoutPadding.padding = emptyRectOffset;
                    styles.Add("toolbar", toolbarWithoutPadding);

                    GUIStyle toolbarButton = new GUIStyle(EditorStyles.toolbarButton);
                    styles.Add("toolbarButton", toolbarButton);

                    GUIStyle toolbarDropdown = new GUIStyle(EditorStyles.toolbarDropDown);
                    styles.Add("toolbarDropdown", toolbarDropdown);

                    GUIStyle noteTextArea = new GUIStyle(EditorStyles.textArea);
                    noteTextArea.richText = true;
                    noteTextArea.normal.background = null;
                    styles.Add("noteTextArea", noteTextArea);

                    GUIStyle linkTextArea = new GUIStyle(EditorStyles.toolbarTextField);
                    linkTextArea.padding = new RectOffset(5, 0, 0, 0);
                    linkTextArea.alignment = TextAnchor.MiddleLeft;
                    styles.Add("linkTextArea", linkTextArea);

                    GUIStyle linkLabel = new GUIStyle(EditorStyles.label);
                    linkLabel.padding = new RectOffset(5, 0, 0, 0);
                    linkLabel.normal.textColor = Color.cyan;
                    styles.Add("linkLabel", linkLabel);

                    GUIStyle closeButton = new GUIStyle(EditorStyles.label);
                    closeButton.padding = new RectOffset(5, 0, 0, 0);
                    styles.Add("closeButton", closeButton);

                }
                return styles;
            }
        }
        #endregion
        /// <summary>
        /// Define & cache our custom labels for faster access and easier interchangeability
        /// </summary>
        #region Custom Labels
        static Dictionary<string, GUIContent> labels = null;
        static Dictionary<string, GUIContent> Labels
        {
            get
            {
                if (labels == null)
                {
                    labels = new Dictionary<string, GUIContent>();

                    GUIContent collapsedNote = new GUIContent("Collapse");
                    labels.Add("collapsedNote", collapsedNote);

                    GUIContent expandedNote = new GUIContent("Expand");
                    labels.Add("expandedNote", expandedNote);

                    GUIContent removeNote = new GUIContent("Remove");
                    labels.Add("removeNote", removeNote);

                    GUIContent selectNote = new GUIContent(" Select Note", EditorGUIUtility.FindTexture("d_UnityEditor.ConsoleWindow"));
                    labels.Add("selectNote", selectNote);

                    GUIContent linkButton = new GUIContent(EditorGUIUtility.FindTexture("d_UnityEditor.ConsoleWindow"));
                    labels.Add("linkButton", linkButton);

                    GUIContent closeButton = new GUIContent(EditorGUIUtility.FindTexture("d_winbtn_win_close_a"));
                    labels.Add("closeButton", closeButton);

                    GUIContent editLocked = new GUIContent(" Edit Note", EditorGUIUtility.FindTexture("LockIcon-On"));
                    labels.Add("noteEditLocked-On", editLocked);

                    GUIContent editUnlocked = new GUIContent(" Edit Note", EditorGUIUtility.FindTexture("LockIcon"));
                    labels.Add("noteEditLocked", editUnlocked);

                    GUIContent addLink = new GUIContent("Add Link");
                    labels.Add("addLink", addLink);
                }
                return labels;
            }
        }
        #endregion

        /// <summary>
        /// Called to initialie the hover window
        /// </summary>
        /// <param name="notePath">Note data path</param>
        /// <param name="noteIndex">Index of note to open</param>
        public static void Initialize(string notePath, int noteIndex)
        {
            UniNotesSettings.UniNoteData data = null;

            using (StreamReader reader = new StreamReader(notePath))
            {
                Deserializer deserializer = new Deserializer();
                data = deserializer.Deserialize<UniNotesSettings.UniNoteData>(reader);
            }

            //Only create the reference if it doesn't exist
            if (reference == null)
            {
                reference = CreateInstance<HoverWindow>();

                //Adjust the dimension rect
                reference.dimension.x = 0;
                reference.dimension.y = 0;
                reference.dimension.width = windowWidth;
                reference.dimension.height = windowHeight;

                //Adjust the scroll content rect
                float textHeight = Styles["noteTextArea"].CalcHeight(new GUIContent(data.notes[noteIndex].text), reference.dimension.width - 15);

                reference.scrollContent = new Rect(0, 0, reference.dimension.width - 15, textHeight < reference.height ? reference.height : textHeight);

                //Rect used by the scroll view
                reference.scrollViewRect = new Rect(reference.dimension.x, EditorGUIUtility.singleLineHeight + 2, reference.dimension.width, reference.dimension.height - (EditorGUIUtility.singleLineHeight + 2) * 2);

                reference.dimension.height += (EditorGUIUtility.singleLineHeight + 2) * data.notes[noteIndex].urls.Count;

                //Show the window
                reference.ShowAsDropDown(reference.dimension, new Vector2(reference.dimension.width, reference.dimension.height));
            }

            //Set the respective references
            reference.filePath = notePath;
            reference.noteIndex = noteIndex;
            AdvancedNoteDrawer.Settings.FindSetting(data.notes[noteIndex].id, out reference.setting);
            reference.data = data;
        }

        private void OnGUI()
        {
            //This is a hack used to adjust the window position, this only needs to be called once
            if (!initialized)
            {
                Rect newPosition = new Rect(Event.current.mousePosition.x - dimension.width / 2, Event.current.mousePosition.y + dimension.height, dimension.width, dimension.height);

                //Check if the window will be positioned outside of the sceen and adjust it
                float offsetX = (newPosition.x + newPosition.width) - (Screen.currentResolution.width - 40);
                float offsetY = (newPosition.y + newPosition.height) - (Screen.currentResolution.height - 40);

                if (offsetX > 0)
                {
                    newPosition.x -= offsetX;
                }
                if (offsetY > 0)
                {
                    newPosition.y -= offsetY;
                }

                position = newPosition;

                initialized = true;
            }

            //Be sure that the settings are not null
            if (setting == null)
            {
                setting = new UniNotesSettings.NoteSetting()
                {
                    backgroundColor = new Color32(0, 0, 0, 0),
                    textColor = Color.white,
                    noteId = ""
                };
            }

            //Draw the background of the window using the settings color
            EditorExtensions.DrawRect(dimension, setting.backgroundColor);

            #region Top toolbar

            EditorGUILayout.BeginHorizontal();

            //Button used to collapse/expand the note
            bool isExpanded = (data.expandedIndex - 1) == noteIndex;

            if (GUILayout.Button(isExpanded ? Labels["collapsedNote"] : Labels["expandedNote"], Styles["toolbarButton"], GUILayout.Width(75)))
            {
                data.expandedIndex = isExpanded ? -1 : noteIndex + 1;
                Save();
            }

            //Dropdown button
            bool showDropDown = false;
            Debug.unityLogger.logEnabled = false;
            GUIContent dropdownContent = string.IsNullOrEmpty(setting.noteId) ? Labels["selectNote"] : new GUIContent(" " + setting.noteName, GetSettingIcon(setting));
            showDropDown = EditorGUILayout.DropdownButton(dropdownContent, FocusType.Keyboard, Styles["toolbarDropdown"], GUILayout.Width(dimension.width - 150));
            Debug.unityLogger.logEnabled = true;

            if (showDropDown)
            {
                //Actual dropdown menu that is only generated if the dropdown button is clicked
                GenericMenu menu = new GenericMenu();

                for (int i = 0; i < AdvancedNoteDrawer.Settings.notes.Count; i++)
                {
                    int index = i;
                    UniNotesSettings.NoteSetting nSetting = AdvancedNoteDrawer.Settings.notes[index];

                    //If any option is selected update the note data
                    menu.AddItem(new GUIContent(" " + nSetting.noteName, EditorGUIUtility.FindTexture(nSetting.unityIcon)), nSetting.noteId.Equals(data.notes[noteIndex].id), () =>
                    {
                        data.notes[noteIndex].id = nSetting.noteId;
                        setting = nSetting;
                        Save();
                    });
                }

                menu.ShowAsContext();
            }

            if (GUILayout.Button(Labels["removeNote"], Styles["toolbarButton"], GUILayout.Width(75)))
            {
                if (data.notes.Count == 1)
                {
                    Delete();
                }
                else
                {
                    data.notes.RemoveAt(noteIndex);
                    Save();
                }

                CloseMe();
                return;
            }

            EditorGUILayout.EndHorizontal();

            #endregion

            #region Note text area
            //Calculate the height the text will be using
            float textHeight = Styles["noteTextArea"].CalcHeight(new GUIContent(data.notes[noteIndex].text), reference.dimension.width - 15);

            scrollContent.height = Mathf.Max(height, textHeight);

            //Reserve the layout space that the scroll view will be using
            GUILayoutUtility.GetRect(scrollViewRect.width, scrollViewRect.height);

            //Resize the window depending on how many urls are available
            maxSize = minSize = new Vector2(maxSize.x, windowHeight + (EditorGUIUtility.singleLineHeight + 2) * (data.notes[noteIndex].urls.Count));

            scrollPosition = GUI.BeginScrollView(scrollViewRect, scrollPosition, scrollContent, false, true);

            EditorGUI.BeginChangeCheck();
            Styles["noteTextArea"].normal.textColor = setting.textColor;

            GUI.SetNextControlName("NoteText");
            //Draw the editor depending on if is currently being edited
            if (isOnEdit)
            {
                data.notes[noteIndex].text = EditorGUI.TextArea(scrollContent, data.notes[noteIndex].text, Styles["noteTextArea"]);
            }
            else
            {
                EditorGUI.LabelField(scrollContent, data.notes[noteIndex].text, Styles["noteTextArea"]);
            }

            //If the note text is changed saved the values
            if (EditorGUI.EndChangeCheck())
            {
                Save();
            }

            GUI.EndScrollView();
            #endregion

            #region Url section

            //Used to save what link needs to be removed
            int linkToRemove = -1;

            //Iterate all the note urls
            for (int i = 0; i < data.notes[noteIndex].urls.Count; i++)
            {
                int index = i;

                EditorGUILayout.BeginHorizontal(Styles["toolbar"]);
                //Button to enable/disable link edit
                if (GUILayout.Button(Labels["linkButton"], Styles["toolbarButton"], GUILayout.Width(26)))
                {
                    // disable note edit
                    isOnEdit = false;

                    if (urlEdit != index)
                        urlEdit = index;
                    else
                        urlEdit = -1;
                }

                //If the current url is the one being edited
                if (urlEdit == index)
                {
                    GUI.SetNextControlName("urlfield" + index);

                    EditorGUI.BeginChangeCheck();
                    data.notes[noteIndex].urls[index] = EditorGUILayout.TextField(data.notes[noteIndex].urls[index], Styles["linkTextArea"], GUILayout.ExpandWidth(true));
                    if (EditorGUI.EndChangeCheck())
                    {
                        Save();
                    }

                    EditorGUI.FocusTextInControl("urlfield" + index);
                }
                //Normal url draw
                else
                {
                    string[] parts = data.notes[noteIndex].urls[index].Split('|');
                    string url = "";
                    string label = "";

                    if (parts.Length == 2)
                    {
                        label = parts[0];
                        url = parts[1];
                    }
                    else
                    {
                        url = label = data.notes[noteIndex].urls[index];
                    }

                    if (GUILayout.Button(label, Styles["linkLabel"]))
                    {
                        Application.OpenURL(url);
                    }

                    // show LINK mouse cursor over button
                    Rect lastRect = GUILayoutUtility.GetLastRect();
                    Repaint();
                    if (Event.current.type == EventType.Repaint && lastRect.Contains(Event.current.mousePosition))
                        EditorGUIUtility.AddCursorRect(lastRect, MouseCursor.Link);
                }

                if (GUILayout.Button(Labels["closeButton"], Styles["closeButton"], GUILayout.Width(28)))
                {
                    linkToRemove = index;
                }
                EditorGUILayout.EndHorizontal();
            }

            //If there is any url to remove, remove it
            if (linkToRemove >= 0)
            {
                data.notes[noteIndex].urls.RemoveAt(linkToRemove);
                EditorGUI.FocusTextInControl("");
                Save();
            }

            #endregion

            #region Bottom toolbar

            EditorGUILayout.BeginHorizontal(Styles["toolbar"]);

            if (GUILayout.Button(Labels["noteEditLocked" + (!isOnEdit ? "-On" : "")], Styles["toolbarButton"]))
            {
                isOnEdit = !isOnEdit;
                if (isOnEdit)
                {
                    urlEdit = -1;
                }
                EditorGUI.FocusTextInControl(isOnEdit ? "NoteText" : "");
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(Labels["addLink"], Styles["toolbarButton"]))
            {
                data.notes[noteIndex].urls.Add(defaultUrl);
                Save();
            }

            EditorGUILayout.EndHorizontal();

            #endregion
        }

        private Texture GetSettingIcon(UniNotesSettings.NoteSetting setting)
        {
            return setting.icon == null ? EditorGUIUtility.FindTexture(setting.unityIcon) : setting.icon;
        }

        private void Delete()
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        private void Save()
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                Serializer serializer = new Serializer();
                serializer.Serialize(writer, data);
            }
            // we need to repaint hierachy or project windows, otherwise expand/ collapse won't be visible directly
            EditorApplication.RepaintHierarchyWindow();
            EditorApplication.RepaintProjectWindow();

            AdvancedNoteDrawer.UpdateNote(filePath, data);
        }

        private void OnDestroy()
        {
            reference = null;
        }

        public static void CloseMe()
        {
            if (reference != null)
            {
                reference.Close();
            }
        }
    }
}