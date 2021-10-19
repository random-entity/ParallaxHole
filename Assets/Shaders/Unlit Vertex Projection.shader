Shader "Random Entity/Unlit Vertex Projection"
{
    Properties
    {
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

            float4 _Eye;

            struct VertexData
            {
                float4 vertex : POSITION;
            };

            struct Interpolators
            {
                float4 vertex : SV_POSITION;
                // float4 proj : TEXCOORD1;
            };

            Interpolators vert (VertexData v)
            {
                Interpolators o;

                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                float4 proj = lerp(_Eye, worldPos, (_Eye.y / (_Eye.y - worldPos.y)));
                
                o.vertex = mul(UNITY_MATRIX_VP, proj);
                // o.proj = proj;
                // o.vertex = UnityObjectToClipPos(v.vertex);

                return o;
            }

            fixed4 frag (Interpolators i) : SV_Target
            {
                return 1;
            }
            ENDCG
        }
    }
}
