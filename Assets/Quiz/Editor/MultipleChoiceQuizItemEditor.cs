using CVRLabSJSU;
using UnityEditor;

namespace CVRLabSJSUEditor
{
    public class MultipleChoiceQuizItemEditor
    {
        [MenuItem("Assets/Create/Quiz/Multiple Choice Quiz Item")]
        public static void Create()
        {
            EditorUtilities.CreateNewAsset<MultipleChoiceQuizItem>("New Quiz Item");
        }
    }
}