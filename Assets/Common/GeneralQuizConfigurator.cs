using UnityEngine;

namespace CVRLabSJSU
{
    public class GeneralQuizConfigurator : MonoBehaviour
    {
        private void Start()
        {
            var tc_quiz_objects = GameObject.FindGameObjectsWithTag("Quiz Controller");
            foreach (var tc_quiz_object in tc_quiz_objects)
            {
                {
                    var controller = tc_quiz_object.GetComponent<IMCQuizController>();
                    if (controller != null)
                    {
                        controller.DisplayResults += HandleMCQuizDisplayResults;
                        continue;
                    }
                }
                Debug.LogWarning("Quiz Controller object does not have a controller component.", tc_quiz_object);
            }
        }

        private void HandleMCQuizDisplayResults(object sender, MCQuizResultsEventArgs args)
        {
            var quiz_log_manager = FindObjectOfType<QuizLogManager>();
            quiz_log_manager.OnLogQuizResult(args.QuizId, args.Choices);
        }
    }
}