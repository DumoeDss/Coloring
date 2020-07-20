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

    int mask;
    Color targetColor;
    List<int> paramPresetIndexList;
    List<Vector3> paramPresetList;
    List<Matrix4x4> _MixingMatrices;

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

    private void Awake()
    {
        paramPresetIndexList = new List<int>();
        for (int i = 0; i < sourceColors.Count; i++)
        {
            paramPresetIndexList.Add(-1);
        }

        paramPresetList = new List<Vector3>();
        paramPresetList.Add(new Vector3(2, -0.5f, -0.5f));
        paramPresetList.Add(new Vector3(-0.5f, 2, -0.5f));
        paramPresetList.Add(new Vector3(-0.5f, -0.5f, 2));

        ApplyMask();
        InitMatrices();
        ApplyMainTexture();
    }

    /// <summary>
    /// 为贴图染色
    /// </summary>
    /// <param name="source"></param>
    public void Coloring(Texture source)
    {
        if (renderTexture == null)
        {
            renderTexture = new RenderTexture(source.width, source.height, 0);
        }
        RenderTexture.active = renderTexture;
        Graphics.Blit(source, renderTexture, coloringMat);
    }

    /// <summary>
    /// 设置mask
    /// </summary>
    /// <param name="index"></param>
    public void SetColorMask(int index )
    {
        mask = index;
    }

    /// <summary>
    /// 设置目标颜色
    /// </summary>
    /// <param name="color"></param>
    public void SetTargetColor(Color color)
    {
        targetColor = color;
        ChangeColor(sourceColors[mask], targetColor, mask);

    }

    /// <summary>
    /// 材质设置贴图
    /// </summary>
    public void ApplyMainTexture()
    {
        targetMat.SetTexture("_MainTex", _MainTexture);
    }

    /// <summary>
    /// 材质设置Mask贴图
    /// </summary>
    public void ApplyMask()
    {
        coloringMat.SetTexture("_RecolorMask", _RecolorMask);
    }

    /// <summary>
    /// 材质设置参数矩阵
    /// </summary>
    public void ApplyMatrix()
    {
        coloringMat.SetMatrixArray("_MixingMatrices", _MixingMatrices);
    }

    /// <summary>
    /// 初始化参数矩阵
    /// </summary>
    void InitMatrices()
    {
        _MixingMatrices = new List<Matrix4x4>();
        for (int i = 0; i < sourceColors.Count; i++)
        {
            Vector4 r = new Vector4(1, 0, 0, 0);
            Vector4 g = new Vector4(0, 1, 0, 0);
            Vector4 b = new Vector4(0, 0, 1, 0);
            Matrix4x4 color = new Matrix4x4(r, g, b, Vector4.zero);

            _MixingMatrices.Add(color);
        }
    }

    /// <summary>
    /// 设置参数矩阵，改变颜色
    /// </summary>
    /// <param name="SourceColor">源颜色</param>
    /// <param name="TargetColor">目标颜色</param>
    /// <param name="index">参数矩阵索引</param>
    public void ChangeColor(Color SourceColor,Color TargetColor,int index)
    {
        if (_MixingMatrices != null)
        {
            Matrix4x4 channelParams = CalcMixChannelParams(SourceColor, TargetColor);
            _MixingMatrices[index] = channelParams;
            ApplyMatrix();
            Coloring(sourceTex);
        }
   
    }

    /// <summary>
    /// 计算参数矩阵
    /// </summary>
    /// <param name="sourceColor">源颜色</param>
    /// <param name="targetColor">目标颜色</param>
    /// <returns>参数矩阵</returns>
    Matrix4x4 CalcMixChannelParams(Color sourceColor, Color targetColor)
    {
        Vector4 ret1 = CalcMixChannelParamEx(sourceColor, targetColor[0], 0);

        Vector4 ret2 = CalcMixChannelParamEx(sourceColor, targetColor[1], 1);

        Vector4 ret3 = CalcMixChannelParamEx(sourceColor, targetColor[2], 2);

        Matrix4x4 ret = new Matrix4x4();

        ret.SetRow(0, ret1);
        ret.SetRow(1, ret2);
        ret.SetRow(2, ret3);

        #region 用于测试知乎提供的算法
        //ret.SetColumn(0, ret1);
        //ret.SetColumn(1, ret2);
        //ret.SetColumn(2, ret3);
        #endregion      


        #region Debug
        //float R = ret1.x * sourceColor.r + ret1.y * sourceColor.g + ret1.z * sourceColor.b + ret1.w;
        //float G = ret2.x * sourceColor.r + ret2.y * sourceColor.g + ret2.z * sourceColor.b + ret2.w;
        //float B = ret3.x * sourceColor.r + ret3.y * sourceColor.g + ret3.z * sourceColor.b + ret3.w;
        //Color calcColor = new Color(R, G, B);
        //Debug.Log(
        //    ret + "\n" +
        //    $"sourceColor: {sourceColor}  targetColor: { targetColor}   calcColor: {calcColor}");
        #endregion

        return ret;
    }

    #region 知乎提供的通道参数计算算法，源色为深色的话不起作用
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
    #endregion

    /// <summary>
    /// 计算目标通道的参数
    /// 原本每个通道需要计算r,g,b,const四个值
    /// 但是理论上，通道调色结果要保证 r+g+b=1，不然颜色“不正”
    /// 那么直接取r,g,b为-0.5,-0.5，2这三个值，剩余的值靠const补齐。
    /// 这样问题就简化为找出-0.5,-0.5，2这三个值与r,g,b通道对应关系。
    /// 即求三种对应关系下 const = targte - r*R + g*G + b*B 的最小值
    /// </summary>
    /// <param name="sourceColor">源颜色</param>
    /// <param name="targetChannelColorValue">该通道的目标值</param>
    /// <param name="channel">通道索引(r,g,b)</param>
    /// <returns>目标通道的参数</returns>
    Vector4 CalcMixChannelParamEx(Color sourceColor, float targetChannelColorValue, int channel)
    {
        Vector4 ret = Vector4.zero;
        Vector3 colorVec = new Vector3(sourceColor.r, sourceColor.g, sourceColor.b);

        #region 如果每次都计算的话，当参数处于临界值跳动时，会发生跳色情况
        //float dis0 = targetChannelColorValue - Vector3.Dot(colorVec, paramPresetList[0]);
        //float dis1 = targetChannelColorValue - Vector3.Dot(colorVec, paramPresetList[1]);
        //float dis2 = targetChannelColorValue - Vector3.Dot(colorVec, paramPresetList[2]);

        //List<float> disArray = new List<float>() { dis0, dis1, dis2 };
        //List<float> AbsDisArray = new List<float>() { Mathf.Abs(dis0), Mathf.Abs(dis1), Mathf.Abs(dis2) };

        //int index = AbsDisArray.IndexOf(AbsDisArray.Min());

        //ret.x = paramPresetList[index].x;
        //ret.y = paramPresetList[index].y;
        //ret.z = paramPresetList[index].z;
        //ret.w = disArray[index];
        #endregion

        #region 改为每个Mask的颜色只计算一次rgb通道参数，然后修改const值
        float constValue = 0;
        if (paramPresetIndexList[channel] < 0)
        {
            float dis0 = targetChannelColorValue - Vector3.Dot(colorVec, paramPresetList[0]);
            float dis1 = targetChannelColorValue - Vector3.Dot(colorVec, paramPresetList[1]);
            float dis2 = targetChannelColorValue - Vector3.Dot(colorVec, paramPresetList[2]);

            List<float> disArray = new List<float>() { dis0, dis1, dis2 };
            List<float> AbsDisArray = new List<float>() { Mathf.Abs(dis0), Mathf.Abs(dis1), Mathf.Abs(dis2) };
            paramPresetIndexList[channel] = AbsDisArray.IndexOf(AbsDisArray.Min());
            constValue = disArray[paramPresetIndexList[channel]];
        }
        else
        {
            constValue = targetChannelColorValue - Vector3.Dot(colorVec, paramPresetList[paramPresetIndexList[channel]]);
        }

        ret.x = paramPresetList[paramPresetIndexList[channel]].x;
        ret.y = paramPresetList[paramPresetIndexList[channel]].y;
        ret.z = paramPresetList[paramPresetIndexList[channel]].z;
        ret.w = constValue;
        #endregion

        return ret;
    }

}
