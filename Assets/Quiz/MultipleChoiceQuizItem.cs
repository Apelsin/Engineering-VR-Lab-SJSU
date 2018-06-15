using System;
using UnityEngine;

#if ODIN_INSPECTOR

using Sirenix.OdinInspector;

#endif

namespace CVRLabSJSU
{
    public class MultipleChoiceQuizItem : ScriptableObject
    {
        [Serializable]
        public struct Option
        {
            public string Id;
            public string Text;
            public bool IsCorrect;
        }
        [Serializable]
        public struct Image
        {
            public string Id;
            public Texture Texture;
            public string Caption;
        }

        public string Id;
        public string Text;
        public Option[] Options;
        public Image[] Images;
        //public MultipleChoiceQuizItem[] Related;
    }
}