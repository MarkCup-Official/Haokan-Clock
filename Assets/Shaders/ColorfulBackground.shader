Shader "Custom/ColorfulBackground"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (0.89, 0.58, 0.82, 1) // #e493d0
        
        _HueShift ("Hue Shift", Range(0, 1)) = 0
        _Saturation ("Saturation", Range(0, 2)) = 1
        
        [Header(Ball 1)]
        _Ball1Pos ("Ball 1 Position (UV)", Vector) = (-0.8, -0.8, 0, 0)
        _Ball1Size ("Ball 1 Size", Float) = 1.3
        _Ball1Color ("Ball 1 Color", Color) = (0.92, 0.41, 0.31, 1) // #eb694e
        
        [Header(Ball 2)]
        _Ball2Pos ("Ball 2 Position (UV)", Vector) = (0.6, -0.3, 0, 0)
        _Ball2Size ("Ball 2 Size", Float) = 0.8
        _Ball2Color ("Ball 2 Color", Color) = (0.95, 0.04, 0.64, 1) // #f30ba4
        
        [Header(Ball 3)]
        _Ball3Pos ("Ball 3 Position (UV)", Vector) = (0.1, 0.1, 0, 0)
        _Ball3Size ("Ball 3 Size", Float) = 0.9
        _Ball3Color ("Ball 3 Color", Color) = (1, 0.92, 0.51, 1) // #feea83
        
        [Header(Ball 4)]
        _Ball4Pos ("Ball 4 Position (UV)", Vector) = (-0.3, -0.1, 0, 0)
        _Ball4Size ("Ball 4 Size", Float) = 1.1
        _Ball4Color ("Ball 4 Color", Color) = (0.67, 0.56, 0.96, 1) // #aa8ef5
        
        [Header(Ball 5)]
        _Ball5Pos ("Ball 5 Position (UV)", Vector) = (0.5, 0.5, 0, 0)
        _Ball5Size ("Ball 5 Size", Float) = 0.9
        _Ball5Color ("Ball 5 Color", Color) = (0.97, 0.75, 0.58, 1) // #f8c093

        [Header(Posterization and Outline)]
        _PosterizeSteps ("Steps", Range(2, 20)) = 5
        _OutlineWidth ("Outline Width", Range(0, 0.5)) = 0.05
        _OutlineColor ("Outline Color", Color) = (1, 1, 1, 1)
        _OutlineSmoothness ("Outline Smoothness", Range(0, 0.1)) = 0.01
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline" = "UniversalPipeline" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _BaseColor;
            float _HueShift;
            float _Saturation;

            float2 _Ball1Pos, _Ball2Pos, _Ball3Pos, _Ball4Pos, _Ball5Pos;
            float _Ball1Size, _Ball2Size, _Ball3Size, _Ball4Size, _Ball5Size;
            float4 _Ball1Color, _Ball2Color, _Ball3Color, _Ball4Color, _Ball5Color;

            float _PosterizeSteps;
            float _OutlineWidth;
            float4 _OutlineColor;
            float _OutlineSmoothness;

            // Simple Hue shift function
            float3 HueShift(float3 Color, float Shift)
            {
                float3 k = float3(0.57735, 0.57735, 0.57735);
                float cosAngle = cos(Shift * 6.28318530718);
                return Color * cosAngle + cross(k, Color) * sin(Shift * 6.28318530718) + k * dot(k, Color) * (1.0 - cosAngle);
            }

            Varyings vert (Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                return output;
            }

            float4 frag (Varyings input) : SV_Target
            {
                // Use World Position XY for ball calculations
                float2 p = input.positionWS.xy;
                
                // Calculate individual ball influences
                float d1 = distance(p, _Ball1Pos);
                float alpha1 = smoothstep(_Ball1Size, 0, d1) * _Ball1Color.a;
                
                float d2 = distance(p, _Ball2Pos);
                float alpha2 = smoothstep(_Ball2Size, 0, d2) * _Ball2Color.a;
                
                float d3 = distance(p, _Ball3Pos);
                float alpha3 = smoothstep(_Ball3Size, 0, d3) * _Ball3Color.a;
                
                float d4 = distance(p, _Ball4Pos);
                float alpha4 = smoothstep(_Ball4Size, 0, d4) * _Ball4Color.a;
                
                float d5 = distance(p, _Ball5Pos);
                float alpha5 = smoothstep(_Ball5Size, 0, d5) * _Ball5Color.a;

                // Total influence for posterization
                float totalAlpha = alpha1 + alpha2 + alpha3 + alpha4 + alpha5;
                float clampedAlpha = saturate(totalAlpha);
                
                // Posterization logic
                float steps = max(2.0, _PosterizeSteps);
                float posterizedAlpha = floor(clampedAlpha * steps) / (steps - 1.0);
                posterizedAlpha = saturate(posterizedAlpha);

                // Calculate which color to use based on relative weights
                // We use the raw alphas to blend the colors first
                float3 mixedBallColor = float3(0,0,0);
                if(totalAlpha > 0.001)
                {
                    mixedBallColor = (_Ball1Color.rgb * alpha1 + 
                                      _Ball2Color.rgb * alpha2 + 
                                      _Ball3Color.rgb * alpha3 + 
                                      _Ball4Color.rgb * alpha4 + 
                                      _Ball5Color.rgb * alpha5) / totalAlpha;
                }
                
                // Final color before outline
                float3 finalColor = lerp(_BaseColor.rgb, mixedBallColor, posterizedAlpha);

                // Edge Outline logic
                // The transitions happen when (clampedAlpha * steps) is an integer
                float edgeValue = clampedAlpha * steps;
                float f = frac(edgeValue);
                // We want a peak at f=0 and f=1
                // Dist from nearest integer
                float distToEdge = min(f, 1.0 - f);
                float outlineMask = smoothstep(_OutlineWidth + _OutlineSmoothness, _OutlineWidth, distToEdge);
                
                // Don't draw outline at the very center (totalAlpha > 0.99) or very edge (totalAlpha < 0.01) if needed, 
                // but usually it looks better everywhere
                finalColor = lerp(finalColor, _OutlineColor.rgb, outlineMask * _OutlineColor.a);

                // Apply Hue Shift
                finalColor = HueShift(finalColor, _HueShift);
                
                // Saturation adjustment
                float luma = dot(finalColor, float3(0.299, 0.587, 0.114));
                finalColor = lerp(float3(luma, luma, luma), finalColor, _Saturation);

                return float4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}

