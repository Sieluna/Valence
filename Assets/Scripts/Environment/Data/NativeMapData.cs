using System.Collections;
using Environment.System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Environment.Data
{
    public class NativeMapData
    {
        private NativeArray<Block> nativeBlocks;
        public JobHandle jobHandle;

        public NativeMapData(int3 chunkSize) => nativeBlocks = new NativeArray<Block>(chunkSize.x * chunkSize.y * chunkSize.z, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

        ~NativeMapData()
        {
            jobHandle.Complete(); Dispose();
        }

        public void Dispose()
        {
            if (nativeBlocks.IsCreated) nativeBlocks.Dispose();
        }

        public IEnumerator Generate(Block[] blocks, int3 chunkPosition, int3 chunkSize)
        {
            nativeBlocks.CopyFrom(blocks);

            jobHandle = new BuildMapSystem
            {
                chunkPosition = chunkPosition,
                chunkSize = chunkSize,
                blocks = nativeBlocks,
            }.Schedule(nativeBlocks.Length, 32);

            int frameCount = 1;
            yield return new WaitUntil(() =>
            {
                frameCount++;
                return jobHandle.IsCompleted || frameCount >= 4;
            });
                
            jobHandle.Complete();
            nativeBlocks.CopyTo(blocks);
            nativeBlocks.Dispose();
        }
    }
}