using System.Collections;
using Environment.System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Utilities;

namespace Environment.Data
{
    public class NativeMeshData
    {
        private NativeArray<Block> nativeBlocks;
        public NativeArray<float3> nativeVertices;
        public NativeArray<float3> nativeNormals;
        public NativeList<int> nativeIndices;
        public NativeList<int> nativeSubIndices;
        public NativeList<int> nativeMorIndices;
        public NativeArray<float4> nativeUVs;
        public NativeArray<Color> nativeColors;
        public JobHandle jobHandle;
        private NativeCounter counter;

        public NativeMeshData(int3 chunkSize)
        {
            var numBlocks = chunkSize.x * chunkSize.y * chunkSize.z;
            var numLayer = chunkSize.x * chunkSize.z;

            nativeBlocks = new NativeArray<Block>(numBlocks, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            nativeVertices = new NativeArray<float3>(12 * numBlocks, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            nativeNormals = new NativeArray<float3>(12 * numBlocks, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            nativeUVs = new NativeArray<float4>(12 * numBlocks, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            nativeColors = new NativeArray<Color>(12 * numBlocks, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            nativeIndices = new NativeList<int>(18 * numBlocks, Allocator.TempJob);
            nativeSubIndices = new NativeList<int>(36 * numLayer, Allocator.TempJob);
            nativeMorIndices = new NativeList<int>(72 * numLayer, Allocator.TempJob);
            counter = new NativeCounter(Allocator.TempJob);
        }

        ~NativeMeshData()
        {
            jobHandle.Complete(); Dispose();
        }

        public void Dispose()
        {
            if (nativeBlocks.IsCreated) nativeBlocks.Dispose();
            if (nativeVertices.IsCreated) nativeVertices.Dispose();
            if (nativeNormals.IsCreated) nativeNormals.Dispose();
            if (nativeUVs.IsCreated) nativeUVs.Dispose();
            if (nativeColors.IsCreated) nativeColors.Dispose();
            if (nativeIndices.IsCreated) nativeIndices.Dispose();
            if (nativeSubIndices.IsCreated) nativeSubIndices.Dispose();
            if (nativeMorIndices.IsCreated) nativeMorIndices.Dispose();
            if (counter.IsCreated) counter.Dispose();
        }

        public IEnumerator ScheduleMeshingJob(Block[] blocks, NativeLightData lightData, int3 chunkSize, bool argent = false)
        {
            nativeBlocks.CopyFrom(blocks);
            
            jobHandle = new BuildMeshSystem
            {
                blocks = nativeBlocks,
                blockData = World.Instance.BlockData,
                chunkSize = chunkSize,
                vertices = nativeVertices,
                normals = nativeNormals,
                uvs = nativeUVs,
                colors = nativeColors,
                indices = nativeIndices,
                subIndices = nativeSubIndices,
                morIndices = nativeMorIndices,
                lightData = lightData.nativeLightData,
                counter = counter
            }.Schedule();
            JobHandle.ScheduleBatchedJobs();

            int frameCount = lightData.frameCount;
            yield return new WaitUntil(() =>
            {
                frameCount++;
                return jobHandle.IsCompleted || frameCount >= 4 || argent;
            });

            jobHandle.Complete();
        }

        public void GetMeshInformation(out int verticesSize) => verticesSize = counter.Count * 4;
    }
}