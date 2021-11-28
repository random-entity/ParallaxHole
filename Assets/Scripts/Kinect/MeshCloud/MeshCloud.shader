Shader "Random Entity/MeshCloud"
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
            #pragma multi_compile_instancing
            #pragma instancing_options procedural:setup

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            struct PointMesh {
                float3 position;
            };

            StructuredBuffer<PointMesh> pointMeshesBuffer;

            void setup() {}

            float4x4 getTransformMatrix(float3 pos, float3 dir, float3 up) {
                float3 zaxis = normalize(dir);
                float3 xaxis = normalize(cross(up, zaxis));
                float3 yaxis = cross(zaxis, xaxis);
                return float4x4(
                    xaxis.x, yaxis.x, zaxis.x, pos.x,
                    xaxis.y, yaxis.y, zaxis.y, pos.y,
                    xaxis.z, yaxis.z, zaxis.z, pos.z,
                    0, 0, 0, 1
                );
            }

            v2f vert (appdata v)
            {
                v2f o = (v2f)0;

                UNITY_SETUP_INSTANCE_ID(v);

                #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                    float3 pos = pointMeshesBuffer[unity_InstanceID].position;
                    o.vertex = UnityWorldToClipPos(pos);
                #endif

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = float4(1, 0, 0, 1);
                return col;
            }
            ENDCG
        }
    }
}