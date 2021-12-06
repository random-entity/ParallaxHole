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

        // [Header(Bound)]
        // [Space]
        // _BoundRadius ("Tube Radius", Range(0,2)) = 1
        // Too many texture interpolators would be used for ForwardBase pass 에러 때문에 Properties 절약해야 함. 우물 반지름 1로 고정하는 걸로.

        // [Header(Wave Mixer)] 
        // [Space]
        
        [Header(WaveSource Properties (from WaveSourceManager))]
        [Space]
        _ActiveWaveSourceCountSmooth ("Active Wave Source Count Smooth", Float) = 1 // wave source 개수 많아지면 그만큼 나누기 위함, 쌩 값으로 받는 것보다 네제곱근 정도가 자연스러워 보여서 4제곱근 때려서 보내 줌.
        _WaveSourceMaxTime ("Wave Source MaxTime", Float) = 8 // WaveSourceManager.Awake()에서 시작할 때 딱 한 번만(!) 보내 줌. 어디에서 보내주기가 애매한 데이터라 그냥 그렇게 해.

        [Header(Sine Wave Properties)] 
        [Space]
        _SineWaveConfig ("x = Amp, y = Wavelength, z = Speed, w = Distance Damp", Vector) = (0.15, 0.2, 0.5, 8) // packing for room for others
        _SineWaveHeightScale ("Height Scale * 10^5", Range(0,100)) = 20

        [Header(Gerstner Wave Properties)]
        [Space]
        _GerWaveConfig ("x = Steepness, y = Wavelength, z = Speed, w = Gravity", Vector) = (0.5, 0.5, 1, 9.8) // packing for room for others

        [Header(Fog)]
        [Space]
		_WaterFogColor ("Water Fog Color", Color) = (0, 0, 0, 0)
		_WaterFogDensity ("Water Fog Density", Range(0, 2)) = 0.15

        [Header(Refraction)]
        [Space]
        _RefractionStrength ("Refraction Strength", Range(0, 1)) = 0.25
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 200

        GrabPass { "_WaterBackground" }

        CGPROGRAM

        #pragma surface surf Standard alpha vertex:vert finalcolor:ResetAlpha

        #pragma target 4.0

        #include "Waves.cginc"
		#include "LookingThroughWater.cginc"

        sampler2D _MainTex;

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        struct Input
        {
            float2 uv_MainTex;
            float distanceFromCenterWorld;
            float4 screenPos;
        };

        void vert (inout appdata_full data, out Input o) {
            //////////
            // INIT //
            //////////
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float3 vertex = data.vertex.xyz;
            float3 worldPos = mul(unity_ObjectToWorld, vertex);
            
            ///////////
            // BOUND //
            ///////////
            o.distanceFromCenterWorld = length(worldPos); // projection plane의 center는 (0, 0, 0)이겠지

            ///////////
            // WAVES //
            ///////////
            float3 displacement = 0;
            float3 tangent = float3(1, 0, 0);
            float3 binormal = float3(0, 0, 1);
            _SineWaveHeightScale /= 100000;

            for(int i = 0; i < 200; i++) { // max index should match WaveSourceManager.size
                float4 waveSourceData = _WaveSourcesData[i];
                float3 waveSourceWorldPos = waveSourceData.xyz;
                float progress = waveSourceData.w;
                float3 worldDirFromWaveSource = worldPos - waveSourceWorldPos;

                sineWave(displacement, tangent, binormal, worldDirFromWaveSource, progress);
                // GerstnerWave(displacement, worldPos, tangent, binormal, worldDirFromWaveSource, progress);
            }

            worldPos += displacement;
            float4 modelPos = mul(unity_WorldToObject, worldPos);

            // float3 worldPosPlusTangent = worldPos + tangent;
            // float4 modelPosPlusTangent = mul(unity_WorldToObject, worldPosPlusTangent);
            // float4 modelTangent = modelPosPlusTangent - modelPos;
            float4 modelTangent = normalize(mul(unity_WorldToObject, float4(tangent, 0)));

            float3 normal = cross(binormal, tangent);

            // float3 worldPosPlusNormal = worldPos + normal;
            // float4 modelPosPlusNormal = mul(unity_WorldToObject, worldPosPlusNormal);
            // float4 modelNormal = modelPosPlusNormal - modelPos;
            float4 modelNormal = normalize(mul(unity_WorldToObject, float4(normal, 0)));

            data.vertex = modelPos;
            data.tangent = modelTangent;
            data.normal = modelNormal;
        }

        void ResetAlpha (Input IN, SurfaceOutputStandard o, inout fixed4 color) {
			color.a = 1;
		}

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // clip(1 - IN.distanceFromCenterWorld);

            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;

            o.Emission = ColorBelowWater(IN.screenPos, o.Normal) * (1 - c.a);
        }
        ENDCG
    }
}