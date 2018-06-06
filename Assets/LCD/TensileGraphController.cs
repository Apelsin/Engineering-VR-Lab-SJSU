using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using CVRLabSJSU.Utilities;

namespace CVRLabSJSU
{
    public class TensileGraphController : MonoBehaviour, ISerializationCallbackReceiver
    {
        public enum TensilePointType
        {
            None,
            Yield,
            Ultimate,
            Fracture,
            FractureAndUltimate
        }

        //public struct StringKVP
        //{
        //    public string Key, Value;
        //}

        [Serializable]
        public class TensilePointEntry
        {
            public TensilePointType Type;
            public string CorrectLabelText;
            //public GraphLabel LabelComponent;
        }

        //private Dictionary<TensilePointType, GraphLabel> TensilePointLabels = new Dictionary<TensilePointType, GraphLabel>();

        [Header("Don't change these at runtime!")]
        [Tooltip("Unless you really know what you are doing!")]
        [SerializeField]
        private List<TensilePointEntry> TensilePointEntriesList;

        private Dictionary<TensilePointType, GraphLabel> TensilePointGraphLabels = new Dictionary<TensilePointType, GraphLabel>();

        public IDictionary<TensilePointType, string> TensilePointLabelTexts
        {
            get { return TensilePointEntriesList.ToDictionary(e => e.Type, e => e.CorrectLabelText); }
        }

        public string UnknownLabelText = "?";

        public string CorrectSublabelText = "Correct";
        public string IncorrectSublabelText = "Incorrect";

        //[SerializeField] private string YieldStrengthLabelText = "Yield Strength";

        //[SerializeField] private string UltimateTensileStrengthLabelText = "Ult. Tensile Strength";

        //[SerializeField] private string FracturePointLabelText = "Fracture";

        //[SerializeField] private string FractureAndUltimateStrengthLabelText = "Ult. Strength & Fracture";

        [Header("Properties")]
        public float YieldStrength = 0.25f;

        public float UltimateTensileStrength = 0.5f;
        public float FracturePoint = 0.75f;

        public Color DefaultColor = new Color(0.2f, 0.2f, 0.2f);
        public Color SelectedColor = new Color(0.1f, 0.3f, 1.0f);
        public Color CorrectColor = new Color(0.1f, 0.9f, 0.1f);
        public Color IncorrectColor = new Color(0.8f, 0.0f, 0.0f);

        public GameObject LabelComboBox;

        public Button CheckAnswersButton;

        //private GraphLabel YieldStrengthLabel;
        //private GraphLabel UltimateTensileStrengthLabel;
        //private GraphLabel FracturePointLabel;
        //private GraphLabel FractureAndUltimateStrengthLabel;

        private static bool InRange(float value, float lo, float hi)
        {
            return value >= lo && value <= hi;
        }

        private static bool InRangeX(float x, Rect area, Vector3 p1, Vector3 p2)
        {
            return InRange(Mathf.Lerp(area.xMin, area.xMax, x), p1.x, p2.x);
        }

        // This is a severe hack
        //private List<GameObject> _ActiveLabels = new List<GameObject>();

        //private IEnumerable<GameObject> ActiveLabels
        //{
        //    get { return _ActiveLabels = _ActiveLabels.Where(l => l.activeSelf).ToList(); }
        //}

        // Public for Unity editor
        public void HandlePointAdded(object sender, CurveGrapher.PointAddedEventArgs args)
        {
            // Technically speaking, this is all plainly horrible :))))
            // TODO: make this not bad code

            var area = args.Area ?? new Rect(0f, 0f, 1f, 1f);

            var grapher = (CurveGrapher)sender;

            var t1 = args.Segment.PointA.transform;
            var t2 = args.Segment.PointB.transform;

            var p1 = t1.localPosition;
            var p2 = t2.localPosition;

            var is_yield = InRangeX(YieldStrength, area, p1, p2);
            var is_ultimate = InRangeX(UltimateTensileStrength, area, p1, p2);
            var is_fracture = InRangeX(FracturePoint, area, p1, p2);

            if (is_fracture)
            {
                grapher.Cancel();
            }

            TensilePointType type;
            if (is_yield)
                type = TensilePointType.Yield;
            else if (is_fracture && is_ultimate)
                type = TensilePointType.FractureAndUltimate;
            else if (is_ultimate)
                type = TensilePointType.Ultimate;
            else if (is_fracture)
                type = TensilePointType.Fracture;
            else
                type = TensilePointType.None;

            if (type != TensilePointType.None)
            {
                string label_text = TensilePointLabelTexts[type];
                AddLabel(grapher, label_text, type, t2.position);
            }
        }

        private bool IsLabelKnown(GraphLabel label)
        {
            var label_str = label?.Text.text;
            return !string.IsNullOrEmpty(label_str) && label_str != TensilePointLabelTexts[TensilePointType.None];
        }

        private UnityAction GetLabelComboItemClickedEventHandler(
            GraphLabel label,
            string label_text_str,
            ComboBox combo,
            ComboItem item)
        {
            return () =>
            {
                var item_str = item.Text.text;
                Debug.Log($"Clicked on {label_text_str} x {item_str}");
                label.Text.text = item_str;
                label.Text.color = SelectedColor;
                label.SecondaryText.text = "";
                // If dis our boi
                foreach (var i in combo.Items)
                    i.IsSelected = i == item;
                // Supergalactic hack: string comparison for whether all of our
                // "active" labels have been changed from unknown to known
                //var ready = ActiveLabels.All(l => l.GetComponentInChildren<Text>().text != UnknownLabelText);

                // BETTER METHOD (still ugly though)
                var yield_known = IsLabelKnown(TensilePointGraphLabels.GetValue(TensilePointType.Yield));
                var ultimate_known = IsLabelKnown(TensilePointGraphLabels.GetValue(TensilePointType.Ultimate));
                var fracture_known = IsLabelKnown(TensilePointGraphLabels.GetValue(TensilePointType.Fracture));
                var frac_n_ult_known = IsLabelKnown(TensilePointGraphLabels.GetValue(TensilePointType.FractureAndUltimate));

                var is_ready = yield_known && (ultimate_known && fracture_known) || frac_n_ult_known;

                // TODO: make this possible to animate (don't hard-code with SetActive)
                if (is_ready)
                    CheckAnswersButton.gameObject.SetActive(true);
            };
        }

        private void AddLabel(
            CurveGrapher grapher,
            string label_text_str,
            TensilePointType type,
            Vector3 world_position)
        {
            // Add label to graph
            var label_object = grapher.AddLabel(UnknownLabelText); // Quiz mode
                                                                   // Automatically disable the check answers button until all labels are set by combo box selection
            CheckAnswersButton.gameObject.SetActive(false);

            // Update the labels dictionary
            TensilePointGraphLabels[type] = label_object;

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
                entry.callback.AddListener((BaseEventData data) =>
                {
                    // TODO: DRY this up...
                    var combo_animator = LabelComboBox.GetComponent<Animator>();

                    // Move combo box to selected label (context menu)
                    LabelComboBox.transform.position = label_object.transform.position;
                    combo_animator.SetBool("Visible", true);

                    // Clear the combo list
                    var combo = LabelComboBox.GetComponent<ComboBox>();
                    combo.ClearItems();

                    // Add combo box items in a random order
                    ComboItem
                            fracture_item = null,
                            ultimate_item = null,
                            yield_item = null,
                            frac_n_ult_item = null;
                    {
                        // Array of closures which add + assign the combo items
                        var item_actions = new Action[] {
                        () => fracture_item = combo.AddItem(TensilePointLabelTexts[TensilePointType.Fracture]),
                        () => ultimate_item = combo.AddItem(TensilePointLabelTexts[TensilePointType.Ultimate]),
                        () => yield_item = combo.AddItem(TensilePointLabelTexts[TensilePointType.Yield]),
                        () => frac_n_ult_item = combo.AddItem(TensilePointLabelTexts[TensilePointType.FractureAndUltimate])
                        };
                        // Invoke closures in a random order
                        foreach (var action in item_actions.OrderBy(x => Guid.NewGuid()))
                            action();
                    }

                    // Select the item based on the currently visible label text
                    foreach (var item in combo.Items)
                        item.IsSelected = item.Text.text == label.Text.text;

                    // Add event handlers
                    var items = new[] { fracture_item, ultimate_item, yield_item, frac_n_ult_item };
                    foreach (var item in items)
                    {
                        var handler = GetLabelComboItemClickedEventHandler(
                            label, label_text_str, combo, item);
                        // Replace all previous listeners with ours
                        item.Button.onClick.RemoveAllListeners();
                        item.Button.onClick.AddListener(handler);
                    }
                });
                event_trigger.triggers.Add(entry);
            }

            // Exit handler
            {
                var entry = new EventTrigger.Entry() { eventID = EventTriggerType.PointerExit };
                entry.callback.RemoveAllListeners();
                entry.callback.AddListener((BaseEventData data) =>
                {
                    var animator = LabelComboBox.GetComponent<Animator>();
                    StopCoroutine("ShowLabelCombo");
                    animator.SetBool("Visible", false);
                });
                event_trigger.triggers.Add(entry);
            }
        }

        // Public for Unity editor
        public void HandleClickedCheckAnswers()
        {
            OnCheckAnswers();
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

        private static TEnum[] GetEnumValues<TEnum>()
        {
            return Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .ToArray();
        }

        private void OnCheckAnswers()
        {
            foreach (var type in GetEnumValues<TensilePointType>())
                ShowCorrectness(TensilePointGraphLabels.GetValue(type), TensilePointLabelTexts.GetValue(type));
        }

        private static void EnforceEnumList<T, TEnum>(List<T> list, Action<T, TEnum> action) where T : new()
        {
            var types = GetEnumValues<TEnum>();
            var size_diff = types.Length - list.Count;
            if (size_diff > 0)
            {
                for (int i = 0; i < size_diff; i++)
                    list.Add(new T());
            }
            else if (size_diff < 0)
            {
                list.RemoveRange(types.Length, -size_diff);
            }
            int idx = 0;
            foreach (var type in types)
            {
                action(list[idx++], type);
            }
        }

        public void OnBeforeSerialize()
        {
            EnforceEnumList<TensilePointEntry, TensilePointType>(
                TensilePointEntriesList, (e, type) => e.Type = type);
        }

        public void OnAfterDeserialize()
        {
        }
    }
}