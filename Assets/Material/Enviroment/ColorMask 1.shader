Shader "Custom/ColorMask"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _RevealAmount ("Reveal Amount", Range(0, 1)) = 1.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            #define MAX_CENTERS 32

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float _RevealAmount;
            float4 _Centers[MAX_CENTERS];
            float _Radii[MAX_CENTERS];
            int _CenterCount;

            float3 _CameraWorldPos;

            Varyings vert(Attributes v)
            {
                Varyings o;
                float3 worldPos = TransformObjectToWorld(v.positionOS.xyz);
                o.worldPos = worldPos;
                o.positionHCS = TransformWorldToHClip(worldPos);
                o.uv = v.uv;
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                float reveal = 0.0;

        for (int idx = 0; idx < _CenterCount; idx++)
         {
             float distToCenter = distance(i.worldPos, _Centers[idx].xyz);
            float distCameraToCenter = distance(_CameraWorldPos, _Centers[idx].xyz);
            float scale = saturate(7 / distCameraToCenter); 
            float scaledRadius = _Radii[idx] * scale*5;

            float strength = 1.0 - smoothstep(scaledRadius * 0.5, scaledRadius, distToCenter);
            reveal = max(reveal, strength);
        }


                float blend = saturate(reveal +  _RevealAmount);
                float4 col = tex2D(_MainTex, i.uv);
                float lum = dot(col.rgb, float3(0.299, 0.587, 0.114));
                float3 maskedColor = float3(lum, lum, lum);
                float3 finalColor = lerp(col.rgb, maskedColor, 1 - blend);
                return float4(finalColor, col.a);
            }
            ENDHLSL
        }
    }
}
