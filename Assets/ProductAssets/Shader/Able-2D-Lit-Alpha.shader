Shader "Able/2D/Lit-Alpha"
{
    Properties
    {
        _MainTex("Diffuse", 2D) = "white" {}

        // Legacy properties. They're here so that materials using this shader can gracefully fallback to the legacy sprite shader.
        [HideInInspector] _Color("Tint", Color) = (1,1,1,1)
        [HideInInspector] _RendererColor("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip("Flip", Vector) = (1,1,1,1)
        [HideInInspector] _AlphaTex("External Alpha", 2D) = "white" {}
        [HideInInspector] _EnableExternalAlpha("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags 
        {
            "Queue" = "Transparent" 
            "RenderType" = "Transparent" 
            "RenderPipeline" = "UniversalPipeline" 
        }

        Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            Tags 
            { 
                "LightMode" = "UniversalForward" 
                "Queue"="Transparent" 
                "RenderType"="Transparent"
            }

            HLSLPROGRAM
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            #pragma vertex UnlitVertex
            #pragma fragment UnlitFragment

            struct Attributes
            {
                float3 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                float4 color       : COLOR;
                float2 uv          : TEXCOORD0;
                float3 positionWS  : TEXCOORD2;
                float4 shadowCoord : TEXCOORD6;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;
            float4 _Color;
            half4 _RendererColor;

            Varyings UnlitVertex(Attributes IN)
            {
                Varyings OUT = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                //OUT.positionCS = TransformObjectToHClip(IN.positionOS);
                //OUT.positionWS = TransformObjectToWorld(IN.positionOS);
                VertexPositionInputs vertexInput = GetVertexPositionInputs(IN.positionOS.xyz);
                OUT.positionCS = vertexInput.positionCS;
                OUT.positionWS = vertexInput.positionWS;
                OUT.shadowCoord = TransformWorldToShadowCoord(OUT.positionWS); //阴影坐标
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                OUT.color = IN.color * _Color * _RendererColor;

                return OUT;
            }

            float4 UnlitFragment(Varyings IN) : SV_Target
            {
                float4 mainTex = IN.color * SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                #if defined(DEBUG_DISPLAY)
                SurfaceData2D surfaceData;
                InputData2D inputData;
                half4 debugColor = 0;

                InitializeSurfaceData(mainTex.rgb, mainTex.a, surfaceData);
                InitializeInputData(IN.uv, inputData);
                SETUP_DEBUG_DATA_2D(inputData, IN.positionWS);

                if(CanDebugOverrideOutputColor(surfaceData, inputData, debugColor))
                {
                    return debugColor;
                }
                #endif

                //主光源
                float4 shadowCoord = TransformWorldToShadowCoord(IN.positionWS); //阴影坐标
                Light lightMain = GetMainLight(shadowCoord);
                half3 colorLightMain = lightMain.color * lightMain.distanceAttenuation;
                //次光源
                uint lightsCount = GetAdditionalLightsCount();
                LIGHT_LOOP_BEGIN(lightsCount)
                    Light light = GetAdditionalLight(lightIndex, IN.positionWS);
                    half3 lightColor = light.color * light.distanceAttenuation;
                    colorLightMain += lightColor;
                LIGHT_LOOP_END
                //计算阴影衰减
                colorLightMain = colorLightMain * lightMain.shadowAttenuation;

                //最终颜色
                mainTex.rgb = mainTex.rgb * colorLightMain;

                return mainTex;
            }
            ENDHLSL
        }
    }

    Fallback "Sprites/Default"
}
