using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CVRLabSJSU
{
    public class ResultsBoard : MonoBehaviour
    {
        public Text Text;
        private void Start()
        {
        }
        public void HandleMCQuizResults(object sender, MCQuizResultsEventArgs args)
        {
            var report_lines = args.Choices.Select(q =>
            {
                var correct_text = q.Value.IsCorrect ? "correct" : "incorrect";
                return $"{q.Key}: {q.Value.Text} ({correct_text})";
            }).ToArray();
            var report = String.Join("\n", report_lines);
            Text.text = report;
        }
    }
}