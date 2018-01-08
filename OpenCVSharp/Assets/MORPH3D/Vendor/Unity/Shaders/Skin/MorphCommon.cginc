float4 Overlay(float2 uv)
{
#if !defined(_OVERLAY)
	return 0;
#else
	return tex2D(_Overlay, uv);
#endif
}

float4 EyeTex(float2 uv)
{
#if !defined(_EYETEX)
	return 0;
#else
	return tex2D(_EyeTex, uv);
#endif
}

bool IsEyeball(float2 uv)
{
	return uv.x > 0.4 && uv.x < 0.8 && uv.y < 0.202 && uv.y > 0.010;
}

bool IsIris(float2 uv)
{
	return uv.x > 0.507 && uv.x < 0.695 && uv.y < 0.098 && uv.y > 0.010;
}
//this is needed for M3DMale
bool IsEyeRing(float2 uv, float3 color)
{
#if defined(_INCLUDERINGCHECK)
	bool region = uv.y < 0.146 && uv.y > 0.121 &&
		((uv.x > 0.455 && uv.x < 0.480) || (uv.x > 0.720 && uv.x < 0.747));
	if (!region) {
		return false;
	}

	float x = color.r * color.g * color.b;
	float y = color.r + color.g + color.b;
	if (x < 0.2 && y > 0) {
		return true;
	}
	return false;
#else
	return false;
#endif
}

bool IsOpaqueEyeMask(float2 uv)
{
	uv = frac(uv);
	//return  uv.x > 0.454 && uv.x < 0.769  && uv.y < 0.14 && uv.y > 0.11;

	

	//return  uv.x > 0.454 && uv.x < 0.479  && uv.y < 0.14 && uv.y > 0.11;
	return
		(uv.x > 0.44 && uv.x < 0.479  && uv.y < 0.14 && uv.y > 0.11)
		|| (uv.x > 0.71 && uv.x < 0.77  && uv.y < 0.14 && uv.y > 0.11)
	;
}