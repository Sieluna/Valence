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
        private NativeArray<Block> m_nativeBlocks;
        public JobHandle jobHandle;

        public NativeMapData(int3 chunkSize) => m_nativeBlocks = new NativeArray<Block>(chunkSize.x * chunkSize.y * chunkSize.z, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

        ~NativeMapData()
        {
            jobHandle.Complete(); Dispose();
        }

        public void Dispose()
        {
            if (m_nativeBlocks.IsCreated) m_nativeBlocks.Dispose();
        }

        public IEnumerator Generate(Block[] blocks, int3 chunkPosition, int3 chunkSize)
        {
            var maxHeight = 0;
            
            var baseJobHandle = new BuildBaseSystem
            {
                chunkPosition = chunkPosition,
                chunkSize = chunkSize,
                maxHeight = maxHeight,
                blocks = m_nativeBlocks,
            }.Schedule(m_nativeBlocks.Length, 32);

            var mineJobHandle = new BuildMineSystem
            {
                chunkPosition = chunkPosition,
                chunkSize = chunkSize,
                blocks = m_nativeBlocks,
            }.Schedule(m_nativeBlocks.Length, 32, baseJobHandle);

            jobHandle = new BuildFoliageSystem
            {
                chunkPosition = chunkPosition,
                chunkSize = chunkSize,
                blocks = m_nativeBlocks,
            }.Schedule(m_nativeBlocks.Length, 32, mineJobHandle);
            
            //jobHandle = new BuildBiomesSystem
            //{
            //    chunkPosition = chunkPosition,
            //    chunkSize = chunkSize,
            //    blocks = m_NativeBlocks,
            //}.Schedule(m_NativeBlocks.Length, 32);
            
            var frameCount = 1;
            yield return new WaitUntil(() =>
            {
                frameCount++;
                return jobHandle.IsCompleted || frameCount >= 4;
            });
            
            jobHandle.Complete();
            m_nativeBlocks.CopyTo(blocks);
            m_nativeBlocks.Dispose();
        }
    }
}