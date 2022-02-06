Shader "Skybox/PixelSky"
{
	SubShader
	{
		Tags { "Queue" = "Background" "RenderType" = "Background" "PreviewType" = "Skybox" "IgnoreProjector" = "True" }
		Cull Back     // Render side
		Fog { Mode Off } // Don't use fog
		ZWrite Off    // Don't draw to bepth buffer

		Pass
		{
			HLSLPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#include "UnityCG.cginc"

			// Textures
		    uniform sampler2D   _SunTexture;
	        uniform sampler2D   _MoonTexture;
			uniform samplerCUBE _StarfieldTexture;

			// Scattering
			uniform float3 _Br;
			uniform float3 _Bm;
			uniform float  _Kr;
			uniform float  _Km;
			uniform float  _Scattering;
			uniform float  _SunIntensity;
			uniform float  _NightIntensity;
			uniform float  _Exposure;
			uniform float4 _RayleighColor;
			uniform float4 _MieColor;
			uniform float3 _MieG;
			uniform float  _Pi316;
			uniform float  _Pi14;
			uniform float  _Pi;

			// Night sky
			uniform float4 _MoonDiskColor;
			uniform float4 _MoonBrightColor;
			uniform float  _MoonBrightRange;
			uniform float  _StarfieldIntensity;
			uniform float  _MilkyWayIntensity;
			uniform float3 _StarfieldColorBalance;

			// Options
			uniform float  _SunDiskSize;
			uniform float  _MoonDiskSize;

			// Directions
			uniform float3 _SunDirection;
			uniform float3 _MoonDirection;

			// Matrix
			uniform float4x4 _SkyUpDirectionMatrix;
			uniform float4x4 _SunMatrix;
			uniform float4x4 _MoonMatrix;
			uniform float4x4 _StarfieldMatrix;

			// Vertex shader inputs from mesh data
			struct Attributes
			{
				float4 vertex : POSITION;
			};

			// Vertex to Fragment
			// Vertex shader outputs || Fragment shader inputs
			struct Varyings
			{
				float4 Position     : SV_POSITION;
				float3 LocalPos     : TEXCOORD0;
				float3 StarfieldPos : TEXCOORD1;
				float3 SunPos       : TEXCOORD2;
				float3 MoonPos      : TEXCOORD3;
			};
			
			// Vertex shader
			Varyings vert(Attributes Input)
			{
				Varyings Output = (Varyings)0;

				Output.Position = UnityObjectToClipPos(Input.vertex);
				Output.LocalPos = normalize(mul((float3x3)unity_WorldToObject, Input.vertex.xyz));
				Output.LocalPos = normalize(mul((float3x3)_SkyUpDirectionMatrix, Output.LocalPos));

				// Matrix
				Output.SunPos       = mul((float3x3)_SunMatrix, Input.vertex.xyz) * 0.75 * _SunDiskSize;
				Output.StarfieldPos = mul((float3x3)_SunMatrix, Input.vertex.xyz);
				Output.StarfieldPos = mul((float3x3)_StarfieldMatrix, Output.StarfieldPos);
				Output.MoonPos      = mul((float3x3)_MoonMatrix, Input.vertex.xyz) * 0.75 * _MoonDiskSize;
				Output.MoonPos.x   *= -1.0;

				return Output;
			}
			
			// Fragment shader || Pixel shader
			float4 frag(Varyings Input) : SV_Target
			{
				// Directions
			    float r = length(float3(0.0, 50.0, 0.0));
				float3 viewDir = normalize(Input.LocalPos);
				float sunCosTheta = dot(viewDir, _SunDirection);
				float sunRise = saturate(dot(float3(0.0, 500.0, 0.0), _SunDirection) / r);
				float moonRise = saturate(dot(float3(0.0, 500.0, 0.0), _MoonDirection) / r);

				// Optical depth
				float zenith = acos(saturate(dot(float3(0.0, 1.0, 0.0), viewDir)));
				float z = cos(zenith) + 0.15 * pow(93.885 - ((zenith * 180.0) / _Pi), -1.253);
				float SR = _Kr / z;
				float SM = _Km / z;

				// Total extinction
				float3 fex = exp(-(_Br*SR + _Bm * SM));
				float  sunset = clamp(dot(float3(0.0, 1.0, 0.0), _SunDirection), 0.0, 0.6);
				float3 extinction = lerp(fex, (1.0 - fex), sunset);

				// Scattering
				//float  rayPhase = 1.0 + pow(sunCosTheta, 2.0);										 // Preetham rayleigh phase function.
				float  rayPhase = 2.0 + 0.5 * pow(sunCosTheta, 2.0);									 // Rayleigh phase function based on the Nielsen's paper.
				float  miePhase = _MieG.x / pow(_MieG.y - _MieG.z * sunCosTheta, 1.5); // The Henyey-Greenstein phase function.

				float3 BrTheta = _Pi316 * _Br * rayPhase * _RayleighColor.rgb * extinction;
				float3 BmTheta = _Pi14 * _Bm * miePhase * _MieColor.rgb * extinction * sunRise;
				float3 BrmTheta = (BrTheta + BmTheta) / (_Br + _Bm);

				float3 inScatter = BrmTheta * _Scattering * (1.0 - fex);
				inScatter *= sunRise;

				// Night sky
				BrTheta = _Pi316 * _Br * rayPhase * _RayleighColor.rgb;
				BrmTheta = (BrTheta) / (_Br + _Bm);
				float3 nightSky = BrmTheta * _NightIntensity * (1.0 - fex);

				float horizonExtinction = saturate((viewDir.y) * 1000.0) * fex.b;

				// Sun disk
				float3 sunTex = tex2D(_SunTexture, Input.SunPos + 0.5).rgb * _SunIntensity;
				sunTex = pow(sunTex, 2.0);
				sunTex *= fex.b * saturate(sunCosTheta);

				// Moon disk
				float moonFix = saturate(dot(Input.LocalPos, _MoonDirection)); // Delete other side moon.
				float4 moonTex = tex2D(_MoonTexture, Input.MoonPos.xy + 0.5) * moonFix;
				float moonMask = 1.0 - moonTex.a;
				float3 moonColor = (moonTex.rgb * _MoonDiskColor * moonTex.a) * horizonExtinction;

				// Moon bright
				float3 moonBright = 1.0 + dot(viewDir, -_MoonDirection);
				moonBright = 1.0 / (0.25 + moonBright * _MoonBrightRange) * _MoonBrightColor.rgb;
				moonBright *= moonRise;

				// Starfield
				float4 starTex = texCUBE(_StarfieldTexture, Input.StarfieldPos);
				float3 stars = starTex.rgb * starTex.a;
				float3 milkyWay = pow(starTex.rgb, 1.5) * _MilkyWayIntensity;
				float3 starfield = (stars + milkyWay) * _StarfieldColorBalance * moonMask * horizonExtinction * _StarfieldIntensity;

				// Output
				float3 OutputColor = inScatter + sunTex + nightSky + starfield + moonColor + moonBright;

				// Tonemapping
				OutputColor = 1.0 - exp(-_Exposure * OutputColor);

				// Color correction
				OutputColor = pow(OutputColor, 2.2);
				#ifdef UNITY_COLORSPACE_GAMMA
				OutputColor = pow(OutputColor, 0.4545);
				#else
				OutputColor = OutputColor;
				#endif

				return float4(OutputColor, 1.0);
			}

			ENDHLSL
		}
	}
}