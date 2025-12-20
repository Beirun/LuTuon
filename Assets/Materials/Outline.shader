Shader "Unlit/Fast Outline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (0,1,0,1)
        _OutlineWidth ("Outline Width", Range (0, 10)) = 0.5
    }
 
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }
 
        ZWrite Off
        Cull Off
        ZTest LEqual
        Blend SrcAlpha OneMinusSrcAlpha
 
        HLSLINCLUDE
        #pragma fragment frag
 
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
 
        CBUFFER_START(UnityPerMaterial)
            half4 _OutlineColor;
            half _OutlineWidth;
        CBUFFER_END
 
        struct Attributes
        {
            float4 positionOS : POSITION;
            half3 normalOS : NORMAL;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };
 
        struct Varyings
        {
            float4 positionHCS : SV_POSITION;
            UNITY_VERTEX_OUTPUT_STEREO
        };
 
        half4 frag(Varyings IN) : SV_Target
        {
            return _OutlineColor;
        }
        ENDHLSL
 
        Pass
        {
            Name "NORMAL VECTOR (CLIP SPACE)"
 
            HLSLPROGRAM
            #pragma vertex vert
 
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
 
                half width = _OutlineWidth;
 
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
 
                float3 normalHCS = mul((float3x3)UNITY_MATRIX_VP, mul((float3x3)UNITY_MATRIX_M, IN.normalOS));
                OUT.positionHCS.xy += normalize(normalHCS.xy) / _ScreenParams.xy * OUT.positionHCS.w * width * 2;
 
                return OUT;
            }
            ENDHLSL
        }
    }
}