using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI.Extensions;
using UnityEngine.UI.Extensions.ColorPicker;

public class RadialColorSlider : MonoBehaviour
{
    public ColorPickerControl ColorPicker;

    /// <summary>
    /// Which value this slider can edit.
    /// </summary>
    public ColorValues type;

    private RadialSlider slider;

    private bool listen = true;

    private void Awake()
    {
        slider = GetComponent<RadialSlider>();

        ColorPicker.onValueChanged.AddListener(ColorChanged);
        ColorPicker.onHSVChanged.AddListener(HSVChanged);
        slider.onValueChanged.AddListener(SliderChanged);
    }

    private void OnDestroy()
    {
        ColorPicker.onValueChanged.RemoveListener(ColorChanged);
        ColorPicker.onHSVChanged.RemoveListener(HSVChanged);
        slider.onValueChanged.RemoveListener(SliderChanged);
    }

    private void ColorChanged(Color newColor)
    {
        listen = false;
        switch (type)
        {
            case ColorValues.R:
                slider.normalizedValue = newColor.r;
                break;
            case ColorValues.G:
                slider.normalizedValue = newColor.g;
                break;
            case ColorValues.B:
                slider.normalizedValue = newColor.b;
                break;
            case ColorValues.A:
                slider.normalizedValue = newColor.a;
                break;
            default:
                break;
        }
    }

    private void HSVChanged(float hue, float saturation, float value)
    {
        listen = false;
        switch (type)
        {
            case ColorValues.Hue:
                slider.normalizedValue = hue; //1 - hue;
                break;
            case ColorValues.Saturation:
                slider.normalizedValue = saturation;
                break;
            case ColorValues.Value:
                slider.normalizedValue = value;
                break;
            default:
                break;
        }
    }

    private void SliderChanged(float newValue)
    {
        if (listen)
        {
            newValue = slider.normalizedValue-0.25f;
            if (newValue <0)
                newValue =1+newValue;
            //if (type == ColorValues.Hue)
            //    newValue = 1 - newValue;
            newValue = 1 - newValue;
            ColorPicker.AssignColor(type, newValue);
        }
        listen = true;
    }
}