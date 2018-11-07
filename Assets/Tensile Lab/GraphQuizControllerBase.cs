using CVRLabSJSU.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CVRLabSJSU
{
    public abstract class GraphQuizControllerBase<TEnum, TEntry> :
        MonoBehaviour,
        ISerializationCallbackReceiver,
        IMCQuizController
        where TEntry : GraphQuizControllerBase<TEnum, TEntry>.BaseEntry, new()
    {
        [SerializeField]
        protected MCQuizResultsEvent _DisplayResults = new MCQuizResultsEvent();

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

        public abstract class BaseEntry
        {
            public abstract TEnum Type { get; set; }
            public string CorrectLabelText;
        }

        public TEnum NoneType;

        [SerializeField]
        protected abstract List<TEntry> EntriesList { get; }

        protected Dictionary<TEnum, GraphLabel> GraphLabels = new Dictionary<TEnum, GraphLabel>();

        public IDictionary<TEnum, string> LabelTexts
        {
            get { return EntriesList.ToDictionary(e => e.Type, e => e.CorrectLabelText); }
        }

        public string UnknownLabelText = "?";

        public string CorrectSublabelText = "Correct";
        public string IncorrectSublabelText = "Incorrect";

        public Color DefaultColor = new Color(0.2f, 0.2f, 0.2f);
        public Color SelectedColor = new Color(0.1f, 0.3f, 1.0f);
        public Color CorrectColor = new Color(0.1f, 0.9f, 0.1f);
        public Color IncorrectColor = new Color(0.8f, 0.0f, 0.0f);

        public GameObject LabelComboBox;

        public Button CheckAnswersButton;

        protected static bool InRange(float value, float lo, float hi)
        {
            return value >= lo && value <= hi;
        }

        protected static bool InRangeX(float x, Rect area, Vector3 p1, Vector3 p2)
        {
            return InRange(Mathf.Lerp(area.xMin, area.xMax, x), p1.x, p2.x);
        }

        // Public for Unity editor
        public abstract void HandlePointAdded(object sender, CurveGrapher.PointAddedEventArgs args);

        protected virtual bool IsLabelKnown(GraphLabel label)
        {
            var label_str = label?.Text.text;
            return !string.IsNullOrEmpty(label_str) && label_str != LabelTexts[NoneType];
        }

        protected abstract bool IsQuizReady();

        private UnityAction GetLabelComboItemClickedEventHandler(
            GraphLabel label,
            string label_text_str,
            ComboBox combo,
            ComboItem item)
        {
            return () =>
            {
                // Set graph label selection text + color
                var item_str = item.Text.text;
                Debug.Log($"Clicked on {label_text_str} x {item_str}");
                label.Text.text = item_str;
                label.Text.color = SelectedColor;
                label.SecondaryText.text = "";
                // Set combo list selection state
                foreach (var i in combo.Items)
                    i.IsSelected = i == item;

                // TODO: make this possible to animate (don't hard-code with SetActive)
                if (IsQuizReady())
                    CheckAnswersButton.gameObject.SetActive(true);
            };
        }

        protected virtual void OnLabelEnter(BaseEventData data, GraphLabel label)
        {
        }

        protected virtual void OnLabelExit(BaseEventData data, GraphLabel label)
        {
        }

        private UnityAction<BaseEventData> GetLabelEnterHandler(GraphLabel label, string label_text_str)
        {
            return (b) =>
            {
                // TODO: DRY this up...
                var combo_animator = LabelComboBox.GetComponent<Animator>();

                // Move combo box to selected label (context menu)
                LabelComboBox.transform.position = label.gameObject.transform.position;
                combo_animator.SetBool("Visible", true);

                // Clear the combo list
                var combo = LabelComboBox.GetComponent<ComboBox>();
                combo.ClearItems();

                // Add combo box items in a random order
                var item_label_texts = Utility.GetEnumValues<TEnum>()
                    .Except(new[] { NoneType })
                    .Select(e => LabelTexts[e])
                    .OrderBy(x => Guid.NewGuid())
                    .ToArray();
                // Note: mutates combo object
                var items = item_label_texts.Select(i => combo.AddItem(i)).ToArray();

                // Select the item based on the currently visible label text
                foreach (var item in combo.Items)
                    item.IsSelected = item.Text.text == label.Text.text;

                // Add event handlers
                foreach (var item in items)
                {
                    var handler = GetLabelComboItemClickedEventHandler(
                        label, label_text_str, combo, item);
                    // Replace all previous listeners with ours
                    item.Button.onClick.RemoveAllListeners();
                    item.Button.onClick.AddListener(handler);
                }

                OnLabelEnter(b, label);
            };
        }

        private UnityAction<BaseEventData> GetLabelExitHandler(GraphLabel label, string label_text_str)
        {
            return (b) =>
            {
                var animator = LabelComboBox.GetComponent<Animator>();
                StopCoroutine("ShowLabelCombo");
                animator.SetBool("Visible", false);
                OnLabelExit(b, label);
            };
        }

        protected void AddLabel(
            CurveGrapher grapher,
            string label_text_str,
            TEnum type,
            Vector3 world_position)
        {
            // Add label to graph
            var label_object = grapher.AddLabel(UnknownLabelText); // Quiz mode
                                                                   // Automatically disable the check answers button until all labels are set by combo box selection
            CheckAnswersButton.gameObject.SetActive(false);

            // Update the labels dictionary
            GraphLabels[type] = label_object;

            // Add this label to the active labels list
            //_ActiveLabels.Add(label_object);
            var label = label_object.GetComponent<GraphLabel>();
            // Set its position
            label_object.transform.position = world_position;
            // Add the handler to its its PointerClick event
            var event_trigger = label_object.GetComponent<EventTrigger>();

            // Enter handler
            {
                var entry = new EventTrigger.Entry() { eventID = EventTriggerType.PointerEnter };
                // Remove existing handlers
                entry.callback.RemoveAllListeners();
                // Make handlers for type of label
                entry.callback.AddListener(GetLabelEnterHandler(label, label_text_str));
                event_trigger.triggers.Add(entry);
            }

            // Exit handler
            {
                var entry = new EventTrigger.Entry() { eventID = EventTriggerType.PointerExit };
                entry.callback.RemoveAllListeners();
                entry.callback.AddListener(GetLabelExitHandler(label, label_text_str));
                event_trigger.triggers.Add(entry);
            }
        }

        private void ShowCorrectness(GraphLabel label, string correct_str)
        {
            if (label)
            {
                var is_correct = label.Text.text == correct_str;
                var color = is_correct ? CorrectColor : IncorrectColor;
                label.Text.color = color;
                label.SecondaryText.text = is_correct ? CorrectSublabelText : IncorrectSublabelText;
                label.SecondaryText.color = color;
            }
        }

        // Public for Unity editor
        public void HandleClickedCheckAnswers()
        {
            OnDisplayQuizResults();
        }

        protected void OnDisplayQuizResults()
        {  
            var types = Utility.GetEnumValues<TEnum>();
            foreach (var type in types)
                ShowCorrectness(GraphLabels.GetValue(type), LabelTexts.GetValue(type));

            // Precondition: GraphLabels exist for each TEnum (GraphLabels.GetValue(t) != null)
            var dict = types
                .Where(t => GraphLabels.GetValue(t) != null)
                .ToDictionary(
                t => t.ToString(),
                t => new MultipleChoiceQuizItem.Option()
                {
                    Id = GraphLabels.GetValue(t).Text.text,
                    Text = GraphLabels.GetValue(t).Text.text,
                    IsCorrect = GraphLabels.GetValue(t).Text.text == LabelTexts.GetValue(t),
                });
            _DisplayResults.Invoke(this, new MCQuizResultsEventArgs(name, dict));
        }

        public virtual void OnBeforeSerialize()
        {
            Utility.EnforceEnumList<TEntry, TEnum>(
                EntriesList, (e, type) => e.Type = type);
        }

        public virtual void OnAfterDeserialize()
        {
        }
    }
}