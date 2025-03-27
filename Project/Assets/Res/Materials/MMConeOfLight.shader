Shader "MoreMountains/ConeOfLight"
{
    Properties
    {
        _MainTex("Diffuse Texture", 2D) = "white" {}
        _Contrast("Contrast", Float) = 0.5
        _Color("Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags
        {
            "ForceNoShadowCasting" = "True"
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "ConeOfLight"
            ZTest Always
            AlphaTest Greater 0.0
            Blend DstColor One
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float _Contrast;
                float4 _Color;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float4 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.uv = input.uv.xy;
                output.color = input.color;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                return output;
            }

            float4 frag(Varyings input) : SV_Target
            {
                float4 diffuse = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                diffuse.rgb = diffuse.rgb * _Color.rgb * input.color.rgb;
                diffuse.rgb *= diffuse.a * _Color.a * input.color.a;
                diffuse *= _Contrast;
                return float4(diffuse);
            }
            
            ENDHLSL
        }
    }        
}
