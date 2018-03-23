using UnityEngine;

public class TTSpecimenProperties : MonoBehaviour
{
    public AnimationCurve NormalizedStressStrain = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    public float MaxStress = 1f;
    public float MaxStrain = 1f;
}