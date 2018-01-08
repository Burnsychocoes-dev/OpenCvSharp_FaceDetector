Shader "Morph3D/Skin Deferred" {

	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_NormalTex("Normal", 2D) = "bump" {}

		_Overlay("Overlay",2D) = "clear" {}
		_OverlayColor("Overlay Color", Color) = (0,0,0,0)
		_EyeTint("Eye Tint", Color) = (0,0,0,0)
		_EyeTex("Eye Albedo", 2D) = "clear" {}

		_Glossiness("Smoothness", Range(0,1)) = 0.5
		//_Metallic("Metallic", Range(0,1)) = 0.0

		_AlphaTex("Alpha Texture", 2D) = "white" {}
		_AlphaCutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

		_SpecColorA("Specular", Color) = (0.2,0.2,0.2,1)
		_SpecGlossMap("Specular", 2D) = "white" {}
		_OcclusionStrength("Strength", Range(0.0, 1.0)) = 1
		_OcclusionMap("Occlusion", 2D) = "white" {}
	}

	SubShader{
		Tags{
			"RenderType"="TransparentCutout"
			"Queue" = "AlphaTest"
		}


		LOD 200

			Cull Back

			CGPROGRAM
			#pragma target 3.0
			#pragma surface surf StandardSpecular addshadow


			#pragma shader_feature _OVERLAY
			#pragma shader_feature _EYETINT
			#pragma shader_feature _EYETEX
			#pragma shader_feature _INCLUDERINGCHECK

			sampler2D _MainTex;

			sampler2D _NormalTex;

			float _AlphaCutoff;

			sampler2D _SpecGlossMap;
			sampler2D _OcclusionMap;
			float4 _SpecColorA;
			float _OcclusionStrength;

			//MORPH3D uniforms
			uniform sampler2D _AlphaTex;
			uniform sampler2D   _Overlay;
			uniform float4       _OverlayColor;
			uniform sampler2D   _EyeTex;

			uniform float4       _EyeTint;

			struct Input {
				float2 uv_MainTex;
				float2 uv_NormalTex;
				float2 uv_OcclusionTex;
				float2 uv_SpecGlossTex;
			};

			float _Glossiness;
			//half _Metallic;
			float4 _Color;
			
			#include "MorphCommon.cginc"

			void surf(Input IN, inout SurfaceOutputStandardSpecular o) {
				// Albedo comes from a texture tinted by color
				float2 uv = IN.uv_MainTex.xy;
				float4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
				o.Alpha = tex2D(_AlphaTex, IN.uv_MainTex.xy).r;//use our custom alpha map
				clip(o.Alpha - _AlphaCutoff);

				if (IsOpaqueEyeMask(uv)) {
					float x = c.r + c.g + c.b;
					if (x < 0.4) {
						discard;
					}
				}

				#if _OVERLAY
					//check for our overlay
					float4 overlay = Overlay(uv);
					if (overlay.a > 0.05) {
						//blend the overlay color with the overlay
						overlay.rgb = ( overlay.rgb * (1 - _OverlayColor.a)) + (_OverlayColor.rgb * _OverlayColor.a);
						//blend the overlay with the skin
						overlay.rgb = ((1 - overlay.a) * c.rgb) + (overlay.a * overlay.rgb);

						c = overlay.rgba;
					}
				#endif

				#if _EYETEX && _EYETINT
					if (IsIris(uv)) {
						//check for our overlay
						half4 eyeTex = EyeTex(uv);
						if (eyeTex.a > 0) {
							//blend the tint color with the replacement
							eyeTex.rgb = (eyeTex.rgb * (1 - _EyeTint.a)) + (_EyeTint.rgb * _EyeTint.a);
							//blend the replacement with the base
							eyeTex.rgb = ((1 - eyeTex.a) * c.rgb) + (eyeTex.a * eyeTex.rgb);

							c = eyeTex;
						}
					}

				#endif

				#if _EYETEX && !_EYETINT
					if (IsIris(uv)) {
						half4 eyeTex = EyeTex(uv);
						if (eyeTex.a > 0) {
							//blend the replacement with the base
							eyeTex.rgb = ((1 - eyeTex.a) * c.rgb) + (eyeTex.a * eyeTex.rgb);

							c = eyeTex;
						}
					}

				#endif

				#if !_EYETEX && _EYETINT
					bool isEyeRing = IsEyeRing(uv, c);
					if (IsIris(uv) || isEyeRing) {
						if (isEyeRing) {
							//fade out the tint a bit on the ring
							_EyeTint.a *= 0.9;
						}
						c.rgb = ((1 - _EyeTint.a) * c.rgb) + (_EyeTint.a * _EyeTint.rgb);
					}
				#endif

				c.a = 1;
				
				o.Albedo = c.rgba;



				o.Alpha = c.a;
				o.Normal = UnpackNormal(tex2D(_NormalTex, IN.uv_NormalTex));
				o.Occlusion = tex2D(_OcclusionMap, IN.uv_OcclusionTex) * (_OcclusionStrength);

				//SurfaceOutputStandard
				//o.Metallic = _Metallic;

				//SurfaceOutputStandardSpecular
				o.Specular = tex2D(_SpecGlossMap, IN.uv_SpecGlossTex) * _SpecColorA;
				o.Smoothness = _Glossiness;
			}
			ENDCG
			
	}
	FallBack "Diffuse"
	CustomEditor "MorphSkinDeferredGUI"
}
