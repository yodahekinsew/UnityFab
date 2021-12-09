Shader "Unlit/RoundedCorners"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _XScale ("X Scale", float) = 1
        _YScale ("Y Scale", float) = 1
        _Radius ("Radius", Range(0, .5)) = .1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

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
            float _XScale;
            float _YScale;
            float _Radius;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            float rectangle(float2 samplePosition, float2 halfSize, float2 center){
                float2 componentWiseEdgeDistance = abs(samplePosition - center) - halfSize;
                float outsideDistance = length(max(componentWiseEdgeDistance, 0));
                float insideDistance = min(max(componentWiseEdgeDistance.x, componentWiseEdgeDistance.y), 0);
                return outsideDistance + insideDistance;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 scale = float2(_XScale, _YScale);
                float2 scaledUV = float2(scale.x*i.uv.x, scale.y*i.uv.y);
                float2 rectCenter = .5f * scale;
                float rect = rectangle(scaledUV, rectCenter - _Radius, rectCenter) < _Radius;
                clip(rect - _Radius);
                return i.color;
            }
            ENDCG
        }
    }
}
