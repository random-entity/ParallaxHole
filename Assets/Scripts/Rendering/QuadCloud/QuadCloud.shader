Shader "Unlit/QuadCloud"
{
    Properties
    {
        _ColorWidth ("Color Frame Width", int) = 1920
        _ColorHeight("Color Frame Height", int) = 1080
        _QuadSize ("Quad Size", float) = 0.005

        _Bounds ("Bounds (maxAbsX, minY, maxY, maxZ)", Vector) = (1.5, -0.9, 1.2, 4)
        _PointCloudDownSample ("PointCloud DownSample", float) = 2

        _LookTarget ("Look Target", Vector) = (0, 0, 0, 0)

        _BoidFactor ("Boid Factor", float) = 1

        _ColorTexture ("Color Frame Texture", 2D) = "white" {}
        _QuadTexture ("Local Quad Texture", 2D) = "white" {}
        [HDR] _HDRTint("HDR Tint", Color) = (1.0, 1.0, 1.0, 1.0)
    }
    SubShader
    {
        Pass
        {
            Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
			Blend SrcAlpha OneMinusSrcAlpha
			ZWrite Off
            Cull Off

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            #pragma target 5.0

            float random (float2 seed)
            {
                return frac(sin(dot(seed, float2(12.9898, 78.2337))) * 2.5453);
            }
            float Unity_RandomRange_float(float2 Seed, float Min, float Max)
            {
                float randomno =  frac(sin(dot(Seed, float2(12.9898, 78.233)))*43758.5453);
                return lerp(Min, Max, randomno);
            }

            static const float2 localQuadUVFromVertexId[] = { 
                float2(0, 0),
                float2(0, 1),
                float2(1, 1),
                float2(0, 0),
                float2(1, 1),
                float2(1, 0)
            };

            static const float3 localPosFromVertexId[] = {
                float3(-0.5, -0.5, 0),
                float3(-0.5, 0.5, 0),
                float3(0.5, 0.5, 0),
                float3(-0.5, -0.5, 0),
                float3(0.5, 0.5, 0),
                float3(0.5, -0.5, 0),
            };

            float4x4 transform_matrix(float3 pos, float3 dir, float3 up) {
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

            // Struct and Buffer
            struct CameraSpacePoint {
                float x;
                float y;
                float z;
            };
            StructuredBuffer<CameraSpacePoint> cameraSpacePointBuffer;

            struct ColorSpacePoint {
                float x;
                float y;
            };
            StructuredBuffer<ColorSpacePoint> colorSpacePointBuffer;

            struct Boid {
                float3 position;
                float3 direction;
                float noise;
            };
            StructuredBuffer<Boid> boidBuffer;

            // Shader Structs
            struct v2f
            {
                float4 position : SV_POSITION;
                float2 colorUV : TEXCOORD0;
                float2 localQuadUV : TEXCOORD1;
                float doClip : TEXCOORD2;
                float2 randomSeed : TEXCOORD3;
            };

            // Properties
            uniform int _ColorWidth;
            uniform int _ColorHeight;
            uniform float _QuadSize;
            uniform float4 _Bounds;
            uniform float _PointCloudDownSample;
            uniform float4 _LookTarget;
            uniform float _BoidFactor;
            uniform int _FlockDownSample;
            uniform sampler2D _ColorTexture;
            uniform sampler2D _QuadTexture;
            uniform float4 _HDRTint;

            // Shaders
            v2f vert (uint vertex_id : SV_VERTEXID, uint instance_id : SV_INSTANCEID)
            {
                v2f o;

                float2 localQuadUV = localQuadUVFromVertexId[vertex_id];
                o.localQuadUV = localQuadUV;

                CameraSpacePoint camPoint = cameraSpacePointBuffer[instance_id];
                float3 camSpacePos = float3(camPoint.x, camPoint.y, camPoint.z);

                float doClip = 1;
                if(
                    abs(camSpacePos.x) > _Bounds.x 
                    || camSpacePos.y < _Bounds.y 
                    || camSpacePos.y > _Bounds.z 
                    || camSpacePos.z > _Bounds.w
                    || instance_id % _PointCloudDownSample >= 1
                    ) {
                    doClip = -1;
                }
                o.doClip = doClip;

                ColorSpacePoint colPoint = colorSpacePointBuffer[instance_id];
                float2 colorUV = float2(colPoint.x / _ColorWidth, colPoint.y / _ColorHeight);
                o.colorUV = colorUV;

                Boid boid = boidBuffer[instance_id / (_FlockDownSample * _FlockDownSample)];
                camSpacePos += _BoidFactor * boid.position;

                float4 localPos = float4(_QuadSize * localPosFromVertexId[vertex_id], 1);
                float4x4 localToWorld = transform_matrix(camSpacePos, _LookTarget.xyz - camSpacePos, float3(0, 1, 0));
                float4 worldPos = mul(localToWorld, localPos);
                o.position = UnityWorldToClipPos(worldPos);

                o.randomSeed = float2(
                    random(float2(sin((float)instance_id / 234.3491), sin((float)instance_id / 1246.4165))),
                    random(float2(sin((float)instance_id / 941.2356), sin((float)instance_id / 6786.2345)))
                );

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                clip(i.doClip);

                float2 localQuadUV = i.localQuadUV;

                float distFromFishHead = 1 - localQuadUV.y;
                float swish = 0.5 * (distFromFishHead - 0.4); // pow(distFromFishHead, 2)

                localQuadUV.x += 0.2 * swish * sin(6.28 * (distFromFishHead * 1.5 + (_Time.y + random(i.randomSeed)) * 1.5));

                float3 pixelColor = tex2D(_ColorTexture, i.colorUV);
                float4 localColor = tex2D(_QuadTexture, localQuadUV);

                return _HDRTint * float4(pixelColor, localColor.a * 0.2);
            }
            ENDCG
        }
    }
}