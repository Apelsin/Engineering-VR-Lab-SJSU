using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using RotaryHeart.Lib.YamlDotNet.Serialization;
using System.Collections;

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

        string notesPath;
        string projectPath;
        string hierarchyPath;

        [MenuItem("Window/Rotary Heart/UniNotes")]
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
            if (Directory.Exists(notesPath))
                EditorCoroutines.EditorCoroutines.StartCoroutine(DisplayLoading(), this);
        }

        void OnEnable()
        {
            toolbarContent[0].image = EditorGUIUtility.FindTexture("d_Project");
            toolbarContent[1].image = EditorGUIUtility.FindTexture("UnityEditor.SceneHierarchyWindow");

            notesPath = Constants.NotesPath;
            projectPath = Path.Combine(notesPath, "Project");
            hierarchyPath = Path.Combine(notesPath, "Hierarchy");

            if (!Directory.Exists(notesPath))
            {
                Debug.LogError("Path specified on project prefs not found. Be sure that the folder: '" + notesPath + "' exists");

                if (EditorUtility.DisplayDialog("Notes path not found", "Path specified on preferences not found. Be sure that the folder: '" + notesPath + "' exists. Uni notes will not be able to save notes. Do you want to open the preferences window?", "Yes", "No"))
                {
                    var asm = System.Reflection.Assembly.GetAssembly(typeof(EditorWindow));
                    var T = asm.GetType("UnityEditor.PreferencesWindow");
                    var M = T.GetMethod("ShowPreferencesWindow", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                    M.Invoke(null, null);
                }
            }
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

            GUI.enabled = Directory.Exists(notesPath);

            //Get the correct texture depending on the skin
            Texture refreshTexture = EditorGUIUtility.isProSkin ? EditorGUIUtility.FindTexture("d_RotateTool On") : ((GUIStyle)"Icon.ExtrapolationPingPong").normal.background;

            if (GUILayout.Button(refreshTexture, EditorStyles.toolbarButton, GUILayout.MaxWidth(30)))
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

            GUILayout.FlexibleSpace();
            searchString = GUILayout.TextField(searchString, GUI.skin.FindStyle("ToolbarSeachTextField"), GUILayout.MaxWidth(250));
            if (GUILayout.Button("", string.IsNullOrEmpty(searchString) ? GUI.skin.FindStyle("ToolbarSeachCancelButtonEmpty") : GUI.skin.FindStyle("ToolbarSeachCancelButton")))
            {
                searchString = "";
                GUI.FocusControl(null);
            }

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

            GUI.enabled = true;
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

            //Get the path using the GUID
            //Remove Project Notes
            for (int i = projectNotes.Count - 1; i >= 0; i--)
            {
                var note = projectNotes[i];

                if (!note.exists)
                {
                    //Current note path
                    string filePath = Path.Combine(projectPath, note.key);

                    File.Delete(filePath);

                    projectNotes.RemoveAt(i);
                }
            }

            if (!doScenes)
                return;

            //Remove scene notes
            for (int i = hierarchyNotes.Keys.Count - 1; i >= 0; i--)
            {
                //Get the scene path using the GUID
                string scenePath = Path.Combine(hierarchyPath, hierarchyNotes.Keys.ElementAt(i));

                var section = hierarchyNotes[hierarchyNotes.Keys.ElementAt(i)];

                if (!section.exists)
                {
                    Directory.Delete(scenePath, true);
                    hierarchyNotes.Remove(hierarchyNotes.Keys.ElementAt(i));
                    continue;
                }

                for (int l = section.notes.Count - 1; l >= 0; l--)
                {
                    var note = section.notes[l];

                    if (!note.exists)
                    {
                        //Current note path
                        string filePath = Path.Combine(scenePath, note.key + ".UniNote");

                        File.Delete(filePath);

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

                //Get the path using the GUID
                //Current note path
                string filePath = Path.Combine(projectPath, note.key);

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

                            UniNotesSettings.UniNoteData data = null;

                            using (StreamReader reader = new StreamReader(filePath))
                            {
                                Deserializer deserializer = new Deserializer();
                                data = deserializer.Deserialize<UniNotesSettings.UniNoteData>(reader);
                            }

                            using (StreamWriter writer = new StreamWriter(filePath))
                            {
                                data.notes[index].text = textFieldTmp;

                                Serializer serializer = new Serializer();
                                serializer.Serialize(writer, data);
                            }
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
                            UniNotesSettings.UniNoteData data = null;

                            using (StreamReader reader = new StreamReader(filePath))
                            {
                                Deserializer deserializer = new Deserializer();
                                data = deserializer.Deserialize<UniNotesSettings.UniNoteData>(reader);
                            }

                            if (data.notes.Count == 1)
                            {
                                File.Delete(filePath);
                                projectNotes.Remove(note);
                            }
                            else
                            {
                                using (StreamWriter writer = new StreamWriter(filePath))
                                {
                                    data.notes.RemoveAt(index);

                                    Serializer serializer = new Serializer();
                                    serializer.Serialize(writer, data);
                                }

                                note.notes.RemoveAt(index);
                            }
                        });

                        menu.AddSeparator("");

                        foreach (var noteSetting in AdvancedNoteDrawer.Settings.notes)
                        {
                            menu.AddItem(new GUIContent("Change Note/" + noteSetting.noteName), note.notes.Exists(x => x.setting.noteId.Equals(noteSetting.noteId)), () =>
                            {
                                UniNotesSettings.UniNoteData data = null;

                                using (StreamReader reader = new StreamReader(filePath))
                                {
                                    Deserializer deserializer = new Deserializer();
                                    data = deserializer.Deserialize<UniNotesSettings.UniNoteData>(reader);
                                }

                                using (StreamWriter writer = new StreamWriter(filePath))
                                {
                                    data.notes[index].id = noteSetting.noteId;
                                    AdvancedNoteDrawer.Settings.FindSetting(noteSetting.noteId, out note.notes[index].setting);

                                    Serializer serializer = new Serializer();
                                    serializer.Serialize(writer, data);
                                }
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
        /// Draws the hierarchy notes
        /// </summary>
        void DrawHierarchy()
        {
            int index = 0;
            //Iterate all the sections
            foreach (var section in hierarchyNotes)
            {
                //Get the scene path using the GUID
                string scenePath = Path.Combine(hierarchyPath, section.Key);

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
                content.text = " " + Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(section.Key));
                EditorGUIUtility.SetIconSize(prevSize);

                if (extendedNotes.Count - 1 < index)
                    extendedNotes.Add(false);

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

                        //Current note path
                        string filePath = Path.Combine(scenePath, section.Value.notes[currentIndex].key + ".UniNote");

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

                                    UniNotesSettings.UniNoteData data = null;

                                    using (StreamReader reader = new StreamReader(filePath))
                                    {
                                        Deserializer deserializer = new Deserializer();
                                        data = deserializer.Deserialize<UniNotesSettings.UniNoteData>(reader);
                                    }

                                    using (StreamWriter writer = new StreamWriter(filePath))
                                    {
                                        data.notes[tmpIndex].text = textFieldTmp;

                                        Serializer serializer = new Serializer();
                                        serializer.Serialize(writer, data);
                                    }
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
                                    UniNotesSettings.UniNoteData data = null;

                                    using (StreamReader reader = new StreamReader(filePath))
                                    {
                                        Deserializer deserializer = new Deserializer();
                                        data = deserializer.Deserialize<UniNotesSettings.UniNoteData>(reader);
                                    }

                                    if (data.notes.Count == 1)
                                    {
                                        File.Delete(filePath);

                                        if (hierarchyNotes[section.Key].notes.Count == 1)
                                            hierarchyNotes[section.Key].notes.RemoveAt(currentIndex);
                                        else
                                            hierarchyNotes[section.Key].notes[currentIndex].notes.RemoveAt(tmpIndex);

                                        return;
                                    }
                                    else
                                    {
                                        using (StreamWriter writer = new StreamWriter(filePath))
                                        {
                                            data.notes.RemoveAt(tmpIndex);

                                            Serializer serializer = new Serializer();
                                            serializer.Serialize(writer, data);
                                        }

                                        hierarchyNotes[section.Key].notes[currentIndex].notes.RemoveAt(tmpIndex);
                                    }
                                });

                                menu.AddSeparator("");

                                foreach (var note in AdvancedNoteDrawer.Settings.notes)
                                {
                                    menu.AddItem(new GUIContent("Change Note/" + note.noteName), hierarchyNotes[section.Key].notes[currentIndex].notes.Exists(x => x.setting.noteId.Equals(note.noteId)), () =>
                                    {
                                        UniNotesSettings.UniNoteData data = null;

                                        using (StreamReader reader = new StreamReader(filePath))
                                        {
                                            Deserializer deserializer = new Deserializer();
                                            data = deserializer.Deserialize<UniNotesSettings.UniNoteData>(reader);
                                        }

                                        using (StreamWriter writer = new StreamWriter(filePath))
                                        {
                                            data.notes[tmpIndex].id = note.noteId;
                                            AdvancedNoteDrawer.Settings.FindSetting(note.noteId, out hierarchyNotes[section.Key].notes[currentIndex].notes[tmpIndex].setting);

                                            Serializer serializer = new Serializer();
                                            serializer.Serialize(writer, data);
                                        }
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

                        //Get the path using the GUID
                        //Iterate through the project notes
                        for (int i = projectNotes.Count - 1; i >= 0; i--)
                        {
                            var note = projectNotes[i];

                            //Only delete the selected items
                            if (note.selected)
                            {
                                //Current note path
                                string filePath = Path.Combine(projectPath, note.key);

                                File.Delete(filePath);

                                projectNotes.RemoveAt(i);
                            }
                        }
                        break;

                    case 1:
                        //Iterate through the scenes
                        for (int i = hierarchyNotes.Keys.Count - 1; i >= 0; i--)
                        {
                            var section = hierarchyNotes[hierarchyNotes.Keys.ElementAt(i)];

                            //Get the scene path using the GUID
                            string scenePath = Path.Combine(hierarchyPath, hierarchyNotes.Keys.ElementAt(i));

                            //Iterate through the scene notes
                            for (int l = section.notes.Count - 1; l >= 0; l--)
                            {
                                var note = section.notes[l];

                                //Only delete the selected items
                                if (note.selected)
                                {
                                    //Current note path
                                    string filePath = Path.Combine(scenePath, note.key);

                                    File.Delete(filePath);
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
        IEnumerator DisplayLoading()
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

            List<string> notesData = new List<string>();

            int loadingIndex = 0;
            int currentIndex = 0;
            int count = 0;

            #region Project Notes

            //Add all project notes
            foreach (var sceneFile in Directory.GetFiles(projectPath, "*.UniNote", SearchOption.TopDirectoryOnly))
            {
                notesData.Add(Path.GetFileName(sceneFile));

                count++;
            }

            //Iterate through all the notes
            foreach (var noteDataPath in notesData)
            {
                EditorUtility.DisplayProgressBar("Loading Data", noteDataPath, ((float)currentIndex / (float)count));

                UniNotesSettings.UniNoteData data = null;

                using (StreamReader reader = new StreamReader(Path.Combine(projectPath, noteDataPath)))
                {
                    Deserializer deserializer = new Deserializer();
                    data = deserializer.Deserialize<UniNotesSettings.UniNoteData>(reader);
                }

                //Safe check for the data stored
                if (data == null)
                {
                    continue;
                }

                string correctName = Path.GetFileNameWithoutExtension(noteDataPath);

                string path = AssetDatabase.GUIDToAssetPath(correctName);

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

                projectNotes.Add(new NoteData() { exists = !string.IsNullOrEmpty(path), key = noteDataPath, content = content, notes = currentNotes });

                yield return new WaitForSeconds(0.001f);

                currentIndex++;
            }

            #endregion

            notesData.Clear();
            count = 0;
            loadingIndex = 0;
            currentIndex = 0;

            #region Scene Notes

            //Add all hierarchy notes
            foreach (var sceneFile in Directory.GetDirectories(hierarchyPath, "*", SearchOption.TopDirectoryOnly))
            {
                notesData.Add(sceneFile);

                count += Directory.GetFiles(sceneFile, "*.UniNote", SearchOption.TopDirectoryOnly).Length + 1;
            }

            //Get the component notes GUID
            string componentNotesGUID = "";

            foreach (var componentScript in AssetDatabase.FindAssets("UniNoteComponent"))
            {
                string path = AssetDatabase.GUIDToAssetPath(componentScript);

                if (path.EndsWith("UniNoteComponent.cs") && Path.GetDirectoryName(path).EndsWith("UniNotes"))
                {
                    componentNotesGUID = componentScript;
                    break;
                }
            }

            //Iterate through all the notes
            foreach (var noteDataPath in notesData)
            {
                Dictionary<string, Hashtable> sceneObjects = new Dictionary<string, Hashtable>();

                string scenePath = AssetDatabase.GUIDToAssetPath(Path.GetFileName(notesData[loadingIndex]));

                //Check if the scene file exists
                if (File.Exists(scenePath))
                {
                    string line;

                    //Display the loading dialog since we will be processing the scene file
                    EditorUtility.DisplayProgressBar("Loading Data", "Parsing scene " + Path.GetFileNameWithoutExtension(scenePath), ((float)currentIndex / (float)count));

                    yield return new WaitForSeconds(0.001f);

                    using (StreamReader file = new StreamReader(scenePath))
                    {
                        System.Text.StringBuilder builder = new System.Text.StringBuilder();

                        //Start reading the scene data
                        while ((line = file.ReadLine()) != null)
                        {
                            //Ignore the first 2 lines since they make the yaml parser crash
                            if (line.StartsWith("%YAML") || line.StartsWith("%TAG"))
                                continue;

                            //Replace this text in the line since it makes the parser crash
                            if (line.StartsWith("--- !u!"))
                            {
                                //Need to modify the way the id and type is stored to make the parser work without issues
                                builder.AppendLine("- a_Id: " + line.Substring(line.LastIndexOf("&") + 1));
                                string type = file.ReadLine();
                                type = type.Substring(0, type.Length - 1);
                                builder.AppendLine("  a_Type: " + type);
                            }
                            else
                            {
                                builder.AppendLine(line);
                            }
                        }

                        //Call the yaml parser with the scene data text
                        var deserializer = new Deserializer();
                        var deserializedScene = deserializer.Deserialize<List<Hashtable>>(builder.ToString());

                        //Add all the deserialized objects to the dictionary
                        foreach (var objects in deserializedScene)
                        {
                            sceneObjects.Add(objects["a_Id"].ToString(), objects);
                        }
                    }
                }

                foreach (var notePath in Directory.GetFiles(notesData[loadingIndex], "*.UniNote", SearchOption.TopDirectoryOnly))
                {
                    EditorUtility.DisplayProgressBar("Loading Data", Path.GetFileNameWithoutExtension(notePath), ((float)currentIndex / (float)count));

                    yield return new WaitForSeconds(0.001f);

                    UniNotesSettings.UniNoteData data = null;

                    using (StreamReader reader = new StreamReader(notePath))
                    {
                        Deserializer deserializer = new Deserializer();
                        data = deserializer.Deserialize<UniNotesSettings.UniNoteData>(reader);
                    }

                    //Safe check for the data stored
                    if (data == null)
                    {
                        continue;
                    }

                    SectionData value;

                    if (!hierarchyNotes.TryGetValue(Path.GetFileNameWithoutExtension(notesData[loadingIndex]), out value))
                    {
                        value = new SectionData();
                        value.notes = new List<NoteData>();
                        hierarchyNotes.Add(Path.GetFileNameWithoutExtension(notesData[loadingIndex]), value);
                    }

                    value.exists = File.Exists(scenePath);
                    string correctName = Path.GetFileNameWithoutExtension(notePath);
                    bool contains = false;

                    if (sceneObjects.ContainsKey(Path.GetFileNameWithoutExtension(notePath)))
                    {
                        correctName = sceneObjects[Path.GetFileNameWithoutExtension(notePath)]["m_Name"].ToString();
                        contains = true;
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

                    value.notes.Add(new NoteData() { exists = contains, key = Path.GetFileNameWithoutExtension(notePath), content = new GUIContent(correctName), notes = currentNotes });

                    hierarchyNotes[Path.GetFileNameWithoutExtension(notesData[loadingIndex])] = value;
                    extendedNotes.Add(true);
                }

                SectionData dataValue;

                //Be sure that the current section exists
                if (!hierarchyNotes.TryGetValue(Path.GetFileNameWithoutExtension(notesData[loadingIndex]), out dataValue))
                {
                    dataValue = new SectionData();
                    dataValue.notes = new List<NoteData>();
                    hierarchyNotes.Add(Path.GetFileNameWithoutExtension(notesData[loadingIndex]), dataValue);
                }

                //Iterate all the objects on the scene
                foreach (var sceneObject in sceneObjects)
                {
                    //Only check for GameObjects
                    if (!sceneObject.Value["a_Type"].Equals("GameObject"))
                        continue;

                    //This will be the list of components id that the GO has
                    List<string> myComponentsId = new List<string>();

                    //Iterate the components list
                    foreach (var component in sceneObject.Value["m_Component"] as IList)
                    {
                        //Parse the data to be component: {dictionary of ids}
                        Dictionary<object, object> componentsDict = component as Dictionary<object, object>;

                        //Iterate the parsed data
                        foreach (var components in componentsDict)
                        {
                            //Parse the component ids
                            Dictionary<object, object> fileIds = components.Value as Dictionary<object, object>;

                            //Add all the ids to the list
                            foreach (var fileId in fileIds)
                            {
                                myComponentsId.Add(fileId.Value.ToString());
                            }
                        }
                    }

                    //This will be the list of all the component notes on this GO
                    List<NoteData.Data> componentNotes = new List<NoteData.Data>();
                    //Thee GO id
                    string notesObjectId = "";

                    //Iterate all the components id
                    foreach (var id in myComponentsId)
                    {
                        //Make sure that this component is a script
                        if (sceneObjects[id].ContainsKey("m_Script"))
                        {
                            Dictionary<object, object> componentsDict = sceneObjects[id]["m_Script"] as Dictionary<object, object>;

                            //Make sure that this component guid is the one used by the UniNoteComponent
                            if (componentsDict["guid"].Equals(componentNotesGUID))
                            {
                                //Get the note data
                                Dictionary<object, object> noteData = sceneObjects[id]["myNote"] as Dictionary<object, object>;

                                UniNotesSettings.NoteSetting setting;
                                int settingIndex = AdvancedNoteDrawer.Settings.FindSetting(noteData["noteSettingId"] == null ? "" : noteData["noteSettingId"].ToString(), out setting);

                                if (setting == null)
                                    setting = new UniNotesSettings.NoteSetting() { noteId = "-1" };

                                //Add the note to the list
                                componentNotes.Add(new NoteData.Data() { setting = setting, settingIndex = settingIndex, noteText = noteData["note"].ToString() });
                            }
                        }

                        //If the system found any note be sure to add the gameobject id
                        if (componentNotes.Count > 0 && sceneObjects[id].ContainsKey("m_GameObject"))
                        {
                            Dictionary<object, object> noteData = sceneObjects[id]["m_GameObject"] as Dictionary<object, object>;

                            notesObjectId = noteData["fileID"].ToString();
                        }
                    }

                    //Only execute if the id was found
                    if (!string.IsNullOrEmpty(notesObjectId))
                    {
                        dataValue.exists = true;

                        //Check if this obect has been added by the hierarchy notes search
                        NoteData savedData = dataValue.notes.Where(x => x.key.Equals(notesObjectId)).FirstOrDefault();

                        //Save the notes
                        if (savedData == null)
                        {
                            dataValue.notes.Add(new NoteData() { exists = true, key = notesObjectId, content = new GUIContent(sceneObject.Value["m_Name"].ToString()), notes = componentNotes });
                        }
                        else
                        {
                            savedData.notes.AddRange(componentNotes);
                        }

                        hierarchyNotes[Path.GetFileNameWithoutExtension(notesData[loadingIndex])] = dataValue;
                    }
                }

                loadingIndex++;
            }

            #endregion

            SortNotes();
            EditorUtility.ClearProgressBar();
        }
    }
}