using UnityEngine;
using UnityEngine.Serialization;

[ExecuteInEditMode]
public class LTDeformer : DeformerBase
{
    [Range(1f, 100f)]
    public float MaximumScaleFactor = 10f;

    [Range(0f, 1f)]
    public float LateralRigidity = 0f;

    private void Update()
    {
        float ex2 = -0.5f + 0.5f * LateralRigidity;
        float tensile_scale = Mathf.Pow(MaximumScaleFactor, Deformation);
        float lateral_scale = Mathf.Pow(MaximumScaleFactor, ex2 * Deformation);
        var scale = new Vector3(lateral_scale, lateral_scale, tensile_scale);
        transform.localScale = scale;
    }
}