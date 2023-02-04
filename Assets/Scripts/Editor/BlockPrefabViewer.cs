using System;
using Environment;
using Environment.Data;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(BlockPrefab))]
public class BlockPrefabViewer : Editor
{
    private BlockPrefab m_prefab;

    private ReorderableList m_atlasUVArray;

    private Material m_previewMaterial;
    private Mesh m_cubeMesh;

    private readonly BlockType[] m_caches = new BlockType[2];
    private readonly string[] m_direction = {"Right", "Left", "Up", "Down", "Front", "Back"};

    private void OnEnable()
    {
        m_prefab = target as BlockPrefab;

        m_caches[1] = m_prefab!.block;

        m_previewMaterial = new Material(Shader.Find("Hidden/BlockPrefabPreview"));
        m_cubeMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");

        m_atlasUVArray = new ReorderableList(serializedObject, serializedObject.FindProperty("atlasPositions"),
            false, true, false, false)
        {
            drawHeaderCallback = rect => GUI.Label(rect, "Atlas Positions"),
            elementHeight = EditorGUIUtility.singleLineHeight * 1.2f,
            drawElementCallback = (rect, index, active, focused) =>
            {
                var item = m_atlasUVArray.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 4; // keep a little bit lower
                EditorGUI.PropertyField(rect, item, new GUIContent(m_direction[index]));
            },
            elementHeightCallback = index =>
            {
                var item = m_atlasUVArray.serializedProperty.GetArrayElementAtIndex(index);
                return EditorGUI.GetPropertyHeight(item, true) * 1.2f;
            }
        };
    }

    private void OnDisable()
    {
        AssetDatabase.SaveAssets();
        DestroyPreview();
    }

    #region BlockSettings

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        m_caches[0] = (BlockType) EditorGUILayout.EnumPopup("Block Type", m_prefab.block);
        EditorGUILayout.Space(5);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("shape"), new GUIContent("Block Shape"));
        EditorGUILayout.Space(10);
        m_atlasUVArray.DoLayoutList();
        if (m_caches[0] != m_prefab.block)
        {
            var fallback = string.IsNullOrWhiteSpace(AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(m_prefab.GetInstanceID()), m_caches[0].ToString()));
            m_prefab.block = fallback ? m_caches[0] : m_caches[1];
            m_prefab.name = (fallback ? m_caches[0] : m_caches[1]).ToString();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            m_caches[1] = m_prefab.block;
        }

        serializedObject.ApplyModifiedProperties();
    }

    #endregion

    #region BlockPreview

    private PreviewRenderUtility m_previewRenderUtility;

    private Vector2 m_drag;

    private readonly Vector4[] m_uvs = new Vector4[3];

    private static readonly int Uvs = Shader.PropertyToID("uvs");
    private static readonly int Scale = Shader.PropertyToID("scale");

    public override bool HasPreviewGUI()
    {
        if (m_prefab == null) throw new Exception("No prefab Data");

        for (int i = 0; i < 3; i++)
            m_uvs[i] = new Vector4(m_prefab.atlasPositions[i * 2].x, m_prefab.atlasPositions[i * 2].y, m_prefab.atlasPositions[i * 2 + 1].x, m_prefab.atlasPositions[i * 2 + 1].y);

        DestroyPreview();

        m_previewRenderUtility = new PreviewRenderUtility();
        GC.SuppressFinalize(m_previewRenderUtility);
        m_previewRenderUtility.camera.fieldOfView = 30f;

        return true;
    }

    public override void OnPreviewGUI(Rect rect, GUIStyle background)
    {
        m_drag = Drag2D(m_drag, rect);

        if (Event.current.type != EventType.Repaint) return;

        if (m_previewRenderUtility == null)
        {
            EditorGUI.DropShadowLabel(rect, "Error");
        }
        else
        {
            m_previewMaterial.SetVectorArray(Uvs, m_uvs);
            m_previewMaterial.SetFloat(Scale, 1f / Shared.AtlasSize.x);

            m_previewRenderUtility.BeginPreview(rect, background);

            m_previewRenderUtility.DrawMesh(m_cubeMesh, Matrix4x4.identity, m_previewMaterial, 0);

            m_previewRenderUtility.camera.transform.position = Vector2.zero;
            m_previewRenderUtility.camera.transform.rotation = Quaternion.Euler(new Vector3(-m_drag.y, -m_drag.x, 0));
            m_previewRenderUtility.camera.transform.position = m_previewRenderUtility.camera.transform.forward * -6f;

            m_previewRenderUtility.Render(true);

            m_previewRenderUtility.EndAndDrawPreview(rect);
        }
    }

    private void DestroyPreview() => m_previewRenderUtility?.Cleanup();

    private static Vector2 Drag2D(Vector2 scrollPos, Rect position)
    {
        var controlID = GUIUtility.GetControlID("Slider".GetHashCode(), FocusType.Passive);
        var current = Event.current;
        switch (current.GetTypeForControl(controlID))
        {
            case EventType.MouseDown:
                if (position.Contains(current.mousePosition) && position.width > 50f)
                {
                    GUIUtility.hotControl = controlID;
                    current.Use();
                    EditorGUIUtility.SetWantsMouseJumping(1);
                }

                break;
            case EventType.MouseUp:
                if (GUIUtility.hotControl == controlID)
                {
                    GUIUtility.hotControl = 0;
                }

                EditorGUIUtility.SetWantsMouseJumping(0);
                break;
            case EventType.MouseDrag:
                if (GUIUtility.hotControl == controlID)
                {
                    scrollPos -= current.delta * (!current.shift ? 1 : 3) / Mathf.Min(position.width, position.height) * 140f;
                    scrollPos.y = Mathf.Clamp(scrollPos.y, -90f, 90f);
                    current.Use();
                    GUI.changed = true;
                }

                break;
        }

        return scrollPos;
    }

    #endregion
}