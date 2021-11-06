Shader "Random Entity/Water"
{
    Properties
    {
        [Header(Default Surface Shader Properties)] 
        [Space]
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        [Header(Wave Mixer)] 
        [Space]
        _WaveMix ("Wave Mix", Range(0,1)) = 0 // 0 = full sine, 1 = full gerstner
        
        [Header(WaveSource Properties (from WaveSourceManager))]
        [Space]
        _ActiveWaveSourceCountSmooth ("Active Wave Source Count Smooth", Float) = 1 // wave source 개수 많아지면 그만큼 나누기 위함, 쌩 값으로 받는 것보다 네제곱근 정도가 자연스러워 보여서 4제곱근 때려서 보내 줌.
        _WaveSourceMaxTime ("Wave Source MaxTime", Float) = 8 // WaveSourceManager.Awake()에서 시작할 때 딱 한 번만(!) 보내 줌. 어디에서 보내주기가 애매한 데이터라 그냥 그렇게 해.

        [Header(Sine Wave Properties)] 
        [Space]
        _SineWaveAmp ("Sine Wave Amplitude", Range(0,1)) = 0.15
        _SineWaveLength ("Sine Wave Length (m)", Range(0.1, 2)) = 0.25
        _SineWaveSpeed ("Sine Wave Speed (m/s)", Range(0.1, 2)) = 0.5
        _SineWaveDistanceDamp ("1 + a dist^2", Range(1, 16)) = 8
        
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

        // WaveSource Properties
        // array size should match WaveSourceManager.size
        uniform float4 _WaveSourcesData[200]; // xyz = waveSource position xyz, w = timeSinceEnabled / maxTime (0 ~ 1)
        uniform float _ActiveWaveSourceCountSmooth;
        uniform float _WaveSourceMaxTime;

        // Sine Wave Properties
        uniform float _SineWaveAmp;
        uniform float _SineWaveLength;
        uniform float _SineWaveSpeed;
        uniform float _SineWaveDistanceDamp;

        void sineWave(inout float3 displacement, inout float3 tangent, inout float3 binormal, float3 worldDirFromWaveSource, float progress) {         
            if(progress >= 1) return;

            float sqrDist = dot(worldDirFromWaveSource, worldDirFromWaveSource);
            float dist = sqrt(sqrDist);

            float time = progress * _WaveSourceMaxTime;

            float startTime = 0.5 * dist / _SineWaveSpeed;
            float timeSinceStart = time - startTime;
            if(timeSinceStart < 0) return;

            float localAmpByTime = saturate(128 * timeSinceStart);
            float globalDampByTime = 0.5 * (1 + cos(UNITY_PI * progress));

            float wave = cos(2 * UNITY_PI * (dist - _SineWaveSpeed * time) / _SineWaveLength);
            float globalAmpDivideByDist = 1 + _SineWaveDistanceDamp * sqrDist;
            
            float cpuAmp = _SineWaveAmp / _ActiveWaveSourceCountSmooth;

            float finalAmp = cpuAmp * globalDampByTime * localAmpByTime;
            float finalWave = wave / globalAmpDivideByDist;

            // vertex displacement y = const(x, z) * function(x, z)
            displacement.y += finalAmp * finalWave;

            // tangent & binormal

            // partial derivatives (by x => .x and z => .z) of the two functions that has x, z as parameters
            float3 waveDerivative = -sin(2 * UNITY_PI * (dist - _SineWaveSpeed * time) / _SineWaveLength) * (2 * UNITY_PI / _SineWaveLength) * (worldDirFromWaveSource / dist);
            float3 globalAmpDivideByDistDerivative = _SineWaveDistanceDamp * 2 * worldDirFromWaveSource;

            float3 yDerivatives = waveDerivative * globalAmpDivideByDist - wave * globalAmpDivideByDistDerivative / (globalAmpDivideByDist * globalAmpDivideByDist);
            yDerivatives *= finalAmp;

            tangent.y += yDerivatives.x;
            binormal.y += yDerivatives.z;
        }

        // void gertsnerWave(inout float3 gertsnerWaveXYZ, float3 worldDirFromWaveSource, float progress) {
        //     float sqrDist = dot(worldDirFromWaveSource, worldDirFromWaveSource);
        //     float dist = sqrt(sqrDist);
        // }

        void vert (inout appdata_full data) {
            float3 vertex = data.vertex.xyz;
            float3 worldPos = mul(unity_ObjectToWorld, vertex);
            
            float3 displacement = 0;
            float3 tangent = float3(1, 0, 0);
            float3 binormal = float3(0, 0, 1);

            for(int i = 0; i < 200; i++) { // max index should match WaveSourceManager.size
                float4 waveSourceData = _WaveSourcesData[i];
                float3 waveSourceWorldPos = waveSourceData.xyz;
                float progress = waveSourceData.w;
                float3 worldDirFromWaveSource = worldPos - waveSourceWorldPos;

                sineWave(displacement, tangent, binormal, worldDirFromWaveSource, progress);
            }

            worldPos += displacement;
            data.vertex = mul(unity_WorldToObject, worldPos);

            tangent = normalize(tangent);
            binormal = normalize(binormal);
            float3 normal = normalize(cross(tangent, binormal));
            data.normal = mul(unity_WorldToObject, normal);
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