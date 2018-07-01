using System;
using UnityEngine;
using UnityEngine.UI;

public class FormattedNumericReadout : MonoBehaviour, ISerializationCallbackReceiver
{
    [SerializeField]
    private Text _Text;

    public Text Text
    {
        get { return _Text; }
        set { _Text = value; }
    }

    [SerializeField]
    private string _Format = "{0}";

    public string Format
    {
        get { return _Format; }
        set
        {
            _Format = value;
            UpdateText(Text, value, Value, RoundingMode);
        }
    }

    [SerializeField]
    private float _Value;

    public float Value
    {
        get { return _Value; }
        set
        {
            _Value = value;
            UpdateText(Text, Format, value, RoundingMode);
        }
    }

    [SerializeField]
    private RoundingMode _RoundingMode;

    public RoundingMode RoundingMode
    {
        get { return _RoundingMode; }
        set
        {
            _RoundingMode = value;
            UpdateText(Text, Format, Value, value);
        }
    }

    private static float ApplyRounding(float f, RoundingMode mode)
    {
        switch (mode)
        {
            default:
            case RoundingMode.None:
                return f;

            case RoundingMode.Floor:
                return Mathf.Floor(f);

            case RoundingMode.Round:
                return Mathf.Round(f);

            case RoundingMode.Ceil:
                return Mathf.Ceil(f);
        }
    }

    private static void UpdateText(Text text_component, string format, float value, RoundingMode rounding_mode)
    {
        if (String.IsNullOrWhiteSpace(format))
        {
            if (text_component)
                text_component.text = value.ToString();
            return;
        }
        try
        {
            var rounded_value = ApplyRounding(value, rounding_mode);
            var formatted = string.Format(format, rounded_value);
            if(text_component)
                text_component.text = formatted;
        }
        catch (FormatException)
        {
            if (text_component)
                text_component.text = format;
        }
    }

    private void Start()
    {
    }

    private void Update()
    {
    }

    public void OnBeforeSerialize()
    {
        if (Text == null)
            Text = GetComponent<Text>();
        UpdateText(Text, Format, Value, RoundingMode);
    }

    public void OnAfterDeserialize()
    {
    }
}