using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using RotaryHeart.Lib.ProjectPreferences;

namespace RotaryHeart.Lib.UniNotes
{
    public class EditorExtensions
    {
        public enum Anchor
        {
            Top, TopRight, Right, BottomRight, Bottom, BottomLeft, Left, TopLeft
        }

        //Used to draw rects with color
        private static readonly Texture2D backgroundTexture = Texture2D.whiteTexture;
        private static readonly GUIStyle textureStyle = new GUIStyle { normal = new GUIStyleState { background = backgroundTexture } };

        static long lastMilliseconds;
        static PropertyInfo inspectorModeInfo = typeof(SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// Draws a rect with a solid color
        /// </summary>
        /// <param name="position">Position to draw the rect</param>
        /// <param name="color">Color to draw the rect</param>
        /// <param name="content">Content, if any</param>
        public static void DrawRect(Rect position, Color color, GUIContent content = null)
        {
            var backgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = color;
            GUI.Box(position, content ?? GUIContent.none, textureStyle);
            GUI.backgroundColor = backgroundColor;
        }

        /// <summary>
        /// Included option on right clicking the component so that the note can be edited
        /// </summary>
        [MenuItem("CONTEXT/UniNoteComponent/Edit Note")]
        private static void EditComment(MenuCommand menuCommand)
        {
            var comment = menuCommand.context as UniNoteComponent;

            comment.myNote.editable = !comment.myNote.editable;
        }

        /// <summary>
        /// Option to add a note on the hierarchy window
        /// </summary>
        [MenuItem("GameObject/UniNotes/Add Note", false, 1)]
        static void HierarchyUniNotes()
        {
            var temp = System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond;

            //Added to prevent it from being called multiple times when multiple objects are selected
            if ((temp - lastMilliseconds) < 150)
                return;

            if (!Directory.Exists(Constants.NotesPath))
            {
                Debug.LogError("Path specified on project prefs not found. Be sure that the folder: '" + Constants.NotesPath + "' exists");
                return;
            }

            foreach (var obj in Selection.objects)
            {
                GameObject go = obj as GameObject;

                if (go == null)
                    continue;

                string id = GetFileId(go).ToString();

                if (id.Equals("0"))
                {
                    if (EditorUtility.DisplayDialog("UniNotes Warning", "The scene needs to be saved before adding notes to a GameObject. Do you want to save it now?", "Yes", "No"))
                    {
                        EditorSceneManager.SaveScene(go.scene);
                        EditorApplication.RepaintHierarchyWindow();
                        HierarchyUniNotes();
                        break;
                    }
                    else
                    {
                        //Didn't save
                        break;
                    }
                }
                else if (!id.Equals("-1"))
                {

                    //Get the scene path using the GUID
                    string scenePath = Path.Combine(Path.Combine(Constants.NotesPath, "Hierarchy"), AssetDatabase.AssetPathToGUID(go.scene.path));
                    //Create any missing directory
                    Directory.CreateDirectory(scenePath);
                    //Current note path
                    string filePath = Path.Combine(scenePath, id + ".UniNote");

                    SerializeData(filePath);
                }
            }

            lastMilliseconds = temp;
        }

        /// <summary>
        /// Option to add a not on the project window
        /// </summary>
        [MenuItem("Assets/UniNotes/Add Note", false, 1)]
        static void ProjectUniNotes()
        {
            if (!Directory.Exists(Constants.NotesPath))
            {
                Debug.LogError("Path specified on project prefs not found. Be sure that the folder: '" + Constants.NotesPath + "' exists");
                return;
            }

            foreach (var id in Selection.assetGUIDs)
            {
                //Get the path using the GUID
                string projectPath = Path.Combine(Constants.NotesPath, "Project");
                //Create any missing directory
                Directory.CreateDirectory(projectPath);
                //Current note path
                string filePath = Path.Combine(projectPath, id + ".UniNote");

                SerializeData(filePath);
            }
        }

        /// <summary>
        /// Function used to serialize a new note
        /// </summary>
        /// <param name="filePath">Path to save the note</param>
        static void SerializeData(string filePath)
        {
            UniNotesSettings.UniNoteData data = null;

            //If the file exists deserialize it
            if (File.Exists(filePath))
            {
                //Deserialize the data
                YamlDotNet.Serialization.Deserializer deserializer = new YamlDotNet.Serialization.Deserializer();
                using (StreamReader reader = new StreamReader(filePath))
                {
                    data = deserializer.Deserialize<UniNotesSettings.UniNoteData>(reader);
                }

                //Add the new note
                data.notes.Add(new UniNotesSettings.UniNoteData.Note()
                {
                    id = "-1",
                    text = "<- Select Note"
                });
            }

            //Be usre that data is not null
            if (data == null)
            {
                data = new UniNotesSettings.UniNoteData()
                {
                    expandedIndex = 1,
                    notes = new System.Collections.Generic.List<UniNotesSettings.UniNoteData.Note>()
                    {
                        new UniNotesSettings.UniNoteData.Note()
                        {
                            id = "-1",
                            text = "<- Select Note"
                        }
                    }
                };
            }

            //Serialize the data
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                YamlDotNet.Serialization.Serializer serializer = new YamlDotNet.Serialization.Serializer();

                serializer.Serialize(writer, data);
            }
        }

        /// <summary>
        /// Returns the FileId of an Object
        /// </summary>
        /// <param name="obj">Object to check</param>
        /// <returns>FileId value</returns>
        public static long GetFileId(Object obj)
        {
            if (obj == null)
                return -1;

            SerializedObject serializedObject = new SerializedObject(obj);
            inspectorModeInfo.SetValue(serializedObject, InspectorMode.Debug, null);

            SerializedProperty localIdProp = serializedObject.FindProperty("m_LocalIdentfierInFile");   //note the misspelling!

            return localIdProp.longValue;
        }
    }
}