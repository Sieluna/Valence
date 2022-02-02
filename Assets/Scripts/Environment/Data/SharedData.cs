using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Utilities;

namespace Environment.Data
{
    public class SharedData : MonoSingleton<SharedData>
    {
        public static readonly SharedStatic<FixedArray<long>> BlockData = SharedStatic<FixedArray<long>>.GetOrCreate<SharedData, BD>(); private class BD{}
        public static readonly SharedStatic<FixedArray<int>> DirectionAlignedX = SharedStatic<FixedArray<int>>.GetOrCreate<SharedData, DAX>(); private class DAX{}
        public static readonly SharedStatic<FixedArray<int>> DirectionAlignedY = SharedStatic<FixedArray<int>>.GetOrCreate<SharedData, DAY>(); private class DAY{}
        public static readonly SharedStatic<FixedArray<int>> DirectionAlignedZ = SharedStatic<FixedArray<int>>.GetOrCreate<SharedData, DAZ>(); private class DAZ{}
        public static readonly SharedStatic<FixedArray<int>> DirectionAlignedSign = SharedStatic<FixedArray<int>>.GetOrCreate<SharedData, DAS>(); private class DAS {}
        public static readonly SharedStatic<FixedArray<int3>> CubeDirectionOffsets = SharedStatic<FixedArray<int3>>.GetOrCreate<SharedData, CDO>(); private class CDO {}
        public static readonly SharedStatic<FixedArray<float3>> CubeVertices = SharedStatic<FixedArray<float3>>.GetOrCreate<SharedData, CV>(); private class CV {}
        public static readonly SharedStatic<FixedArray<int>> CubeFaces = SharedStatic<FixedArray<int>>.GetOrCreate<SharedData, CD>(); private class CD {}
        public static readonly SharedStatic<FixedArray<float2>> CubeUVs = SharedStatic<FixedArray<float2>>.GetOrCreate<SharedData, CU>(); private class CU {}
        public static readonly SharedStatic<FixedArray<int>> CubeIndices = SharedStatic<FixedArray<int>>.GetOrCreate<SharedData, CNI>(); private class CNI {}
        public static readonly SharedStatic<FixedArray<int>> CubeFlippedIndices = SharedStatic<FixedArray<int>>.GetOrCreate<SharedData, CFI>(); private class CFI {}
        public static readonly SharedStatic<FixedArray<int>> CubeCrossIndices = SharedStatic<FixedArray<int>>.GetOrCreate<SharedData, CCI>(); private class CCI {}
        public static readonly SharedStatic<FixedArray<int>> AONeighborOffsets = SharedStatic<FixedArray<int>>.GetOrCreate<SharedData, AO>(); private class AO {}

        private void Awake()
        {
            BlockData.Data = new FixedArray<long>(Enum.GetValues(typeof(BlockType)).Length);
            Array.ForEach(Resources.LoadAll<BlockPrefab>("Blocks"), prefab =>
            {
                var data = 0L;
                data |= (UnsafeUtility.EnumToInt(prefab.shape) & 0xFFL) << 56; // blockTypeBuffer
                data |= (255 & 0xFFL) << 48; // empty byte
                for (int i = 0, currentBit = 40; i < 6; i++, currentBit -= 8)
                    data |= (prefab.atlasPositions[i].PackUVCoord() & 0xFFL) << currentBit;

                BlockData.Data[UnsafeUtility.EnumToInt(prefab.block)] = data;
            });
            
            // right, left, top, bottom, front, back
            DirectionAlignedX.Data = new FixedArray<int>(6); 
            DirectionAlignedX.Data.CopyFrom(2, 2, 0, 0, 0, 0);

            DirectionAlignedY.Data = new FixedArray<int>(6);
            DirectionAlignedY.Data.CopyFrom(1, 1, 2, 2, 1, 1);

            DirectionAlignedZ.Data = new FixedArray<int>(6);
            DirectionAlignedZ.Data.CopyFrom(0, 0, 1, 1, 2, 2);

            DirectionAlignedSign.Data = new FixedArray<int>(6);
            DirectionAlignedSign.Data.CopyFrom(1, -1, 1, -1, 1, -1);

            CubeDirectionOffsets.Data = new FixedArray<int3>(6);
            CubeDirectionOffsets.Data.CopyFrom(new int3(1, 0, 0), new int3(-1, 0, 0), new int3(0, 1, 0), new int3(0, -1, 0), new int3(0, 0, 1), new int3(0, 0, -1));

            CubeVertices.Data = new FixedArray<float3>(8);
            CubeVertices.Data.CopyFrom(new float3(0f, 0f, 0f), new float3(1f, 0f, 0f), new float3(1f, 0f, 1f), new float3(0f, 0f, 1f), new float3(0f, 1f, 0f), new float3(1f, 1f, 0f), new float3(1f, 1f, 1f), new float3(0f, 1f, 1f));

            CubeFaces.Data = new FixedArray<int>(24);
            CubeFaces.Data.CopyFrom(/* right */1, 2, 5, 6, /* left */ 0, 3, 4, 7, /* top */ 4, 5, 7, 6, /* bottom */ 0, 1, 3, 2, /* front */ 3, 2, 7, 6, /* back */ 0, 1, 4, 5);

            CubeUVs.Data = new FixedArray<float2>(4);
            CubeUVs.Data.CopyFrom(new float2(0f, 0f), new float2(1f, 0f), new float2(0f, 1f), new float2(1f, 1f));

            CubeIndices.Data = new FixedArray<int>(36);
            CubeIndices.Data.CopyFrom(0, 3, 1, 0, 2, 3, 1, 3, 0, 3, 2, 0, 0, 3, 1, 0, 2, 3, 1, 3, 0, 3, 2, 0, 1, 3, 0, 3, 2, 0, 0, 3, 1, 0, 2, 3);

            CubeFlippedIndices.Data = new FixedArray<int>(36);
            CubeFlippedIndices.Data.CopyFrom(/* right */ 0, 2, 1, 1, 2, 3, /* left */ 1, 2, 0, 3, 2, 1, /* top */ 0, 2, 1, 1, 2, 3, /* bottom */ 1, 2, 0, 3, 2, 1, /* front */ 1, 2, 0, 3, 2, 1, /* back */ 0, 2, 1, 1, 2, 3);

            CubeCrossIndices.Data = new FixedArray<int>(12);
            CubeCrossIndices.Data.CopyFrom(4, 3, 1, 4, 6, 3, 0, 7, 5, 0, 2, 7);

            AONeighborOffsets.Data = new FixedArray<int>(12);
            AONeighborOffsets.Data.CopyFrom(0, 1, 2, 6, 7, 0, 2, 3, 4, 4, 5, 6);
        }
    }
}