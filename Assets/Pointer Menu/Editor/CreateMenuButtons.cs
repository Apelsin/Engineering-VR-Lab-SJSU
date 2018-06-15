using CVRLabSJSU;
using UnityEditor;

namespace CVRLabSJSUEditor
{
    public class CreateMenuButtons
    {
        [MenuItem("Assets/Create/Menu Buttons")]
        public static void Create()
        {
            EditorUtilities.CreateNewAsset<MultipleChoiceQuizItem>("New Menu Buttons");
        }
    }
}