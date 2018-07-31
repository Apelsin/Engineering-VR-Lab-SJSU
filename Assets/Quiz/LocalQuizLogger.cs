using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CVRLabSJSU
{
    public class LocalQuizLogger : MonoBehaviour, IQuizLogger
    {
        public void LogQuizResult(string quiz_id, IReadOnlyDictionary<string, MultipleChoiceQuizItem.Option> choices)
        {
            var log_dir = $"{Application.persistentDataPath}/Quiz Choices/";
            Directory.CreateDirectory(log_dir);
            var log_file_path = $"{log_dir}{quiz_id}-choices.csv";
            using (var writer = new StreamWriter(log_file_path, true))
            {
                // TODO: custom formatting
                // Omit headers because it will make merging the result files for the
                // same quizzes a lot easier (e.g. cat TCQuiz*.csv > TCQuizAll.csv)
                //writer.WriteLine($"item,choice,text,is correct");
                foreach (var choice in choices)
                    writer.WriteLine($"{choice.Key},{choice.Value.Id},{choice.Value.Text},{choice.Value.IsCorrect}");
            }
            Debug.Log($"Quiz results saved to {log_file_path}");
        }
    }
}