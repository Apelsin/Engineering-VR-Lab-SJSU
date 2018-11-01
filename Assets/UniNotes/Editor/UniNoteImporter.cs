using System.IO;
using UnityEngine;
using UnityEditor;
using RotaryHeart.Lib.ProjectPreferences;
using RotaryHeart.Lib.YamlDotNet.Serialization;

namespace RotaryHeart.Lib.UniNotes
{
    public static class UniNoteImporter
    {
        public static void CheckImport()
        {
            string[] sections = ProjectPrefs.GetSections();

            bool doImport = false;

            foreach (var section in sections)
            {
                if (section.StartsWith("UniNotes_Project") || section.StartsWith("UniNotes_Hierarchy:"))
                {
                    if (EditorUtility.DisplayDialog("UniNotes save change", "The system for saving advanced note has changed, you seem to have old notes data on your project preferences. Do you want to import them to the new system? Note that notes will not be drawn unless they are using the new system.", "Yes", "No"))
                    {
                        doImport = true;
                    }

                    break;
                }
            }

            if (doImport)
            {
                //Get the path using the GUID
                string projectPath = Path.Combine(Constants.NotesPath, "Project");

                foreach (var section in sections)
                {
                    if (section.StartsWith("UniNotes_Project"))
                    {
                        foreach (var key in ProjectPrefs.GetKeys(section))
                        {
                            UniNotesSettings.UniNoteData data = JsonUtility.FromJson<UniNotesSettings.UniNoteData>(ProjectPrefs.GetString(section, key));

                            string path = AssetDatabase.GUIDToAssetPath(key);

                            if (!string.IsNullOrEmpty(path))
                            {
                                //Current note path
                                string filePath = Path.Combine(projectPath, key + ".UniNote");

                                using (StreamWriter writer = new StreamWriter(filePath))
                                {
                                    Serializer serializer = new Serializer();
                                    serializer.Serialize(writer, data);
                                }
                            }
                        }
                    }
                    else if (section.StartsWith("UniNotes_Hierarchy:"))
                    {

                        string sceneGUID = AssetDatabase.AssetPathToGUID(section.Replace("UniNotes_Hierarchy:", ""));
                        //Get the path using the GUID
                        string scenePath = Path.Combine(Path.Combine(Constants.NotesPath, "Hierarchy"), sceneGUID);

                        foreach (var key in ProjectPrefs.GetKeys(section))
                        {
                            if (string.IsNullOrEmpty(key))
                                continue;

                            UniNotesSettings.UniNoteData data = JsonUtility.FromJson<UniNotesSettings.UniNoteData>(ProjectPrefs.GetString(section, key));

                            if (data == null)
                                continue;

                            if (data.expandedIndex != -1)
                                data.expandedIndex++;

                            //Current note path
                            string filePath = Path.Combine(scenePath, key + ".UniNote");

                            using (StreamWriter writer = new StreamWriter(filePath))
                            {
                                Serializer serializer = new Serializer();
                                serializer.Serialize(writer, data);
                            }
                        }
                    }
                    else
                    {
                        continue;
                    }

                    ProjectPrefs.RemoveSection(section);
                }
            }
        }
    }
}