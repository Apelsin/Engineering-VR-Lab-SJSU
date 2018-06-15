using UnityEngine;

#if ODIN_INSPECTOR

using Sirenix.OdinInspector;

#endif

namespace CVRLabSJSU
{
    public class MultipleChoiceQuiz : ScriptableObject
    {
        public string Text;
#if ODIN_INSPECTOR

        [InlineEditor]
#endif
        public MultipleChoiceQuizItem[] Items;
    }
}