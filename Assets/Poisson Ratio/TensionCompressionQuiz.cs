using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CVRLabSJSU
{
    public class TensionCompressionQuiz : MonoBehaviour, ISerializationCallbackReceiver, IMCQuizController
    {
        [SerializeField]
        private MCQuizResultsEvent _DisplayResults = new MCQuizResultsEvent();

        public event UnityAction<object, MCQuizResultsEventArgs> DisplayResults
        {
            add
            {
                _DisplayResults.AddListener(value);
            }
            remove
            {
                _DisplayResults.RemoveListener(value);
            }
        }

        [Serializable]
        public struct TCObjects : IEnumerable<GameObject>
        {
            public GameObject Reference;
            public GameObject Material01;
            public GameObject Material02;
            public GameObject Material03;

            public IEnumerator<GameObject> GetEnumerator()
            {
                yield return Reference;
                yield return Material01;
                yield return Material02;
                yield return Material03;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                yield return Reference;
                yield return Material01;
                yield return Material02;
                yield return Material03;
            }
        }

        public struct QuizChoice
        {
            public string ItemId;
            public MultipleChoiceQuizItem.Option ChosenOption;
        }

        public MultipleChoiceQuiz Source;

        public TCObjects TensionObjects;
        public TCObjects CompressionObjects;

        public Animator Animator;

        private Dictionary<string, MultipleChoiceQuizItem.Option> QuizChoices =
            new Dictionary<string, MultipleChoiceQuizItem.Option>();

        private void HandleMenuAddedCallback(object sender, PointerMenuManager.PointerMenuEventArgs args)
        {
            var pointer_menu = (ManagedPointerMenu)sender;
            var buttons = args.Menu.Buttons;

            // Record initial color values
            var normal_colors = args.Menu.Buttons.First().colors;
            var checked_colors = normal_colors;
            checked_colors.normalColor = args.Menu.CheckedColor;
            checked_colors.highlightedColor = args.Menu.CheckedColor;

            // Use a current buttons array
            var current_infos = pointer_menu.Buttons;

            // Set up button click handler
            args.Menu.ButtonClick += (sender2, args2) =>
            {
                // TODO: replace with some pretty Linq statement (it's harder b/c struct type)
                // Read from current buttons array
                foreach (var info in current_infos)
                {
                    // If the clicked button info id matches any of the current button infos
                    if (info.Id == args2.Info.Id)
                    {
                        // TODO: separate color and checked logic (decouple)
                        // Clear button colors
                        foreach (var button in buttons)
                            button.colors = normal_colors;

                        // Clear pointer menu button checked states
                        pointer_menu.ClearCheckedButtons();

                        // Set button colors
                        args2.Button.colors = checked_colors;

                        // Set pointer menu button checked
                        pointer_menu.SetButtonChecked(args2.Info.Id, true);

                        // Quiz logic
                        // Merge the chosen button info's quiz choice to the quiz answers dictionary
                        var choice = (QuizChoice)args2.Info.Data;
                        SetQuizChoice(choice);

                        // Show the check answers button when the quiz is ready for assessment
                        var quiz_ready_for_assessment = IsQuizReadyForAssessment();
                        ShowCheckAnswersButton(quiz_ready_for_assessment);

                        // Break because we matched
                        break;
                    }
                }
            };

            // Navigation handler
            args.Menu.Navigate += (sender2, args2) =>
            {
                // Replace current buttons array
                current_infos = args2.Parent.Children;
                CheckButtons(buttons, current_infos, ref normal_colors, ref checked_colors);
            };

            CheckButtons(buttons, current_infos, ref normal_colors, ref checked_colors);
        }

        public void SetQuizChoice(string item_id, MultipleChoiceQuizItem.Option choice_option)
        {
            QuizChoices[item_id] = choice_option;
        }

        public void SetQuizChoice(QuizChoice choice)
        {
            SetQuizChoice(choice.ItemId, choice.ChosenOption);
        }

        public void SetQuizChoice(string choice_json)
        {
            var choice = JsonUtility.FromJson<QuizChoice>(choice_json);
            SetQuizChoice(choice);
        }

        private bool IsQuizReadyForAssessment()
        {
            return Source.Items.All(i => QuizChoices.ContainsKey(i.Id));
        }

        public void ShowCheckAnswersButton(bool value)
        {
            Animator.SetBool("Show Check Answers", value);
        }

        public void OnDisplayQuizResults()
        {
            // TODO: replace quiz_id: Source.name with something more definitive
            var args = new MCQuizResultsEventArgs(Source.name, QuizChoices);
            _DisplayResults.Invoke(this, args);
        }

        public QuizChoice DbgQuizChoice(QuizChoice choice)
        {
            Debug.Log(JsonUtility.ToJson(choice));
            return choice;
        }

        /// <summary>
        /// Apply the pointer menu button info values by mapping TCObject
        /// members' pointer menus by member names to quiz items by ids.
        /// </summary>
        /// <param name="objects2">
        /// Objects to include when applying quiz items
        /// </param>
        private void SetSharedQuizMenuButtons(
            IEnumerable<MultipleChoiceQuizItem> quiz_items,
            params TCObjects[] objects2)
        {
            // This is simultaneously ugly and beautiful
            // TODO: generalize this logic into something reusable

            // Dynamic programming FTW
            var templates = new Dictionary<string, MenuButtons>();
            foreach (var objects in objects2)
            {
                // TODO: convert this to a generalized utility function
                var associations = quiz_items
                    .Select(quiz_item => new
                    {
                        pointer_menu =
                            (typeof(TCObjects)
                            .GetField(quiz_item.Id)?
                            .GetValue(objects) as GameObject)?
                            .GetComponent<ManagedPointerMenu>(),
                        quiz_item
                    })
                    .Where(e => e.pointer_menu)
                    .ToArray();

                foreach (var e in associations)
                {
                    MenuButtons template;
                    var quiz_item_id = e.quiz_item.Id;
                    if (!templates.TryGetValue(quiz_item_id, out template))
                    {
                        // Make button infos for each quiz item option
                        var button_infos = e.quiz_item.Options
                            .Select(i => new ButtonInfo()
                            {
                                Id = i.Id,
                                Text = i.Text,
                                Data = new QuizChoice()
                                {
                                    ItemId = e.quiz_item.Id,
                                    ChosenOption = i
                                }
                            });
                        template = new MenuButtons()
                        {
                            Buttons = button_infos.ToList()
                        };
                        // Update the templates dictionary since we will
                        // share this quiz item across all menus
                        templates[quiz_item_id] = template;
                    }
                    // Set the pointer menu shared template to this
                    e.pointer_menu.SharedTemplate = template;
                }
            }
        }

        private void Start()
        {
            // Here's where the magic happens
            var tension_pointer_menus = TensionObjects
                .Select(o => o.GetComponent<ManagedPointerMenu>()).ToArray();
            var compression_pointer_menus = CompressionObjects
                .Select(o => o.GetComponent<ManagedPointerMenu>()).ToArray();
            var paired_pointer_menus = tension_pointer_menus
                .Zip(compression_pointer_menus, (t, c) => new
                {
                    tension_pointer_menu = t,
                    compression_pointer_menu = c
                }).ToArray();

            // For all of our objects
            foreach (var e in paired_pointer_menus)
            {
                // Add button logic to all of the menus
                // "Check" the last-pressed button and record the check
                e.tension_pointer_menu.MenuAddedCallback += HandleMenuAddedCallback;
                e.compression_pointer_menu.MenuAddedCallback += HandleMenuAddedCallback;
            }

            // Set all the menu buttons for the quiz (overrides MenuButtons.SharedTemplate)
            SetSharedQuizMenuButtons(Source.Items, TensionObjects, CompressionObjects);
        }

        private static void CheckButtons(
            IEnumerable<Button> buttons,
            IEnumerable<ButtonInfo> infos,
            ref ColorBlock normal_colors,
            ref ColorBlock checked_colors)
        {
            var stuff = buttons.Zip(infos, (button, info) => new { button, info });
            foreach (var e in stuff)
            {
                // Set Button component colors based on navigation event args and parent button info
                e.button.colors = e.info.Checked ? checked_colors : normal_colors;
            }
        }

        private void Update()
        {
        }

        public void OnBeforeSerialize()
        {
            if (Animator == null)
                Animator = GetComponent<Animator>();
        }

        public void OnAfterDeserialize()
        {
        }
    }
}