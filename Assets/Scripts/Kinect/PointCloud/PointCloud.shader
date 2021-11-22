Shader "Random Entity/PointCloud"
{
    Properties
    {
        _DepthTexture ("Depth Texture", 2D) = "white" {}
        _ColorTexture ("Color Texture", 2D) = "white" {}


        _Displacement ("Displacement", Range(0, 1)) = 0.03
        _Color("Particle color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
    		Cull Off
        
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv_color : TEXCOORD1;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 col : COLOR;
            };

            sampler2D _DepthTexture;
            float4 _DepthTexture_ST;

            sampler2D _ColorTexture;


            float _Displacement;
            fixed4 _Color;

            v2f vert (appdata v)
            {
				float4 col = tex2Dlod(_DepthTexture, float4(v.uv, 0, 0));

                float d = col.x * 4000 * _Displacement;

                v.vertex.x *= d / 3.656;
                v.vertex.y *= d / 3.656;
                v.vertex.z = d;

                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _DepthTexture);

                // o.col = float4(frac(d * 0.25), 0, 1, 1);
                o.col = tex2Dlod(_ColorTexture, float4(v.uv_color, 0, 0));

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 col = i.col;
                
                return col;
            }
            ENDCG
        }
    }
}