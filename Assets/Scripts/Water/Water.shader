Shader "Random Entity/Water"
{
    Properties
    {
        [Header(Default Surface Shader Properties)] [Space]
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        [Header(Wave Mixer)] [Space]
        _WaveMix ("0 Full Sine, 1 Full Gerstner", Range(0,1)) = 0

        [Header(Sine Wave Properties)] [Space]
        _WaveAmp ("Wave Amplitude", Range(0,1)) = 0.5
        _ActiveWaveSourceCountSmooth ("Active Wave Source Count Smooth", Float) = 1

        // [Header(Gerstner Wave Properties)]
        
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
        uniform float4 _WaveSourcesData[200]; // xyz = waveSource position xyz, w = timeSinceEnabled / maxTime (0 ~ 1)
        uniform float _WaveAmp;
        uniform float _ActiveWaveSourceCountSmooth;

        void sinWave(inout float sinWaveY, float3 worldDirFromWaveSource, float progress) {         
            if(progress >= 1) return;

            float sqrDist = dot(worldDirFromWaveSource, worldDirFromWaveSource);
            float dist = sqrt(sqrDist);

            // 1. simplest
            // sinWaveY += 0.5 * (1 + cos(UNITY_PI * progress)) * sin(2 * UNITY_PI * (2 * dist - 4 * progress)) / (1 + 8 * sqrDist);

            // 2. thoughtful
            // float peakAmpProgress = min(0.1 * dist, 0.9);
            // float ampByDistAndTime;
            // if(progress < peakAmpProgress) {
            //     ampByDistAndTime = 0.5 * (1 - cos(UNITY_PI * progress / peakAmpProgress));
            // } else {
            //     ampByDistAndTime = 0.5 * (1 + cos(UNITY_PI * (progress - peakAmpProgress) / (1 - peakAmpProgress)));
            // }
            // sinWaveY += ampByDistAndTime * sin(2 * UNITY_PI * (2 * dist - 4 * progress)) / (1 + 8 * sqrDist);

            // 3. simpler thoughtful
            float waveStartProgress = abs(0.1 * (dist - 0.1));
            float progressTrimmed = progress - waveStartProgress;
            if(progressTrimmed > 0) {
                float amp = sin(2 * UNITY_PI * 2 * progressTrimmed);
                float damp = 0.5 * (1 + cos(UNITY_PI * progress));
                sinWaveY += amp * damp * sin(2 * UNITY_PI * (4 * dist - 4 * progress)) / (1 + 8 * sqrDist);
            }

            // to do : tangent/bitangent/normal setting
        }

        void gertsnerWave(inout float3 gertsnerWaveXYZ, float3 worldDirFromWaveSource, float progress) {
            float sqrDist = dot(worldDirFromWaveSource, worldDirFromWaveSource);
            float dist = sqrt(sqrt(sqrDist));


        }

        void vert (inout appdata_full data){
            float3 pos = data.vertex.xyz;
            float3 worldPos = mul(unity_ObjectToWorld, pos);
            
            float sinWaveY = 0;
            float3 gertsnerWaveXYZ = 0;

            for(int i = 0; i < 200; i++) { // max index should match WaveSourceManager.size
                float4 waveSourceData = _WaveSourcesData[i];
                float3 waveSourceWorldPos = waveSourceData.xyz;
                float progress = waveSourceData.w;
                float3 worldDirFromWaveSource = worldPos - waveSourceWorldPos;

                sinWave(sinWaveY, worldDirFromWaveSource, progress);
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