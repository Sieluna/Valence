using System;
using System.Runtime.CompilerServices;
using Environment.System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Utilities;

namespace Environment.Data
{
    public class BlockData : MonoSingleton<BlockData>
    {
        private NativeArray<long> m_NativeBlockData;

        public NativeArray<long> Data => m_NativeBlockData;
        
        private void Awake()
        {
            m_NativeBlockData = new NativeArray<long>(Enum.GetValues(typeof(BlockType)).Length, Allocator.Persistent);
            
            Array.ForEach(Resources.LoadAll<BlockPrefab>("Blocks"), prefab =>
            {
                var data = 0L;
                data |= (UnsafeUtility.EnumToInt(prefab.shape) & 0xFFL) << 56; // blockTypeBuffer
                data |= (255 & 0xFFL) << 48; // empty byte
                for (int i = 0, currentBit = 40; i < 6; i++, currentBit -= 8)
                    data |= (prefab.atlasPositions[i].PackUVCoord() & 0xFFL) << currentBit;

                m_NativeBlockData[(byte) prefab.block] = data;
            });
        }

        private void OnDestroy() => m_NativeBlockData.Dispose();
    }
    
    public static class BlockDataStatic
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte PackUVCoord(this int2 uv) => (byte) ((uv.x & 0xF) << 4 | ((uv.y & 0xF) << 0));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 UnpackUVCoord(this byte uvs) => new int2((uvs >> 4) & 0xF, (uvs >> 0) & 0xF);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 GetBlockUV(this long data, int direction) => ((byte) ((data >> (5 - direction) * 8) & 0xFFL)).UnpackUVCoord();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetBlockShape(this long data) => (data >> 56) & 0xFFL;
    }
}