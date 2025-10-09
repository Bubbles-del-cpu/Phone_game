Shader "UI/EdgeColorFill"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _AspectRatio ("Image Aspect Ratio", Float) = 1.0
        _ScreenAspectRatio ("Screen Aspect Ratio", Float) = 1.0
        _EdgeSampleWidth ("Edge Sample Width", Range(0.01, 0.2)) = 0.05
        _Darkening ("Edge Darkening", Range(0, 1)) = 0.2

        _Radius ("Radius", Range(0,30)) = 15
        resolution ("Resolution", float) = 800
        hstep("HorizontalStep", Range(0,1)) = 0.5
        vstep("VerticalStep", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }

        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _AspectRatio;
            float _ScreenAspectRatio;
            float _EdgeSampleWidth;
            float _Darkening;

            float _Radius;
            float resolution;
            float hstep;
            float vstep;


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            // Sample average color from edge region
            fixed4 sampleEdgeAverage(float2 edgeUV, bool isHorizontal)
            {
                fixed4 avgColor = fixed4(0, 0, 0, 0);
                int samples = 20;

                [unroll]
                for (int i = 0; i < 20; i++)
                {
                    float offset = (float(i) / 19.0 - 0.5) * _EdgeSampleWidth;
                    float2 sampleUV = edgeUV;

                    if (isHorizontal)
                    {
                        sampleUV.x += offset;
                        sampleUV.x = clamp(sampleUV.x, 0.0, 1.0);
                    }
                    else
                    {
                        sampleUV.y += offset;
                        sampleUV.y = clamp(sampleUV.y, 0.0, 1.0);
                    }

                    avgColor += tex2D(_MainTex, sampleUV);
                }

                return avgColor / samples;
            }

            fixed4 blurEdge(float2 uv)
            {
                float4 sum = float4(0.0, 0.0, 0.0, 0.0);
                float2 tc = uv;

                //blur radius in pixels
                float blur = _Radius/resolution/4;

                // Gaussian blur sampling
                sum += tex2D(_MainTex, float2(tc.x - 4.0*blur*hstep, tc.y - 4.0*blur*vstep)) * 0.0162162162;
                sum += tex2D(_MainTex, float2(tc.x - 3.0*blur*hstep, tc.y - 3.0*blur*vstep)) * 0.0540540541;
                sum += tex2D(_MainTex, float2(tc.x - 2.0*blur*hstep, tc.y - 2.0*blur*vstep)) * 0.1216216216;
                sum += tex2D(_MainTex, float2(tc.x - 1.0*blur*hstep, tc.y - 1.0*blur*vstep)) * 0.1945945946;

                sum += tex2D(_MainTex, float2(tc.x, tc.y)) * 0.2270270270;

                sum += tex2D(_MainTex, float2(tc.x + 1.0*blur*hstep, tc.y + 1.0*blur*vstep)) * 0.1945945946;
                sum += tex2D(_MainTex, float2(tc.x + 2.0*blur*hstep, tc.y + 2.0*blur*vstep)) * 0.1216216216;
                sum += tex2D(_MainTex, float2(tc.x + 3.0*blur*hstep, tc.y + 3.0*blur*vstep)) * 0.0540540541;

                return sum;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Calculate where the preserved aspect image would be
                float aspectDiff = _ScreenAspectRatio / _AspectRatio;

                float2 preservedUV = i.uv;
                float edgeMin, edgeMax;
                float distanceFromEdge = 0;
                bool inBlurZone = false;

                if (aspectDiff > 1.0)
                {
                    // Screen is wider - black bars on top/bottom
                    float scale = aspectDiff;
                    preservedUV.y = (i.uv.y - 0.5) * scale + 0.5;
                    edgeMin = (1.0 - 1.0/scale) * 0.5;
                    edgeMax = 1.0 - edgeMin;

                    // Check if we're in the black bar area (top or bottom)
                    if (i.uv.y < edgeMin)
                    {
                        inBlurZone = true;
                        distanceFromEdge = (edgeMin - i.uv.y) / edgeMin;

                        // Sample from top edge
                        float2 edgeUV = float2(i.uv.x, 0.0);
                        fixed4 edgeColor = blurEdge(edgeUV); //sampleEdgeAverage(edgeUV, true);

                        // Apply darkening based on distance from edge
                        edgeColor.rgb *= (1.0 - _Darkening * distanceFromEdge);

                        return edgeColor * i.color;
                    }
                    else if (i.uv.y > edgeMax)
                    {
                        inBlurZone = true;
                        distanceFromEdge = (i.uv.y - edgeMax) / (1.0 - edgeMax);

                        // Sample from bottom edge
                        float2 edgeUV = float2(i.uv.x, 1.0);
                        fixed4 edgeColor = blurEdge(edgeUV); //sampleEdgeAverage(edgeUV, true);

                        // Apply darkening based on distance from edge
                        edgeColor.rgb *= (1.0 - _Darkening * distanceFromEdge);

                        return edgeColor * i.color;
                    }
                }
                else
                {
                    // Screen is taller - black bars on left/right
                    float scale = 1.0 / aspectDiff;
                    preservedUV.x = (i.uv.x - 0.5) * scale + 0.5;
                    edgeMin = (1.0 - 1.0/scale) * 0.5;
                    edgeMax = 1.0 - edgeMin;

                    // Check if we're in the black bar area (left or right)
                    if (i.uv.x < edgeMin)
                    {
                        inBlurZone = true;
                        distanceFromEdge = (edgeMin - i.uv.x) / edgeMin;

                        // Sample from left edge
                        float2 edgeUV = float2(0.0, i.uv.y);
                        fixed4 edgeColor = sampleEdgeAverage(edgeUV, false);

                        // Apply darkening based on distance from edge
                        edgeColor.rgb *= (1.0 - _Darkening * distanceFromEdge);

                        return edgeColor * i.color;
                    }
                    else if (i.uv.x > edgeMax)
                    {
                        inBlurZone = true;
                        distanceFromEdge = (i.uv.x - edgeMax) / (1.0 - edgeMax);

                        // Sample from right edge
                        float2 edgeUV = float2(1.0, i.uv.y);
                        fixed4 edgeColor = sampleEdgeAverage(edgeUV, false);

                        // Apply darkening based on distance from edge
                        edgeColor.rgb *= (1.0 - _Darkening * distanceFromEdge);

                        return edgeColor * i.color;
                    }
                }

                // In the main image area - show sharp
                return tex2D(_MainTex, i.uv) * i.color;
            }
            ENDCG
        }
    }
}