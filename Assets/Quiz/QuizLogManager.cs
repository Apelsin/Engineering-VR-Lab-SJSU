using System;
using System.Collections.Generic;
using UnityEngine;

namespace CVRLabSJSU
{
    public class QuizLogManager : MonoBehaviour
    {
        public static string GetShortUID()
        {
            return Guid.NewGuid().ToString().Substring(0, 8);
        }

        /// <summary>
        /// GUID that is the same for the application's life cycle
        /// </summary>
        public static readonly string ProcessSessionId = GetShortUID();

        //public string SessionId; // TODO
        public void OnLogQuizResult(string quiz_id, IReadOnlyDictionary<string, MultipleChoiceQuizItem.Option> choices)
        {
            var loggers = GetComponents<IQuizLogger>();
            var session_id = $"{ProcessSessionId}-{GetShortUID()}";
            foreach (var logger in loggers)
            {
                logger.LogQuizResult($"{quiz_id}-{ProcessSessionId}-{GetShortUID()}", choices);
            }
        }

        public void _HandleResults(object sender, MCQuizResultsEventArgs args)
        {
            OnLogQuizResult(args.QuizId, args.Choices);
        }
    }
}