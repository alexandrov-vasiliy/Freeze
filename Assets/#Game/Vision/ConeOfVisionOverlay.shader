Shader "Game/ConeOfVisionOverlay"
{
    Properties
    {
        _OverlayColor ("Цвет затемнения", Color) = (0, 0, 0, 1)
        _PlayerWorldPos ("Player World Pos", Vector) = (0, 0, 0, 0)
        _ViewAngle ("Угол направления (градусы)", Float) = 0
        _ConeHalfAngle ("Полуугол конуса (градусы)", Float) = 45
        _ConeRadius ("Радиус конуса", Float) = 10
        _FadeWidth ("Ширина фейда", Float) = 1
        _CameraWorldPos ("Camera World Pos", Vector) = (0, 0, 0, 0)
        _OrthoSize ("Ortho Size", Float) = 5
        _AspectRatio ("Aspect Ratio", Float) = 1.78
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Overlay"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest Always
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"

            float4 _OverlayColor;
            float4 _PlayerWorldPos;
            float _ViewAngle;
            float _ConeHalfAngle;
            float _ConeRadius;
            float _FadeWidth;
            float4 _CameraWorldPos;
            float _OrthoSize;
            float _AspectRatio;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            float AngleDiffDegrees(float a, float b)
            {
                float d = abs(a - b);
                if (d > 180.0) d = 360.0 - d;
                return d;
            }

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                float2 orthoHalf = float2(_OrthoSize * _AspectRatio, _OrthoSize);
                output.worldPos.xy = _CameraWorldPos.xy + (input.uv - 0.5) * 2.0 * orthoHalf;
                output.worldPos.z = 0.0;
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 toPlayer = input.worldPos.xy - _PlayerWorldPos.xy;
                float distanceToPlayer = length(toPlayer);
                float pointAngleDeg = atan2(toPlayer.y, toPlayer.x) * (180.0 / 3.14159265);
                float angleDiff = AngleDiffDegrees(pointAngleDeg, _ViewAngle);

                float visibilityAngle = 1.0 - smoothstep(_ConeHalfAngle, _ConeHalfAngle + _FadeWidth, angleDiff);
                float visibilityRadius = 1.0 - smoothstep(_ConeRadius, _ConeRadius + _FadeWidth, distanceToPlayer);
                float visibility = visibilityAngle * visibilityRadius;

                float overlayAlpha = 1.0 - visibility;
                return half4(_OverlayColor.rgb, _OverlayColor.a * overlayAlpha);
            }
            ENDHLSL
        }
    }
    Fallback "Universal Render Pipeline/Unlit"
}
