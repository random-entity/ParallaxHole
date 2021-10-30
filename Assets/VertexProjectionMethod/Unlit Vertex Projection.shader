Shader "Random Entity/Unlit Vertex Projection"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Eye ("Eye World Position", Vector) = (0, 2, 0, 1)
    }
    SubShader
    {
        Pass
        {            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct VertexData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Interpolators
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Eye;

            Interpolators vert (VertexData v)
            {
                Interpolators o;

                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                float4 proj = lerp(_Eye, worldPos, (_Eye.y / (_Eye.y - worldPos.y)));
                o.vertex = mul(UNITY_MATRIX_VP, proj); // World Space to Clip Space
                
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                return o;
            }

            fixed4 frag (Interpolators i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);

                return col;
            }
            ENDCG
        }
    }
}
