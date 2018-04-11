using UnityEngine;
using UnityEngine.Serialization;

[ExecuteInEditMode]
public abstract class DeformerBase : MonoBehaviour
{
    [Range(-1f, 1f)]
    [SerializeField]
    private float _Deformation = 0f;

    public bool ConstrainDeformation = true;

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
}