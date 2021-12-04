Shader "Random Entity/Point Cloud/PointCloud"
{
    Properties
    {
        _DepthTexture ("Depth Texture", 2D) = "white" {}
        _ColorTexture ("Color Texture", 2D) = "white" {}

        _PointSize ("Point Size", Range(1, 100)) = 10

        _Pitch ("Pitch", Float) = 0.01
        _DepthScale ("Depth Scale", Range(0, 2)) = 1

        _Bounds ("Bounds: x = X far, y = Y bottom, z = Y top, w = Z far", Vector) = (2, 2, 0.5, 2.5)

        _BrightnessScale ("Brightness Scale", Vector) = (2, 2, 2, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
    		Cull Off
        
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv_depth : TEXCOORD0;
                float2 uv_color : TEXCOORD1;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 col : COLOR;
                float3 kinectSpacePos : TEXCOORD0;
                float psize : PSIZE;
            };

            sampler2D _DepthTexture;
            sampler2D _ColorTexture;
            uniform float _PointSize;
            float _Pitch;
            float _DepthScale;
            float4 _Bounds;
            float4 _BrightnessScale;

            v2f vert (appdata v)
            {
				float4 rawDepth = tex2Dlod(_DepthTexture, float4(v.uv_depth, 0, 0));
                float depth = rawDepth.x * 64 * _DepthScale;
                float scale = depth / (365.5 * _Pitch);

                v.vertex.x *= scale;
                v.vertex.y *= scale;
                v.vertex.z = depth;

                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.col = tex2Dlod(_ColorTexture, float4(v.uv_color, 0, 0));
                o.kinectSpacePos = v.vertex.xyz;

                o.psize = _PointSize;
                                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float doClip = 1;
                if(abs(i.kinectSpacePos.x) > _Bounds.x) doClip = -1;
                if(-i.kinectSpacePos.y < _Bounds.y) doClip = -1;
                if(-i.kinectSpacePos.y > _Bounds.z) doClip = -1;
                if(i.kinectSpacePos.z > _Bounds.w) doClip = -1;

                clip(doClip);

                fixed4 col = i.col;
                
                return saturate(col * _BrightnessScale);
            }
            ENDCG
        }
    }
}