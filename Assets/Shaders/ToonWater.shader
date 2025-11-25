Shader "Custom/ToonWaterURP"
{
    Properties
    {
        _DepthGradientShallow("Depth Gradient Shallow", Color) = (0.325, 0.807, 0.971, 0.725)
        _DepthGradientDeep("Depth Gradient Deep", Color) = (0.086, 0.407, 1, 0.749)

        _DepthMaxDistance("Depth Maximum Distance", Float) = 1

        _FoamColor("Foam Color", Color) = (1,1,1,1)

        _SurfaceNoise("Surface Noise", 2D) = "white" {}
        _SurfaceNoiseScroll("Surface Noise Scroll Amount", Vector) = (0.03, 0.03, 0, 0)
        _SurfaceNoiseCutoff("Surface Noise Cutoff", Range(0,1)) = 0.777

        _SurfaceDistortion("Surface Distortion", 2D) = "white" {}
        _SurfaceDistortionAmount("Surface Distortion Amount", Range(0, 1)) = 0.27

        _FoamMaxDistance("Foam Maximum Distance", Float) = 0.4
        _FoamMinDistance("Foam Minimum Distance", Float) = 0.04
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "Forward"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #define SMOOTHSTEP_AA 0.01

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uvNoise : TEXCOORD0;
                float2 uvDistort : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
                float3 viewNormal : TEXCOORD3;
            };

            TEXTURE2D(_SurfaceNoise);
            SAMPLER(sampler_SurfaceNoise);

            TEXTURE2D(_SurfaceDistortion);
            SAMPLER(sampler_SurfaceDistortion);

            TEXTURE2D_X_FLOAT(_CameraDepthTexture);
            SAMPLER(sampler_CameraDepthTexture);

            float4 _DepthGradientShallow;
            float4 _DepthGradientDeep;

            float4 _FoamColor;

            float _DepthMaxDistance;
            float _FoamMaxDistance;
            float _FoamMinDistance;

            float _SurfaceNoiseCutoff;
            float _SurfaceDistortionAmount;

            float2 _SurfaceNoiseScroll;

            float4 _SurfaceNoise_ST;
            float4 _SurfaceDistortion_ST;

            Varyings vert(Attributes v)
            {
                Varyings o;

                VertexPositionInputs pos = GetVertexPositionInputs(v.positionOS.xyz);
                VertexNormalInputs normal = GetVertexNormalInputs(v.normalOS);

                o.positionCS = pos.positionCS;
                o.screenPos = ComputeScreenPos(o.positionCS);

                o.viewNormal = normal.normalWS;

                o.uvNoise = TRANSFORM_TEX(v.uv, _SurfaceNoise);
                o.uvDistort = TRANSFORM_TEX(v.uv, _SurfaceDistortion);

                return o;
            }

            float4 alphaBlend(float4 top, float4 bottom)
            {
                float3 c = top.rgb * top.a + bottom.rgb * (1 - top.a);
                float a = top.a + bottom.a * (1 - top.a);
                return float4(c, a);
            }

            float4 frag(Varyings i) : SV_Target
            {
                float2 screenUV = i.screenPos.xy / i.screenPos.w;

                float rawDepth = SAMPLE_TEXTURE2D_X(_CameraDepthTexture, sampler_CameraDepthTexture, screenUV).r;

                float sceneDepth = LinearEyeDepth(rawDepth, _ZBufferParams);

                float waterDepth = i.positionCS.w;

                float depthDifference = sceneDepth - waterDepth;

                float waterDepth01 = saturate(depthDifference / _DepthMaxDistance);
                float4 waterColor = lerp(_DepthGradientShallow, _DepthGradientDeep, waterDepth01);

                float foamDepth = saturate(depthDifference / lerp(_FoamMaxDistance, _FoamMinDistance, saturate(i.viewNormal.z)));

                float2 distort = (SAMPLE_TEXTURE2D(_SurfaceDistortion, sampler_SurfaceDistortion, i.uvDistort).xy * 2 - 1)
                                * _SurfaceDistortionAmount;

                float2 uvNoise = float2(
                        i.uvNoise.x + _Time.y * _SurfaceNoiseScroll.x + distort.x,
                        i.uvNoise.y + _Time.y * _SurfaceNoiseScroll.y + distort.y
                    );

                float noiseSample = SAMPLE_TEXTURE2D(_SurfaceNoise, sampler_SurfaceNoise, uvNoise).r;

                float cutoff = foamDepth * _SurfaceNoiseCutoff;
                float foam = smoothstep(cutoff - SMOOTHSTEP_AA, cutoff + SMOOTHSTEP_AA, noiseSample);

                float4 foamColor = _FoamColor;
                foamColor.a *= foam;

                return alphaBlend(foamColor, waterColor);
            }

            ENDHLSL
        }
    }
}
