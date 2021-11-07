// WaveSource Properties
// array size should match WaveSourceManager.size
uniform float4 _WaveSourcesData[200]; // xyz = waveSource position xyz, w = timeSinceEnabled / maxTime (0 ~ 1)
uniform float _ActiveWaveSourceCountSmooth;
uniform float _WaveSourceMaxTime;

// Sine Wave Properties
uniform float4 _SineWaveConfig;
uniform float _SineWaveHeightScale;

// Gerstner Wave Properties
uniform float4 _GerWaveConfig;

void sineWave(inout float3 displacement, inout float3 tangent, inout float3 binormal, float3 worldDirFromWaveSource, float progress) {         
    if(progress >= 1) return;

    float _SineWaveAmp = _SineWaveConfig.x;
    float _SineWaveLength = _SineWaveConfig.y;
    float _SineWaveSpeed = _SineWaveConfig.z;
    float _SineWaveDistanceDamp = _SineWaveConfig.w;

    float sqrDist = dot(worldDirFromWaveSource, worldDirFromWaveSource);
    float dist = sqrt(sqrDist);

    float time = progress * _WaveSourceMaxTime;

    float startTime = 0.5 * dist / _SineWaveSpeed;
    float timeSinceStart = time - startTime;
    if(timeSinceStart < 0) return;

    float localAmpByTime = saturate(128 * (timeSinceStart - 0.1));
    float globalDampByTime = 0.5 * (1 + cos(UNITY_PI * progress));

    float wave = cos(2 * UNITY_PI * (dist - _SineWaveSpeed * time) / _SineWaveLength);
    float globalAmpDivideByDist = 1 + _SineWaveDistanceDamp * sqrDist;
    
    float cpuAmp = _SineWaveAmp / _ActiveWaveSourceCountSmooth;

    float finalAmp = cpuAmp * globalDampByTime * localAmpByTime;
    float finalWave = wave / globalAmpDivideByDist;

    // VERTEX DISPLACEMENT // y = const(x, z) * function(x, z) form
    displacement.y += finalAmp * finalWave;

    // TANGENT & BINORMAL SETTING
    // partial derivatives (by x => .x and z => .z) of the two functions that has x, z as parameters
    float3 waveDerivative = 
        -sin(2 * UNITY_PI * (dist - _SineWaveSpeed * time) / _SineWaveLength) 
        * (2 * UNITY_PI / _SineWaveLength) 
        * (worldDirFromWaveSource / dist);
    float3 globalAmpDivideByDistDerivative = 
        _SineWaveDistanceDamp * 2 
        * worldDirFromWaveSource;
    // quotient rule : (f/g)' = (f'g - fg')/g^2
    float3 yDerivatives = 
        (waveDerivative * globalAmpDivideByDist - wave * globalAmpDivideByDistDerivative) 
        / (globalAmpDivideByDist * globalAmpDivideByDist);
    // multiply constant coeff
    yDerivatives *= finalAmp;
    yDerivatives *= _SineWaveHeightScale;
    // height scaling is important
    
    tangent.y += yDerivatives.x;
    binormal.y += yDerivatives.z;
}

void GerstnerWave (inout float3 displacement, inout float3 tangent, inout float3 binormal, float3 vertexWorldPos, float3 worldDirFromWaveSource, float progress) {
    if(progress > 1) return;

    float _GSteepness = _GerWaveConfig.x;
    float _GWaveLength = _GerWaveConfig.y;
    float _GSpeed = _GerWaveConfig.z;
    float _GGravity = _GerWaveConfig.w;

    float steepness = _GSteepness;
    float wavelength = _GWaveLength;
    float k = 2 * UNITY_PI / wavelength;
    float c = sqrt(_GGravity / k);
    float2 d = normalize(worldDirFromWaveSource);
    float3 p = vertexWorldPos;
    float f = k * (dot(d, p.xz) - c * (progress * _WaveSourceMaxTime));
    float a = steepness / k;

    tangent += float3(
        -d.x * d.x * (steepness * sin(f)),
        d.x * (steepness * cos(f)),
        -d.x * d.y * (steepness * sin(f))
    );
    binormal += float3(
        -d.x * d.y * (steepness * sin(f)),
        d.y * (steepness * cos(f)),
        -d.y * d.y * (steepness * sin(f))
    );
    displacement += float3(
        d.x * (a * cos(f)),
        a * sin(f),
        d.y * (a * cos(f))
    );
}