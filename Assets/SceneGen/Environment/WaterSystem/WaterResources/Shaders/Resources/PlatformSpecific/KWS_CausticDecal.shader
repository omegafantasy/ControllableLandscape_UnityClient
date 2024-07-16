Shader "Hidden/KriptoFX/KWS/CausticDecal"
{
	Subshader
	{
		ZWrite Off
		Cull Front

		ZTest Always
		Blend DstColor Zero
		//Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 4.6

			#pragma multi_compile _ USE_DISPERSION
			#pragma multi_compile _ USE_LOD1 USE_LOD2 USE_LOD3
			#pragma multi_compile _ KW_DYNAMIC_WAVES
			#pragma multi_compile _ USE_DEPTH_SCALE

			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/AtmosphericScattering/AtmosphericScattering.hlsl"

			#include "../Common/KWS_WaterVariables.cginc"
			#include "../Common/KWS_WaterPassHelpers.cginc"
			#include "KWS_PlatformSpecificHelpers.cginc"
			#include "../Common/KWS_CommonHelpers.cginc"
			#include "../Common/KWS_WaterHelpers.cginc"
			#include "../Common/CommandPass/KWS_CausticDecal_Common.cginc"

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float4 screenUV : TEXCOORD0;
			};

			v2f vert(float4 vertex : POSITION)
			{
				v2f o;
				o.vertex = ObjectToClipPos(vertex);
				o.screenUV = ComputeScreenPos(o.vertex);
				return o;
			}


			half4 frag(v2f i) : SV_Target
			{
				float2 screenUV = i.screenUV.xy / i.screenUV.w;

				float depth;
				float3 worldPos;
				half4 caustic = GetCaustic(screenUV, depth, worldPos);

				float3 viewDir = GetWorldSpaceViewDirNorm(worldPos);
				half3 fogColor;
                half3 fogOpacity;
				
				i.vertex.z = depth;
				GetInternalFogVariables(i.vertex, viewDir, 0, 0, fogColor, fogOpacity);
				
				caustic.rgb = lerp(caustic.rgb, 1, fogOpacity);
				return caustic;
			}

			ENDHLSL
		}
	}
}
