Shader "URP2D/ContourFillRipple"
{
    Properties
    {
        _DistTex ("Distance (R=px)", 2D) = "black" {}
        _MaskTex ("Mask (optional)", 2D) = "black" {}
        _MaskThreshold ("Mask Threshold", Range(0,1)) = 0.5
        _UseMaskClip ("Clip Inside Text (1=on)", Float) = 1

        _PeriodPx ("Contour Period (px)", Float) = 16
        _LineWidthPx ("Line Width (px)", Float) = 1.5
        _SpeedPx ("Expand Speed (px/sec)", Float) = 30

        _MaxDistPx ("Fade Out Distance (px)", Float) = 300
        _Opacity ("Opacity", Range(0,1)) = 1

        _LineColor ("Line Color", Color) = (1,1,1,1)

        _UsePalette ("Use Palette (1=on)", Float) = 1
        _FillColorA ("Fill Color A", Color) = (0.2,0.6,1,1)
        _FillColorB ("Fill Color B", Color) = (0.1,0.3,0.8,1)
        _PaletteTex ("Palette (Repeat)", 2D) = "white" {}
        _PaletteTiling ("Palette Tiling", Float) = 0.12
        _PaletteOffset ("Palette Offset", Float) = 0

        _DistToScreenScale ("Dist->Screen Scale", Float) = 1
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
            Name "ContourFill"
            Tags { "LightMode"="Universal2D" }

            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

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
                float2 uv          : TEXCOORD0;
            };

            TEXTURE2D(_DistTex);    SAMPLER(sampler_DistTex);
            TEXTURE2D(_MaskTex);    SAMPLER(sampler_MaskTex);
            TEXTURE2D(_PaletteTex); SAMPLER(sampler_PaletteTex);

            float _MaskThreshold;
            float _UseMaskClip;

            float _PeriodPx;
            float _LineWidthPx;
            float _SpeedPx;

            float _MaxDistPx;
            float _Opacity;

            float4 _LineColor;

            float _UsePalette;
            float4 _FillColorA;
            float4 _FillColorB;
            float _PaletteTiling;
            float _PaletteOffset;

            float _DistToScreenScale;

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionHCS = TransformObjectToHClip(v.positionOS.xyz);
                o.uv = v.uv;
                return o;
            }

            float3 GetFillColor(float bandIndex)
            {
                if (_UsePalette > 0.5)
                {
                    // 使用 frac 确保 UV 在 0-1 之间，即便 index 为负
                    float u = frac(bandIndex * _PaletteTiling + _PaletteOffset);
                    return SAMPLE_TEXTURE2D(_PaletteTex, sampler_PaletteTex, float2(u, 0.5)).rgb;
                }
                else
                {
                    // 使用 frac(x * 0.5) 代替 fmod 处理负数
                    float sel = step(0.5, frac(bandIndex * 0.5));
                    return lerp(_FillColorA.rgb, _FillColorB.rgb, sel);
                }
            }

            half4 frag (Varyings i) : SV_Target
            {
                if (_UseMaskClip > 0.5)
                {
                    float m = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, i.uv).r;
                    if (m > _MaskThreshold) discard;
                }

                float distPx = SAMPLE_TEXTURE2D(_DistTex, sampler_DistTex, i.uv).r * _DistToScreenScale;

                // 移除 discard 逻辑，解决空缺不断往外扩大的问题
                float d = distPx - _Time.y * _SpeedPx;

                float fade = saturate(1.0 - distPx / max(_MaxDistPx, 1e-3));

                float period = max(_PeriodPx, 1e-3);
                float t = d / period;
                float f = frac(t);
                float edge = min(f, 1.0 - f);

                float aa = fwidth(t);
                float w = (_LineWidthPx / period);

                float lineMask = 1.0 - smoothstep(w, w + aa, edge);

                float band = floor(t);
                float3 fillCol = GetFillColor(band);

                float3 col = lerp(fillCol, _LineColor.rgb, lineMask);

                float alpha = _Opacity * fade;
                alpha *= lerp(0.85, 1.0, lineMask);

                return half4(col, alpha);
            }
            ENDHLSL
        }
    }
}
