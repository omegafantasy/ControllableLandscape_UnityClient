Shader "NatureManufacture Shaders/Trees/Tree Bark Specular"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,0)
		_MainTex("MainTex", 2D) = "white" {}
		[NoScaleOffset]_BumpMap("BumpMap", 2D) = "bump" {}
		_BumpScale("BumpScale", Range( 0 , 5)) = 1
		[NoScaleOffset]_SpecularRGBSmothnessA("Specular (RGB) Smothness (A)", 2D) = "white" {}
		_SpecularPower("Specular Power", Range( 0 , 2)) = 0
		_SmoothnessPower("Smoothness Power", Range( 0 , 2)) = 0
		[NoScaleOffset]_AmbientOcclusionG("Ambient Occlusion (G)", 2D) = "white" {}
		_AmbientOcclusionPower("Ambient Occlusion Power", Range( 0 , 1)) = 1
		_DetailMask("DetailMask", 2D) = "black" {}
		_DetailAlbedoMap("DetailAlbedoMap", 2D) = "white" {}
		[NoScaleOffset]_DetailNormal("Detail Normal", 2D) = "bump" {}
		[Toggle(_DETALUSEUV3_ON)] _DetalUseUV3("Detal Use UV3", Float) = 0
		_DetailNormalMapScale("DetailNormalMapScale", Range( 0 , 5)) = 1
		[NoScaleOffset]_DetailSpecularRGBSmothnessA("Detail Specular (RGB) Smothness (A)", 2D) = "white" {}
		[NoScaleOffset]_DetailAmbientOcclusionG("Detail Ambient Occlusion (G)", 2D) = "white" {}
		_InitialBend("Wind Initial Bend", Float) = 1
		_Stiffness("Wind Stiffness", Float) = 1
		_Drag("Wind Drag", Float) = 1
		[Toggle(_TOUCHREACTACTIVE_ON)] _TouchReactActive("TouchReactActive", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] _texcoord3( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" }
		Cull Back
		CGPROGRAM
		#include "UnityStandardUtils.cginc"
		#pragma target 3.0
		#pragma multi_compile_instancing
		#pragma shader_feature _TOUCHREACTACTIVE_ON
		#pragma shader_feature _DETALUSEUV3_ON
		#include "NMWindNoShiver.cginc"
		#include "NM_indirect.cginc"
		#pragma vertex vert
		#pragma instancing_options procedural:setup
		#pragma multi_compile GPU_FRUSTUM_ON __
		#pragma surface surf StandardSpecular keepalpha addshadow fullforwardshadows dithercrossfade 
		struct Input
		{
			float2 uv_texcoord;
			float2 uv3_texcoord3;
		};

		uniform float _BumpScale;
		uniform sampler2D _BumpMap;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform float _DetailNormalMapScale;
		uniform sampler2D _DetailNormal;
		uniform sampler2D _DetailAlbedoMap;
		uniform float4 _DetailAlbedoMap_ST;
		uniform float4 _DetailNormal_ST;
		uniform sampler2D _DetailMask;
		uniform float4 _DetailMask_ST;
		uniform float4 _Color;
		uniform sampler2D _SpecularRGBSmothnessA;
		uniform sampler2D _DetailSpecularRGBSmothnessA;
		uniform float _SpecularPower;
		uniform float _SmoothnessPower;
		uniform sampler2D _AmbientOcclusionG;
		uniform sampler2D _DetailAmbientOcclusionG;
		uniform float _AmbientOcclusionPower;

		void surf( Input i , inout SurfaceOutputStandardSpecular o )
		{
			float2 uv0_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			float2 uv0_DetailAlbedoMap = i.uv_texcoord * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
			float2 uv2_DetailNormal = i.uv3_texcoord3 * _DetailNormal_ST.xy + _DetailNormal_ST.zw;
			#ifdef _DETALUSEUV3_ON
				float2 staticSwitch123 = uv2_DetailNormal;
			#else
				float2 staticSwitch123 = uv0_DetailAlbedoMap;
			#endif
			float2 uv_DetailMask = i.uv_texcoord * _DetailMask_ST.xy + _DetailMask_ST.zw;
			float4 tex2DNode25 = tex2D( _DetailMask, uv_DetailMask );
			float3 lerpResult19 = lerp( UnpackScaleNormal( tex2D( _BumpMap, uv0_MainTex ), _BumpScale ) , UnpackScaleNormal( tex2D( _DetailNormal, staticSwitch123 ), _DetailNormalMapScale ) , tex2DNode25.a);
			o.Normal = lerpResult19;
			float4 lerpResult16 = lerp( tex2D( _MainTex, uv0_MainTex ) , tex2D( _DetailAlbedoMap, staticSwitch123 ) , tex2DNode25.a);
			o.Albedo = ( lerpResult16 * _Color ).rgb;
			float4 lerpResult18 = lerp( tex2D( _SpecularRGBSmothnessA, uv0_MainTex ) , tex2D( _DetailSpecularRGBSmothnessA, staticSwitch123 ) , tex2DNode25.a);
			float4 break22 = lerpResult18;
			float3 appendResult29 = (float3(break22.r , break22.g , break22.b));
			o.Specular = ( appendResult29 * _SpecularPower );
			o.Smoothness = ( break22.a * _SmoothnessPower );
			float lerpResult30 = lerp( tex2D( _AmbientOcclusionG, uv0_MainTex ).g , tex2D( _DetailAmbientOcclusionG, staticSwitch123 ).g , tex2DNode25.a);
			float clampResult34 = clamp( lerpResult30 , ( 1.0 - _AmbientOcclusionPower ) , 1.0 );
			o.Occlusion = clampResult34;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
}