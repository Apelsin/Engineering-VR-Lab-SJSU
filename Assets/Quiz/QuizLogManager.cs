using System;
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
        public void HandleLogQuizResult(object sender, MCQuizResultsEventArgs args)
        {
            var loggers = GetComponents<IQuizLogger>();
            var session_id = $"{ProcessSessionId}-{GetShortUID()}";
            foreach (var logger in loggers)
            {
                logger.LogQuizResult($"{args.QuizId}-{ProcessSessionId}-{GetShortUID()}", args.Choices);
            }
        }
    }
}