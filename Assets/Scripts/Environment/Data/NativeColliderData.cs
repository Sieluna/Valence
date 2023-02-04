using System.Collections;
using System.Collections.Generic;
using Environment.System;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Utilities;

namespace Environment.Data
{
    public class NativeColliderData : MonoSingleton<NativeColliderData>
    {
        private struct MeshNode { public Chunk chunk; public Mesh mesh; }
        
        private readonly List<MeshNode> m_meshes = new List<MeshNode>();
        
        private NativeArray<int> m_meshIds;
        private JobHandle m_jobHandle;
        
        public void Enqueue(Chunk chunk, Mesh mesh) => m_meshes.Add(new MeshNode { chunk = chunk, mesh = mesh });

        private void Start() => StartCoroutine(BakeUpdater());

        private void OnDestroy()
        {
            m_jobHandle.Complete();
            if (m_meshIds.IsCreated) m_meshIds.Dispose();
        }
        
        private IEnumerator BakeUpdater()
        {
            var counter = 0;
            while (true)
            {
                if (m_meshes.Count == 0) { counter = 0; yield return null; continue; }
                if (counter < 4 && m_meshes.Count < 5) { counter++; yield return null; continue; } // 批处理

                m_meshIds = new NativeArray<int>(m_meshes.Count, Allocator.TempJob);

                for (int i = 0, iMax = m_meshes.Count; i < iMax; ++i) 
                    m_meshIds[i] = m_meshes[i].mesh.GetInstanceID();
                
                m_jobHandle = new BuildColliderSystem { meshIds = m_meshIds }.Schedule(m_meshIds.Length, 32);
                JobHandle.ScheduleBatchedJobs();
                
                var frameCount = 1;
                yield return new WaitUntil(() =>
                {
                    frameCount++;
                    return m_jobHandle.IsCompleted || frameCount >= 4;
                });
                
                m_jobHandle.Complete();
                m_meshIds.Dispose();

                for (int i = 0, iMax = m_meshes.Count; i < iMax; i++)
                    m_meshes[i].chunk.SetSharedMesh(m_meshes[i].mesh);

                m_meshes.Clear();
                counter = 0;
                yield return null;
            }
        }
    }
}