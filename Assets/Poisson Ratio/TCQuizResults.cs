using cakeslice;
using System.Collections.Generic;
using UnityEngine;

namespace CVRLabSJSU
{
    public class TCQuizResults : MonoBehaviour
    {
        public TensionCompressionQuiz.TCObjects TensionObjects;
        public TensionCompressionQuiz.TCObjects CompressionObjects;
        public int CorrectOutlineColorIndex = 1;
        public int IncorrectOutlineColorIndex = 2;
        private List<Outline> Outlines = new List<Outline>();

        public void HandleDisplayResults(object sender, MCQuizResultsEventArgs args)
        {
            Clear();
            MarkCorrect(args.Choices, TensionObjects);
            MarkCorrect(args.Choices, CompressionObjects);
        }

        private void Clear()
        {
            foreach (var outline in Outlines)
                Destroy(outline);
            Outlines.Clear();
        }

        private void MarkCorrect(IReadOnlyDictionary<string, MultipleChoiceQuizItem.Option> choices, TensionCompressionQuiz.TCObjects objects)
        {
            foreach (var choice in choices)
            {
                var id = choice.Key;
                var tc_object =
                    (typeof(TensionCompressionQuiz.TCObjects)
                    .GetField(id)?
                    .GetValue(objects) as GameObject);
                if (tc_object)
                {
                    var outline = tc_object.AddComponent<Outline>();
                    if (choice.Value.IsCorrect)
                        outline.color = CorrectOutlineColorIndex;
                    else
                        outline.color = IncorrectOutlineColorIndex;
                    Outlines.Add(outline);
                }
            }
        }
    }
}