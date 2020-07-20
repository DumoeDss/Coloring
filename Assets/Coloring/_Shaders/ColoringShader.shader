Shader "Unlit/ColoringShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _RecolorMask("_RecolorMask",2D) = "white"{}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                //UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _RecolorMask;
            float4 _MainTex_ST;
            Matrix _MixingMatrices[3];
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            inline fixed3 channelMixing(fixed3 src, float2 uv)
            {
                half bias = 0.1f;
                fixed4 mask = tex2D(_RecolorMask, uv);
                int maskIndex = (int)(step(bias, mask.r)
                    + step(bias, mask.g) * 2
                    + step(bias, mask.b) * 4);
                fixed4x4 mixingMatrix = _MixingMatrices[maskIndex];

                fixed3 dst = mul((fixed3x3)mixingMatrix, src);
                dst.r += mixingMatrix[0].w;
                dst.g += mixingMatrix[1].w;
                dst.b += mixingMatrix[2].w;
                return dst;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed3 coloring = channelMixing(col.rgb, i.uv);
                col = fixed4(coloring, col.a);
                return col;
            }
            ENDCG
        }
    }
}
