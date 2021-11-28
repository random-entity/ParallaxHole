Shader "Custom/MeshCloud_surf"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        CGPROGRAM

        sampler2D _MainTex;   
        fixed4 _Color;
        half _Glossiness;
        half _Metallic;
        
        struct Input
        {
            float2 uv_MainTex;
        };

        #pragma surface surf Standard vertex:vert
        #pragma instancing_options procedural:setup

        float4x4 _TransformMatrix;
        float3 _Position;

        #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            struct PointMesh
            {
                float3 position;
            };

            StructuredBuffer<PointMesh> pointMeshesBuffer; 
        #endif

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

        void setup()
        {
            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                _Position = pointMeshesBuffer[unity_InstanceID].position;
                _TransformMatrix = getTransformMatrix(_Position, float3(0, 0, 1), float3(0.0, 1.0, 0.0));
            #endif
        }

        void vert(inout appdata_full v, out Input data) {
            UNITY_INITIALIZE_OUTPUT(Input, data);
        
            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                v.vertex = mul(_TransformMatrix, v.vertex);
            #endif
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}