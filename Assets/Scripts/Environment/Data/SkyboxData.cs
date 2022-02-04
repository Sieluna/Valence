using UnityEngine;
using UnityEngine.Rendering;

namespace Environment.Data
{
    public class SkyboxData
    {
        private float m_curveTime;
        private float m_gradientTime;
        private float m_timeProgression = 0.0f;
        private Vector3 m_sunLocalDirection;
        private Vector3 m_moonLocalDirection;
        public float timeByCurve = 6.0f;
        public float timeline = 6.0f;
       
        private readonly Transform m_SunTransform;
        private readonly Transform m_MoonTransform;
        private readonly Transform m_LightTransform;
        
        private Vector3 m_Br = new Vector3(0.00519673f, 0.0121427f, 0.0296453f);
        private Vector3 m_Bm = new Vector3(0.005721017f, 0.004451339f, 0.003146905f);
        private Vector3 m_MieG = new Vector3(0.4375f, 1.5625f, 1.5f);
        private float m_Pi316 = 0.0596831f;
        private float m_Pi14 = 0.07957747f;
        
        private float m_cloudRotationSpeed = 0f;
        
        private Light m_lightComponent;
        private LensFlareComponentSRP m_SunFlareComponent;
        private float m_sunElevation = 0.0f;
        
        private bool m_isDaytime = false;
        public bool IsDaytime => m_isDaytime;
        
        public SkyboxData(SkyboxPrefab prefab, Transform transform, Transform sunTransform, Transform moonTransform, Transform lightTransform)
        {
            this.m_SunTransform = sunTransform;
            this.m_MoonTransform = moonTransform;
            this.m_LightTransform = lightTransform;
            
            // Get components
            m_lightComponent = lightTransform.GetComponent<Light>();
            m_SunFlareComponent = lightTransform.GetComponent<LensFlareComponentSRP>();
           
            prefab.UpdateSkySettings();
            Refresh(prefab, transform);
        }

        public void Refresh(SkyboxPrefab prefab, Transform transform)
        {
            if (Application.isPlaying)
            {
                timeline += m_timeProgression * Time.deltaTime;
                if (timeline >= 24f) timeline = 0f;
            }

            if (prefab.setTimeByCurve)
            {
                timeByCurve = prefab.dayLengthCurve.Evaluate(timeline);
                m_curveTime = timeByCurve;
                m_gradientTime = timeByCurve / 24f;
            }
            else
            {
                m_curveTime = timeline;
                m_gradientTime = timeline / 24.0f;
            }
            
            // References
            Shader.SetGlobalTexture(ShaderIDs.StarfieldTexture, prefab.starfieldTexture);
            Shader.SetGlobalTexture(ShaderIDs.SunTexture, prefab.sunTexture);
            Shader.SetGlobalTexture(ShaderIDs.MoonTexture, prefab.moonTexture);
            Shader.SetGlobalTexture(ShaderIDs.CloudTexture, prefab.cloudTexture);
            
            // Scattering
            Shader.SetGlobalVector(ShaderIDs.Br, m_Br * prefab.rayleighCurve.Evaluate(m_curveTime));
            Shader.SetGlobalVector(ShaderIDs.Bm, m_Bm * prefab.mieCurve.Evaluate(m_curveTime));
            Shader.SetGlobalFloat(ShaderIDs.Kr, prefab.krCurve.Evaluate(m_curveTime));
            Shader.SetGlobalFloat(ShaderIDs.Km, prefab.kmCurve.Evaluate(m_curveTime));
            Shader.SetGlobalFloat(ShaderIDs.Scattering, prefab.scatteringCurve.Evaluate(m_curveTime));
            Shader.SetGlobalFloat(ShaderIDs.SunIntensity, prefab.sunIntensityCurve.Evaluate(m_curveTime));
            Shader.SetGlobalFloat(ShaderIDs.NightIntensity, prefab.nightIntensityCurve.Evaluate(m_curveTime));
            Shader.SetGlobalFloat(ShaderIDs.Exposure, prefab.exposureCurve.Evaluate(m_curveTime));
            Shader.SetGlobalColor(ShaderIDs.RayleighColor, prefab.rayleighGradientColor.Evaluate(m_gradientTime));
            Shader.SetGlobalColor(ShaderIDs.MieColor, prefab.mieGradientColor.Evaluate(m_gradientTime));
            Shader.SetGlobalVector(ShaderIDs.MieG, m_MieG);
            Shader.SetGlobalFloat(ShaderIDs.Pi316, m_Pi316);
            Shader.SetGlobalFloat(ShaderIDs.Pi14, m_Pi14);
            Shader.SetGlobalFloat(ShaderIDs.Pi, Mathf.PI);
            
            // Night sky
            Shader.SetGlobalColor(ShaderIDs.MoonDiskColor, prefab.moonDiskGradientColor.Evaluate(m_gradientTime));
            Shader.SetGlobalColor(ShaderIDs.MoonBrightColor, prefab.moonBrightGradientColor.Evaluate(m_gradientTime));
            Shader.SetGlobalFloat(ShaderIDs.MoonBrightRange, Mathf.Lerp(150.0f, 5.0f, prefab.moonBrightRangeCurve.Evaluate(m_curveTime)));
            Shader.SetGlobalFloat(ShaderIDs.StarfieldIntensity, prefab.starfieldIntensityCurve.Evaluate(m_curveTime));
            Shader.SetGlobalFloat(ShaderIDs.MilkyWayIntensity, prefab.milkyWayIntensityCurve.Evaluate(m_curveTime));
            Shader.SetGlobalVector(ShaderIDs.StarfieldColorBalance, prefab.starfieldColorBalance);
            
            // Clouds
            Shader.SetGlobalColor(ShaderIDs.CloudColor, prefab.cloudGradientColor.Evaluate(m_gradientTime));
            Shader.SetGlobalFloat(ShaderIDs.CloudScattering, prefab.cloudScatteringCurve.Evaluate(m_curveTime));
            Shader.SetGlobalFloat(ShaderIDs.CloudExtinction, prefab.cloudExtinctionCurve.Evaluate(m_curveTime));
            Shader.SetGlobalFloat(ShaderIDs.CloudPower, prefab.cloudPowerCurve.Evaluate(m_curveTime));
            Shader.SetGlobalFloat(ShaderIDs.CloudIntensity, prefab.cloudIntensityCurve.Evaluate(m_curveTime));
            if (Application.isPlaying && prefab.cloudRotationSpeed != 0.0f)
            {
                m_cloudRotationSpeed += prefab.cloudRotationSpeed * Time.deltaTime;
                if (m_cloudRotationSpeed >= 1.0f)
                    m_cloudRotationSpeed -= 1.0f;
            }
            Shader.SetGlobalFloat(ShaderIDs.CloudRotationSpeed, m_cloudRotationSpeed);
            
            // Fog scattering
            Shader.SetGlobalFloat(ShaderIDs.FogDistance, prefab.fogDistanceCurve.Evaluate(m_curveTime));
            Shader.SetGlobalFloat(ShaderIDs.FogBlend, prefab.fogBlend);
            Shader.SetGlobalFloat(ShaderIDs.MieDistance, prefab.mieDistance);

            // Options
            Shader.SetGlobalFloat(ShaderIDs.SunDiskSize, Mathf.Lerp(5.0f, 1.0f, prefab.sunDiskSize));
            Shader.SetGlobalFloat(ShaderIDs.MoonDiskSize, Mathf.Lerp(20.0f, 1.0f, prefab.moonDiskSize));

            // Directions
            m_sunLocalDirection = transform.InverseTransformDirection(m_SunTransform.transform.forward);
            m_moonLocalDirection = transform.InverseTransformDirection(m_MoonTransform.transform.forward);
            Shader.SetGlobalVector(ShaderIDs.SunDirection, -m_sunLocalDirection);
            Shader.SetGlobalVector(ShaderIDs.MoonDirection, -m_moonLocalDirection);

            // Matrix
            Shader.SetGlobalMatrix(ShaderIDs.SkyUpDirectionMatrix, transform.worldToLocalMatrix);
            Shader.SetGlobalMatrix(ShaderIDs.SunMatrix, m_SunTransform.transform.worldToLocalMatrix);
            Shader.SetGlobalMatrix(ShaderIDs.MoonMatrix, m_MoonTransform.transform.worldToLocalMatrix);
            Shader.SetGlobalMatrix(ShaderIDs.StarfieldMatrix, Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(prefab.starfieldPosition), Vector3.one).inverse);

            m_SunTransform.transform.localRotation = prefab.setTimeByCurve ? Quaternion.Euler((timeByCurve * 360.0f / 24.0f) - 90.0f, 180.0f, 0.0f) :
                                                                             Quaternion.Euler((timeline * 360.0f / 24.0f) - 90.0f, 180.0f, 0.0f);
            m_MoonTransform.transform.localRotation = Quaternion.LookRotation(-m_sunLocalDirection);
            
            m_sunElevation = Vector3.Dot(-m_SunTransform.transform.forward, transform.up);
            if (m_sunElevation >= 0.0f)
            {
                m_LightTransform.transform.localRotation = Quaternion.LookRotation(m_sunLocalDirection);
                m_isDaytime = true;
            }
            else
            {
                m_LightTransform.transform.localRotation = Quaternion.LookRotation(m_moonLocalDirection);
                m_isDaytime = false;
            }
            
            // Lighting
            m_lightComponent.intensity = prefab.lightIntensityCurve.Evaluate(m_curveTime);
            m_SunFlareComponent.intensity = prefab.flareIntensityCurve.Evaluate(m_curveTime);
            m_lightComponent.color = prefab.lightGradientColor.Evaluate(m_gradientTime);
            RenderSettings.ambientIntensity = prefab.ambientIntensityCurve.Evaluate(m_curveTime);
            RenderSettings.ambientSkyColor = prefab.ambientSkyGradientColor.Evaluate(m_gradientTime);
            RenderSettings.ambientEquatorColor = prefab.equatorSkyGradientColor.Evaluate(m_gradientTime);
            RenderSettings.ambientGroundColor = prefab.groundSkyGradientColor.Evaluate(m_gradientTime);
        }
    }
}