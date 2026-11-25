Shader "Unlit/UI_Wave"
{
     Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        _Color ("Wave Color", Color) = (0.0, 0.45, 0.65, 1)

        // White Outline
        _OutlineColor ("Outline Color", Color) = (1, 1, 1, 1)
        _OutlineThickness ("Outline Thickness (Pixels)", Range(0, 20)) = 6

        // Wave Settings
        _BaseHeight ("Base Height", Range(0, 1)) = 0.30
        _Amplitude ("Amplitude", Range(0, 0.3)) = 0.06

        _Frequency ("Frequency", Range(0, 20)) = 2.0
        _Speed ("Wave Speed", Range(-5, 5)) = 0.5
        _Phase ("Phase Offset", Range(0, 6.28)) = 0

        // Secondary Waves
        _SecondaryAmplitude ("Secondary Amplitude", Range(0, 0.15)) = 0.025
        _SecondaryFrequency ("Secondary Frequency", Range(0, 30)) = 5.0

        _WaveSharpness ("Edge Smoothness", Range(0.001, 0.05)) = 0.005

        // UI Mask Support
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)]
        _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]

        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #pragma target 3.0

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;

            fixed4 _Color;
            fixed4 _OutlineColor;

            float _OutlineThickness;

            float _BaseHeight;
            float _Amplitude;
            float _Frequency;
            float _Speed;
            float _Phase;

            float _SecondaryAmplitude;
            float _SecondaryFrequency;

            float _WaveSharpness;

            float4 _ClipRect;

            // Vertex Shader
            v2f vert(appdata v)
            {
                v2f o;

                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);

                o.uv = v.uv;
                o.color = v.color * _Color;

                return o;
            }

            // Fragment Shader
            fixed4 frag(v2f i) : SV_Target
            {
                float x = i.uv.x;
                float y = i.uv.y;

                float t = _Time.y * _Speed;

                // Main Wave
                float wave1 = sin(
                    x * _Frequency * 6.28318
                    + t
                    + _Phase
                ) * _Amplitude;

                // Secondary Wave
                float wave2 = sin(
                    x * _SecondaryFrequency * 6.28318
                    - t * 0.7
                    + _Phase * 1.5
                ) * _SecondaryAmplitude;

                // Natural Oscillation
                float wave3 = sin(
                    x * 17.0
                    + t * 0.4
                    + _Phase
                ) * (_Amplitude * 0.12);

                // Final Wave Position
                float waveHeight =
                    _BaseHeight +
                    wave1 +
                    wave2 +
                    wave3;

                // Distance from Wave Surface
                float distance = waveHeight - y;

                // Anti-Aliasing
                float aa = max(
                    fwidth(distance),
                    _WaveSharpness
                );

                // Outline Width in UV Units
                float outlineWidth =
                    _OutlineThickness * fwidth(y);

                // Wave Fill
                float fill = smoothstep(
                    -aa,
                    aa,
                    distance
                );

                // White Outline
                float outline = smoothstep(
                    -outlineWidth - aa,
                    -outlineWidth + aa,
                    distance
                );

                // Final Alpha
                float alpha = smoothstep(
                    -outlineWidth - aa,
                    aa,
                    distance
                );

                // Color Composition
                fixed4 finalColor;

                finalColor.rgb = lerp(
                    _OutlineColor.rgb,
                    i.color.rgb,
                    fill
                );

                finalColor.a = i.color.a * alpha;

                // UI RectMask2D Support
                #ifdef UNITY_UI_CLIP_RECT

                finalColor.a *= UnityGet2DClipping(
                    i.worldPosition.xy,
                    _ClipRect
                );

                #endif

                // UI Alpha Clipping
                #ifdef UNITY_UI_ALPHACLIP

                clip(finalColor.a - 0.001);

                #endif

                return finalColor;
            }

            ENDCG
        }
    }
}
