using UnityEngine;
#if !UNITY_EDITOR
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine.UI;
#endif

namespace Utilities
{
    public class FPSCounter : MonoBehaviour
    {
#if !UNITY_EDITOR
        private const float MemoryDivider = 1048576;
        private Canvas m_canvas;
        private Text m_text;

        private ProfilerRecorder m_mainThreadTimeRecorder;
        private ProfilerRecorder m_systemMemoryRecorder, m_gcMemoryRecorder;
        private ProfilerRecorder m_drawCallsRecorder, m_verticesRecorder, m_trianglesRecorder;
        
        private static float Fps => 1 / Time.unscaledDeltaTime;
        private static double GetRecorderFrameAverage(ProfilerRecorder recorder)
        {
            var samplesCount = recorder.Capacity;
            if (samplesCount == 0) return 0;

            double r = 0;
            unsafe
            {
                var samples = stackalloc ProfilerRecorderSample[samplesCount];
                recorder.CopyTo(samples, samplesCount);
                for (var i = 0; i < samplesCount; ++i)
                    r += samples[i].Value;
                r /= samplesCount;
            }

            return r;
        }
        
        private void OnEnable()
        {
            m_mainThreadTimeRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 15);
            m_systemMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "System Used Memory");
            m_gcMemoryRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Reserved Memory");
            m_drawCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
            m_verticesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Vertices Count");
            m_trianglesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count");
        }

        private void OnDisable()
        {
            m_mainThreadTimeRecorder.Dispose();
            m_systemMemoryRecorder.Dispose();
            m_gcMemoryRecorder.Dispose();
            m_drawCallsRecorder.Dispose();
            m_verticesRecorder.Dispose();
            m_trianglesRecorder.Dispose();
        }
        
        private void Awake()
        {
            #region GUIBuilder

            m_canvas = gameObject.AddComponent<Canvas>();
            m_canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            m_text = Instantiate(new GameObject("Text Field") { hideFlags = HideFlags.HideInHierarchy }, m_canvas.transform, true).AddComponent<Text>();
            m_text.rectTransform.localPosition = new Vector3(10, -10);
            m_text.rectTransform.sizeDelta = new Vector2(Screen.width / 4f, Screen.height / 3f);
            m_text.rectTransform.anchorMax = Vector2.up;
            m_text.rectTransform.anchorMin = Vector2.up;
            m_text.rectTransform.pivot = Vector2.up;
            m_text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            m_text.lineSpacing = 1.2f;
            m_text.fontSize = math.clamp(Screen.height / 35, 14, 30);
            m_text.horizontalOverflow = HorizontalWrapMode.Overflow;

            #endregion
            
            InvokeRepeating(nameof(SysInfoUpdater), 0, 0.6f);
        }

        private void SysInfoUpdater()
        {
            m_text.text = $"Fps: {Fps:F1}  [{GetRecorderFrameAverage(m_mainThreadTimeRecorder) * (1e-6f):F1} ms]\n" +
                          $"Sys Memory: {m_systemMemoryRecorder.LastValue / MemoryDivider:F0}MB\n" +
                          $"GC Memory: {m_gcMemoryRecorder.LastValue / MemoryDivider:F0}MB\n" +
                          $"Draw Calls: {m_drawCallsRecorder.LastValue}\n" +
                          $"Verts: {m_verticesRecorder.LastValue * (1e-3f):F1}k  Tris: {m_trianglesRecorder.LastValue * (1e-3f):F1}k";
        }
#endif
    }
}
