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

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            #pragma target 5.0

			struct Vertex {
				float3 position;
				float2 uv;
			};

			StructuredBuffer<Vertex> vertexBuffer;

            struct v2f
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _CircleTex;

            v2f vert (uint vertex_id : SV_VERTEXID, uint instance_id : SV_INSTANCEID)
            {
                v2f o;

                int vertexIndex = instance_id * 6 + vertex_id;

                o.position = UnityObjectToClipPos(float4(vertexBuffer[vertexIndex].position, 1));
                o.uv = vertexBuffer[vertexIndex].uv;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // fixed4 col = tex2D(_CircleTex, i.uv);
                return float4(0, 1, 1, 1);
            }
            ENDCG
        }
    }
}
