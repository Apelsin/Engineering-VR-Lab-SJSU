using CVRLabSJSU.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CVRLabSJSU
{
    public class TensileGraphController : GraphQuizControllerBase<TensileGraphController.TensilePointType, TensileGraphController.Entry>
    {
        public enum TensilePointType
        {
            None,
            Yield,
            Ultimate,
            Fracture,
            FractureAndUltimate
        }

        [Serializable]
        public class Entry : BaseEntry
        {
            [SerializeField]
            private TensilePointType _Type;

            public override TensilePointType Type { get { return _Type; } set { _Type = value; } }
        }

        //private Dictionary<TensilePointType, GraphLabel> TensilePointLabels = new Dictionary<TensilePointType, GraphLabel>();

        [Header("Don't change these at runtime!")]
        [Tooltip("Unless you really know what you are doing!")]
        [SerializeField]
        private List<Entry> _EntriesList;

        protected override List<Entry> EntriesList { get { return _EntriesList; } }

        [Header("Properties")]
        public float YieldStrength = 0.25f;

        public float UltimateTensileStrength = 0.5f;
        public float FracturePoint = 0.75f;

        // Public for Unity editor
        public override void HandlePointAdded(object sender, CurveGrapher.PointAddedEventArgs args)
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

            // Type logic
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
                string label_text = LabelTexts[type];
                AddLabel(grapher, label_text, type, t2.position);
            }
        }

        protected override bool IsQuizReady()
        {
            // BETTER METHOD (still ugly though)
            var yield_known = IsLabelKnown(GraphLabels.GetValue(TensilePointType.Yield));
            var ultimate_known = IsLabelKnown(GraphLabels.GetValue(TensilePointType.Ultimate));
            var fracture_known = IsLabelKnown(GraphLabels.GetValue(TensilePointType.Fracture));
            var frac_n_ult_known = IsLabelKnown(GraphLabels.GetValue(TensilePointType.FractureAndUltimate));

            return yield_known && (ultimate_known && fracture_known) || frac_n_ult_known;
        }
    }
}