using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CVRLabSJSU
{
    public interface IQuizLogger
    {
        void LogQuizResult(string session_id, string quiz_id, IReadOnlyDictionary<string, MultipleChoiceQuizItem.Option> choices);
    }
}