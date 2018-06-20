using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

internal static class MCSManager
{
    private static bool ODIN_INSPECTOR
    {
        get
        {
#if ODIN_INSPECTOR
            return true;
#else
            return false;
#endif
        }
    }

    [InitializeOnLoadMethod]
    private static void AddOrRemoveDefines()
    {
        // Odin Inspector
        AddOrRemoveDefine("Sirenix.OdinInspector.Editor", "-define:ODIN_INSPECTOR", ODIN_INSPECTOR);
    }

    private static void AddOrRemoveDefine(string assembly_name_prefix, string mcs_define, bool has_define)
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var add_define = assemblies.Any(x => x.FullName.StartsWith(assembly_name_prefix));

        if (add_define == has_define)
        {
            return;
        }

        var data_path = Application.dataPath;
        var mcs_path = Path.Combine(data_path, "mcs.rsp");
        var has_mcs_file = File.Exists(mcs_path);

        if (add_define)
        {
            var lines = has_mcs_file ? File.ReadAllLines(mcs_path).ToList() : new List<string>();
            if (!lines.Any(x => x.Trim() == mcs_define))
            {
                lines.Add(mcs_define);
                File.WriteAllLines(mcs_path, lines.ToArray());
                AssetDatabase.Refresh();
            }
        }
        else if (has_mcs_file)
        {
            var lines = File.ReadAllLines(mcs_path);
            var lines_without_define = lines.Where(x => x.Trim() != mcs_define).ToArray();

            if (lines_without_define.Length == 0)
            {
                // Optional - Remove the mcs file instead if it doesn't contain any lines.
                File.Delete(mcs_path);
            }
            else
            {
                File.WriteAllLines(mcs_path, lines_without_define);
            }

            AssetDatabase.Refresh();
        }
    }
}