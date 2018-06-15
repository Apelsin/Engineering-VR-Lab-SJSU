using CVRLabSJSU;
using UnityEditor;

namespace CVRLabSJSUEditor
{
    public class MultipleChoiceQuizEditor
    {
        [MenuItem("Assets/Create/Quiz/Multiple Choice Quiz")]
        public static void Create()
        {
            EditorUtilities.CreateNewAsset<MultipleChoiceQuiz>("New Quiz");
        }
    }
}