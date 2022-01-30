using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Utilities;

namespace Environment.System
{
    public class BuildColliderSystem : MonoSingleton<BuildColliderSystem>
    {
        private struct MeshNode { public Chunk chunk; public Mesh mesh; }

        private List<MeshNode> meshes = new();
        
        private NativeArray<int> meshIds;
        private JobHandle jobHandle;
        
        public void Enqueue(Chunk chunk, Mesh mesh) => meshes.Add(new MeshNode { chunk = chunk, mesh = mesh });

        private void Start() => StartCoroutine(BakeUpdater());

        private void OnDestroy()
        {
            jobHandle.Complete();
            if (meshIds.IsCreated) meshIds.Dispose();
        }

        private IEnumerator BakeUpdater()
        {
            int counter = 0;
            while (true)
            {
                if (meshes.Count == 0) { counter = 0; yield return null; continue; }
                if (counter < 4 && meshes.Count < 5) { counter++; yield return null; continue; } // 批处理

                meshIds = new NativeArray<int>(meshes.Count, Allocator.TempJob);

                for (int i = 0, iMax = meshes.Count; i < iMax; ++i) 
                    meshIds[i] = meshes[i].mesh.GetInstanceID();
                
                jobHandle = new BakeJob { meshIds = meshIds }.Schedule(meshIds.Length, 32);
                JobHandle.ScheduleBatchedJobs();
                
                int frameCount = 1;
                yield return new WaitUntil(() =>
                {
                    frameCount++;
                    return jobHandle.IsCompleted || frameCount >= 4;
                });
                
                jobHandle.Complete();
                meshIds.Dispose();

                for (int i = 0, iMax = meshes.Count; i < iMax; i++)
                    meshes[i].chunk.SetSharedMesh(meshes[i].mesh);

                meshes.Clear();
                counter = 0;
                yield return null;
            }
        }

        [BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
        public struct BakeJob : IJobParallelFor
        {
            public NativeArray<int> meshIds;

            public void Execute(int index) => Physics.BakeMesh(meshIds[index], false);
        }
    }
}