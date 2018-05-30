using System.IO;
using UnityEditor;
using UnityEngine;

public static class MakeMenuButtons
{
    [MenuItem("Assets/Create/Menu Buttons")]
    public static void CreateMyAsset()
    {
        EditorUtility.FocusProjectWindow();
        var path = AssetDatabase.GetAssetPath(Selection.activeObject);
        Debug.Log(path);
        if(Directory.Exists(path))
        {
            MenuButtons asset = ScriptableObject.CreateInstance<MenuButtons>();
            AssetDatabase.CreateAsset(asset, $"{path}/New Menu Buttons.asset");
            AssetDatabase.SaveAssets();
            Selection.activeObject = asset;
        }
    }
}