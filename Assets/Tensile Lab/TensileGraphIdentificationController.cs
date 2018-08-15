using CVRLabSJSU.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CVRLabSJSU
{
    public class TensileGraphIdentificationController :
        GraphQuizControllerBase<
            TensileGraphIdentificationController.SpecimenMaterialType,
            TensileGraphIdentificationController.Entry>
    {
        [Serializable]
        public class Entry : BaseEntry
        {
            [SerializeField]
            private SpecimenMaterialType _Type;

            public override SpecimenMaterialType Type { get { return _Type; } set { _Type = value; } }
        }

        [Header("Don't change these at runtime!")]
        [Tooltip("Unless you really know what you are doing!")]
        [SerializeField]
        private List<Entry> _EntriesList = new List<Entry>();

        protected override List<Entry> EntriesList { get { return _EntriesList; } }

        public enum SpecimenMaterialType
        {
            None,
            Polymer,
            Metal,
            Ceramic
        }

        /// <summary>
        /// Strength-axis value for where to add label for the curve
        /// </summary>
        [Header("Properties")]
        [Tooltip("Strength-axis value for where to add label for the curve")]
        public float CurveLabelStrength = 0.5f;

        public SpecimenMaterialType CurrentSpecimenType;

        public bool SetCurrentSpecimenType(string str)
        {
            return Enum.TryParse(str, true, out CurrentSpecimenType);
        }

        public override void HandlePointAdded(object sender, CurveGrapher.PointAddedEventArgs args)
        {
            // TODO: abstract this more?
            var area = args.Area ?? new Rect(0f, 0f, 1f, 1f);
            var grapher = (CurveGrapher)sender;
            var t1 = args.Segment.PointA.transform;
            var t2 = args.Segment.PointB.transform;
            var p1 = t1.localPosition;
            var p2 = t2.localPosition;
            var add_label = InRangeX(CurveLabelStrength, area, p1, p2);

            if (add_label)
            {
                string label_text = LabelTexts[CurrentSpecimenType];
                AddLabel(grapher, label_text, CurrentSpecimenType, t2.position);
            }
        }

        protected override bool IsQuizReady()
        {
            var polymer_ready = IsLabelKnown(GraphLabels.GetValue(SpecimenMaterialType.Polymer));
            var metal_ready = IsLabelKnown(GraphLabels.GetValue(SpecimenMaterialType.Metal));
            var ceramic_ready = IsLabelKnown(GraphLabels.GetValue(SpecimenMaterialType.Ceramic));
            return polymer_ready && metal_ready && ceramic_ready;
        }
    }
}