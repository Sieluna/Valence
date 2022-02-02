using System;
using Environment.Data;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Utilities;

namespace Environment.System
{
    public struct BuildSharedSystem
    {
        public void InitSharedStatic(string path)
        {
            SharedData.BlockData.Data = new FixedArray<long>(Enum.GetValues(typeof(BlockType)).Length);
            Array.ForEach(Resources.LoadAll<BlockPrefab>(path), prefab =>
            {
                var data = 0L;
                data |= (UnsafeUtility.EnumToInt(prefab.shape) & 0xFFL) << 56; // blockTypeBuffer
                data |= (255 & 0xFFL) << 48; // empty byte
                for (int i = 0, currentBit = 40; i < 6; i++, currentBit -= 8)
                    data |= (prefab.atlasPositions[i].PackUVCoord() & 0xFFL) << currentBit;

                SharedData.BlockData.Data[UnsafeUtility.EnumToInt(prefab.block)] = data;
            });
            
            // right, left, top, bottom, front, back
            SharedData.DirectionAlignedX.Data = new FixedArray<int>(6); 
            SharedData.DirectionAlignedX.Data.CopyFrom(2, 2, 0, 0, 0, 0);

            SharedData.DirectionAlignedY.Data = new FixedArray<int>(6);
            SharedData.DirectionAlignedY.Data.CopyFrom(1, 1, 2, 2, 1, 1);

            SharedData.DirectionAlignedZ.Data = new FixedArray<int>(6);
            SharedData.DirectionAlignedZ.Data.CopyFrom(0, 0, 1, 1, 2, 2);

            SharedData.DirectionAlignedSign.Data = new FixedArray<int>(6);
            SharedData.DirectionAlignedSign.Data.CopyFrom(1, -1, 1, -1, 1, -1);

            SharedData.CubeDirectionOffsets.Data = new FixedArray<int3>(6);
            SharedData.CubeDirectionOffsets.Data.CopyFrom(new int3(1, 0, 0), new int3(-1, 0, 0), new int3(0, 1, 0), new int3(0, -1, 0), new int3(0, 0, 1), new int3(0, 0, -1));

            SharedData.CubeVertices.Data = new FixedArray<float3>(8);
            SharedData.CubeVertices.Data.CopyFrom(new float3(0f, 0f, 0f), new float3(1f, 0f, 0f), new float3(1f, 0f, 1f), new float3(0f, 0f, 1f), new float3(0f, 1f, 0f), new float3(1f, 1f, 0f), new float3(1f, 1f, 1f), new float3(0f, 1f, 1f));

            SharedData.CubeFaces.Data = new FixedArray<int>(24);
            SharedData.CubeFaces.Data.CopyFrom(/* right */1, 2, 5, 6, /* left */ 0, 3, 4, 7, /* top */ 4, 5, 7, 6, /* bottom */ 0, 1, 3, 2, /* front */ 3, 2, 7, 6, /* back */ 0, 1, 4, 5);

            SharedData.CubeUVs.Data = new FixedArray<float2>(4);
            SharedData.CubeUVs.Data.CopyFrom(new float2(0f, 0f), new float2(1f, 0f), new float2(0f, 1f), new float2(1f, 1f));

            SharedData.CubeIndices.Data = new FixedArray<int>(36);
            SharedData.CubeIndices.Data.CopyFrom(/* right */ 0, 3, 1, 0, 2, 3, /* left */ 1, 3, 0, 3, 2, 0, /* top */ 0, 3, 1, 0, 2, 3, /* bottom */ 1, 3, 0, 3, 2, 0, /* front */ 1, 3, 0, 3, 2, 0, /* back */ 0, 3, 1, 0, 2, 3);

            SharedData.CubeFlippedIndices.Data = new FixedArray<int>(36);
            SharedData.CubeFlippedIndices.Data.CopyFrom(/* right */ 0, 2, 1, 1, 2, 3, /* left */ 1, 2, 0, 3, 2, 1, /* top */ 0, 2, 1, 1, 2, 3, /* bottom */ 1, 2, 0, 3, 2, 1, /* front */ 1, 2, 0, 3, 2, 1, /* back */ 0, 2, 1, 1, 2, 3);

            SharedData.CubeCrossIndices.Data = new FixedArray<int>(12);
            SharedData.CubeCrossIndices.Data.CopyFrom(4, 3, 1, 4, 6, 3, 0, 7, 5, 0, 2, 7);

            SharedData.AONeighborOffsets.Data = new FixedArray<int>(12);
            SharedData.AONeighborOffsets.Data.CopyFrom(0, 1, 2, 6, 7, 0, 2, 3, 4, 4, 5, 6);
        }
    }
}