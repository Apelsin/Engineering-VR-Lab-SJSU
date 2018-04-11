using UnityEngine;
using UnityEngine.Serialization;

[ExecuteInEditMode]
public class PhysicallyModeledLTDeformer : MonoBehaviour
{
    [SerializeField]
    private float _Stress = 0f;

    public float Stress
    {
        get { return _Stress; }
        set { _Stress = value; }
    }

    public float ModulusOfElasticity = 2000000f;

    /// <summary>
    /// Poisson's ratio
    /// </summary>
    [Tooltip("Poisson's ratio")]
    [Range(0f, 0.5f)]
    public float PoissonRatio = 0f;

    private void Update()
    {
        //float ex2 = -0.5f + 0.5f * LateralRigidity;
        //float tensile_scale = Mathf.Pow(MaximumScaleFactor, Deformation);
        //float lateral_scale = Mathf.Pow(MaximumScaleFactor, ex2 * Deformation);
        //var scale = new Vector3(lateral_scale, lateral_scale, tensile_scale);

        var e_z = Stress / ModulusOfElasticity;

        var tensile_scale = 1f + e_z;
        var lateral_scale = 1f - PoissonRatio * e_z;
        var scale = new Vector3(lateral_scale, lateral_scale, tensile_scale);

        transform.localScale = scale;
    }
}