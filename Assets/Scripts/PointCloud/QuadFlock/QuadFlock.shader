Shader "Random Entity/Point Cloud/QuadFlock"
{
    Properties
    {
        _CircleAlphaMask ("Texture", 2D) = "white" {}
        _FishAlphaMask ("Texture", 2D) = "black" {}
    }
    SubShader
    {
        Pass
        {
            Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
            Cull Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            #pragma target 5.0

            struct Quoid {
                float3 position;
                float3 direction;
                float noise;
            };

            StructuredBuffer<Quoid> quoidBuffer;

			struct Vertex {
				float3 position;
				float2 uv;
			};

			StructuredBuffer<Vertex> vertexBuffer;

            float2 getUVFromVertexId(uint vertex_id) {
                if(vertex_id == 0) return float2(0, 0);
                if(vertex_id == 1) return float2(0, 1);
                if(vertex_id == 2) return float2(1, 1);
                if(vertex_id == 3) return float2(0, 0);
                if(vertex_id == 4) return float2(1, 1);
                if(vertex_id == 5) return float2(1, 0);
                return float2(0, 0);
            }

            float2 UVFromVertexId[] = { 
                float2(0, 0),
                float2(0, 1),
                float2(1, 1),
                float2(0, 0),
                float2(1, 1),
                float2(1, 0)
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _CircleAlphaMask;

            v2f vert (uint vertex_id : SV_VERTEXID, uint instance_id : SV_INSTANCEID)
            {
                v2f o;

                int vertexIndex = instance_id * 6 + vertex_id;

                o.position = UnityObjectToClipPos(float4(quoidBuffer[instance_id].position + 0.5 * float3(getUVFromVertexId(vertex_id), 0), 1));
                o.uv = getUVFromVertexId(vertex_id); // UVFromVertexId[vertex_id]; // getUVFromVertexId(vertex_id); //vertexBuffer[vertexIndex].uv;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_CircleAlphaMask, i.uv);
                return col;
            }
            ENDCG
        }
    }
}
