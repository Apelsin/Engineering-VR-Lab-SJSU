using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace CVRLabSJSU
{
    public class MCQuizResultsEventArgs : EventArgs
    {
        public readonly string QuizId;
        public readonly IReadOnlyDictionary<string, MultipleChoiceQuizItem.Option> Choices;

        public MCQuizResultsEventArgs(string quiz_id, IReadOnlyDictionary<string, MultipleChoiceQuizItem.Option> choices)
        {
            QuizId = quiz_id;
            Choices = choices;
        }
    }

    [Serializable]
    public class MCQuizResultsEvent : UnityEvent<object, MCQuizResultsEventArgs> { }
}