using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CVRLabSJSU
{
    public class LocalQuizLogger : MonoBehaviour, IQuizLogger
    {
        public void LogQuizResult(string session_id, string quiz_id, IReadOnlyDictionary<string, MultipleChoiceQuizItem.Option> choices)
        {
            var log_dir = $"{Application.persistentDataPath}/Quiz Choices/";
            Directory.CreateDirectory(log_dir);
            var log_file_path = $"{log_dir}{session_id}-quiz-choices.csv";
            using (var writer = new StreamWriter(log_file_path, true))
            {
                foreach(var choice in choices)
                {
                    // TODO: custom formatting
                    // Quiz, item id, choice id, choice text, choice correctness
                    writer.WriteLine($"{quiz_id}, {choice.Key}, {choice.Value.Id}, {choice.Value.Text}, {choice.Value.IsCorrect}");
                }
            }
            Debug.Log($"Quiz results saved to {log_file_path}");
        }
    }
}