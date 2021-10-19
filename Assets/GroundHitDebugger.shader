Shader "Unlit/GroundHitDebugger"
{
    Properties
    {
        _x ("X", Float) = 0.5
        _y ("Y", Float) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };
            
            float _x;
            float _y;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return float4(_x, _y, 0, 1);
            }
            ENDCG
        }
    }
}
