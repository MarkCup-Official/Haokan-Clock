Shader "Hidden/URP2D/ShowRT_CameraUV"
{

    Properties
    {
        _RenderTex ("RenderTex (Optional)", 2D) = "white" {}
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Transparent"
            // 透明渲染队列与类型，便于与其他半透明内容正确混合
            "RenderType"="Transparent"
        }

        Pass
        {
            ZTest Always
            ZWrite Off
            Cull Off
            // 常规透明混合，使用 RT 的 alpha
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_RenderTex);
            SAMPLER(sampler_RenderTex);

            struct Attributes
            {
                float3 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float4 screenPos   : TEXCOORD0;
            };

            Varyings Vert (Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS);
                o.screenPos   = ComputeScreenPos(o.positionHCS);
                return o;
            }

            half4 Frag (Varyings i) : SV_Target
            {
                // 摄像机 UV（0~1）
                float2 camUV = i.screenPos.xy / i.screenPos.w;

                // 采样 RenderTexture
                return SAMPLE_TEXTURE2D(_RenderTex, sampler_RenderTex, camUV);
            }
            ENDHLSL
        }
    }
}
