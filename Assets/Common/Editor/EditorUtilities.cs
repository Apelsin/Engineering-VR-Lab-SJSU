using System.IO;
using UnityEditor;
using UnityEngine;

namespace CVRLabSJSUEditor
{
    public static class EditorUtilities
    {
        private static string GetActivePath()
        {
            EditorUtility.FocusProjectWindow();
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException($"Asset path for active selection object not found\n{path}");
            return path;
        }

        public static void CreateNewAsset<TAsset>(string name)
            where TAsset : ScriptableObject
        {
            CreateNewAsset<TAsset>(name, GetActivePath());
        }

        public static void CreateNewAsset<TAsset>(string name, string path)
            where TAsset : ScriptableObject
        {
            TAsset asset = ScriptableObject.CreateInstance<TAsset>();
            AssetDatabase.CreateAsset(asset, $"{path}/{name}.asset");
            AssetDatabase.SaveAssets();
            Selection.activeObject = asset;
        }
    }
}