using System;
using UnityEngine;
using UnityEngine.Events;

public class SineDeformController : MonoBehaviour, ISerializationCallbackReceiver
{
    public const float TwoPi = 6.283185307179586476925286766559f;
    public const float TwentyPi = 62.83185307179586476925286766559f;
    [Serializable]
    public class ValueUpdateEvent : UnityEvent<float> { }

    public ValueUpdateEvent ValueUpdate;

    [Tooltip("In radians")]
    [Range(0f, TwoPi)]
    public float Phase = 0f;

    [SerializeField]
    [Tooltip("In radians per second")]
    [Range(0.0f, TwentyPi)]
    private float _Rate = 1f;

    public float Rate
    {
        get { return _Rate; }
        set
        {
            _Rate = value;
        }
    }

    private float Value = 0f;
    private float _Time = 0f;

    private void Start()
    {
    }

    private void Update()
    {
        _Time = (_Time + Time.deltaTime * Rate) % TwoPi;
        Value = Mathf.Sin(_Time);
        ValueUpdate.Invoke(Value);
    }

    public void OnBeforeSerialize()
    {
        Rate = Rate;
    }

    public void OnAfterDeserialize()
    {
    }
}