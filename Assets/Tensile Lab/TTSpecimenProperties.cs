using UnityEngine;

namespace CVRLabSJSU
{
    public class TTSpecimenProperties : MonoBehaviour
    {
        public string MaterialType = "Default";
        public Color CurveColor = Color.black;
        public AnimationCurve NormalizedStressStrain = AnimationCurve.Linear(0f, 0f, 1f, 1f);
        public float MaxStress = 1f;
        public float MaxStrain = 1f;

        [Header("As functions of strain...")]
        public float YieldStrength = 0.25f;

        public float UltimateTensileStrength = 0.5f;
        public float FracturePoint = 0.75f;
    }
}