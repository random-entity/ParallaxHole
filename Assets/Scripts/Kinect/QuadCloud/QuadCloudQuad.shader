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


            struct Point
            {
                float3 position;
                float4 color;
            };

            struct Vertex {
                float3 position;
                float2 uv;
            };

            StructuredBuffer<Point> pointsBuffer;
            StructuredBuffer<Vertex> verticesBuffer;

            sampler2D _MainTex;

            struct v2f
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            v2f vert (uint vertex_id : SV_VERTEXID, uint instance_id : SV_INSTANCEID)
            {
                v2f o;

                o.color = pointsBuffer[instance_id].color;

                int vertexIndex = instance_id * 6 + vertex_id;

                o.position = UnityWorldToClipPos(float4(verticesBuffer[vertexIndex].position, 1));
                o.uv = verticesBuffer[vertexIndex].uv;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                return col;
            }
            ENDCG
        }
    }
}
