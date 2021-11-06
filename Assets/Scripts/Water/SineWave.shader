Shader "Random Entity/Sine Wave"
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
        
        [Header(Shared Wave Properties)] [Space]
        // wave source 개수 많아지면 그만큼 나누기 위함, 쌩 값으로 받는 것보다 네제곱근 정도가 자연스러워 보여서 4제곱근 때려서 보내 줌.
        // data sent from WaveSourceManager.FixedUpdate()
        _ActiveWaveSourceCountSmooth ("Active Wave Source Count Smooth", Float) = 1 
        // WaveSourceManager.Awake()에서 시작할 때 딱 한 번만(!) 보내 줌. 어디에서 보내주기가 애매한 데이터라 그냥 그렇게 해.
        // static WaveSource.MaxTime
        _WaveSourceMaxTime ("Wave Source MaxTime", Float) = 8 
        // 거리 1 가는 데에 얼마나 걸리는지 progress(0 ~ 1) 단위로 (WaveSource.MaxTime 곱하면 전파 속도)
        // At what progress wave reaches distance 1
        _ProgressToOneMeter ("Progress To Reach 1 Meter", Range(0,1)) = 0.2

        [Header(Sine Wave Properties)] [Space]
        _SineWaveAmp ("Wave Amplitude", Range(0,1)) = 0.5
        
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


        // Sine Wave Properties
        // array size should match WaveSourceManager.size
        uniform float4 _WaveSourcesData[200]; // xyz = waveSource position xyz, w = timeSinceEnabled / maxTime (0 ~ 1)
        uniform float _SineWaveAmp;
        uniform float _ActiveWaveSourceCountSmooth;
        uniform float _WaveSourceMaxTime;
        uniform float _ProgressToOneMeter;

        void sinWave(inout float sinWaveY, float3 worldDirFromWaveSource, float progress) {         
            if(progress >= 1) return;

            float sqrDist = dot(worldDirFromWaveSource, worldDirFromWaveSource);
            float dist = sqrt(sqrDist);

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
            float dist = sqrt(sqrDist);


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

            sinWaveY *= _SineWaveAmp / _ActiveWaveSourceCountSmooth;
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