using System;
using UnityEngine;

namespace CVRLabSJSU
{
    public class TTSpecimenPhysics : MonoBehaviour, ISerializationCallbackReceiver
    {
        public struct MorphCurve2
        {
            public readonly float Remainder;
            public readonly AnimationCurve A;
            public readonly AnimationCurve B;

            public float Evaluate(float time)
            {
                var eval_a = A.Evaluate(time);
                var eval_b = B.Evaluate(time);
                return Mathf.Lerp(eval_a, eval_b, Remainder);
            }

            public MorphCurve2(float remainder, AnimationCurve a, AnimationCurve b)
            {
                Remainder = remainder;
                A = a;
                B = b;
            }
        }

        public static MorphCurve2 GetMorphCurve2(AnimationCurve[] scale_morphs, float normalized_morph)
        {
            var number_of_morphs = scale_morphs.Length;
            var morph = number_of_morphs * normalized_morph;
            var idx_a = Mathf.FloorToInt(morph);
            var idx_b = idx_a + 1;
            idx_a = Mathf.Clamp(idx_a, 0, number_of_morphs - 1);
            idx_b = Mathf.Clamp(idx_b, 0, number_of_morphs - 1);
            var remainder = morph - idx_a;
            return new MorphCurve2(remainder, scale_morphs[idx_a], scale_morphs[idx_b]);
        }

        public static void ApplyScaleCurve(Transform[] transforms, Func<float, float> eval)
        {
            for (int i = 0; i < transforms.Length; i++)
            {
                var t = (float)(i) / (float)(transforms.Length - 1);
                var f = eval(t);
                transforms[i].localScale = new Vector3(f, f, f);
            }
        }

        public static void ApplyLateralOffsetCurve(Transform[] transforms, Func<float, float> eval)
        {
            if (transforms.Length <= 2)
                return;
            var pos_0 = transforms[0].localPosition;
            var pos_n = transforms[transforms.Length - 1].localPosition;
            for (int i = 1; i < transforms.Length - 1; i++)
            {
                var t = (float)(i) / (float)(transforms.Length - 1);
                var u = (float)(i + 1) / (float)(transforms.Length + 1);
                var f = eval(t);
                var position = Vector3.Lerp(pos_0, pos_n, u + f);
                transforms[i].localPosition = position;
            }
        }

        [SerializeField]
        [Range(0f, 1f)]
        private float _Morph;

        public float Morph
        {
            get { return _Morph; }
            set
            {
                _Morph = Mathf.Clamp01(value);
                var mapped_value = Mathf.Clamp01(MorphCurve.Evaluate(value));
                var scale_curve = GetMorphCurve2(ScaleMorphs, mapped_value);
                var displacement_curve = GetMorphCurve2(LateralDisplacementMorphs, mapped_value);
                ApplyScaleCurve(BoneTransforms, scale_curve.Evaluate);
                ApplyLateralOffsetCurve(BoneTransforms, displacement_curve.Evaluate);
            }
        }

        public AnimationCurve MorphCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        public AnimationCurve[] ScaleMorphs = new[] { AnimationCurve.Linear(0f, 1f, 1f, 1f) };
        public AnimationCurve[] LateralDisplacementMorphs = new[] { AnimationCurve.Linear(0f, 0f, 1f, 0f) };

        public Transform[] BoneTransforms = new Transform[] { };

        public void OnBeforeSerialize()
        {
            Morph = Morph;
        }

        public void OnAfterDeserialize()
        {
        }
    }
}