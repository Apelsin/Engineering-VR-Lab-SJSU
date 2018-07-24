using System;
using UnityEngine;

namespace CVRLabSJSU
{
    public class QuizLogManager : MonoBehaviour
    {
        /// <summary>
        /// GUID that is the same for the application's life cycle
        /// </summary>
        public static readonly string ProcessSessionId = Guid.NewGuid().ToString();
        //public string SessionId; // TODO
        public void HandleLogQuizResult(object sender, MCQuizResultsEventArgs args)
        {
            var loggers = GetComponents<IQuizLogger>();
            foreach (var logger in loggers)
            {
                logger.LogQuizResult(ProcessSessionId, args.QuizId, args.Choices);
            }
        }
    }
}