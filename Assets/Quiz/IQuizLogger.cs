using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CVRLabSJSU
{
    public interface IQuizLogger
    {
        void LogQuizResult(string result_id, IReadOnlyDictionary<string, MultipleChoiceQuizItem.Option> choices);
    }
}