using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

[CustomEditor(typeof(SkyBoxHandler))]
public class SkyBoxHandlerEditor : Editor
{
	// Editor only
	private SkyBoxHandler m_target;
	private Rect m_controlRect;

	private Color col1 = new Color(1,1,1,1);//Normal.
	private Color col2 = new Color(0,0,0,0);//All Transparent.
	private Color col3 = new Color(0.35f,0.65f,1,1);//Blue.
	private Color col4 = new Color(0.15f,0.5f,1,0.35f);//Blue semi transparent.
	private Color col5 = new Color(0.75f, 1.0f, 0.75f, 1.0f);//Green;
	private Color col6 = new Color(1.0f, 0.5f, 0.5f, 1.0f);//Red;
	private Color m_curveColor = Color.yellow;
	private Vector3 m_starFieldColor = Vector3.one;
	private Vector3 m_starFieldPosition = Vector3.zero;

	// GUIContents
	private readonly GUIContent[] m_guiContent =
	{
		new GUIContent("Timeline", "The current 'time position' in the day-night cycle."),
		new GUIContent("Latitude", "The north-south angle of a position on the Earth's surface."),
		new GUIContent("Longitude", "The east-west angle of a position on the Earth's surface."),
		new GUIContent("Utc", "Universal Time Coordinated."),
		new GUIContent("Day Cycle in Minutes", "Duration of the day-night cycle in minutes."),
		new GUIContent("Evaluate Time of Day by Curve?", "Will the 'time of day' be evaluated based on the timeline or based on the day-night length curve?"),
		new GUIContent("Current Time by Curve:", "Displays the timeline value if evaluated by the curve."),
		new GUIContent("Rayleigh", "The rayleigh multiplier coefficient."),
		new GUIContent("Mie", "The mie multiplier coefficient."),
		new GUIContent("Kr", "Rayleigh height."),
		new GUIContent("Km", "Mie height."),
		new GUIContent("Scattering", "The light scattering multiplier coefficient."),
		new GUIContent("Sun Intensity", "The intensity of the sun texture."),
		new GUIContent("Night Intensity", "The intensity of the night sky."),
		new GUIContent("Exposure", "The color exposure of the internal tonemapping effect."),
		new GUIContent("Rayleigh Color", "Rayleigh color multiplier."),
		new GUIContent("Mie Color", "Mie color multiplier."),
		new GUIContent("Moon Disk Color", "The color of the moon texture."),
		new GUIContent("Moon Bright Color", "The color of the bright coming from the moon."),
		new GUIContent("Moon Bright Range", "The range of the moon bright in the sky."),
		new GUIContent("Starfield Intensity", "The intensity of the regular stars."),
		new GUIContent("Milky Way Intensity", "The intensity of the Milky Way."),
		new GUIContent("Color", "The color of the clouds."),
		new GUIContent("Scattering", ""),
		new GUIContent("Extinction", ""),
		new GUIContent("Power", ""),
		new GUIContent("Intensity", ""),
		new GUIContent("Rotation Speed", ""),
		new GUIContent("Distance", "The distance of the global fog in real world scale(meters)."),
		new GUIContent("Blend", "Smooths the fog transition between the distance where the global fog starts and where it is completely covered by the global fog"),
		new GUIContent("Mie Distance", "Sets the Mie depth of the fog scattering."),
		new GUIContent("Directional Light Intensity", "The intensity of the directional light."),
		new GUIContent("Directional Light Flare Intensity", "The intensity of the directional light flare."),
		new GUIContent("Directional Light Color", "The color of the directional light."),
		new GUIContent("Environment Intensity", "The intensity multiplier of the environment lighting."),
		new GUIContent("Environment Ambient Color", "The ambient color of the environment lighting."),
		new GUIContent("Environment Equator Color", "The equator color of the environment lighting."),
		new GUIContent("Environment Ground Color", "The ground color of the environment lighting."),
		new GUIContent("Follow Target", "If attached, the sky prefab will follow the target position."),
		new GUIContent("Sun Disk Size", "The size of the sun texture."),
		new GUIContent("Moon Disk Size", "The size of the moon texture."),
		new GUIContent("Cloud Mode", ""),
	};

	// Header groups
	SerializedProperty m_showTimeOfDayTab;
	SerializedProperty m_showReferencesTab;
	SerializedProperty m_showScatteringTab;
	SerializedProperty m_showNightSkyTab;
	SerializedProperty m_showCloudTab;
	SerializedProperty m_showFogTab;
	SerializedProperty m_showLightingTab;
	SerializedProperty m_showOptionsTab;
	SerializedProperty m_showOutputsTab;

	// Serialized properties
	SerializedProperty m_timeline;
	SerializedProperty m_latitude;
	SerializedProperty m_longitude;
	SerializedProperty m_utc;
	SerializedProperty m_dayCycleInMinutes;
	SerializedProperty m_setTimeByCurve;
	SerializedProperty m_dayLengthCurve;
	SerializedProperty m_sunTransform;
	SerializedProperty m_moonTransform;
	SerializedProperty m_lightTransform;
	SerializedProperty m_starfieldTexture;
	SerializedProperty m_sunTexture;
	SerializedProperty m_moonTexture;
	SerializedProperty m_cloudTexture;
	SerializedProperty m_skyMaterial;
	SerializedProperty m_rayleighCurve;
	SerializedProperty m_mieCurve;
	SerializedProperty m_krCurve;
	SerializedProperty m_kmCurve;
	SerializedProperty m_scatteringCurve;
	SerializedProperty m_sunIntensityCurve;
	SerializedProperty m_nightIntensityCurve;
	SerializedProperty m_exposureCurve;
	SerializedProperty m_rayleighGradientColor;
	SerializedProperty m_mieGradientColor;
	SerializedProperty m_moonDiskGradientColor;
	SerializedProperty m_moonBrightGradientColor;
	SerializedProperty m_moonBrightRangeCurve;
	SerializedProperty m_starfieldIntensityCurve;
	SerializedProperty m_milkyWayIntensityCurve;
	SerializedProperty m_cloudGradientColor;
	SerializedProperty m_cloudScatteringCurve;
	SerializedProperty m_cloudExtinctionCurve;
	SerializedProperty m_cloudPowerCurve;
	SerializedProperty m_cloudIntensityCurve;
	SerializedProperty m_cloudRotationSpeed;
	SerializedProperty m_fogDistanceCurve;
	SerializedProperty m_fogBlend;
	SerializedProperty m_mieDistance;
	SerializedProperty m_directionalLightIntensityCurve;
	SerializedProperty m_directionalLightFlareIntensityCurve;
	SerializedProperty m_directionalLightGradientColor;
	SerializedProperty m_environmentIntensityCurve;
	SerializedProperty m_ambientGradientColor;
	SerializedProperty m_equatorGradientColor;
	SerializedProperty m_groundGradientColor;
	SerializedProperty m_followTarget;
	SerializedProperty m_sunDiskSize;
	SerializedProperty m_moonDiskSize;
	SerializedProperty m_cloudMode;

	SerializedProperty MieSunColor;
	SerializedProperty RayleighSunColor;
	SerializedProperty MoonDiskColor;
	SerializedProperty MoonBrightColor;
	SerializedProperty LightColor;
	SerializedProperty AmbientSkyColor;
	SerializedProperty EquatorSkyColor;
	SerializedProperty GroundSkyColor;
	SerializedProperty CloudColor;

	// Outputs
	private ReorderableList    reorderableCurveList;
	private ReorderableList    reorderableGradientList;
	private SerializedProperty serializedCurve;
	private SerializedProperty serializedGradient;

	void OnEnable()
	{
		// Get target
		m_target = target as SkyBoxHandler;

		if (m_target is null) return;
		
		// Find the serialized properties
		m_showTimeOfDayTab = serializedObject.FindProperty("showTimeOfDayTab");
		m_showReferencesTab = serializedObject.FindProperty("showReferencesTab");
		m_showScatteringTab = serializedObject.FindProperty("showScatteringTab");
		m_showNightSkyTab = serializedObject.FindProperty("showNightSkyTab");
		m_showCloudTab = serializedObject.FindProperty("showCloudTab");
		m_showFogTab = serializedObject.FindProperty("showFogTab");
		m_showLightingTab = serializedObject.FindProperty("showLightingTab");
		m_showOptionsTab = serializedObject.FindProperty("showOptionsTab");
		m_showOutputsTab = serializedObject.FindProperty("showOutputsTab");
		m_timeline = serializedObject.FindProperty("timeline");
		m_latitude = serializedObject.FindProperty("latitude");
		m_longitude = serializedObject.FindProperty("longitude");
		m_utc = serializedObject.FindProperty("utc");
		m_dayCycleInMinutes = serializedObject.FindProperty("dayCycleInMinutes");
		m_setTimeByCurve = serializedObject.FindProperty("setTimeByCurve");
		m_dayLengthCurve = serializedObject.FindProperty("dayLengthCurve");
		m_sunTransform = serializedObject.FindProperty("sunTransform");
		m_moonTransform = serializedObject.FindProperty("moonTransform");
		m_lightTransform = serializedObject.FindProperty("lightTransform");
		m_starfieldTexture = serializedObject.FindProperty("starfieldTexture");
		m_sunTexture = serializedObject.FindProperty("sunTexture");
		m_moonTexture = serializedObject.FindProperty("moonTexture");
		m_cloudTexture = serializedObject.FindProperty("cloudTexture");
		m_skyMaterial = serializedObject.FindProperty("skyMaterial");
		m_rayleighCurve = serializedObject.FindProperty("rayleighCurve");
		m_mieCurve = serializedObject.FindProperty("mieCurve");
		m_krCurve = serializedObject.FindProperty("krCurve");
		m_kmCurve = serializedObject.FindProperty("kmCurve");
		m_scatteringCurve = serializedObject.FindProperty("scatteringCurve");
		m_sunIntensityCurve = serializedObject.FindProperty("sunIntensityCurve");
		m_nightIntensityCurve = serializedObject.FindProperty("nightIntensityCurve");
		m_exposureCurve = serializedObject.FindProperty("exposureCurve");
		m_rayleighGradientColor = serializedObject.FindProperty("rayleighGradientColor");
		m_mieGradientColor = serializedObject.FindProperty("mieGradientColor");
		m_moonDiskGradientColor = serializedObject.FindProperty("moonDiskGradientColor");
		m_moonBrightGradientColor = serializedObject.FindProperty("moonBrightGradientColor");
		m_moonBrightRangeCurve = serializedObject.FindProperty("moonBrightRangeCurve");
		m_starfieldIntensityCurve = serializedObject.FindProperty("starfieldIntensityCurve");
		m_milkyWayIntensityCurve = serializedObject.FindProperty("milkyWayIntensityCurve");
		m_cloudGradientColor = serializedObject.FindProperty("cloudGradientColor");
		m_cloudScatteringCurve = serializedObject.FindProperty("cloudScatteringCurve");
		m_cloudExtinctionCurve = serializedObject.FindProperty("cloudExtinctionCurve");
		m_cloudPowerCurve = serializedObject.FindProperty("cloudPowerCurve");
		m_cloudIntensityCurve = serializedObject.FindProperty("cloudIntensityCurve");
		m_cloudRotationSpeed = serializedObject.FindProperty("cloudRotationSpeed");
		m_fogDistanceCurve = serializedObject.FindProperty("fogDistanceCurve");
		m_fogBlend = serializedObject.FindProperty("fogBlend");
		m_mieDistance = serializedObject.FindProperty("mieDistance");
		m_directionalLightIntensityCurve = serializedObject.FindProperty("lightIntensityCurve");
		m_directionalLightFlareIntensityCurve = serializedObject.FindProperty("flareIntensityCurve");
		m_directionalLightGradientColor = serializedObject.FindProperty("lightGradientColor");
		m_environmentIntensityCurve = serializedObject.FindProperty("ambientIntensityCurve");
		m_ambientGradientColor = serializedObject.FindProperty("ambientSkyGradientColor");
		m_equatorGradientColor = serializedObject.FindProperty("equatorSkyGradientColor");
		m_groundGradientColor = serializedObject.FindProperty("groundSkyGradientColor");
		m_followTarget = serializedObject.FindProperty("followTarget");
		m_sunDiskSize = serializedObject.FindProperty("sunDiskSize");
		m_moonDiskSize = serializedObject.FindProperty("moonDiskSize");
		m_cloudMode = serializedObject.FindProperty("cloudMode");


		RayleighSunColor = serializedObject.FindProperty("rayleighGradientColor");
		MieSunColor = serializedObject.FindProperty("mieGradientColor");
		MoonDiskColor = serializedObject.FindProperty("moonDiskGradientColor");
		MoonBrightColor = serializedObject.FindProperty("moonBrightGradientColor");
		LightColor = serializedObject.FindProperty("lightGradientColor");
		AmbientSkyColor = serializedObject.FindProperty("ambientSkyGradientColor");
		EquatorSkyColor = serializedObject.FindProperty("equatorSkyGradientColor");
		GroundSkyColor = serializedObject.FindProperty("groundSkyGradientColor");
        CloudColor = serializedObject.FindProperty("cloudGradientColor");

		// Create the curves output list
		serializedCurve = serializedObject.FindProperty ("outputCurveList");
		reorderableCurveList = new ReorderableList (serializedObject, serializedCurve, false, true, true, true);
		reorderableCurveList.drawElementCallback = (rect, index, isActive, isFocused) =>
		{
			rect.y += 2;
			EditorGUI.LabelField(rect, "element index " + index);
			EditorGUI.PropertyField(new Rect (rect.x+100, rect.y, rect.width-100, EditorGUIUtility.singleLineHeight), serializedCurve.GetArrayElementAtIndex(index), GUIContent.none);
		};

		reorderableCurveList.onAddCallback = l =>
		{
			var index = l.serializedProperty.arraySize;
			l.serializedProperty.arraySize++;
			l.index = index;
			serializedCurve.GetArrayElementAtIndex(index).animationCurveValue = AnimationCurve.Linear(0,0,24,0);
		};

		reorderableCurveList.drawHeaderCallback = rect =>
		{
			EditorGUI.LabelField(rect, "Curve Output", EditorStyles.boldLabel);
		};

		reorderableCurveList.drawElementBackgroundCallback = (rect, index, active, focused) => {
			Texture2D tex = new Texture2D (1, 1);
			tex.SetPixel (0, 0, col4);
			tex.Apply ();
			if (active)
				GUI.DrawTexture (rect, tex as Texture);
		};

		// Create the gradients output list
		serializedGradient = serializedObject.FindProperty ("outputGradientList");
		reorderableGradientList = new ReorderableList (serializedObject, serializedGradient, false, true, true, true);
		reorderableGradientList.drawElementCallback = (rect, index, isActive, isFocused) =>
		{
			rect.y += 2;
			EditorGUI.LabelField(rect, "element index " + index);
			EditorGUI.PropertyField(new Rect (rect.x+100, rect.y, rect.width-100, EditorGUIUtility.singleLineHeight), serializedGradient.GetArrayElementAtIndex(index), GUIContent.none);
		};

		reorderableGradientList.drawHeaderCallback = rect =>
		{
			EditorGUI.LabelField(rect, "Gradient Output", EditorStyles.boldLabel);
		};

		reorderableGradientList.drawElementBackgroundCallback = (rect, index, active, focused) => {
			Texture2D tex = new Texture2D (1, 1);
			tex.SetPixel (0, 0, col4);
			tex.Apply ();
			if (active) GUI.DrawTexture (rect, tex as Texture);
		};
	}

	public override void OnInspectorGUI()
	{
		// Start custom Inspector
		serializedObject.Update();
		EditorGUI.BeginChangeCheck();

		// Time of day header group
		GUILayout.Space(2);
		m_showTimeOfDayTab.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(m_showTimeOfDayTab.isExpanded, "Time of Day");
		if (m_showTimeOfDayTab.isExpanded)
		{
			EditorGUILayout.Slider(m_timeline, 0.0f, 24.0f, m_guiContent[0]);
			EditorGUILayout.Slider(m_latitude, -90.0f, 90.0f, m_guiContent[1]);
			EditorGUILayout.Slider(m_longitude, -180.0f, 180.0f, m_guiContent[2]);
			EditorGUILayout.Slider(m_utc, -12.0f, 12.0f, m_guiContent[3]);
			EditorGUILayout.PropertyField(m_dayCycleInMinutes, m_guiContent[4]);

			// Day-Night length curve
			EditorGUILayout.BeginVertical("Box");
			GUILayout.Space(-5);
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.Space();
			GUILayout.Label("Day and Night Length", EditorStyles.boldLabel);
			EditorGUILayout.EndHorizontal();

			// Toggle
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label(m_guiContent[5]);
			EditorGUILayout.PropertyField(m_setTimeByCurve, GUIContent.none, GUILayout.Width(15));
			EditorGUILayout.EndHorizontal();

			// Reset Button
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("R", GUILayout.Width(25), GUILayout.Height(25)))
				m_dayLengthCurve.animationCurveValue = AnimationCurve.Linear(0, 0, 24, 24);

			// Curve field
			EditorGUILayout.CurveField(m_dayLengthCurve, Color.yellow, new Rect(0, 0, 24, 24), GUIContent.none, GUILayout.Height(25));
			EditorGUILayout.EndHorizontal();

			// Current time display
			EditorGUILayout.BeginHorizontal();
			GUILayout.Label(m_guiContent[6]);
			GUILayout.Label(m_target.timeByCurve.ToString(), GUILayout.ExpandWidth(false));
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndVertical();
		}
		EditorGUILayout.EndFoldoutHeaderGroup();

		// References header group
		GUILayout.Space(2);
		m_showReferencesTab.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(m_showReferencesTab.isExpanded, "Objects & Materials");
		if (m_showReferencesTab.isExpanded)
		{
			EditorGUILayout.PropertyField(m_sunTransform);
			EditorGUILayout.PropertyField(m_moonTransform);
			EditorGUILayout.PropertyField(m_lightTransform);
			EditorGUILayout.PropertyField(m_starfieldTexture);
			EditorGUILayout.PropertyField(m_sunTexture);
			EditorGUILayout.PropertyField(m_moonTexture);
			EditorGUILayout.PropertyField(m_cloudTexture);
			EditorGUILayout.PropertyField(m_skyMaterial);
		}
		EditorGUILayout.EndFoldoutHeaderGroup();

		// Scattering header group
		GUILayout.Space(2);
		m_showScatteringTab.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(m_showScatteringTab.isExpanded, "Scattering");
		if (m_showScatteringTab.isExpanded)
		{
			// Rayleigh
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.CurveField(m_rayleighCurve, m_curveColor, new Rect(0.0f, 0.0f, 24.0f, 5.0f), m_guiContent[7]);
			GUILayout.TextField(m_target.rayleigh.ToString(), GUILayout.Width(36));
			if (GUILayout.Button("R", GUILayout.Width(18), GUILayout.Height(15))) { m_target.rayleighCurve = AnimationCurve.Linear(0.0f, 1.0f, 24.0f, 1.0f); }
			EditorGUILayout.EndHorizontal();

			// Mie
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.CurveField(m_mieCurve, m_curveColor, new Rect(0.0f, 0.0f, 24.0f, 30.0f), m_guiContent[8]);
			GUILayout.TextField(m_target.mie.ToString(), GUILayout.Width(36));
			if (GUILayout.Button("R", GUILayout.Width(18), GUILayout.Height(15))) { m_target.mieCurve = AnimationCurve.Linear(0.0f, 1.0f, 24.0f, 1.0f); }
			EditorGUILayout.EndHorizontal();

			// Kr
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.CurveField(m_krCurve, m_curveColor, new Rect(0.0f, 1.0f, 24.0f, 29.0f), m_guiContent[9]);
			GUILayout.TextField(m_target.kr.ToString(), GUILayout.Width(36));
			if (GUILayout.Button("R", GUILayout.Width(18), GUILayout.Height(15))) { m_target.krCurve = AnimationCurve.Linear(0.0f, 8.4f, 24.0f, 8.4f); }
			EditorGUILayout.EndHorizontal();

			// Km
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.CurveField(m_kmCurve, m_curveColor, new Rect(0.0f, 1.0f, 24.0f, 29.0f), m_guiContent[10]);
			GUILayout.TextField(m_target.km.ToString(), GUILayout.Width(36));
			if (GUILayout.Button("R", GUILayout.Width(18), GUILayout.Height(15))) { m_target.kmCurve = AnimationCurve.Linear(0.0f, 1.25f, 24.0f, 1.25f); }
			EditorGUILayout.EndHorizontal();

			// Scattering
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.CurveField(m_scatteringCurve, m_curveColor, new Rect(0.0f, 0.0f, 24.0f, 75.0f), m_guiContent[11]);
			GUILayout.TextField(m_target.scattering.ToString(), GUILayout.Width(36));
			if (GUILayout.Button("R", GUILayout.Width(18), GUILayout.Height(15))) { m_target.scatteringCurve = AnimationCurve.Linear(0.0f, 15.0f, 24.0f, 15.0f); }
			EditorGUILayout.EndHorizontal();

			// Sun Intensity
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.CurveField(m_sunIntensityCurve, m_curveColor, new Rect(0.0f, 0.0f, 24.0f, 5.0f), m_guiContent[12]);
			GUILayout.TextField(m_target.sunIntensity.ToString(), GUILayout.Width(36));
			if (GUILayout.Button("R", GUILayout.Width(18), GUILayout.Height(15))) { m_target.sunIntensityCurve = AnimationCurve.Linear(0.0f, 3.0f, 24.0f, 3.0f); }
			EditorGUILayout.EndHorizontal();

			// Night Intensity
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.CurveField(m_nightIntensityCurve, m_curveColor, new Rect(0.0f, 0.0f, 24.0f, 5.0f), m_guiContent[13]);
			GUILayout.TextField(m_target.nightIntensity.ToString(), GUILayout.Width(36));
			if (GUILayout.Button("R", GUILayout.Width(18), GUILayout.Height(15))) { m_target.nightIntensityCurve = AnimationCurve.Linear(0.0f, 0.5f, 24.0f, 0.5f); }
			EditorGUILayout.EndHorizontal();

			// Exposure
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.CurveField(m_exposureCurve, m_curveColor, new Rect(0.0f, 0.0f, 24.0f, 8.0f), m_guiContent[14]);
			GUILayout.TextField(m_target.exposure.ToString(), GUILayout.Width(36));
			if (GUILayout.Button("R", GUILayout.Width(18), GUILayout.Height(15))) { m_target.exposureCurve = AnimationCurve.Linear(0.0f, 1.75f, 24.0f, 1.75f); }
			EditorGUILayout.EndHorizontal();

			// Rayleigh color
			EditorGUILayout.PropertyField(m_rayleighGradientColor, m_guiContent[15]);

			// Mie color
			EditorGUILayout.PropertyField(m_mieGradientColor, m_guiContent[16]);
		}
		EditorGUILayout.EndFoldoutHeaderGroup();


		// Night sky header group
		GUILayout.Space(2);
		m_showNightSkyTab.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(m_showNightSkyTab.isExpanded, "Night Sky");
		if (m_showNightSkyTab.isExpanded)
		{
			// Moon disk color
			EditorGUILayout.PropertyField(m_moonDiskGradientColor, m_guiContent[17]);

			// Moon bright color
			EditorGUILayout.PropertyField(m_moonBrightGradientColor, m_guiContent[18]);

			// Moon disk range
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.CurveField(m_moonBrightRangeCurve, m_curveColor, new Rect(0.0f, 0.0f, 24.0f, 1.0f), m_guiContent[19]);
			GUILayout.TextField(m_target.moonBrightRange.ToString(), GUILayout.Width(36));
			if (GUILayout.Button("R", GUILayout.Width(18), GUILayout.Height(15))) { m_target.moonBrightRangeCurve = AnimationCurve.Linear(0.0f, 0.9f, 24.0f, 0.9f); }
			EditorGUILayout.EndHorizontal();

			// Starfield intensity
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.CurveField(m_starfieldIntensityCurve, m_curveColor, new Rect(0.0f, 0.0f, 24.0f, 10.0f), m_guiContent[20]);
			GUILayout.TextField(m_target.starfieldIntensity.ToString(), GUILayout.Width(36));
			if (GUILayout.Button("R", GUILayout.Width(18), GUILayout.Height(15))) { m_target.starfieldIntensityCurve = AnimationCurve.Linear(0.0f, 0.0f, 24.0f, 0.0f); }
			EditorGUILayout.EndHorizontal();

			// Milky Way intensity
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.CurveField(m_milkyWayIntensityCurve, m_curveColor, new Rect(0.0f, 0.0f, 24.0f, 1.0f), m_guiContent[21]);
			GUILayout.TextField(m_target.milkyWayIntensity.ToString(), GUILayout.Width(36));
			if (GUILayout.Button("R", GUILayout.Width(18), GUILayout.Height(15))) { m_target.milkyWayIntensityCurve = AnimationCurve.Linear(0.0f, 0.0f, 24.0f, 0.0f); }
			EditorGUILayout.EndHorizontal();

			// Starfield color balance
			m_starFieldColor.x = EditorGUILayout.Slider ("Starfield Color R", m_target.starfieldColorBalance.x, 1.0f, 2.0f);
			m_starFieldColor.y = EditorGUILayout.Slider ("Starfield Color G", m_target.starfieldColorBalance.y, 1.0f, 2.0f);
			m_starFieldColor.z = EditorGUILayout.Slider ("Starfield Color B", m_target.starfieldColorBalance.z, 1.0f, 2.0f);

			// Starfield position
			m_starFieldPosition.x = EditorGUILayout.Slider ("Starfield Position X", m_target.starfieldPosition.x, 0.0f, 360.0f);
			m_starFieldPosition.y = EditorGUILayout.Slider ("Starfield Position Y", m_target.starfieldPosition.y, 0.0f, 360.0f);
			m_starFieldPosition.z = EditorGUILayout.Slider ("Starfield Position Z", m_target.starfieldPosition.z, 0.0f, 360.0f);
		}
		EditorGUILayout.EndFoldoutHeaderGroup();


		// Cloud header group
		GUILayout.Space(2);
		m_showCloudTab.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(m_showCloudTab.isExpanded, "Clouds");
		if (m_showCloudTab.isExpanded)
		{
			// Color
			EditorGUILayout.PropertyField(m_cloudGradientColor, m_guiContent[22]);

			// Scattering
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.CurveField(m_cloudScatteringCurve, m_curveColor, new Rect(0.0f, 0.0f, 24.0f, 1.5f), m_guiContent[23]);
			GUILayout.TextField(m_target.cloudScattering.ToString(), GUILayout.Width(36));
			if (GUILayout.Button("R", GUILayout.Width(18), GUILayout.Height(15))) { m_target.cloudScatteringCurve = AnimationCurve.Linear(0.0f, 1.0f, 24.0f, 1.0f); }
			EditorGUILayout.EndHorizontal();

			// Extinction
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.CurveField(m_cloudExtinctionCurve, m_curveColor, new Rect(0.0f, 0.0f, 24.0f, 1.0f), m_guiContent[24]);
			GUILayout.TextField(m_target.cloudExtinction.ToString(), GUILayout.Width(36));
			if (GUILayout.Button("R", GUILayout.Width(18), GUILayout.Height(15))) { m_target.cloudExtinctionCurve = AnimationCurve.Linear(0.0f, 0.25f, 24.0f, 0.25f); }
			EditorGUILayout.EndHorizontal();

			// Power
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.CurveField(m_cloudPowerCurve, m_curveColor, new Rect(0.0f, 1.8f, 24.0f, 2.4f), m_guiContent[25]);
			GUILayout.TextField(m_target.cloudPower.ToString(), GUILayout.Width(36));
			if (GUILayout.Button("R", GUILayout.Width(18), GUILayout.Height(15))) { m_target.cloudPowerCurve = AnimationCurve.Linear(0.0f, 2.2f, 24.0f, 2.2f); }
			EditorGUILayout.EndHorizontal();

			// Intensity
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.CurveField(m_cloudIntensityCurve, m_curveColor, new Rect(0.0f, 0.0f, 24.0f, 2.0f), m_guiContent[26]);
			GUILayout.TextField(m_target.cloudIntensity.ToString(), GUILayout.Width(36));
			if (GUILayout.Button("R", GUILayout.Width(18), GUILayout.Height(15))) { m_target.cloudIntensityCurve = AnimationCurve.Linear(0.0f, 1.0f, 24.0f, 1.0f); }
			EditorGUILayout.EndHorizontal();

			// Rotation Speed
			EditorGUILayout.Slider(m_cloudRotationSpeed, -0.01f, 0.01f, m_guiContent[27]);
        }
		EditorGUILayout.EndFoldoutHeaderGroup();


		// Fog scattering header group
		GUILayout.Space(2);
		m_showFogTab.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(m_showFogTab.isExpanded, "Fog Scattering");
		if (m_showFogTab.isExpanded)
		{
			// Distance
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.CurveField(m_fogDistanceCurve, m_curveColor, new Rect(0.0f, 0.0f, 24.0f, 20000.0f), m_guiContent[28]);
			GUILayout.TextField(m_target.fogDistance.ToString(), GUILayout.Width(36));
			if (GUILayout.Button("R", GUILayout.Width(18), GUILayout.Height(15))) { m_target.fogDistanceCurve = AnimationCurve.Linear(0.0f, 3500.0f, 24.0f, 3500.0f); }
			EditorGUILayout.EndHorizontal();

			// Blend
			EditorGUILayout.Slider(m_fogBlend, 0.0f, 1.0f, m_guiContent[29]);

			// Mie distance
			EditorGUILayout.Slider(m_mieDistance, 0.0f, 1.0f, m_guiContent[30]);
        }
		EditorGUILayout.EndFoldoutHeaderGroup();

		
		// Lighting header group
		GUILayout.Space(2);
		m_showLightingTab.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(m_showLightingTab.isExpanded, "Lighting");
		if (m_showLightingTab.isExpanded)
		{
			// Directional light intensity
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.CurveField(m_directionalLightIntensityCurve, m_curveColor, new Rect(0.0f, 0.0f, 24.0f, 8.0f), m_guiContent[31]);
			GUILayout.TextField(m_target.lightIntensity.ToString(), GUILayout.Width(36));
			if (GUILayout.Button("R", GUILayout.Width(18), GUILayout.Height(15))) { m_target.lightIntensityCurve = AnimationCurve.Linear(0.0f, 1.0f, 24.0f, 1.0f); }
			EditorGUILayout.EndHorizontal();

			// Directional light flare intensity
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.CurveField(m_directionalLightFlareIntensityCurve, m_curveColor, new Rect(0.0f, 0.0f, 24.0f, 8.0f), m_guiContent[32]);
			GUILayout.TextField(m_target.flareIntensity.ToString(), GUILayout.Width(36));
			if (GUILayout.Button("R", GUILayout.Width(18), GUILayout.Height(15))) { m_target.lightIntensityCurve = AnimationCurve.Linear(0.0f, 1.0f, 24.0f, 1.0f); }
			EditorGUILayout.EndHorizontal();
			
			// Directional light color
			EditorGUILayout.PropertyField(m_directionalLightGradientColor, m_guiContent[33]);

			// Environment intensity
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.CurveField(m_environmentIntensityCurve, m_curveColor, new Rect(0.0f, 0.0f, 24.0f, 8.0f), m_guiContent[34]);
			GUILayout.TextField(m_target.ambientIntensity.ToString(), GUILayout.Width(36));
			if (GUILayout.Button("R", GUILayout.Width(18), GUILayout.Height(15))) { m_target.ambientIntensityCurve = AnimationCurve.Linear(0.0f, 1.0f, 24.0f, 1.0f); }
			EditorGUILayout.EndHorizontal();

			// Ambient color
			EditorGUILayout.PropertyField(m_ambientGradientColor, m_guiContent[35]);

			// Equator color
			EditorGUILayout.PropertyField(m_equatorGradientColor, m_guiContent[36]);

			// Ground color
			EditorGUILayout.PropertyField(m_groundGradientColor, m_guiContent[37]);
		}
		EditorGUILayout.EndFoldoutHeaderGroup();


		// Options header group
		GUILayout.Space(2);
		m_showOptionsTab.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(m_showOptionsTab.isExpanded, "Options");
		if (m_showOptionsTab.isExpanded)
		{
			// Sun disk size
			EditorGUILayout.Slider(m_sunDiskSize, 0.0f, 1.0f, m_guiContent[39]);

			// Moon disk size.
			EditorGUILayout.Slider(m_moonDiskSize, 0.0f, 1.0f, m_guiContent[40]);

			// Follow target
			EditorGUILayout.PropertyField(m_followTarget, m_guiContent[38]);

			// Cloud mode
			EditorGUILayout.PropertyField(m_cloudMode, m_guiContent[41]);
		}
		EditorGUILayout.EndFoldoutHeaderGroup();

		// Outputs header group
		GUILayout.Space(2);
		m_showOutputsTab.isExpanded = EditorGUILayout.BeginFoldoutHeaderGroup(m_showOutputsTab.isExpanded, "Outputs");
		if (m_showOutputsTab.isExpanded)
		{
			EditorGUILayout.Space();
			reorderableCurveList.DoLayoutList();
			EditorGUILayout.Space();
			reorderableGradientList.DoLayoutList();
		}
		EditorGUILayout.EndFoldoutHeaderGroup();


		// End custom Inspector
		if (EditorGUI.EndChangeCheck())
		{
			Undo.RecordObject(m_target, "Undo Azure Sky Controller");
			serializedObject.ApplyModifiedProperties();
			m_target.starfieldColorBalance = m_starFieldColor;
			m_target.starfieldPosition = m_starFieldPosition;
			m_target.UpdateSkySettings();
		}
	}
}