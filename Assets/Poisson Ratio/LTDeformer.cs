using UnityEngine;

[ExecuteInEditMode]
public class LTDeformer : MonoBehaviour
{
    [Range(1f, 100f)]
    public float MaximumScaleFactor = 10f;

    [Range(-1f, 1f)]
    [SerializeField]
    private float _Deformation = 0f;

    public bool ConstrainDeformation = true;

    [Range(0f, 1f)]
    public float LateralRegidity = 0f;

    public float Deformation
    {
        get { return _Deformation; }
        set
        {
            if (ConstrainDeformation)
            {
                _Deformation = Mathf.Clamp(value, -1f, 1f);
                if (value != _Deformation)
                    Debug.LogWarning("Attempted to set Deformation property set outside of acceptible range: [-1, 1]");
            }
            else
                _Deformation = value;
        }
    }

    private void Start()
    {
    }

    private void Update()
    {
        float ex2 = -0.5f + 0.5f * LateralRegidity;
        float tensile_scale = Mathf.Pow(MaximumScaleFactor, Deformation);
        float lateral_scale = Mathf.Pow(MaximumScaleFactor, ex2 * Deformation);
        var scale = new Vector3(lateral_scale, lateral_scale, tensile_scale);
        //Debug.Log(scale.x * scale.y * scale.z);
        transform.localScale = scale;
    }
}