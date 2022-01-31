Shader "Azure[Sky]/Fog Scattering"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		//_Azure_FogScatteringTexture("Scattering Texture", 2D) = "white" {}
	}

	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			HLSLPROGRAM
			#pragma vertex vertex_program
			#pragma fragment fragment_program
			#pragma target 3.0
			#include "UnityCG.cginc"

			//  Start: LuxWater
			#pragma multi_compile __ LUXWATER_DEFERREDFOG

			#if defined(LUXWATER_DEFERREDFOG)
				sampler2D _UnderWaterMask;
				float4 _LuxUnderWaterDeferredFogParams; // x: IsInsideWatervolume?, y: BelowWaterSurface shift, z: EdgeBlend
			#endif
			//  End: LuxWater

			uniform sampler2D _MainTex;
			uniform sampler2D_float _CameraDepthTexture;
			uniform float4x4  _FrustumCorners;
			uniform float4    _MainTex_TexelSize;

			// Scattering
			uniform float3 _Azure_Br;
			uniform float3 _Azure_Bm;
			uniform float  _Azure_Kr;
			uniform float  _Azure_Km;
			uniform float  _Azure_Scattering;
			uniform float  _Azure_SunIntensity;
			uniform float  _Azure_NightIntensity;
			uniform float  _Azure_Exposure;
			uniform float4 _Azure_RayleighColor;
			uniform float4 _Azure_MieColor;
			uniform float3 _Azure_MieG;
			uniform float  _Azure_Pi316;
			uniform float  _Azure_Pi14;
			uniform float  _Azure_Pi;

			// Night sky
			uniform float4 _Azure_MoonBrightColor;
			uniform float  _Azure_MoonBrightRange;

			// Fog
			uniform float _Azure_FogDistance;
			uniform float _Azure_FogBlend;
			uniform float _Azure_MieDistance;

			// Directions
			uniform float3 _Azure_SunDirection;
			uniform float3 _Azure_MoonDirection;

			// Matrix
			uniform float4x4 _Azure_SkyUpDirectionMatrix;
			uniform float4x4 _Azure_SunMatrix;
			uniform float4x4 _Azure_MoonMatrix;
			uniform float4x4 _Azure_StarfieldMatrix;

			// Vertex shader inputs from mesh data
			struct Attributes
			{
				float4 vertex   : POSITION;
				float4 texcoord : TEXCOORD0;
			};

			// Vertex to Fragment
			// Vertex shader outputs || Fragment shader inputs
			struct Varyings
			{
				float4 Position        : SV_POSITION;
				float2 uv 	           : TEXCOORD0;
				float4 interpolatedRay : TEXCOORD1;
				float2 uv_depth        : TEXCOORD2;
			};

			// Vertex shader.
			Varyings vertex_program(Attributes v)
			{
				Varyings Output;

				v.vertex.z = 0.1;
				Output.Position = UnityObjectToClipPos(v.vertex);
				Output.uv = v.texcoord.xy;
				Output.uv_depth = v.texcoord.xy;
				#if UNITY_UV_STARTS_AT_TOP
				if (_MainTex_TexelSize.y < 0)
					Output.uv.y = 1 - Output.uv.y;
				#endif

				//Based on Unity5.6 GlobalFog.
				//--------------------------------
				int index = v.texcoord.x + (2.0 * Output.uv.y);
				Output.interpolatedRay = _FrustumCorners[index];
				Output.interpolatedRay.xyz = mul((float3x3)_Azure_SkyUpDirectionMatrix, Output.interpolatedRay.xyz);
				Output.interpolatedRay.w = index;

				return Output;
			}

			// Fragment shader || Pixel shader.
			float4 fragment_program(Varyings Input) : SV_Target
			{
				//Original scene.
				float3 screen = tex2D(_MainTex, UnityStereoTransformScreenSpaceTex(Input.uv)).rgb;

				//Reconstruct world space position and direction towards this screen pixel.
				float depth = Linear01Depth(UNITY_SAMPLE_DEPTH(tex2D(_CameraDepthTexture,UnityStereoTransformScreenSpaceTex(Input.uv_depth))));
				if (depth == 1.0) return float4(screen, 1.0);
				float3 viewDir = normalize(depth * Input.interpolatedRay.xyz);
				float sunCosTheta = dot(viewDir, _Azure_SunDirection);
				float r = length(float3(0.0, 50.0, 0.0));
				float sunRise = saturate(dot(float3(0.0, 500.0, 0.0), _Azure_SunDirection) / r);
				float moonRise = saturate(dot(float3(0.0, 500.0, 0.0), _Azure_MoonDirection) / r);
				float mieDepth = saturate(lerp(1.0, depth * 4.0, _Azure_MieDistance));

				//Optical Depth.
				float zenith = acos(saturate(dot(float3(-1.0, 1.0, -1.0), depth)));//Fog Scale.
				float z = cos(zenith) + 0.15 * pow(93.885 - ((zenith * 180.0) / _Azure_Pi), -1.253);
				float SR = _Azure_Kr / z;
				float SM = _Azure_Km / z;

				// Total Extinction.
				float3 fex = exp(-(_Azure_Br*SR + _Azure_Bm * SM));
				float sunset = clamp(dot(float3(0.0, 1.0, 0.0), _Azure_SunDirection), 0.0, 0.6);
				float3 extinction = lerp(fex, (1.0 - fex), sunset);

				// Scattering.
				//float  rayPhase = 1.0 + pow(sunCosTheta, 2.0);										 // Preetham rayleigh phase function.
				float  rayPhase = 2.0 + 0.5 * pow(sunCosTheta, 2.0);									 // Rayleigh phase function based on the Nielsen's paper.
				float  miePhase = _Azure_MieG.x / pow(_Azure_MieG.y - _Azure_MieG.z * sunCosTheta, 1.5); // The Henyey-Greenstein phase function.

				float3 BrTheta = _Azure_Pi316 * _Azure_Br * rayPhase * _Azure_RayleighColor.rgb * extinction;
				float3 BmTheta = _Azure_Pi14 * _Azure_Bm * miePhase * _Azure_MieColor.rgb * extinction * sunRise;
				BmTheta *= mieDepth;
				float3 BrmTheta = (BrTheta + BmTheta) / (_Azure_Br + _Azure_Bm);

				float3 inScatter = BrmTheta * _Azure_Scattering * (1.0 - fex);
				inScatter *= sunRise;

				// Night Sky.
				BrTheta = _Azure_Pi316 * _Azure_Br * rayPhase * _Azure_RayleighColor.rgb;
				BrmTheta = (BrTheta) / (_Azure_Br + _Azure_Bm);
				float3 nightSky = BrmTheta * _Azure_NightIntensity * (1.0 - fex);

				// Moon Bright.
				float3 moonBright = 1.0 + dot(viewDir, -_Azure_MoonDirection);
				moonBright = 1.0 / (0.25 + moonBright * _Azure_MoonBrightRange) * _Azure_MoonBrightColor.rgb;
				moonBright *= moonRise * mieDepth;

				// Fog Color.
				float3 inScatterOutput = inScatter + nightSky + moonBright;

				// Tonemapping.
				inScatterOutput = saturate(1.0 - exp(-_Azure_Exposure * inScatterOutput));

				//float3 inScatterOutput = tex2D(_Azure_ScatteringTexture2, UnityStereoTransformScreenSpaceTex(IN.uv)).rgb;

				// Color Correction.
				inScatterOutput = pow(inScatterOutput, 2.2);
				#ifdef UNITY_COLORSPACE_GAMMA
				inScatterOutput = pow(inScatterOutput, 0.4545);
				#else
				inScatterOutput = inScatterOutput;
				#endif

				// Apply Fog.
				//float fog = smoothstep(-_Azure_FogBlend, 1.25, depth * _ProjectionParams.z / _Azure_FogDistance);
				float fog = smoothstep(-_Azure_FogBlend, 1.25, length(depth * Input.interpolatedRay.xyz) / _Azure_FogDistance); // Radial fog distance

				//  Start: LuxWater
            	#if defined(LUXWATER_DEFERREDFOG)
				half4 fogMask = tex2D(_UnderWaterMask, UnityStereoTransformScreenSpaceTex(Input.uv));
				float watersurfacefrombelow = DecodeFloatRG(fogMask.ba);

				//	Get distance and lower it a bit in order to handle edge blending artifacts (edge blended parts would not get ANY fog)
				float dist = (watersurfacefrombelow - depth) + _LuxUnderWaterDeferredFogParams.y * _ProjectionParams.w;
				//	Fade fog from above water to below water
				float fogFactor = saturate ( 1.0 + _ProjectionParams.z * _LuxUnderWaterDeferredFogParams.z * dist ); // 0.125 
				//	Clamp above result to where water is actually rendered
				fogFactor = (fogMask.r == 1) ? fogFactor : 1.0;
				//  Mask fog on underwarter parts - only if we are inside a volume (bool... :( )
	            if(_LuxUnderWaterDeferredFogParams.x) {
	                fogFactor *= saturate( 1.0 - fogMask.g * 8.0);
	                if (dist < -_ProjectionParams.w * 4 && fogMask.r == 0 && fogMask.g < 1.0) {
	                    fogFactor = 1.0;
	                }
	            }
				//	Tweak fog factor
				fog *= fogFactor;
            	#endif
        		//  End: LuxWater 

				inScatterOutput.rgb = lerp(screen.rgb, inScatterOutput.rgb, fog);
				return float4(inScatterOutput.rgb, 1.0);
			}

			ENDHLSL
		}
	}
}