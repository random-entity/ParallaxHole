Shader "Unlit/InstancedFlockingUnlit"
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
            #pragma instancing_options procedural:setup

            #include "UnityCG.cginc"

            #pragma target 5.0

            struct Boid
            {
                float3 position;
                float3 direction;
                float noise_offset;
            };

            StructuredBuffer<Boid> boidsBuffer; 

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (uint vertex_id : SV_VertexID, uint instance_id : SV_InstanceID)
            {
                v2f o;

                o.vertex = UnityWorldToClipPos(0);                
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return float4(1, 0, 0, 1);
            }
            ENDCG
        }
    }
}
