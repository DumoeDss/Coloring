using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DemoView : MonoBehaviour
{
    ColoringTexture coloringTexture;

    private void Awake()
    {
        coloringTexture = GetComponent<ColoringTexture>();
    }

    public List<Toggle> toggles;
    public void OnToggleValueChanged()
    {
        for (int i = 0; i < toggles.Count; i++)
        {
            if (toggles[i].isOn)
            {
                coloringTexture.SetColorMask(i);
                return;
            }
        }
    }

    public void OnColorChange(Color color)
    {
        if(coloringTexture==null)
            coloringTexture = GetComponent<ColoringTexture>();
        coloringTexture.SetTargetColor(color);
    }
}
