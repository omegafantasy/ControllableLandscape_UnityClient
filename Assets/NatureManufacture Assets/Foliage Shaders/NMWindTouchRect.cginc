uniform half _TouchReactActive;
sampler2D	_TouchReact_Buffer;
float4 _TouchReact_Pos;

float3 TouchReactAdjustVertex(float3 pos)
{
	float3 worldPos = mul(unity_ObjectToWorld, float4(pos, 1));
	float2 tbPos = saturate((float2(worldPos.x, -worldPos.z) - _TouchReact_Pos.xz) / _TouchReact_Pos.w);
	float2 touchBend = tex2Dlod(_TouchReact_Buffer, float4(tbPos, 0, 0));
	touchBend.y *= 1.0 - length(tbPos - 0.5) * 2;
	if (touchBend.y > 0.01)
	{
		worldPos.y = min(worldPos.y, touchBend.x * 10000);
	}

	float3 changedLocalPos = mul(unity_WorldToObject, float4(worldPos, 1)).xyz;
	return changedLocalPos - pos;
}