using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ColoringTexture : MonoBehaviour
{
    public Material coloringMat;
    public Texture2D _RecolorMask;
    public Texture2D sourceTex;
    public Material targetMat;
    public List<Color> sourceColors;
    RenderTexture renderTexture;

    RenderTexture _MainTexture
    {
        get
        {
            if(renderTexture==null)
                renderTexture = new RenderTexture(sourceTex.width, sourceTex.height, 0);
            return renderTexture;
        }
        set
        {
            renderTexture = value;
        }
    }


    public void Coloring(Texture source)
    {
        if (renderTexture == null)
        {
            renderTexture = new RenderTexture(source.width, source.height, 0);
        }
        RenderTexture.active = renderTexture;
        Graphics.Blit(source, renderTexture, coloringMat);
    }

    int mask;
    Color targetColor;

    public void SetColorIndex(int index )
    {
        mask = index;
    }

    public void SetTargetColor(Color color)
    {
        targetColor = color;
        //print($"Current Color is {sourceColors[mask]}  targetColor is  {targetColor}");
        ChangeColor(sourceColors[mask], targetColor, mask);

    }

    private void Awake()
    {
        ApplyMask();
        InitMatrices();
        ApplyMainTexture();
    }

    List<Matrix4x4> _MixingMatrices;
    public void ApplyMainTexture()
    {
        targetMat.SetTexture("_MainTex", _MainTexture);
    }

    public void ApplyMask()
    {
        coloringMat.SetTexture("_RecolorMask", _RecolorMask);
    }

    public void ApplyMatrix()
    {
        coloringMat.SetMatrixArray("_MixingMatrices", _MixingMatrices);
    }

    void InitMatrices()
    {
        _MixingMatrices = new List<Matrix4x4>();
        for (int i = 0; i < 3; i++)
        {
            Vector4 r = new Vector4(1, 0, 0, 0);
            Vector4 g = new Vector4(0, 1, 0, 0);
            Vector4 b = new Vector4(0, 0, 1, 0);
            Matrix4x4 color = new Matrix4x4(r, g, b, Vector4.zero);

            _MixingMatrices.Add(color);
        }
    }

    public void ChangeColor(Color SourceColor,Color TargetColor,int mask)
    {
        if (_MixingMatrices != null)
        {
            Matrix4x4 channelParams = CalcMixChannelParams(SourceColor, TargetColor);
            _MixingMatrices[mask] = channelParams;
            ApplyMatrix();
            Coloring(sourceTex);
        }
   
    }

    Matrix4x4 CalcMixChannelParams(Color sourceColor, Color targetColor)
    {
        Vector4 ret1 = CalcMixChannelParamEx(sourceColor, targetColor[0], 0);

        Vector4 ret2 = CalcMixChannelParamEx(sourceColor, targetColor[1], 1);

        Vector4 ret3 = CalcMixChannelParamEx(sourceColor, targetColor[2], 2);



        Matrix4x4 ret = new Matrix4x4();
        //new Matrix4x4(
        //      ret1,
        //      ret2,
        //      ret3,
        //      Vector4.zero
        //        );

        ret.SetRow(0, ret1);
        ret.SetRow(1, ret2);
        ret.SetRow(2, ret3);

        float R = ret1.x * sourceColor.r + ret1.y * sourceColor.g + ret1.z * sourceColor.b + ret1.w;
        float G = ret2.x * sourceColor.r + ret2.y * sourceColor.g + ret2.z * sourceColor.b + ret2.w;
        float B = ret3.x * sourceColor.r + ret3.y * sourceColor.g + ret3.z * sourceColor.b + ret3.w;
        Color calcColor = new Color(R, G, B);

        Debug.Log(
            //ret+"\n"+ 
            $"sourceColor: {sourceColor}  targetColor: { targetColor}   calcColor: {calcColor}");
        return ret;
    }

    Vector4 CalcMixChannelParam(Color sourceColor, float targetChannelValue, int targetIndex)
    {
        Vector4 ret = Vector4.zero;
        ret[targetIndex] = 1.0f;
        List<float> sourceColor2 = new List<float>() { sourceColor.r, sourceColor.g, sourceColor.b };
        List<float> sourceColorList = new List<float>() { sourceColor.r, sourceColor.g, sourceColor.b };
        sourceColor2.Sort();
        float maxLimit = sourceColor2.Sum() * 2;
        if (targetChannelValue > maxLimit)
        {
            return new Vector4(2.0f, 2.0f, 2.0f, (targetChannelValue - maxLimit) / 255.0f);
        }

        float currentChannelValue = sourceColor[targetIndex];
        if (currentChannelValue < targetChannelValue)
        {
            for (int i = 0; i < sourceColor2.Count; i++)
            {
                int retIdx = sourceColorList.IndexOf(sourceColor2[2 - i]);
                float diff = targetChannelValue - currentChannelValue;
                float mRatio = 2.0f;
                if (retIdx == targetIndex)
                {
                    mRatio = 1.0f;
                }
                float r = diff / sourceColor2[2 - i];
                if (r > mRatio)
                {
                    currentChannelValue += (mRatio * sourceColor[2 - i]);
                    ret[retIdx] = 2.0f;
                }
                else
                {
                    ret[retIdx] += r;
                    break;
                }
            }
        }
        else if (currentChannelValue > targetChannelValue)
        {
            float r = targetChannelValue / currentChannelValue;
            ret[targetIndex] = r;
        }
        return ret;
    }

    bool isFloatZero(float value)
    {
        return Mathf.Abs(value) < float.MinValue;
    }

    Vector4 CalcMixChannelParamEx(Color sourceColor, float targetChannelColorValue, int channel)
    {
        //sourceColor *= 255;
        //targetChannelColorValue *= 255;
        Vector4 ret = Vector4.zero;
        Vector3 colorVec = new Vector3(sourceColor.r, sourceColor.g, sourceColor.b);

        List<Vector3> paramPresetList = new List<Vector3>();
        paramPresetList.Add(new Vector3(2, -0.5f, -0.5f));
        paramPresetList.Add(new Vector3(-0.5f, 2, -0.5f));
        paramPresetList.Add(new Vector3(-0.5f, -0.5f, 2));

        float dis0 = targetChannelColorValue- Vector3.Dot(colorVec, paramPresetList[0]);
        float dis1 = targetChannelColorValue -Vector3.Dot(colorVec, paramPresetList[1]);
        float dis2 = targetChannelColorValue -Vector3.Dot(colorVec, paramPresetList[2]);

        List<float> disArray = new List<float>() { dis0, dis1, dis2 };
        List<float> AbsDisArray = new List<float>() { Mathf.Abs(dis0), Mathf.Abs(dis1), Mathf.Abs(dis2) };

        int index = AbsDisArray.IndexOf(AbsDisArray.Min());
        ret.x = paramPresetList[index].x;
        ret.y = paramPresetList[index].y;
        ret.z = paramPresetList[index].z;

        ret.w = disArray[index];/// 255.0f;

        return ret;
    }

}
