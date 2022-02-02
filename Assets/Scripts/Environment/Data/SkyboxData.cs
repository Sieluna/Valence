using UnityEngine;

namespace Environment.Data
{
    public class SkyboxData
    {
        public static class Uniforms
        {
            // Textures
            internal static readonly int _StarfieldTexture = Shader.PropertyToID("_StarfieldTexture");
            internal static readonly int _SunTexture = Shader.PropertyToID("_SunTexture");
            internal static readonly int _MoonTexture = Shader.PropertyToID("_MoonTexture");
            internal static readonly int _CloudTexture = Shader.PropertyToID("_CloudTexture");

            // Scattering
            internal static readonly int _Br = Shader.PropertyToID("_Br");
            internal static readonly int _Bm = Shader.PropertyToID("_Bm");
            internal static readonly int _Kr = Shader.PropertyToID("_Kr");
            internal static readonly int _Km = Shader.PropertyToID("_Km");
            internal static readonly int _Scattering = Shader.PropertyToID("_Scattering");
            internal static readonly int _SunIntensity = Shader.PropertyToID("_SunIntensity");
            internal static readonly int _NightIntensity = Shader.PropertyToID("_NightIntensity");
            internal static readonly int _Exposure = Shader.PropertyToID("_Exposure");
            internal static readonly int _RayleighColor = Shader.PropertyToID("_RayleighColor");
            internal static readonly int _MieColor = Shader.PropertyToID("_MieColor");
            internal static readonly int _MieG = Shader.PropertyToID("_MieG");
            internal static readonly int _Pi316 = Shader.PropertyToID("_Pi316");
            internal static readonly int _Pi14 = Shader.PropertyToID("_Pi14");
            internal static readonly int _Pi = Shader.PropertyToID("_Pi");

            // Night sky
            internal static readonly int _MoonDiskColor = Shader.PropertyToID("_MoonDiskColor");
            internal static readonly int _MoonBrightColor = Shader.PropertyToID("_MoonBrightColor");
            internal static readonly int _MoonBrightRange = Shader.PropertyToID("_MoonBrightRange");
            internal static readonly int _StarfieldIntensity = Shader.PropertyToID("_StarfieldIntensity");
            internal static readonly int _MilkyWayIntensity = Shader.PropertyToID("_MilkyWayIntensity");
            internal static readonly int _StarfieldColorBalance = Shader.PropertyToID("_StarfieldColorBalance");

            // Clouds
            internal static readonly int _CloudColor = Shader.PropertyToID("_CloudColor");
            internal static readonly int _CloudScattering = Shader.PropertyToID("_CloudScattering");
            internal static readonly int _CloudExtinction = Shader.PropertyToID("_CloudExtinction");
            internal static readonly int _CloudPower = Shader.PropertyToID("_CloudPower");
            internal static readonly int _CloudIntensity = Shader.PropertyToID("_CloudIntensity");
            internal static readonly int _CloudRotationSpeed = Shader.PropertyToID("_CloudRotationSpeed");

            // Fog scattering
            internal static readonly int _FogDistance = Shader.PropertyToID("_FogDistance");
            internal static readonly int _FogBlend = Shader.PropertyToID("_FogBlend");
            internal static readonly int _MieDistance = Shader.PropertyToID("_MieDistance");

            // Options
            internal static readonly int _SunDiskSize = Shader.PropertyToID("_SunDiskSize");
            internal static readonly int _MoonDiskSize = Shader.PropertyToID("_MoonDiskSize");

            // Directions
            internal static readonly int _SunDirection = Shader.PropertyToID("_SunDirection");
            internal static readonly int _MoonDirection = Shader.PropertyToID("_MoonDirection");

            // Matrix
            internal static readonly int _SkyUpDirectionMatrix = Shader.PropertyToID("_SkyUpDirectionMatrix");
            internal static readonly int _SunMatrix = Shader.PropertyToID("_SunMatrix");
            internal static readonly int _MoonMatrix = Shader.PropertyToID("_MoonMatrix");
            internal static readonly int _StarfieldMatrix = Shader.PropertyToID("_StarfieldMatrix");
        }

        private SkyboxData(SkyboxPrefab prefab)
        {
            
        }
    }
}