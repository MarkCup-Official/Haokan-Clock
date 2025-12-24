Shader "URP2D/MetaballSDF_One_World_Transparent"
{
    Properties
    {
        _CenterWS ("Center WS (XY)", Vector) = (0, 0, 0, 0)

        _SolidRadius ("Solid Radius (World)", Float) = 0.5
        _GradientWidth ("Gradient Width (World)", Float) = 0.25

        _Intensity ("Intensity", Float) = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Pass
        {
            Name "MetaballSDF_World_Transparent"
            ZWrite Off
            ZTest Always
            Cull Off

            // 标准透明混合
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS  : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _CenterWS;
                float  _SolidRadius;
                float  _GradientWidth;
                float  _Intensity;
            CBUFFER_END

            Varyings vert (Attributes v)
            {
                Varyings o;
                float3 ws = TransformObjectToWorld(v.positionOS.xyz);
                o.positionWS  = ws;
                o.positionHCS = TransformWorldToHClip(ws);
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                float2 p = i.positionWS.xy;
                float2 c = _CenterWS.xy;

                float d = distance(p, c);

                float solid = max(_SolidRadius, 0.0);
                float width = max(_GradientWidth, 1e-6);

                // 0 at solid edge, 1 at outer edge
                float t = saturate((d - solid) / width);

                // 强度场：中心=1，外=0
                float v = 1.0 - smoothstep(0.0, 1.0, t);
                v *= _Intensity;

                // 黑色=透明
                return half4(1, 1, 1, v);
            }

            ENDHLSL
        }
    }
}
