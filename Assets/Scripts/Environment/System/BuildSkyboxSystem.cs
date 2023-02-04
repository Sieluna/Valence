using Environment.Data;
using Environment.Interface;
using UnityEngine;
using UnityEngine.Rendering;

namespace Environment.System
{
    public class BuildSkyboxSystem : ISharedSystem
    {
        private readonly SkyboxPrefab m_data;
        private readonly TimePrefab m_time;

        public BuildSkyboxSystem()
        {
            m_data = Resources.Load<SkyboxPrefab>("Skybox");
            m_time = Resources.Load<TimePrefab>("Time");
        }

        private readonly Vector3 m_br = new(0.00519673f, 0.0121427f, 0.0296453f);
        private readonly Vector3 m_bm = new(0.005721017f, 0.004451339f, 0.003146905f);
        private readonly Vector3 m_mieG = new(0.4375f, 1.5625f, 1.5f);

        private float m_curveTime, m_gradientTime;

        private Transform m_holderTransform, m_sunTransform, m_moonTransform, m_lightTransform;

        private float m_cloudRotationSpeed;

        private Light m_lightComponent;
        private LensFlareComponentSRP m_sunFlareComponent;

        private ReflectionProbe m_environmentProbe;

        private Cubemap m_environmentReflection;

        private int m_probeRenderId = -1;

        private bool m_enableReflection;
        
        private ReflectionProbe EnvironmentProbe
        {
            get
            {
                if (m_environmentProbe == null)
                {
                    var probeHolder = new GameObject("~EnvironmentReflectionProbe")
                    {
                        transform =
                        {
                            parent = m_holderTransform,
                            position = new Vector3(0, -1000, 0), rotation = Quaternion.identity, localScale = Vector3.one
                        }
                    };

                    m_environmentProbe = probeHolder.AddComponent<ReflectionProbe>();
                    m_environmentProbe.resolution = m_data.environmentReflectionResolution;
                    m_environmentProbe.size = new Vector3(1, 1, 1);
                    m_environmentProbe.cullingMask = 0;
                    Debug.Log("[Skybox] Reflection Probe Resolution -> " + m_data.environmentReflectionResolution);
                }

                m_environmentProbe.clearFlags = ReflectionProbeClearFlags.Skybox;
                m_environmentProbe.mode = ReflectionProbeMode.Realtime;
                m_environmentProbe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
                m_environmentProbe.timeSlicingMode = m_data.environmentReflectionTimeSlicingMode;
                m_environmentProbe.hdr = false;
                return m_environmentProbe;
            }
        }
        
        private Cubemap EnvironmentReflection => m_environmentReflection ??= new Cubemap(EnvironmentProbe.resolution, TextureFormat.RGBA32, true);
        
        public void Init()
        {
            m_enableReflection = m_data.enableReflection;
            
            m_holderTransform = GameObject.Find("World").transform;
            m_sunTransform = GameObject.Find("Sun").transform;
            m_moonTransform = GameObject.Find("Moon").transform;
            m_lightTransform = GameObject.Find("Light").transform;

            m_lightComponent = m_lightTransform.GetComponent<Light>();
            m_sunFlareComponent = m_lightTransform.GetComponent<LensFlareComponentSRP>();

            m_data.UpdateSkySettings();

            Refresh();
            
            if (m_enableReflection)
            {
                if ((SystemInfo.copyTextureSupport & CopyTextureSupport.RTToTexture) == 0)
                {
                    m_enableReflection = false;
                    Debug.Log("[Skybox] Reflection Disable -> Copy RT Not Allowed");
                }
                else
                {
                    UpdateEnvironmentReflection();
                    Debug.Log("[Skybox] Reflection Enable");
                }
            }
            else
            {
                RenderSettings.defaultReflectionMode = DefaultReflectionMode.Skybox;
                Debug.Log("[Skybox] Use Default Reflection Mode");
            }
        }

        public void Refresh()
        {
            m_curveTime = m_data.setTimeByCurve ? m_data.dayLengthCurve.Evaluate(m_time.time) : m_time.time;
            m_gradientTime = m_curveTime / 24f;

            // Scattering
            Shader.SetGlobalVector(ShaderIDs.Br, m_br * m_data.rayleighCurve.Evaluate(m_curveTime));
            Shader.SetGlobalVector(ShaderIDs.Bm, m_bm * m_data.mieCurve.Evaluate(m_curveTime));
            Shader.SetGlobalFloat(ShaderIDs.Kr, m_data.krCurve.Evaluate(m_curveTime));
            Shader.SetGlobalFloat(ShaderIDs.Km, m_data.kmCurve.Evaluate(m_curveTime));
            Shader.SetGlobalFloat(ShaderIDs.Scattering, m_data.scatteringCurve.Evaluate(m_curveTime));
            Shader.SetGlobalFloat(ShaderIDs.SunIntensity, m_data.sunIntensityCurve.Evaluate(m_curveTime));
            Shader.SetGlobalFloat(ShaderIDs.NightIntensity, m_data.nightIntensityCurve.Evaluate(m_curveTime));
            Shader.SetGlobalFloat(ShaderIDs.Exposure, m_data.exposureCurve.Evaluate(m_curveTime));
            Shader.SetGlobalColor(ShaderIDs.RayleighColor, m_data.rayleighGradientColor.Evaluate(m_gradientTime));
            Shader.SetGlobalColor(ShaderIDs.MieColor, m_data.mieGradientColor.Evaluate(m_gradientTime));
            Shader.SetGlobalVector(ShaderIDs.MieG, m_mieG);

            // Night sky
            Shader.SetGlobalColor(ShaderIDs.MoonDiskColor, m_data.moonDiskGradientColor.Evaluate(m_gradientTime));
            Shader.SetGlobalColor(ShaderIDs.MoonBrightColor, m_data.moonBrightGradientColor.Evaluate(m_gradientTime));
            Shader.SetGlobalFloat(ShaderIDs.MoonBrightRange, Mathf.Lerp(150.0f, 5.0f, m_data.moonBrightRangeCurve.Evaluate(m_curveTime)));
            Shader.SetGlobalFloat(ShaderIDs.StarfieldIntensity, m_data.starfieldIntensityCurve.Evaluate(m_curveTime));
            Shader.SetGlobalFloat(ShaderIDs.MilkyWayIntensity, m_data.milkyWayIntensityCurve.Evaluate(m_curveTime));
            Shader.SetGlobalVector(ShaderIDs.StarfieldColorBalance, m_data.starfieldColorBalance);

            // Clouds
            Shader.SetGlobalColor(ShaderIDs.CloudColor, m_data.cloudGradientColor.Evaluate(m_gradientTime));
            Shader.SetGlobalFloat(ShaderIDs.CloudScattering, m_data.cloudScatteringCurve.Evaluate(m_curveTime));
            Shader.SetGlobalFloat(ShaderIDs.CloudExtinction, m_data.cloudExtinctionCurve.Evaluate(m_curveTime));
            Shader.SetGlobalFloat(ShaderIDs.CloudPower, m_data.cloudPowerCurve.Evaluate(m_curveTime));
            Shader.SetGlobalFloat(ShaderIDs.CloudIntensity, m_data.cloudIntensityCurve.Evaluate(m_curveTime));

            // Fog scattering
            Shader.SetGlobalFloat(ShaderIDs.FogDistance, m_data.fogDistanceCurve.Evaluate(m_curveTime));
            Shader.SetGlobalFloat(ShaderIDs.FogBlend, m_data.fogBlend);
            Shader.SetGlobalFloat(ShaderIDs.MieDistance, m_data.mieDistance);

            // Options
            Shader.SetGlobalFloat(ShaderIDs.SunDiskSize, Mathf.Lerp(5.0f, 1.0f, m_data.sunDiskSize));
            Shader.SetGlobalFloat(ShaderIDs.MoonDiskSize, Mathf.Lerp(20.0f, 1.0f, m_data.moonDiskSize));

            AnimateSky();
            
            if (m_enableReflection)
                UpdateEnvironmentReflection();
        }

        private void AnimateSky()
        {
            if (m_data.cloudRotationSpeed != 0f)
            {
                m_cloudRotationSpeed += m_data.cloudRotationSpeed * Time.deltaTime;
                if (m_cloudRotationSpeed >= 1f)
                    m_cloudRotationSpeed = 0f;
            }

            Shader.SetGlobalFloat(ShaderIDs.CloudRotationSpeed, m_cloudRotationSpeed);
            
            // Directions
            var sunLocalDirection = m_holderTransform.InverseTransformDirection(m_sunTransform.transform.forward);
            var moonLocalDirection = m_holderTransform.InverseTransformDirection(m_moonTransform.transform.forward);
            Shader.SetGlobalVector(ShaderIDs.SunDirection, -sunLocalDirection);
            Shader.SetGlobalVector(ShaderIDs.MoonDirection, -moonLocalDirection);

            // Matrix
            Shader.SetGlobalMatrix(ShaderIDs.SkyUpDirectionMatrix, m_holderTransform.worldToLocalMatrix);
            Shader.SetGlobalMatrix(ShaderIDs.SunMatrix, m_sunTransform.transform.worldToLocalMatrix);
            Shader.SetGlobalMatrix(ShaderIDs.MoonMatrix, m_moonTransform.transform.worldToLocalMatrix);
            Shader.SetGlobalMatrix(ShaderIDs.StarfieldMatrix, Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(m_data.starfieldPosition), Vector3.one).inverse);

            m_sunTransform.transform.localRotation = Quaternion.Euler((m_curveTime * 360.0f / 24.0f) - 90.0f, 180.0f, 0.0f);
            m_moonTransform.transform.localRotation = Quaternion.LookRotation(-sunLocalDirection);

            if (Vector3.Dot(-m_sunTransform.transform.forward, m_holderTransform.up) >= 0.0f)
            {
                m_lightTransform.transform.localRotation = Quaternion.LookRotation(sunLocalDirection);
                m_data.onSunRise.Invoke();
            }
            else
            {
                m_lightTransform.transform.localRotation = Quaternion.LookRotation(moonLocalDirection);
                m_data.onSunSet.Invoke();
            }

            // Lighting
            m_lightComponent.intensity = m_data.lightIntensityCurve.Evaluate(m_curveTime);
            m_sunFlareComponent.intensity = m_data.flareIntensityCurve.Evaluate(m_curveTime);
            m_lightComponent.color = m_data.lightGradientColor.Evaluate(m_gradientTime);

            RenderSettings.ambientIntensity = m_data.ambientIntensityCurve.Evaluate(m_curveTime);
        }
        
        private void UpdateEnvironmentReflection()
        {
            if (EnvironmentProbe.texture == null && m_probeRenderId == -1)
            {
                Debug.Log("[Skybox] Init RT");
                m_probeRenderId = EnvironmentProbe.RenderProbe();
            }
            else if (EnvironmentProbe.texture != null || EnvironmentProbe.IsFinishedRendering(m_probeRenderId))
            {
                if (Time.frameCount % m_data.updateRate == 0)
                {
                    Graphics.CopyTexture(EnvironmentProbe.texture, EnvironmentReflection);
                    RenderSettings.customReflectionTexture = EnvironmentReflection;
                    RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
                    m_probeRenderId = EnvironmentProbe.RenderProbe();
                    Debug.Log("[Skybox] RT Update");   
                }
            }
        }
    }
}