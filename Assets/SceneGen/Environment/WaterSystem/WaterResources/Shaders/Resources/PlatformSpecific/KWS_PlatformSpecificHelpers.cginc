//#define EXPANSE
//#define TIME_OF_DAY
//#define ENVIRO_FOG
//#define AZURE_FOG
//#define ATMOSPHERIC_HEIGHT_FOG
//#define VOLUMETRIC_FOG_AND_MIST

//ATMOSPHERIC_HEIGHT_FOG also need to change the "Queue" = "Transparent-1"      -> "Queue" = "Transparent+2"
//VOLUMETRIC_FOG_AND_MIST also need to enable "Water->Rendering->DrawToDepth"


#define _FrustumCameraPlanes _FrustumPlanes


//------------------  unity includes   ----------------------------------------------------------------

#ifndef UNITY_COMMON_INCLUDED
	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#endif

#ifndef UNITY_SHADER_VARIABLES_INCLUDED
	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
#endif

#ifndef UNITY_COLOR_INCLUDED
	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#endif

#ifndef UNITY_ATMOSPHERIC_SCATTERING_INCLUDED
	#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/AtmosphericScattering/AtmosphericScattering.hlsl"
#endif

//-------------------------------------------------------------------------------------------------------



//------------------  thid party assets  ----------------------------------------------------------------

#if defined(ENVIRO_FOG)
	#include "Assets/Enviro - Sky and Weather/Core/Resources/Shaders/Core/EnviroFogCore.hlsl"
#endif

#if defined(ATMOSPHERIC_HEIGHT_FOG)
	#include "Assets/Third-party assets/BOXOPHOBIC/Atmospheric Height Fog/Core/Includes/AtmosphericHeightFog.cginc"
#endif

#if defined(VOLUMETRIC_FOG_AND_MIST)
	#include "Assets/VolumetricFog/Resources/Shaders/VolumetricFogOverlayVF.cginc"
#endif

#if defined(EXPANSE) 
	#include "Assets/Third-party assets/Expanse/transparency/shaders/transparency.hlsl"
#endif

//-------------------------------------------------------------------------------------------------------



float4 _CameraDepthTexture_TexelSize;
float4 _ColorPyramidTexture_TexelSize;
float3 KWS_AmbientColor;
float4x4 KWS_MATRIX_I_VP;

inline float4 ObjectToClipPos(float4 vertex)
{
	return TransformObjectToHClip(vertex.xyz);
}

inline float2 GetTriangleUV(uint vertexID)
{
	#if UNITY_UV_STARTS_AT_TOP
		return float2((vertexID << 1) & 2, 1.0 - (vertexID & 2));
	#else
		return float2((vertexID << 1) & 2, vertexID & 2);
	#endif
}

inline float4 GetTriangleVertexPosition(uint vertexID, float z = UNITY_NEAR_CLIP_VALUE)
{
	float2 uv = float2((vertexID << 1) & 2, vertexID & 2);
	return float4(uv * 2.0 - 1.0, z, 1.0);
}

inline float3 LocalToWorldPos(float3 localPos)
{
	float3 worldPos = mul(UNITY_MATRIX_M, float4(localPos, 1)).xyz;
	return GetAbsolutePositionWS(worldPos).xyz;
}

inline float3 WorldToLocalPos(float3 worldPos)
{
	return mul(UNITY_MATRIX_I_M, float4(GetCameraRelativePositionWS(worldPos), 1)).xyz;
}

inline float3 WorldToLocalPosWithoutTranslation(float3 worldPos)
{
	return mul((float3x3)UNITY_MATRIX_I_M, float4(worldPos, 1)).xyz;
}

inline float3 GetCameraRelativeWorldPos(float3 worldPos)
{
	return GetCameraRelativePositionWS(worldPos);
}

inline float3 GetWorldSpaceViewDirNorm(float3 worldPos)
{
	return GetWorldSpaceNormalizeViewDir(GetCameraRelativePositionWS(worldPos));
}

inline float3 GetWorldSpaceNormal(float3 normal)
{
	return normalize(mul((float3x3)UNITY_MATRIX_M, normal)).xyz;
}

inline float GetWorldToCameraDistance(float3 worldPos)
{
	return length(_WorldSpaceCameraPos.xyz - worldPos.xyz);
}


float3 GetWorldSpacePositionFromDepth(float2 uv, float deviceDepth)
{
	float4 positionCS = float4(uv * 2.0 - 1.0, deviceDepth, 1.0);
	#if UNITY_UV_STARTS_AT_TOP
		positionCS.y = -positionCS.y;
	#endif
	float4 hpositionWS = mul(KWS_MATRIX_I_VP, positionCS);
	return hpositionWS.xyz / hpositionWS.w;
}

inline float GetSceneDepth(float2 uv)
{
	return SampleCameraDepth(clamp(uv, 0.001, 0.999));
}

inline float4 GetSceneDepthGather(float2 uv)
{
	float4 depth;
	depth.x = SampleCameraDepth(uv + float2(_CameraDepthTexture_TexelSize.x, 0));
	depth.y = SampleCameraDepth(uv + float2(-_CameraDepthTexture_TexelSize.x, 0));
	depth.z = SampleCameraDepth(uv + float2(0, _CameraDepthTexture_TexelSize.y));
	depth.w = SampleCameraDepth(uv + float2(0, -_CameraDepthTexture_TexelSize.y));
	return depth;
}

inline float3 GetAmbientColor()
{
	float exposure = GetCurrentExposureMultiplier();
	return half3(unity_SHAr.w, unity_SHAg.w, unity_SHAb.w) * exposure;
}

inline float3 GetSceneColor(float2 uv)
{
	return LoadCameraColor(clamp(uv, 0.001, 0.999) * _ScreenSize.xy);
}

inline half3 GetSceneColorWithDispersion(float2 uv, float dispersionStrength)
{
	half3 refraction;
	refraction.r = GetSceneColor(uv - _ColorPyramidTexture_TexelSize.xy * dispersionStrength).r;
	refraction.g = GetSceneColor(uv).g;
	refraction.b = GetSceneColor(uv + _ColorPyramidTexture_TexelSize.xy * dispersionStrength).b;
	return refraction;
}

inline float3 GetMainLightDir()
{
	DirectionalLightData dirLight = _DirectionalLightDatas[_DirectionalShadowIndex];
	return -dirLight.forward;
}

inline float3 GetMainLightColor()
{
	DirectionalLightData dirLight = _DirectionalLightDatas[_DirectionalShadowIndex];
	return dirLight.color;
}

inline float4 ComputeNonStereoScreenPos(float4 pos)
{
	float4 o = pos * 0.5f;
	o.xy = float2(o.x, o.y * _ProjectionParams.x) + o.w;
	o.zw = pos.zw;
	return o;
}

inline float4 ComputeScreenPos(float4 pos)
{
	float4 o = ComputeNonStereoScreenPos(pos);
	#if defined(UNITY_SINGLE_PASS_STEREO)
		o.xy = TransformStereoScreenSpaceTex(o.xy, pos.w);
	#endif
	return o;
}

inline float4 ComputeGrabScreenPos(float4 pos)
{
	#if UNITY_UV_STARTS_AT_TOP
		float scale = -1.0;
	#else
		float scale = 1.0;
	#endif
	float4 o = pos * 0.5f;
	o.xy = float2(o.x, o.y * scale) + o.w;
	#ifdef UNITY_SINGLE_PASS_STEREO
		o.xy = TransformStereoScreenSpaceTex(o.xy, pos.w);
	#endif
	o.zw = pos.zw;
	return o;
}

inline float LinearEyeDepth(float z)
{
	return 1.0 / (_ZBufferParams.z * z + _ZBufferParams.w);
}


inline void GetInternalFogVariables(float4 pos, float3 viewDir, float surfaceDepthZ, float screenPosZ, out half3 fogColor, out half3 fogOpacity)
{
	PositionInputs posInput = GetPositionInput(pos.xy, _ScreenSize.zw, pos.z, UNITY_MATRIX_I_VP, UNITY_MATRIX_V);

	#if defined(EXPANSE)
		float4 expanseFogAndClouds = float4(0, 0, 0, 1);
		expanseFogAndClouds = EvaluateExpanseFogAndClouds(Linear01Depth(posInput.deviceDepth, _ZBufferParams), posInput.positionNDC, expanseFogAndClouds, GetCurrentExposureMultiplier());
		fogColor = expanseFogAndClouds.xyz;
		fogOpacity = expanseFogAndClouds.w;
	#else
		EvaluateAtmosphericScattering(posInput, viewDir, fogColor, fogOpacity);
		fogOpacity = saturate(fogOpacity * 2);
	#endif
}


inline half3 ComputeInternalFog(half3 sourceColor, half3 fogColor, half3 fogOpacity)
{
	return lerp(sourceColor, fogColor, fogOpacity);
}

inline half3 ComputeThirdPartyFog(half3 sourceColor, float3 worldPos, float2 screenUV, float3 screenPosZ)
{
	#if defined(ENVIRO_FOG)
		sourceColor = TransparentFog(half4(sourceColor, 1.0), worldPos.xyz, screenUV, screenPosZ);
	#elif defined(AZURE_FOG)
		sourceColor = ApplyAzureFog(half4(sourceColor, 1.0), worldPos.xyz).xyz;
	#elif defined(ATMOSPHERIC_HEIGHT_FOG)
		float4 fogParams = GetAtmosphericHeightFog(GetCameraRelativePositionWS(worldPos));
		fogParams.a = saturate(fogParams.a * 1.5f); //by some reason max value < 0.75;
		sourceColor = ApplyAtmosphericHeightFog(half4(sourceColor, 1.0), fogParams).xyz;
	#endif

	return max(0, sourceColor);
}

inline float GetExposure()
{
	return GetCurrentExposureMultiplier();
}

float GetSurfaceDepth(float screenPosZ)
{
	#if UNITY_REVERSED_Z
		#if SHADER_API_OPENGL || SHADER_API_GLES || SHADER_API_GLES3
			//GL with reversed z => z clip range is [near, -far] -> should remap in theory but dont do it in practice to save some perf (range is close enough)
			return max(-screenPosZ, 0);
		#else
			//D3d with reversed Z => z clip range is [near, 0] -> remapping to [0, far]
			//max is required to protect ourselves from near plane not being correct/meaningfull in case of oblique matrices.
			return max(((1.0 - screenPosZ / _ProjectionParams.y) * _ProjectionParams.z), 0);
		#endif
	#elif UNITY_UV_STARTS_AT_TOP
		//D3d without reversed z => z clip range is [0, far] -> nothing to do
		return screenPosZ;
	#else
		//Opengl => z clip range is [-near, far] -> should remap in theory but dont do it in practice to save some perf (range is close enough)
		return screenPosZ;
	#endif
}
