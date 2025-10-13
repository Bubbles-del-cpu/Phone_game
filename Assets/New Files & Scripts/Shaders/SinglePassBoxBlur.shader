Shader "UI/SinglePassBoxBlur"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _BlurSize ("Blur Size", Range(0, 50)) = 5.0
        _SampleCount ("Sample Count", Range(1, 20)) = 5.0

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
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
            Name "CombinedBlur"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            float4 _ClipRect;
            float4 _MainTex_TexelSize;
            float _BlurSize;
            float _SampleCount;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = v.texcoord;
                OUT.color = v.color * _Color;
                return OUT;
            }

            // Box blur - simpler and cheaper than Gaussian
            fixed4 BoxBlur(float2 uv, float2 direction, float size)
            {
                float2 texelSize = _MainTex_TexelSize.xy * size;
                float2 offset = direction * texelSize;

                fixed4 color = fixed4(0, 0, 0, 0);

                // 9-tap box blur
                int samples = 9;
                int halfSamples = samples / 2;

                for (int i = -halfSamples; i <= halfSamples; i++)
                {
                    color += tex2D(_MainTex, uv + offset * float(i));
                }

                return color / float(samples);
            }

            // Combined horizontal and vertical blur in one pass
            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;
                fixed4 color = fixed4(0, 0, 0, 0);

                float2 texelSize = _MainTex_TexelSize.xy * _BlurSize;

                // Sample in a grid pattern (both horizontal and vertical simultaneously)
                int samples = _SampleCount;
                int halfSamples = samples / 2;
                int totalSamples = 0;

                for (int x = -halfSamples; x <= halfSamples; x++)
                {
                    for (int y = -halfSamples; y <= halfSamples; y++)
                    {
                        float2 offset = float2(x, y) * texelSize;
                        color += tex2D(_MainTex, uv + offset);
                        totalSamples++;
                    }
                }

                color /= float(totalSamples);
                color *= IN.color;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}