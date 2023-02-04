using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Utilities;

public class NoiseTextureViewer : EditorWindow
{
    private Texture2D m_texture;

    private NativeArray<byte> m_buffer;

    private int2 m_resolution;
    
    private float m_record;
    
    [MenuItem("Window/Noise Viewer")]
    private static void ShowWindow() => GetWindow(typeof(NoiseTextureViewer));
    
    private void OnGUI()
    {
        if (m_resolution.Equals(null) || m_resolution.x != (int)position.width || m_resolution.y != (int) position.height)
        {
            m_resolution = new int2((int) position.width, (int) position.height);
            m_buffer = new NativeArray<byte>((int)position.width * (int)position.height * 3, Allocator.Persistent);
            m_record = 0f;
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            Paint((int)position.width, (int)position.height);
            stopWatch.Stop();
            m_record = stopWatch.ElapsedMilliseconds;
        }
        EditorGUI.DrawPreviewTexture(GUILayoutUtility.GetRect(position.width, position.height), m_texture);
        EditorGUI.LabelField(new Rect(5, 5, 100, 20),  m_record + " ms", new GUIStyle { normal = { textColor = Color.green }});
    }

    private void OnDisable()
    {
        if (m_buffer.IsCreated) m_buffer.Dispose();
    }

    private void Paint(int width, int height)
    {
        m_texture = new Texture2D(width, height, TextureFormat.RGB24, false);
        var jobHandle = new GenerateNoiseTexture
        {
            resolution = new int2(width, height),
            buffer = m_buffer
        }.Schedule(width * height, 64);
        jobHandle.Complete();
        m_texture.SetPixelData(m_buffer, 0);
        m_texture.Apply();
    }
    
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    private struct GenerateNoiseTexture : IJobParallelFor
    {
        [ReadOnly] public int2 resolution;
        
        [NativeDisableParallelForRestriction] public NativeArray<byte> buffer;

        public void Execute(int index)
        {
            var position = index.To2DIndex(resolution.x);
            var height = Noise.FractalSimplex(position + new float2(9.0f, 0.5f), 0.008f, 2) * 5f +
                         Noise.FractalSimplex(position + new float2(0.2f, 7.5f), 0.022f, 3) * 4.5f +
                         Noise.FractalSimplex(position + new float2(5.3f, 0.2f), 0.001f, 4) * 30f;//39.5f;

            var output = (byte) math.floor(height + 55);
            buffer[index * 3] = output;
            buffer[index * 3 + 1] = output;
            buffer[index * 3 + 2] = output;
        }
    }
}
