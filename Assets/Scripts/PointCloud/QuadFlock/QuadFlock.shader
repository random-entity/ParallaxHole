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

            // Helpers
            static const float2 uvFromVertexId[] = { 
                float2(0, 0),
                float2(0, 1),
                float2(1, 1),
                float2(0, 0),
                float2(1, 1),
                float2(1, 0)
            };

            static const float4 localPosFromVertexId[] = {
                float4(-0.5, 0, -0.5, 1),
                float4(-0.5, 0, 0.5, 1),
                float4(0.5, 0, 0.5, 1),
                float4(-0.5, 0, -0.5, 1),
                float4(0.5, 0, 0.5, 1),
                float4(0.5, 0, -0.5, 1),
            };

            float4x4 transform_matrix(float3 pos, float3 dir, float3 up) {
                float3 zaxis = normalize(dir);
                float3 xaxis = normalize(cross(up, zaxis));
                float3 yaxis = cross(zaxis, xaxis);

                return float4x4(
                    xaxis.x, yaxis.x, zaxis.x, pos.x,
                    xaxis.y, yaxis.y, zaxis.z, pos.y,
                    xaxis.z, yaxis.z, zaxis.z, pos.z,
                    0, 0, 0, 1
                );
            }

            // Struct and Buffer
            struct Quoid {
                float3 position;
                float3 direction;
                float noise;
            };
            StructuredBuffer<Quoid> quoidBuffer;

            // Shader Structs
            struct v2f
            {
                float4 position : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            // Properties
            sampler2D _CircleAlphaMask;
            sampler2D _FishAlphaMask;

            // Shaders
            v2f vert (uint vertex_id : SV_VERTEXID, uint instance_id : SV_INSTANCEID)
            {
                v2f o;

                float2 uv = uvFromVertexId[vertex_id];
                o.uv = uv;

                Quoid quoid = quoidBuffer[instance_id];
                float4 localPos = localPosFromVertexId[vertex_id];
                float4x4 localToWorld = transform_matrix(quoid.position, quoid.direction, float3(0, 1, 0));
                float4 worldPos = mul(localToWorld, localPos);
                o.position = UnityWorldToClipPos(worldPos);

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