using System.Collections;
using System.Collections.Generic;
using Environment.System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Environment.Data
{
    public class NativeLightData
    {
        public NativeArray<BlockLight> nativeLightData;
        private NativeArray<Block> nativeBlocksWithNeighbor;
        private NativeHashMap<int3, int> nativeNeighborHashMap;
        public int frameCount;
        public JobHandle jobHandle;

        public NativeLightData(int3 chunkSize) => nativeLightData = new NativeArray<BlockLight>(chunkSize.x * chunkSize.y * chunkSize.z, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

        ~NativeLightData()
        {
            jobHandle.Complete(); Dispose();
        }

        public IEnumerator ScheduleLightingJob(List<Block[]> neighborBlocks, int3 chunkPosition, int3 chunkSize, int numNeighbor, bool argent = false)
        {
            nativeNeighborHashMap = new NativeHashMap<int3, int>(neighborBlocks.Count, Allocator.TempJob);

            int blockIndex = 0;
            int numNeighbors = 0;
            for (int x = chunkPosition.x - numNeighbor, xMax = chunkPosition.x + numNeighbor; x <= xMax; x++)
                for (int y = chunkPosition.y - numNeighbor, yMax = chunkPosition.y + numNeighbor; y <= yMax; y++)
                    for (int z = chunkPosition.z - numNeighbor, zMax = chunkPosition.z + numNeighbor; z <= zMax; z++)
                    {
                        int3 neighborChunkPosition = new int3(x, y, z);
                        if (neighborBlocks[blockIndex] == null)
                        {
                            nativeNeighborHashMap.TryAdd(neighborChunkPosition, -1);
                        }
                        else
                        {
                            nativeNeighborHashMap.TryAdd(neighborChunkPosition, numNeighbors);
                            numNeighbors += 1;
                        }

                        blockIndex += 1;
                    }

            int numVoxels = chunkSize.x * chunkSize.y * chunkSize.z;

            nativeBlocksWithNeighbor = new NativeArray<Block>(numNeighbors * numVoxels, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            blockIndex = 0;
            for (int x = chunkPosition.x - numNeighbor, xMax = chunkPosition.x + numNeighbor; x <= xMax; x++)
                for (int y = chunkPosition.y - numNeighbor, yMax = chunkPosition.y + numNeighbor; y <= yMax; y++)
                    for (int z = chunkPosition.z - numNeighbor, zMax = chunkPosition.z + numNeighbor; z <= zMax; z++)
                    {
                        int3 neighborChunkPosition = new int3(x, y, z);
                        if (nativeNeighborHashMap[neighborChunkPosition] != -1)
                        {
                            NativeArray<Block>.Copy(neighborBlocks[blockIndex], 0, nativeBlocksWithNeighbor, nativeNeighborHashMap[neighborChunkPosition] * numVoxels, numVoxels);
                        }
                        blockIndex += 1;
                    }

            jobHandle = new BuildLightSystem
            {
                blockData = World.Instance.BlockData,
                blocksWithNeighbor = nativeBlocksWithNeighbor,
                neighborHashMap = nativeNeighborHashMap,
                chunkPosition = chunkPosition,
                chunkSize = chunkSize,
                lightDatas = nativeLightData
            }.Schedule(nativeLightData.Length, 32);
            
            JobHandle.ScheduleBatchedJobs();

            frameCount = 0;
            yield return new WaitUntil(() =>
            {
                frameCount++;
                return jobHandle.IsCompleted || frameCount >= 3 || argent;
            });

            jobHandle.Complete();
        }

        public void Dispose()
        {
            if (nativeLightData.IsCreated) nativeLightData.Dispose();
            if (nativeBlocksWithNeighbor.IsCreated) nativeBlocksWithNeighbor.Dispose();
            if (nativeNeighborHashMap.IsCreated) nativeNeighborHashMap.Dispose();
        }
    }
}