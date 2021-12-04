Shader "Unlit/QuadCloud"
{
    Properties
    {
        _ColorWidth ("Color Frame Width", int) = 1920
        _ColorHeight("Color Frame Height", int) = 1080
        _QuadSize ("Quad Size", float) = 0.005

        _Bounds ("Bounds (maxAbsX, minY, maxY, maxZ)", Vector) = (1.5, -0.9, 1.2, 4)

        _ColorTexture ("Color Frame Texture", 2D) = "white" {}
        _QuadTexture ("Local Quad Texture", 2D) = "white" {}
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

            // Helpers
            static const float2 localQuadUVFromVertexId[] = { 
                float2(0, 0),
                float2(0, 1),
                float2(1, 1),
                float2(0, 0),
                float2(1, 1),
                float2(1, 0)
            };

            static const float3 localPosFromVertexId[] = {
                float3(-0.5, 0, -0.5),
                float3(-0.5, 0, 0.5),
                float3(0.5, 0, 0.5),
                float3(-0.5, 0, -0.5),
                float3(0.5, 0, 0.5),
                float3(0.5, 0, -0.5),
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

            // Shader Structs
            struct v2f
            {
                float4 position : SV_POSITION;
                float2 colorUV : TEXCOORD0;
                float2 localQuadUV : TEXCOORD1;
                float3 kinectSpacePos : TEXCOORD2;
            };

            // Properties
            uniform int _ColorWidth;
            uniform int _ColorHeight;
            uniform float _QuadSize;
            uniform float4 _Bounds;
            uniform sampler2D _ColorTexture;
            uniform sampler2D _QuadTexture;

            // Shaders
            v2f vert (uint vertex_id : SV_VERTEXID, uint instance_id : SV_INSTANCEID)
            {
                v2f o;

                float2 localQuadUV = localQuadUVFromVertexId[vertex_id];
                o.localQuadUV = localQuadUV;

                CameraSpacePoint camPoint = cameraSpacePointBuffer[instance_id];
                float3 camSpacePos = float3(camPoint.x, camPoint.y, camPoint.z);
                o.kinectSpacePos = camSpacePos;

                ColorSpacePoint colPoint = colorSpacePointBuffer[instance_id];
                float2 colorUV = float2(colPoint.x / _ColorWidth, colPoint.y / _ColorHeight);
                o.colorUV = colorUV;

                float4 localPos = float4(_QuadSize * localPosFromVertexId[vertex_id], 1);
                float4x4 localToWorld = transform_matrix(camSpacePos, float3(0, 0, -1), float3(0, 1, 0));
                float4 worldPos = mul(localToWorld, localPos);
                o.position = UnityWorldToClipPos(worldPos);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float doClip = 1;
                if(abs(i.kinectSpacePos.x) > _Bounds.x) doClip = -1;
                if(i.kinectSpacePos.y < _Bounds.y) doClip = -1;
                if(i.kinectSpacePos.y > _Bounds.z) doClip = -1;
                if(i.kinectSpacePos.z > _Bounds.w) doClip = -1;
                clip(doClip);

                float3 pixelColor = tex2D(_ColorTexture, i.colorUV);
                float4 localColor = tex2D(_QuadTexture, i.localQuadUV);
                return float4(pixelColor, localColor.a * 0.5);
            }
            ENDCG
        }
    }
}