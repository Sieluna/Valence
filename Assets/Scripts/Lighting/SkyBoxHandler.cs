using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class SkyBoxHandler : MonoBehaviour
{
    // Not included in the build
    #if UNITY_EDITOR
    public bool showTimeOfDayTab = true;
    public bool showReferencesTab = false;
    public bool showScatteringTab = false;
    public bool showNightSkyTab = false;
    public bool showCloudTab = false;
    public bool showFogTab = false;
    public bool showLightingTab = false;
    public bool showOptionsTab = false;
    public bool showOutputsTab = false;
    #endif

    // Time of day
    private float m_curveTime;
    private float m_gradientTime;
    private float m_timeProgression = 0.0f;
    private Vector3 m_sunLocalDirection;
    private Vector3 m_moonLocalDirection;
    public float timeline = 6.0f;
    public float latitude = 0.0f;
    public float longitude = 0.0f;
    public float utc = 0.0f;
    public float dayCycleInMinutes = 30.0f;
    public bool setTimeByCurve = false;
    public float timeByCurve = 6.0f;
    public AnimationCurve dayLengthCurve = AnimationCurve.Linear(0.0f, 0.0f, 24.0f, 24.0f);

    // References
    public Transform sunTransform;
    public Transform moonTransform;
    public Transform lightTransform;
    public Cubemap starfieldTexture;
    public Texture sunTexture;
    public Texture moonTexture;
    public Texture cloudTexture;
    public Material skyMaterial;

    // Scattering
    private Vector3 m_Br = new Vector3(0.00519673f, 0.0121427f, 0.0296453f);
    private Vector3 m_Bm = new Vector3(0.005721017f, 0.004451339f, 0.003146905f);
    private Vector3 m_MieG = new Vector3(0.4375f, 1.5625f, 1.5f);
    public float rayleigh = 1.0f;
    public float mie = 1.0f;
    private float m_Pi316 = 0.0596831f;
    private float m_Pi14 = 0.07957747f;
    public float kr = 8.4f;
    public float km = 1.25f;
    public float scattering = 15.0f;
    public float sunIntensity = 3.0f;
    public float nightIntensity = 0.5f;
    public float exposure = 2.0f;
    public Color rayleighColor = Color.white;
    public Color mieColor = Color.white;
    public Gradient rayleighGradientColor = new Gradient();
    public Gradient mieGradientColor = new Gradient();
    public AnimationCurve rayleighCurve = AnimationCurve.Linear(0.0f, 1.0f, 24.0f, 1.0f);
    public AnimationCurve mieCurve = AnimationCurve.Linear(0.0f, 1.0f, 24.0f, 1.0f);
    public AnimationCurve krCurve = AnimationCurve.Linear(0.0f, 8.4f, 24.0f, 8.4f);
    public AnimationCurve kmCurve = AnimationCurve.Linear(0.0f, 1.25f, 24.0f, 1.25f);
    public AnimationCurve scatteringCurve = AnimationCurve.Linear(0.0f, 15.0f, 24.0f, 15.0f);
    public AnimationCurve sunIntensityCurve = AnimationCurve.Linear(0.0f, 3.0f, 24.0f, 3.0f);
    public AnimationCurve nightIntensityCurve = AnimationCurve.Linear(0.0f, 0.5f, 24.0f, 0.5f);
    public AnimationCurve exposureCurve = AnimationCurve.Linear(0.0f, 2.0f, 24.0f, 2.0f);

    // Night sky
    public float starfieldIntensity = 0.0f;
    public float milkyWayIntensity = 0.0f;
    public Vector3 starfieldColorBalance = new Vector3(1.0f, 1.0f, 1.0f);
    public Vector3 starfieldPosition;
    public AnimationCurve starfieldIntensityCurve = AnimationCurve.Linear(0.0f, 0.0f, 24.0f, 0.0f);
    public AnimationCurve milkyWayIntensityCurve = AnimationCurve.Linear(0.0f, 0.0f, 24.0f, 0.0f);
    public float moonBrightRange = 0.9f;
    public Color moonDiskColor = Color.white;
    public Color moonBrightColor = Color.white;
    public Gradient moonDiskGradientColor = new Gradient();
    public Gradient moonBrightGradientColor = new Gradient();
    public AnimationCurve moonBrightRangeCurve = AnimationCurve.Linear(0.0f, 0.9f, 24.0f, 0.9f);

    // Clouds
    public Color cloudColor = Color.white;
    public Gradient cloudGradientColor = new Gradient();
    public float cloudScattering = 1.0f;
    public AnimationCurve cloudScatteringCurve = AnimationCurve.Linear(0.0f, 1.0f, 24.0f, 1.0f);
    public float cloudExtinction = 0.25f;
    public AnimationCurve cloudExtinctionCurve = AnimationCurve.Linear(0.0f, 0.25f, 24.0f, 0.25f);
    public float cloudPower = 2.2f;
    public AnimationCurve cloudPowerCurve = AnimationCurve.Linear(0.0f, 2.2f, 24.0f, 2.2f);
    public float cloudIntensity = 1.0f;
    public AnimationCurve cloudIntensityCurve = AnimationCurve.Linear(0.0f, 1.0f, 24.0f, 1.0f);
    public float cloudRotationSpeed = 0.0f;
    private float m_cloudRotationSpeed = 0.0f;

    // Fog scattering
    public float fogDistance = 3500.0f;
    public AnimationCurve fogDistanceCurve = AnimationCurve.Linear(0.0f, 3500.0f, 24.0f, 3500.0f);
    public float fogBlend = 0.5f;
    public float mieDistance = 0.5f;

    // Lighting
    private Light m_lightComponent;
    private LensFlareComponentSRP m_SunFlareComponent;
    private float m_sunElevation = 0.0f;
    public float lightIntensity = 0.0f;
    public AnimationCurve lightIntensityCurve = AnimationCurve.Linear(0.0f, 0.0f, 24.0f, 0.0f);
    public float flareIntensity = 0.0f;
    public AnimationCurve flareIntensityCurve = AnimationCurve.Linear(0.0f, 0.0f, 24.0f, 0.0f);
    public Color lightColor = Color.white;
    public Gradient lightGradientColor = new Gradient();
    public float ambientIntensity = 1.0f;
    public AnimationCurve ambientIntensityCurve = AnimationCurve.Linear(0.0f, 1.0f, 24.0f, 1.0f);
    public Color ambientSkyColor = Color.white;
    public Color equatorSkyColor = Color.white;
    public Color groundSkyColor = Color.white;
    public Gradient ambientSkyGradientColor = new Gradient();
    public Gradient equatorSkyGradientColor = new Gradient();
    public Gradient groundSkyGradientColor = new Gradient();

    // Options
    public Transform followTarget;
    public float sunDiskSize = 0.5f;
    public float moonDiskSize = 0.5f;
    
    public enum CloudMode { Off, Static };
    public CloudMode cloudMode = CloudMode.Static;

    // Outputs
    public List<AnimationCurve> outputCurveList = new List<AnimationCurve>();
    public List<Gradient> outputGradientList = new List<Gradient>();

    // Read only
    private bool m_isDaytime = false;
    public bool IsDaytime => m_isDaytime;

    // Shader uniforms
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

    // Use this for initialization.
    void Start()
    {
        // Get time progression
        m_timeProgression = GetTimeProgression();

        // Get components
        m_lightComponent = lightTransform.GetComponent<Light>();
        m_SunFlareComponent = lightTransform.GetComponent<LensFlareComponentSRP>();
        
        // First shaders and materials update
        UpdateSkySettings();
        UpdateProperties();
        UpdateShaderUniforms();
    }

    // Update is called once per frame
    void Update()
    {
        // Follow target
        if (followTarget)
            transform.position = followTarget.position;

        // Only in gameplay
        if (Application.isPlaying)
        {
            // Set time progression
            timeline += m_timeProgression * Time.deltaTime;

            // Restart the day
            if (timeline >= 24.0f)
                timeline = 0.0f;
        }

        // Update properties and shader uniforms
        UpdateShaderUniforms();
        UpdateProperties();

        // Apply the Sun and Moon transform directions
        sunTransform.transform.localRotation = SetSunPosition();
        moonTransform.transform.localRotation = Quaternion.LookRotation(-m_sunLocalDirection);
        
        // Get sun elevation and set light position
        m_sunElevation = Vector3.Dot(-sunTransform.transform.forward, transform.up);
        if (m_sunElevation >= 0.0f)
        {
            lightTransform.transform.localRotation = Quaternion.LookRotation(m_sunLocalDirection);
            m_isDaytime = true;
        }
        else
        {
            lightTransform.transform.localRotation = Quaternion.LookRotation(m_moonLocalDirection);
            m_isDaytime = false;
        }

        // Lighting
        m_lightComponent.intensity = lightIntensity;
        m_SunFlareComponent.intensity = flareIntensity;
        m_lightComponent.color = lightColor;
        RenderSettings.ambientIntensity = ambientIntensity;
        RenderSettings.ambientSkyColor = ambientSkyColor;
        RenderSettings.ambientEquatorColor = equatorSkyColor;
        RenderSettings.ambientGroundColor = groundSkyColor;
    }

    /// <summary>
    /// Used by sky controller to apply the time progression.
    /// </summary>
    /// <returns></returns>
    private float GetTimeProgression()
    {
        if (dayCycleInMinutes > 0.0f)
            return (24.0f / 60.0f) / dayCycleInMinutes;
        else
            return 0.0f;
    }

    /// <summary>
    /// Updates the shader uniforms that need to be set only once when the scene starts.
    /// </summary>
    private void InitializeShaderUniforms()
    {
        Shader.SetGlobalTexture(Uniforms._StarfieldTexture, starfieldTexture);
        Shader.SetGlobalTexture(Uniforms._SunTexture, sunTexture);
        Shader.SetGlobalTexture(Uniforms._MoonTexture, moonTexture);
        Shader.SetGlobalTexture(Uniforms._CloudTexture, cloudTexture);
    }

    /// <summary>
    /// Update script variables.
    /// </summary>
    private void UpdateProperties()
    {
        // Compute Curves and Gradients time to evaluate
        m_curveTime = timeline;
        m_gradientTime = timeline / 24.0f;
        if (setTimeByCurve)
        {
            timeByCurve = dayLengthCurve.Evaluate(timeline);
            m_curveTime = timeByCurve;
            m_gradientTime = timeByCurve / 24.0f;
        }

        // Scattering
        rayleigh = rayleighCurve.Evaluate(m_curveTime);
        mie = mieCurve.Evaluate(m_curveTime);
        kr = krCurve.Evaluate(m_curveTime);
        km = kmCurve.Evaluate(m_curveTime);
        scattering = scatteringCurve.Evaluate(m_curveTime);
        sunIntensity = sunIntensityCurve.Evaluate(m_curveTime);
        nightIntensity = nightIntensityCurve.Evaluate(m_curveTime);
        exposure = exposureCurve.Evaluate(m_curveTime);
        rayleighColor = rayleighGradientColor.Evaluate(m_gradientTime);
        mieColor = mieGradientColor.Evaluate(m_gradientTime);

        // Night sky
        moonDiskColor = moonDiskGradientColor.Evaluate(m_gradientTime);
        moonBrightColor = moonBrightGradientColor.Evaluate(m_gradientTime);
        moonBrightRange = moonBrightRangeCurve.Evaluate(m_curveTime);
        starfieldIntensity = starfieldIntensityCurve.Evaluate(m_curveTime);
        milkyWayIntensity = milkyWayIntensityCurve.Evaluate(m_curveTime);

        // Lighting
        lightIntensity = lightIntensityCurve.Evaluate(m_curveTime);
        flareIntensity = flareIntensityCurve.Evaluate(m_curveTime);
        lightColor = lightGradientColor.Evaluate(m_gradientTime);
        ambientIntensity = ambientIntensityCurve.Evaluate(m_curveTime);
        ambientSkyColor = ambientSkyGradientColor.Evaluate(m_gradientTime);
        equatorSkyColor = equatorSkyGradientColor.Evaluate(m_gradientTime);
        groundSkyColor = groundSkyGradientColor.Evaluate(m_gradientTime);

        // Clouds
        cloudColor = cloudGradientColor.Evaluate(m_gradientTime);
        cloudScattering = cloudScatteringCurve.Evaluate(m_curveTime);
        cloudExtinction = cloudExtinctionCurve.Evaluate(m_curveTime);
        cloudPower = cloudPowerCurve.Evaluate(m_curveTime);
        cloudIntensity = cloudIntensityCurve.Evaluate(m_curveTime);
        if (Application.isPlaying && cloudRotationSpeed != 0.0f)
        {
            m_cloudRotationSpeed += cloudRotationSpeed * Time.deltaTime;
            if (m_cloudRotationSpeed >= 1.0f)
            {
                m_cloudRotationSpeed -= 1.0f;
            }
        }

        // Fog
        fogDistance = fogDistanceCurve.Evaluate(m_curveTime);

        // Directions
        m_sunLocalDirection = transform.InverseTransformDirection(sunTransform.transform.forward);
        m_moonLocalDirection = transform.InverseTransformDirection(moonTransform.transform.forward);
    }

    /// <summary>
    /// Update shader uniforms every frame.
    /// </summary>
    private void UpdateShaderUniforms()
    {
        // References
        Shader.SetGlobalTexture(Uniforms._StarfieldTexture, starfieldTexture);
        Shader.SetGlobalTexture(Uniforms._SunTexture, sunTexture);
        Shader.SetGlobalTexture(Uniforms._MoonTexture, moonTexture);
        Shader.SetGlobalTexture(Uniforms._CloudTexture, cloudTexture);

        // Scattering
        Shader.SetGlobalVector(Uniforms._Br, m_Br * rayleigh);
        Shader.SetGlobalVector(Uniforms._Bm, m_Bm * mie);
        Shader.SetGlobalFloat(Uniforms._Kr, kr);
        Shader.SetGlobalFloat(Uniforms._Km, km);
        Shader.SetGlobalFloat(Uniforms._Scattering, scattering);
        Shader.SetGlobalFloat(Uniforms._SunIntensity, sunIntensity);
        Shader.SetGlobalFloat(Uniforms._NightIntensity, nightIntensity);
        Shader.SetGlobalFloat(Uniforms._Exposure, exposure);
        Shader.SetGlobalColor(Uniforms._RayleighColor, rayleighColor);
        Shader.SetGlobalColor(Uniforms._MieColor, mieColor);
        Shader.SetGlobalVector(Uniforms._MieG, m_MieG);
        Shader.SetGlobalFloat(Uniforms._Pi316, m_Pi316);
        Shader.SetGlobalFloat(Uniforms._Pi14, m_Pi14);
        Shader.SetGlobalFloat(Uniforms._Pi, Mathf.PI);

        // Night sky
        Shader.SetGlobalColor(Uniforms._MoonDiskColor, moonDiskColor);
        Shader.SetGlobalColor(Uniforms._MoonBrightColor, moonBrightColor);
        Shader.SetGlobalFloat(Uniforms._MoonBrightRange, Mathf.Lerp(150.0f, 5.0f, moonBrightRange));
        Shader.SetGlobalFloat(Uniforms._StarfieldIntensity, starfieldIntensity);
        Shader.SetGlobalFloat(Uniforms._MilkyWayIntensity, milkyWayIntensity);
        Shader.SetGlobalVector(Uniforms._StarfieldColorBalance, starfieldColorBalance);

        // Clouds
        Shader.SetGlobalColor(Uniforms._CloudColor, cloudColor);
        Shader.SetGlobalFloat(Uniforms._CloudScattering, cloudScattering);
        Shader.SetGlobalFloat(Uniforms._CloudExtinction, cloudExtinction);
        Shader.SetGlobalFloat(Uniforms._CloudPower, cloudPower);
        Shader.SetGlobalFloat(Uniforms._CloudIntensity, cloudIntensity);
        Shader.SetGlobalFloat(Uniforms._CloudRotationSpeed, m_cloudRotationSpeed);

        // Fog scattering
        Shader.SetGlobalFloat(Uniforms._FogDistance, fogDistance);
        Shader.SetGlobalFloat(Uniforms._FogBlend, fogBlend);
        Shader.SetGlobalFloat(Uniforms._MieDistance, mieDistance);

        // Options
        Shader.SetGlobalFloat(Uniforms._SunDiskSize, Mathf.Lerp(5.0f, 1.0f, sunDiskSize));
        Shader.SetGlobalFloat(Uniforms._MoonDiskSize, Mathf.Lerp(20.0f, 1.0f, moonDiskSize));

        // Directions
        Shader.SetGlobalVector(Uniforms._SunDirection, -m_sunLocalDirection);
        Shader.SetGlobalVector(Uniforms._MoonDirection, -m_moonLocalDirection);

        // Matrix
        Shader.SetGlobalMatrix(Uniforms._SkyUpDirectionMatrix, transform.worldToLocalMatrix);
        Shader.SetGlobalMatrix(Uniforms._SunMatrix, sunTransform.transform.worldToLocalMatrix);
        Shader.SetGlobalMatrix(Uniforms._MoonMatrix, moonTransform.transform.worldToLocalMatrix);
        Shader.SetGlobalMatrix(Uniforms._StarfieldMatrix, Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(starfieldPosition), Vector3.one).inverse);
    }
    
    private Quaternion SetSunPosition()
    {
        if (setTimeByCurve)
            return Quaternion.Euler(0.0f, longitude, latitude) * Quaternion.Euler(((timeByCurve + utc) * 360.0f / 24.0f) - 90.0f, 180.0f, 0.0f);
        else
            return Quaternion.Euler(0.0f, longitude, latitude) * Quaternion.Euler(((timeline + utc) * 360.0f / 24.0f) - 90.0f, 180.0f, 0.0f);
    }
    
    public void UpdateSkySettings()
    {
        RenderSettings.skybox = skyMaterial;
        skyMaterial.shader = Shader.Find(cloudMode == CloudMode.Off ? "Skybox/PixelSky" : "Skybox/PixelCloud");
    }
    
    public float GetCurveOutput(int index) => outputCurveList[index].Evaluate(m_curveTime);
    
    public Color GetGradientOutput(int index) => outputGradientList[index].Evaluate(m_gradientTime);
}