Shader "Hidden/KriptoFX/KWS/FoamParticles"
{
    Properties
    {
        _Color ("Color", Color) = (0.9, 0.9, 0.9, 0.2)
        _MainTex ("Texture", 2D) = "white" { }
        KW_VAT_Position ("Position texture", 2D) = "white" { }
        KW_VAT_Alpha ("Alpha texture", 2D) = "white" { }
        KW_VAT_Offset ("Height Offset", 2D) = "black" { }
        KW_VAT_RangeLookup ("Range Lookup texture", 2D) = "white" { }
        _FPS ("FPS", Float) = 6.66666
        _Size ("Size", Float) = 0.09
        _Scale ("AABB Scale", Vector) = (26.3, 4.8, 30.5)
        _NoiseOffset ("Noise Offset", Vector) = (0, 0, 0)
        _Offset ("Offset", Vector) = (-9.35, -2.025, -15.6, 0)
        _Test ("Test", Float) = 0.1
    }
    SubShader
    {
        Tags { "RenderPipeline" = "HDRenderPipeline" "RenderType" = "HDLitShader" "Queue" = "Transparent" }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM

            #define SHADOW_USE_DEPTH_BIAS   0
            #define SHADOW_LOW
            #define SHADOW_AUTO_FLIP_NORMAL 0

            #pragma vertex vert_foam
            #pragma fragment frag_foam
            
            #pragma target 4.5
            
            #pragma multi_compile_fog

            #pragma multi_compile _ FOAM_RECEIVE_SHADOWS
            #pragma multi_compile _ FOAM_COMPUTE_WATER_OFFSET
            #pragma multi_compile _ USE_MULTIPLE_SIMULATIONS

            #define SHADERPASS SHADERPASS_FORWARD
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/GeometricTools.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Builtin/BuiltinData.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Sky/PhysicallyBasedSky/PhysicallyBasedSkyCommon.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/LightLoop/LightLoopDef.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/LightEvaluation.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/AtmosphericScattering/AtmosphericScattering.hlsl"

            #include "../Common/KWS_WaterVariables.cginc"
            #include "../Common/KWS_WaterPassHelpers.cginc"
            #include "../Common/KWS_CommonHelpers.cginc"
            #include "KWS_PlatformSpecificHelpers.cginc"
            #include "../Common/Shoreline/KWS_Shoreline_Common.cginc"
            #include "../Common/KWS_WaterVertPass.cginc"
            #include "../Common/Shoreline/KWS_FoamParticles_Core.cginc"


            inline half3 GetHDRPLighting(float3 worldPos, float2 screenUV, out half lightVolumetricDimmer)
            {
                float4 lightColor = 1;
                float atten = 1;
                if (_DirectionalShadowIndex >= 0)
                {
                    LightLoopContext context;
                    context.shadowContext = InitShadowContext();
                    PositionInputs posInput;
                    DirectionalLightData dirLight = _DirectionalLightDatas[_DirectionalShadowIndex];
                    float3 L = -dirLight.forward;
                    int cascadeCount;
                    posInput.positionWS = worldPos;
                    float shadowVal = 1;

                    if ((dirLight.volumetricLightDimmer > 0) && (dirLight.volumetricShadowDimmer > 0))
                    {
                        //ApplyCameraRelativeXR(posInput.positionWS);
                        #if defined(FOAM_RECEIVE_SHADOWS)
                            if ((dirLight.lightDimmer) > 0 && (dirLight.shadowDimmer > 0))
                                shadowVal = GetDirectionalShadowAttenuation(context.shadowContext, screenUV, GetCameraRelativePositionWS(worldPos), 0.0, dirLight.shadowIndex, L);
                            
                        #endif
                    }
                    lightColor = EvaluateLight_Directional(context, posInput, dirLight);
                    //lightColor.a *= dirLight.volumetricLightDimmer;
                    lightVolumetricDimmer = dirLight.volumetricLightDimmer;
                   
                    lightColor.rgb *= GetCurrentExposureMultiplier();
                     lightColor.rgb *= lightColor.a;
                    #if defined(FOAM_RECEIVE_SHADOWS)
                        lightColor.rgb *= shadowVal;
                    #endif
                }
                else lightVolumetricDimmer = 1;

                lightColor.rgb = lightColor.rgb * 0.25 + GetAmbientColor() * 0.25;

                return clamp(lightColor.rgb, 0, 1.0);
            }

            struct v2f_foam
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float alpha : TEXCOORD1;
                float3 worldPos : TEXCOORD2;
                half4 screenPos : TEXCOORD3;
            };

            v2f_foam vert_foam(appdata_foam v)
            {
                v2f_foam o;

                float particleID = v.uv.z;
                float4 particleData = DecodeParticleData(particleID); //xyz - position, w - alpha
                float depth;
                float3 localPos = ParticleDataToLocalPosition(particleData.xyz, 0.0, depth);
                v.vertex.xyz = CreateBillboardParticle(0.65, v.uv.xy, localPos.xyz);
                o.alpha = particleData.a;
                o.uv = GetParticleUV(v.uv.xy);
                o.worldPos = LocalToWorldPos(v.vertex.xyz);
                o.pos = ObjectToClipPos(v.vertex);

                o.screenPos = ComputeScreenPos(o.pos);
                //o.lightColor = GetHDRPLighting(o.worldPos.xyz, (screenPos.xy / screenPos.w) * _ScreenSize.xy);
                
                return o;
            }

            half4 frag_foam(v2f_foam i) : SV_Target
            {
                half4 result;
                result.a = GetParticleAlpha(i.alpha, _Color.a, i.uv);
                if (result.a < 0.002) return 0;

                float3 viewDir = GetWorldSpaceViewDirNorm(i.worldPos);
                half3 fogColor;
                half3 fogOpacity;
                GetInternalFogVariables(i.pos, viewDir, 0, 0, fogColor, fogOpacity);

                half lightVolumetricDimmer; 
                half3 lightColor = GetHDRPLighting(i.worldPos.xyz, (i.screenPos.xy / i.screenPos.w) * _ScreenSize.xy, lightVolumetricDimmer);

                result.rgb = lightColor;
                result.rgb = ComputeInternalFog(result.rgb, fogColor, fogOpacity);
                //result.rgb = ComputeThirdPartyFog(result.rgb , i.worldPos, i.uv, float3 screenPosZ);
                
                return result;
            }

            ENDHLSL
        }
    }
}