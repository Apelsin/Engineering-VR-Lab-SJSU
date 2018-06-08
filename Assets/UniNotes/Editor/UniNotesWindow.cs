using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RotaryHeart.Lib.ProjectPreferences;

namespace RotaryHeart.Lib.UniNotes
{
    public class UniNotesWindow : EditorWindow
    {
        static UniNotesWindow window;

        public class SectionData
        {
            public bool exists;
            public List<NoteData> notes;
        }
        public class NoteData
        {
            public int expandedIndex = -1;
            public bool selected;
            public bool exists;
            public string key;
            public GUIContent content;
            public List<Data> notes;

            public class Data
            {
                public string noteText;
                public UniNotesSettings.NoteSetting setting;
                public int settingIndex;
            }
        }

        List<bool> extendedNotes = new List<bool>();
        List<NoteData> projectNotes = new List<NoteData>();
        Dictionary<string, SectionData> hierarchyNotes = new Dictionary<string, SectionData>();

        GUIContent[] toolbarContent = new GUIContent[] { new GUIContent("Project Notes"), new GUIContent("Hierarchy Notes") };

        GUIContent[] nameContent = new GUIContent[] { new GUIContent("Name ▲"), new GUIContent("Name"), new GUIContent("Name ▼") };

        string searchString = "";
        Vector2 scrollPosition;

        int toolbarSelection = 0;
        int nameIndex = 0;

        [MenuItem("Window/UniNotes")]
        static void Init()
        {
            if (window == null)
            {
                window = (UniNotesWindow)GetWindow(typeof(UniNotesWindow));
                window.titleContent = new GUIContent("UniNotes");
                window.minSize = new Vector2(450, 100);
            }
            window.Show();
        }

        private void Awake()
        {
            EditorCoroutines.EditorCoroutines.StartCoroutine(DisplayLoading(), this);
        }

        void OnEnable()
        {
            toolbarContent[0].image = EditorGUIUtility.FindTexture("d_Project");
            toolbarContent[1].image = EditorGUIUtility.FindTexture("d_UnityEditor.SceneHierarchyWindow");
        }

        void OnDestroy()
        {
            EditorCoroutines.EditorCoroutines.StopAllCoroutines(this);
            EditorUtility.ClearProgressBar();
        }

        void OnGUI()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.Width(position.width), GUILayout.Height(20));

            //Refresh button
            Vector2 lastSize = EditorGUIUtility.GetIconSize();
            EditorGUIUtility.SetIconSize(Vector2.one * 11);
            if (GUILayout.Button(EditorGUIUtility.IconContent("d_RotateTool On"), EditorStyles.toolbarButton, GUILayout.MaxWidth(30)))
            {
                EditorCoroutines.EditorCoroutines.StartCoroutine(DisplayLoading(), this);
            }
            EditorGUIUtility.SetIconSize(lastSize);

            //Purge button
            if (GUILayout.Button("Purge", EditorStyles.toolbarButton, GUILayout.MaxWidth(50)))
            {
                Purge();
            }

            //Options button
            if (GUILayout.Button("Options", EditorStyles.toolbarButton, GUILayout.MaxWidth(50)))
            {
                GenericMenu menu = new GenericMenu();

                if (toolbarSelection == 0)
                {
                    menu.AddItem(new GUIContent("Select"), false, SelectItems);
                    menu.AddSeparator("");
                }
                menu.AddItem(new GUIContent("Delete"), false, DeleteItem);
                menu.ShowAsContext();

                Event.current.Use();
            }

            //Used to make the search field stay to the right
            GUILayout.FlexibleSpace();

            //Serach field
            searchString = GUILayout.TextField(searchString, GUI.skin.FindStyle("ToolbarSeachTextField"), GUILayout.MaxWidth(200));

            //Rect used to draw the end part of the search field
            Rect lastRect = GUILayoutUtility.GetLastRect();
            EditorGUILayout.GetControlRect(GUILayout.Width(20));
            lastRect.x += lastRect.width - 2;
            lastRect.y -= 1;
            lastRect.width = 20;
            lastRect.height = EditorGUIUtility.singleLineHeight + 1;

            //What icon should we draw depending on if something is typed on the search
            string cancelIcon = string.IsNullOrEmpty(searchString) ? "toolbarsearchCancelButtonOff" : "toolbarsearchCancelButton";

            //If its clicked remove the search input
            if (lastRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
            {
                searchString = "";
                Event.current.Use();
            }

            //Draw the actual icon
            EditorGUI.LabelField(lastRect, EditorGUIUtility.IconContent(cancelIcon));

            //End of toolbar
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();
            //Tabs
            toolbarSelection = GUILayout.Toolbar(toolbarSelection, toolbarContent);
            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            //Button for sorting by name
            if (GUILayout.Button(nameContent[nameIndex], EditorStyles.toolbarButton))
            {
                switch (nameIndex)
                {
                    case 0:
                        nameIndex = 2;
                        break;
                    case 1:
                    case 2:
                        nameIndex = 0;
                        break;
                }

                SortNotes();
                Event.current.Use();
            }

            EditorGUILayout.EndHorizontal();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, false, false);

            switch (toolbarSelection)
            {
                case 0:
                    DrawProject();
                    break;

                case 1:
                    DrawHierarchy();
                    break;
            }

            EditorGUILayout.EndScrollView();
        }

        /// <summary>
        /// Erases all the notes that doesn't have a correct reference
        /// </summary>
        void Purge()
        {
            if (!EditorUtility.DisplayDialog("Warning", "The system will delete all notes that didn't find the respective refrence. Do you want to continue?", "Yes", "No"))
            {
                return;
            }

            bool doScenes = true;

            if (EditorSettings.serializationMode != SerializationMode.ForceText)
            {
                if (!EditorUtility.DisplayDialog("Warning", "There might be binary formatted scenes, this will prevent the system from identifying scene UniNotes, to fix this change your editor serialization to ForceText. The system will not purge scenes notes. Do you want to continue?", "Yes", "No"))
                {
                    return;
                }

                doScenes = false;
            }

            //Remove Project Notes
            for (int i = projectNotes.Count - 1; i >= 0; i--)
            {
                var note = projectNotes[i];

                if (!note.exists)
                {
                    ProjectPrefs.RemoveKey("UniNotes_Project", note.key);

                    projectNotes.RemoveAt(i);
                }
            }

            if (!doScenes)
                return;

            //Remove scene notes
            for (int i = hierarchyNotes.Keys.Count - 1; i >= 0; i--)
            {
                var section = hierarchyNotes[hierarchyNotes.Keys.ElementAt(i)];

                for (int l = section.notes.Count - 1; l >= 0; l--)
                {
                    var note = section.notes[l];

                    if (!note.exists)
                    {
                        ProjectPrefs.RemoveKey("UniNotes_Hierarchy:" + hierarchyNotes.Keys.ElementAt(i), note.key);

                        hierarchyNotes[hierarchyNotes.Keys.ElementAt(i)].notes.RemoveAt(l);
                    }
                }
            }

        }

        /// <summary>
        /// Sorts the notes in ascending or descending order
        /// </summary>
        void SortNotes()
        {
            //Loop through all the scenes and sort the notes
            foreach (var section in hierarchyNotes.Keys)
            {
                if (nameIndex == 0)
                {
                    hierarchyNotes[section].notes.Sort((x, y) => x.content.text.Replace(" ", "").CompareTo(y.content.text.Replace(" ", "")));
                }
                else
                {
                    hierarchyNotes[section].notes.Sort((x, y) => y.content.text.Replace(" ", "").CompareTo(x.content.text.Replace(" ", "")));
                }
            }

            //Sort the project notes
            //Sort based on the name of the Object
            if (nameIndex == 0)
            {
                projectNotes.Sort((x, y) => x.content.text.Replace(" ", "").CompareTo(y.content.text.Replace(" ", "")));
            }
            else
            {
                projectNotes.Sort((x, y) => y.content.text.Replace(" ", "").CompareTo(x.content.text.Replace(" ", "")));
            }
        }

        /// <summary>
        /// Draws the hierarchy notes
        /// </summary>
        void DrawHierarchy()
        {
            int index = 0;
            //Iterate all the sections
            foreach (var section in hierarchyNotes)
            {
                Rect rect = EditorGUILayout.BeginVertical();

                //If the reference is missing, draw a red rect
                if (!section.Value.exists)
                {
                    EditorExtensions.DrawRect(rect, Color.red * .75f);
                }

                rect.x += 10;
                rect.width = 20;

                Vector2 prevSize = EditorGUIUtility.GetIconSize();
                EditorGUIUtility.SetIconSize(Vector2.one * 15);
                GUIContent content = EditorGUIUtility.IconContent("SceneAsset Icon");
                content.text = " " + Path.GetFileNameWithoutExtension(section.Key);
                EditorGUIUtility.SetIconSize(prevSize);

                //Draw the scene text with icon
                extendedNotes[index] = EditorGUILayout.Foldout(extendedNotes[index], content, true);

                if (extendedNotes[index])
                {
                    //Iterate all the notes on this scene
                    for (int i = 0; i < section.Value.notes.Count; i++)
                    {
                        int currentIndex = i;

                        //Check if we should filter the notes
                        if (!string.IsNullOrEmpty(searchString) && !section.Value.notes[currentIndex].content.text.ToLower().Contains(searchString.ToLower()))
                        {
                            continue;
                        }

                        Rect noteRect = EditorGUILayout.BeginHorizontal();

                        //If the reference is missing, draw a red rect
                        if (!section.Value.notes[currentIndex].exists)
                        {
                            EditorExtensions.DrawRect(noteRect, Color.red * .75f);
                        }

                        //Draw a rect for selection
                        if (section.Value.notes[currentIndex].selected)
                        {
                            EditorExtensions.DrawRect(noteRect, new Color(30f / 255f, 144f / 255f, 1, 0.2f));
                        }

                        //Used to hold a space (like indent)
                        EditorGUILayout.GetControlRect(GUILayout.Width(10));
                        //Draw the note info
                        hierarchyNotes[section.Key].notes[currentIndex].selected = EditorGUILayout.ToggleLeft(section.Value.notes[currentIndex].content, section.Value.notes[currentIndex].selected, GUILayout.Width(position.width / 2 - 25));

                        Rect elementRect = EditorGUILayout.GetControlRect();

                        for (int noteIndex = hierarchyNotes[section.Key].notes[currentIndex].notes.Count - 1; noteIndex >= 0; noteIndex--)
                        {
                            var tmpIndex = noteIndex;

                            //If a note is expanded skip all but the expanded one
                            if (hierarchyNotes[section.Key].notes[currentIndex].expandedIndex != -1 && noteIndex != hierarchyNotes[section.Key].notes[currentIndex].expandedIndex)
                                continue;

                            UniNotesSettings.NoteSetting setting = hierarchyNotes[section.Key].notes[currentIndex].notes[noteIndex].setting;

                            Rect iconRect;

                            string textFieldVal = section.Value.notes[currentIndex].notes[noteIndex].noteText;

                            if (hierarchyNotes[section.Key].notes[currentIndex].expandedIndex != -1)
                            {
                                iconRect = new Rect(elementRect.x, elementRect.y, 18, 18);

                                GUIContent iconContent;
                                Vector2 size = EditorGUIUtility.GetIconSize();

                                if (setting != null)
                                {
                                    //Draw the background
                                    EditorExtensions.DrawRect(new Rect(elementRect.x + 20, elementRect.y, elementRect.width - 35, elementRect.height), setting.backgroundColor);

                                    //Draw the icon
                                    if (setting.icon != null)
                                    {
                                        GUI.Label(iconRect, setting.icon);
                                    }
                                    else
                                    {
                                        Debug.unityLogger.logEnabled = false;
                                        EditorGUIUtility.SetIconSize(Vector2.one * 12);
                                        iconContent = EditorGUIUtility.IconContent(setting.unityIcon);
                                        Debug.unityLogger.logEnabled = true;

                                        //An icon is required, draw the default one
                                        if (iconContent.image == null)
                                        {
                                            EditorGUIUtility.SetIconSize(Vector2.one * 16);
                                            iconContent = EditorGUIUtility.IconContent("d_UnityEditor.ConsoleWindow");
                                        }

                                        iconContent.tooltip = textFieldVal;
                                        GUI.Label(iconRect, iconContent);
                                        EditorGUIUtility.SetIconSize(size);
                                    }
                                }
                                else
                                {
                                    EditorGUIUtility.SetIconSize(Vector2.one * 16);
                                    iconContent = EditorGUIUtility.IconContent("d_UnityEditor.ConsoleWindow");

                                    iconContent.tooltip = textFieldVal;
                                    GUI.Label(iconRect, iconContent);
                                    EditorGUIUtility.SetIconSize(size);
                                }

                                GUIStyle style = new GUIStyle(GUI.skin.label);

                                if (setting != null)
                                {
                                    style.normal.textColor = setting.textColor;
                                }

                                //Draw the text field
                                string textFieldTmp = EditorGUI.TextField(new Rect(elementRect.x + 20, elementRect.y, elementRect.width - 35, elementRect.height), textFieldVal, style);

                                //If something has been changed, save it
                                if (!textFieldTmp.Equals(textFieldVal))
                                {
                                    var temp = hierarchyNotes[section.Key].notes[currentIndex].notes[noteIndex];

                                    temp.noteText = textFieldTmp;

                                    hierarchyNotes[section.Key].notes[currentIndex].notes[noteIndex] = temp;

                                    UniNotesSettings.UniNoteData data = JsonUtility.FromJson<UniNotesSettings.UniNoteData>(ProjectPrefs.GetString("UniNotes_Hierarchy:" + section.Key, section.Value.notes[currentIndex].key));
                                    data.notes[tmpIndex].text = textFieldTmp;
                                    ProjectPrefs.SetString("UniNotes_Hierarchy:" + section.Key, section.Value.notes[currentIndex].key, JsonUtility.ToJson(data));
                                }
                            }
                            else
                            {
                                iconRect = new Rect(elementRect.x + elementRect.width - 19 - (elementRect.height * noteIndex), elementRect.y + 1, elementRect.height, elementRect.height);

                                //If the drawer is above any icon, skip it
                                if (elementRect.x > iconRect.x)
                                    continue;

                                GUIContent iconContent;
                                Vector2 size = EditorGUIUtility.GetIconSize();

                                //Draw the icon
                                if (setting != null)
                                {
                                    if (setting.icon != null)
                                    {
                                        GUI.Label(iconRect, setting.icon);
                                    }
                                    else
                                    {
                                        Debug.unityLogger.logEnabled = false;
                                        EditorGUIUtility.SetIconSize(Vector2.one * 12);
                                        iconContent = EditorGUIUtility.IconContent(setting.unityIcon);
                                        Debug.unityLogger.logEnabled = true;

                                        //An icon is required, draw the default one
                                        if (iconContent.image == null)
                                        {
                                            EditorGUIUtility.SetIconSize(Vector2.one * 16);
                                            iconContent = EditorGUIUtility.IconContent("d_UnityEditor.ConsoleWindow");
                                        }

                                        iconContent.tooltip = textFieldVal;
                                        GUI.Label(iconRect, iconContent);
                                        EditorGUIUtility.SetIconSize(size);
                                    }
                                }
                                else
                                {
                                    EditorGUIUtility.SetIconSize(Vector2.one * 16);
                                    iconContent = EditorGUIUtility.IconContent("d_UnityEditor.ConsoleWindow");

                                    iconContent.tooltip = textFieldVal;
                                    GUI.Label(iconRect, iconContent);
                                    EditorGUIUtility.SetIconSize(size);
                                }
                            }

                            #region Icon context menu logic
                            EditorGUIUtility.AddCursorRect(iconRect, MouseCursor.Link);

                            if (iconRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
                            {
                                GenericMenu menu = new GenericMenu();

                                menu.AddItem(new GUIContent(hierarchyNotes[section.Key].notes[currentIndex].expandedIndex == -1 ? "Expand" : "Collapse"), false, () =>
                                {
                                    hierarchyNotes[section.Key].notes[currentIndex].expandedIndex = hierarchyNotes[section.Key].notes[currentIndex].expandedIndex == -1 ? tmpIndex : -1;
                                });
                                menu.AddItem(new GUIContent("Delete"), false, () =>
                                {
                                    if (hierarchyNotes[section.Key].notes[currentIndex].notes.Count == 1)
                                    {
                                        ProjectPrefs.RemoveKey("UniNotes_Hierarchy:" + section.Key, hierarchyNotes[section.Key].notes[currentIndex].key);
                                        hierarchyNotes[section.Key].notes.RemoveAt(currentIndex);
                                    }
                                    else
                                    {
                                        UniNotesSettings.UniNoteData data = JsonUtility.FromJson<UniNotesSettings.UniNoteData>(ProjectPrefs.GetString("UniNotes_Hierarchy:" + section.Key, hierarchyNotes[section.Key].notes[currentIndex].key));
                                        data.notes.RemoveAt(tmpIndex);
                                        ProjectPrefs.SetString("UniNotes_Hierarchy:" + section.Key, hierarchyNotes[section.Key].notes[currentIndex].key, JsonUtility.ToJson(data));
                                        hierarchyNotes[section.Key].notes[currentIndex].notes.RemoveAt(tmpIndex);
                                    }
                                });

                                menu.AddSeparator("");

                                foreach (var note in AdvancedNoteDrawer.Settings.notes)
                                {
                                    menu.AddItem(new GUIContent("Change Note/" + note.noteName), hierarchyNotes[section.Key].notes[currentIndex].notes.Exists(x => x.setting.noteId.Equals(note.noteId)), () =>
                                    {
                                        UniNotesSettings.UniNoteData data = JsonUtility.FromJson<UniNotesSettings.UniNoteData>(ProjectPrefs.GetString("UniNotes_Hierarchy:" + section.Key, hierarchyNotes[section.Key].notes[currentIndex].key));
                                        data.notes[tmpIndex].id = note.noteId;
                                        AdvancedNoteDrawer.Settings.FindSetting(note.noteId, out hierarchyNotes[section.Key].notes[currentIndex].notes[tmpIndex].setting);
                                        ProjectPrefs.SetString("UniNotes_Hierarchy:" + section.Key, hierarchyNotes[section.Key].notes[currentIndex].key, JsonUtility.ToJson(data));
                                    });
                                }

                                menu.ShowAsContext();

                                Event.current.Use();
                            }
                            #endregion

                        }
                        EditorGUILayout.EndHorizontal();
                    }
                }

                EditorGUILayout.EndVertical();
                index++;
            }
        }

        /// <summary>
        /// Draws the project notes
        /// </summary>
        void DrawProject()
        {
            //Iterate through all the project notes
            foreach (var note in projectNotes)
            {
                //Check if we should filter the notes
                if (!string.IsNullOrEmpty(searchString) && !note.content.text.ToLower().Contains(searchString.ToLower()))
                {
                    continue;
                }

                Rect noteRect = EditorGUILayout.BeginHorizontal();

                //If the reference is missing, draw a red rect
                if (!note.exists)
                {
                    EditorExtensions.DrawRect(noteRect, Color.red * .75f);
                }

                //Draw a rect for selection
                if (note.selected)
                {
                    EditorExtensions.DrawRect(noteRect, new Color(30f / 255f, 144f / 255f, 1, 0.2f));
                }

                //Draw the note info
                note.selected = EditorGUILayout.ToggleLeft(note.content, note.selected, GUILayout.Width(position.width / 2 - 10));

                Rect elementRect = EditorGUILayout.GetControlRect();

                for (int i = note.notes.Count - 1; i >= 0; i--)
                {
                    int index = i;

                    //If a note is expanded skip all but the expanded one
                    if (note.expandedIndex != -1 && index != note.expandedIndex)
                        continue;

                    UniNotesSettings.NoteSetting setting = note.notes[index].setting;

                    Rect iconRect;

                    string textFieldVal = note.notes[index].noteText;

                    if (note.expandedIndex != -1)
                    {
                        iconRect = new Rect(elementRect.x, elementRect.y, 18, 18);

                        GUIContent iconContent;
                        Vector2 size = EditorGUIUtility.GetIconSize();

                        if (setting != null)
                        {
                            //Draw the background
                            EditorExtensions.DrawRect(new Rect(elementRect.x + 20, elementRect.y, elementRect.width - 35, elementRect.height), setting.backgroundColor);
                            //Draw the icon
                            if (setting.icon != null)
                            {
                                GUI.Label(iconRect, setting.icon);
                            }
                            else
                            {
                                Debug.unityLogger.logEnabled = false;
                                EditorGUIUtility.SetIconSize(Vector2.one * 12);
                                iconContent = EditorGUIUtility.IconContent(setting.unityIcon);
                                Debug.unityLogger.logEnabled = true;

                                //An icon is required, draw the default one
                                if (iconContent.image == null)
                                {
                                    EditorGUIUtility.SetIconSize(Vector2.one * 16);
                                    iconContent = EditorGUIUtility.IconContent("d_UnityEditor.ConsoleWindow");
                                }

                                iconContent.tooltip = textFieldVal;
                                GUI.Label(iconRect, iconContent);
                                EditorGUIUtility.SetIconSize(size);
                            }
                        }
                        else
                        {
                            EditorGUIUtility.SetIconSize(Vector2.one * 16);
                            iconContent = EditorGUIUtility.IconContent("d_UnityEditor.ConsoleWindow");

                            iconContent.tooltip = textFieldVal;
                            GUI.Label(iconRect, iconContent);
                            EditorGUIUtility.SetIconSize(size);
                        }

                        GUIStyle style = new GUIStyle(GUI.skin.label);
                        if (setting != null)
                        {
                            style.normal.textColor = setting.textColor;
                        }

                        //Draw the text field
                        string textFieldTmp = EditorGUI.TextField(new Rect(elementRect.x + 20, elementRect.y, elementRect.width - 35, elementRect.height), textFieldVal, style);

                        //If something has been changed, save it
                        if (!textFieldTmp.Equals(textFieldVal))
                        {
                            note.notes[index].noteText = textFieldTmp;

                            UniNotesSettings.UniNoteData data = JsonUtility.FromJson<UniNotesSettings.UniNoteData>(ProjectPrefs.GetString("UniNotes_Project", note.key));
                            data.notes[index].text = textFieldTmp;
                            ProjectPrefs.SetString("UniNotes_Project", note.key, JsonUtility.ToJson(data));
                        }
                    }
                    else
                    {
                        iconRect = new Rect(elementRect.x + elementRect.width - 19 - (elementRect.height * index), elementRect.y + 1, elementRect.height, elementRect.height);

                        //If the drawer is above any icon, skip it
                        if (elementRect.x > iconRect.x)
                            continue;

                        GUIContent iconContent;
                        Vector2 size = EditorGUIUtility.GetIconSize();

                        if (setting != null)
                        {
                            //Draw the icon
                            if (setting.icon != null)
                            {
                                GUI.Label(iconRect, setting.icon);
                            }
                            else
                            {
                                Debug.unityLogger.logEnabled = false;
                                EditorGUIUtility.SetIconSize(Vector2.one * 12);
                                iconContent = EditorGUIUtility.IconContent(setting.unityIcon);
                                Debug.unityLogger.logEnabled = true;

                                //An icon is required, draw the default one
                                if (iconContent.image == null)
                                {
                                    EditorGUIUtility.SetIconSize(Vector2.one * 16);
                                    iconContent = EditorGUIUtility.IconContent("d_UnityEditor.ConsoleWindow");
                                }

                                iconContent.tooltip = textFieldVal;
                                GUI.Label(iconRect, iconContent);
                                EditorGUIUtility.SetIconSize(size);
                            }
                        }
                        else
                        {
                            EditorGUIUtility.SetIconSize(Vector2.one * 16);
                            iconContent = EditorGUIUtility.IconContent("d_UnityEditor.ConsoleWindow");

                            iconContent.tooltip = textFieldVal;
                            GUI.Label(iconRect, iconContent);
                            EditorGUIUtility.SetIconSize(size);
                        }
                    }

                    #region Icon context menu logic
                    EditorGUIUtility.AddCursorRect(iconRect, MouseCursor.Link);

                    if (iconRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
                    {
                        GenericMenu menu = new GenericMenu();

                        menu.AddItem(new GUIContent(note.expandedIndex == -1 ? "Expand" : "Collapse"), false, () =>
                        {
                            note.expandedIndex = note.expandedIndex == -1 ? index : -1;
                        });
                        menu.AddItem(new GUIContent("Delete"), false, () =>
                        {
                            if (note.notes.Count == 1)
                            {
                                ProjectPrefs.RemoveKey("UniNotes_Project", note.key);
                                projectNotes.Remove(note);
                            }
                            else
                            {
                                UniNotesSettings.UniNoteData data = JsonUtility.FromJson<UniNotesSettings.UniNoteData>(ProjectPrefs.GetString("UniNotes_Project", note.key));
                                data.notes.RemoveAt(index);
                                ProjectPrefs.SetString("UniNotes_Project", note.key, JsonUtility.ToJson(data));
                                note.notes.RemoveAt(index);
                            }
                        });

                        menu.AddSeparator("");

                        foreach (var noteSetting in AdvancedNoteDrawer.Settings.notes)
                        {
                            menu.AddItem(new GUIContent("Change Note/" + noteSetting.noteName), note.notes.Exists(x => x.setting.noteId.Equals(noteSetting.noteId)), () =>
                            {
                                UniNotesSettings.UniNoteData data = JsonUtility.FromJson<UniNotesSettings.UniNoteData>(ProjectPrefs.GetString("UniNotes_Project", note.key));
                                data.notes[index].id = noteSetting.noteId;
                                AdvancedNoteDrawer.Settings.FindSetting(noteSetting.noteId, out note.notes[index].setting);
                                ProjectPrefs.SetString("UniNotes_Project", note.key, JsonUtility.ToJson(data));
                            });
                        }

                        menu.ShowAsContext();

                        Event.current.Use();
                    }
                    #endregion

                }

                EditorGUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// Called when the select item option is clicked
        /// </summary>
        void SelectItems()
        {
            List<Object> test = new List<Object>();

            //Iterate through all the notes
            for (int i = 0; i < projectNotes.Count; i++)
            {
                //Only if the note is selected
                if (projectNotes[i].selected)
                {
                    Object obj = AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(projectNotes[i].key), typeof(Object));
                    test.Add(obj);

                    //Flash the folder yellow to highlight it
                    EditorGUIUtility.PingObject(obj);
                }
            }

            //Select the object in the project folder
            Selection.objects = test.ToArray();
        }

        /// <summary>
        /// Called when the delete item option is clicked
        /// </summary>
        void DeleteItem()
        {
            if (EditorUtility.DisplayDialog("Delete Notes?", "Are you sure you want to delete the selected notes?", "Yes", "Cancel"))
            {
                //Only delete depending on what tab we are on
                switch (toolbarSelection)
                {
                    case 0:

                        //Iterate through the project notes
                        for (int i = projectNotes.Count - 1; i >= 0; i--)
                        {
                            var note = projectNotes[i];

                            //Only delete the selected items
                            if (note.selected)
                            {
                                ProjectPrefs.RemoveKey("UniNotes_Project", note.key);

                                projectNotes.RemoveAt(i);
                            }
                        }
                        break;

                    case 1:
                        //Iterate through the scenes
                        for (int i = hierarchyNotes.Keys.Count - 1; i >= 0; i--)
                        {
                            var section = hierarchyNotes[hierarchyNotes.Keys.ElementAt(i)];

                            //Iterate through the scene notes
                            for (int l = section.notes.Count - 1; l >= 0; l--)
                            {
                                var note = section.notes[l];

                                //Only delete the selected items
                                if (note.selected)
                                {
                                    ProjectPrefs.RemoveKey("UniNotes_Hierarchy:" + hierarchyNotes.Keys.ElementAt(i), note.key);

                                    hierarchyNotes[hierarchyNotes.Keys.ElementAt(i)].notes.RemoveAt(l);
                                }
                            }
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Used to display the loading while the system reads all the ntoes
        /// </summary>
        System.Collections.IEnumerator DisplayLoading()
        {
            yield return new WaitForSeconds(0.001f);

            //Warning for serialization mode
            if (EditorSettings.serializationMode != SerializationMode.ForceText)
            {
                if (EditorUtility.DisplayDialog("Warning", "There might be binary formatted scenes, this will prevent the system from identifying scene UniNotes. Do you want to change the serialization mode to text?", "Yes", "No"))
                {
                    EditorSettings.serializationMode = SerializationMode.ForceText;
                }
            }

            extendedNotes.Clear();
            projectNotes.Clear();
            hierarchyNotes.Clear();

            string[] projectPrefSections = ProjectPrefs.GetSections();
            List<string> sections = new List<string>(projectPrefSections.Length);

            int loadingIndex = 0;
            int currentIndex = 0;
            int count = 0;

            //Add all the scene notes
            foreach (var section in projectPrefSections)
            {
                if (section.StartsWith("UniNotes_Hierarchy:") || section.StartsWith("UniNotes_Project"))
                {
                    sections.Add(section);

                    count += ProjectPrefs.GetKeys(section).Length;
                }
            }

            //Load all the notes
            while (loadingIndex < sections.Count)
            {
                Dictionary<string, string> correctNames = new Dictionary<string, string>();

                string filePath = Application.dataPath + "/../" + sections[loadingIndex].Replace("UniNotes_Hierarchy:", "");
                if (File.Exists(filePath))
                {
                    string line;

                    EditorUtility.DisplayProgressBar("Loading Data", "Parsing scene " + sections[loadingIndex].Replace("UniNotes_Hierarchy:", ""), ((float)currentIndex / (float)count));

                    //Get the correct name from the scene file
                    using (StreamReader file = new StreamReader(filePath))
                    {
                        while ((line = file.ReadLine()) != null)
                        {
                            if (line.StartsWith("--- !u!1 &") && file.ReadLine().Equals("GameObject:"))
                            {
                                string key = line.Replace("--- !u!1 &", "");
                                while (!(line = file.ReadLine()).Contains("m_Name:")) ;

                                correctNames.Add(key, line.Replace(" m_Name: ", ""));
                            }
                        }
                    }
                }

                //Iterate through all the notes on this section
                foreach (var key in ProjectPrefs.GetKeys(sections[loadingIndex]))
                {
                    EditorUtility.DisplayProgressBar("Loading Data", key, ((float)currentIndex / (float)count));

                    UniNotesSettings.UniNoteData data = JsonUtility.FromJson<UniNotesSettings.UniNoteData>(ProjectPrefs.GetString(sections[loadingIndex], key));

                    //Safe check for the data stored
                    if (data == null)
                    {
                        Debug.LogError("Data not saved correctly for: " + key + " in section: " + sections[loadingIndex]);
                        continue;
                    }

                    //Special check for project notes
                    if (sections[loadingIndex].Equals("UniNotes_Project"))
                    {
                        string correctName = key;

                        string path = AssetDatabase.GUIDToAssetPath(key);

                        //Get the correct name from the GUID
                        if (!string.IsNullOrEmpty(path))
                        {
                            correctName = Path.GetFileNameWithoutExtension(path);
                        }

                        GUIContent content = new GUIContent();
                        content.image = AssetDatabase.GetCachedIcon(path);
                        content.text = correctName;

                        List<NoteData.Data> currentNotes = new List<NoteData.Data>();
                        foreach (var note in data.notes)
                        {
                            //Get the current NoteSetting
                            UniNotesSettings.NoteSetting setting;
                            int settingIndex = AdvancedNoteDrawer.Settings.FindSetting(note.id, out setting);

                            if (setting == null)
                                setting = new UniNotesSettings.NoteSetting() { noteId = "-1" };

                            currentNotes.Add(new NoteData.Data() { setting = setting, settingIndex = settingIndex, noteText = note.text });
                        }

                        projectNotes.Add(new NoteData() { exists = !string.IsNullOrEmpty(path), key = key, content = content, notes = currentNotes });
                    }
                    else
                    {
                        string section = sections[loadingIndex].Replace("UniNotes_Hierarchy:", "");

                        SectionData value;

                        if (!hierarchyNotes.TryGetValue(section, out value))
                        {
                            value = new SectionData();
                            value.notes = new List<NoteData>();
                            hierarchyNotes.Add(section, value);
                        }

                        value.exists = File.Exists(Application.dataPath + "/../" + section);
                        string correctName;
                        bool contains = true;

                        if (!correctNames.TryGetValue(key, out correctName))
                        {
                            correctName = key;
                            contains = false;
                        }

                        List<NoteData.Data> currentNotes = new List<NoteData.Data>();
                        foreach (var note in data.notes)
                        {
                            //Get the current NoteSetting
                            UniNotesSettings.NoteSetting setting;
                            int settingIndex = AdvancedNoteDrawer.Settings.FindSetting(note.id, out setting);

                            if (setting == null)
                                setting = new UniNotesSettings.NoteSetting() { noteId = "-1" };

                            currentNotes.Add(new NoteData.Data() { setting = setting, settingIndex = settingIndex, noteText = note.text });
                        }

                        value.notes.Add(new NoteData() { exists = contains, key = key, content = new GUIContent(correctName), notes = currentNotes });

                        hierarchyNotes[section] = value;
                        extendedNotes.Add(true);
                    }

                    currentIndex++;

                    yield return new WaitForSeconds(0.001f);
                }

                loadingIndex++;
                currentIndex++;

                yield return new WaitForSeconds(0.001f);
            }

            SortNotes();
            EditorUtility.ClearProgressBar();
        }
    }
}