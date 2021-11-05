Shader "Random Entity/Water"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _WaveAmp ("Wave Amplitude", Range(0,1)) = 0.5
        _ActiveWaveSourceCountSmooth ("Active Wave Source Count Smooth", Float) = 1
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

        // array size should match WaveSourceManager.size
        uniform float4 _WaveSourcesData[25]; // xyz = waveSource position xyz, w = timeSinceEnabled / maxTime (0 ~ 1)
        uniform float _WaveAmp;
        uniform float _ActiveWaveSourceCountSmooth;

        void sinWave(float4 waveSourceData, inout float sinWaveY, float3 worldPos) {
            float3 waveSourcePos = waveSourceData.xyz;
            float progress = waveSourceData.w;

            float3 dir = worldPos - waveSourcePos;
            float sqrDist = dot(dir, dir);
            float dist = sqrt(sqrDist);

            sinWaveY += 0.5 * (1 + cos(UNITY_PI * progress)) * sin(2 * UNITY_PI * (2 * dist - 4 * progress)) / (1 + 8 * sqrDist);
        }

        void vert (inout appdata_full data){
            float3 pos = data.vertex.xyz;
            float3 worldPos = mul(unity_ObjectToWorld, pos);
            
            float sinWaveY = 0;

            for(int i = 0; i < 25; i++) { // max index should match WaveSourceManager.size
                sinWave(_WaveSourcesData[i], sinWaveY, worldPos);
            }

            sinWaveY *= _WaveAmp / sqrt(_ActiveWaveSourceCountSmooth);
            worldPos.y += sinWaveY;
            data.vertex = mul(unity_WorldToObject, worldPos);
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