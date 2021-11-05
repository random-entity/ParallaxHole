Shader "Random Entity/Water"
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
        Tags { "RenderType"="Transparent" "RenderQueue"="Transparent" }

        CGPROGRAM

        #pragma surface surf Standard vertex:vert

        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        uniform float4 _WaveSourcesInfo[100]; // xy = waveSource position xz, z = timeSinceEnabled / maxTime (0 ~ 1), w = gameObject.isActiveInHierarchy

        void vert (inout appdata_full data){
            float3 pos = data.vertex.xyz;
            float3 worldPos = mul(unity_ObjectToWorld, pos);
            
            for(int i = 0; i < 100; i++) {
                float4 waveSourceInfo = _WaveSourcesInfo[i];
                if(waveSourceInfo.w != 1) continue;
                else {
                    float3 waveSourcePos = float3(waveSourceInfo.x, 0, waveSourceInfo.z);
                    float progress = waveSourceInfo.z;

                    float3 dir = worldPos - waveSourcePos;
                    float sqrDist = dot(dir, dir);
                    float dist = sqrt(sqrDist);

                    float yOffset = 0.1 * progress * sin(UNITY_PI * dist + progress) / max(0.25, sqrDist);
                    worldPos.y += yOffset;
                }
            }

            data.vertex.xyz = mul(unity_WorldToObject, worldPos);
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
}