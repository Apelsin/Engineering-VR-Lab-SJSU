using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

internal static class MCSManager
{
    private static void AddOrRemoveDefine(string assembly_name_prefix, string mcs_define)
    {
        var addDefine = AppDomain.CurrentDomain.GetAssemblies().Any(x => x.FullName.StartsWith(assembly_name_prefix));

#if ODIN_INSPECTOR
        var hasDefine = true;
#else
            var hasDefine = false;
#endif

        if (addDefine == hasDefine)
        {
            return;
        }

        var mcsPath = Path.Combine(Application.dataPath, "mcs.rsp");
        var hasMcsFile = File.Exists(mcsPath);

        if (addDefine)
        {
            var lines = hasMcsFile ? File.ReadAllLines(mcsPath).ToList() : new List<string>();
            if (!lines.Any(x => x.Trim() == mcs_define))
            {
                lines.Add(mcs_define);
                File.WriteAllLines(mcsPath, lines.ToArray());
                AssetDatabase.Refresh();
            }
        }
        else if (hasMcsFile)
        {
            var linesWithoutOdinDefine = File.ReadAllLines(mcsPath).Where(x => x.Trim() != mcs_define).ToArray();

            if (linesWithoutOdinDefine.Length == 0)
            {
                // Optional - Remove the mcs file instead if it doesn't contain any lines.
                File.Delete(mcsPath);
            }
            else
            {
                File.WriteAllLines(mcsPath, linesWithoutOdinDefine);
            }

            AssetDatabase.Refresh();
        }
    }

    [InitializeOnLoadMethod]
    private static void AddOrRemoveDefines()
    {
        // Odin Inspector
        AddOrRemoveDefine("Sirenix.OdinInspector.Editor", "-define:ODIN_INSPECTOR");
    }
}