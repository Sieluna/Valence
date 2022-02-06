using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

namespace Utilities
{
    public class FPSCounter : MonoBehaviour
    {
        private const float MemoryDivider = 1048576;
        private Canvas m_Canvas;
        private Text m_Text;

        private static float Fps => 1 / Time.unscaledDeltaTime;
        private static float Cpu => 1000 * Time.deltaTime;

        private static float MonoMemory => GC.GetTotalMemory(false) / MemoryDivider;
        private static float AllocMemory => Profiler.GetTotalAllocatedMemoryLong() / MemoryDivider;

        private void Awake()
        {
            #region GUIBuilder

            m_Canvas = gameObject.AddComponent<Canvas>();
            m_Canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            m_Text = Instantiate(new GameObject("Text Field") { hideFlags = HideFlags.HideInHierarchy }, m_Canvas.transform, true).AddComponent<Text>();
            m_Text.rectTransform.localPosition = new Vector3(10, -10);
            m_Text.rectTransform.sizeDelta = new Vector2(Screen.width / 4f, Screen.height / 3f);
            m_Text.rectTransform.anchorMax = Vector2.up;
            m_Text.rectTransform.anchorMin = Vector2.up;
            m_Text.rectTransform.pivot = Vector2.up;
            m_Text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            m_Text.lineSpacing = 1.2f;
            m_Text.fontSize = math.clamp(Screen.height / 35, 14, 30);
            m_Text.horizontalOverflow = HorizontalWrapMode.Overflow;

            #endregion
            
            InvokeRepeating(nameof(SysInfoUpdater), 0, 0.5f);
        }

        private void SysInfoUpdater()
        {
            m_Text.text = $"FPS: {Fps:f1}  [{Cpu:f} MS]\n" +
                          $"MEM TOTAL: {MonoMemory:f}MB\n" +
                          $"MEM ALLOC: {AllocMemory:f}MB";
        }
    }
}
