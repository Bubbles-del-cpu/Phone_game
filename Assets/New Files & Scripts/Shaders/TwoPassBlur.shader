Shader "UI/TwoPassBlur"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _BlurSize ("Blur Size", Range(0, 10)) = 1.0

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

        CGINCLUDE
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
        fixed4 _TextureSampleAdd;
        float4 _ClipRect;
        float4 _MainTex_TexelSize;
        float _BlurSize;

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

        fixed4 GaussianBlur(float2 uv, float2 direction)
        {
            float2 texelSize = _MainTex_TexelSize.xy * _BlurSize;
            float2 offset = direction * texelSize;

            // 9-tap Gaussian kernel
            fixed4 color = tex2D(_MainTex, uv) * 0.2270270270;

            color += tex2D(_MainTex, uv + offset * 1.0) * 0.1945945946;
            color += tex2D(_MainTex, uv - offset * 1.0) * 0.1945945946;

            color += tex2D(_MainTex, uv + offset * 2.0) * 0.1216216216;
            color += tex2D(_MainTex, uv - offset * 2.0) * 0.1216216216;

            color += tex2D(_MainTex, uv + offset * 3.0) * 0.0540540541;
            color += tex2D(_MainTex, uv - offset * 3.0) * 0.0540540541;

            color += tex2D(_MainTex, uv + offset * 4.0) * 0.0162162162;
            color += tex2D(_MainTex, uv - offset * 4.0) * 0.0162162162;

            return color;
        }
        ENDCG

        // Pass 0: Horizontal Blur
        Pass
        {
            Name "HorizontalBlur"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 color = GaussianBlur(IN.texcoord, float2(1, 0));
                color += _TextureSampleAdd;
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

        // Pass 1: Vertical Blur
        Pass
        {
            Name "VerticalBlur"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 color = GaussianBlur(IN.texcoord, float2(0, 1));
                color += _TextureSampleAdd;
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