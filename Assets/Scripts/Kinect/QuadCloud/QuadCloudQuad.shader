Shader "Unlit/QuadCloudQuad"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            #pragma target 5.0

            struct Vertex {
                float3 position;
                float2 uv;
            };

            StructuredBuffer<Vertex> verticesBuffer;

            sampler2D _MainTex;

            struct v2f
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (uint vertex_id : SV_VERTEXID, uint instance_id : SV_INSTANCEID)
            {
                v2f o;

                int index = instance_id * 6 + vertex_id;

                o.position = UnityWorldToClipPos(float4(verticesBuffer[index].position, 1));
                o.uv = verticesBuffer[index].uv;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                return float4(1, 0, 0, 1);
            }
            ENDCG
        }
    }
}
