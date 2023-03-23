using System;
using Environment.Data;
using Environment.Interface;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using Utilities;

namespace Environment.System
{
    public class BuildBlockSystem : ISharedSystem
    {
        private readonly BlockPrefab[] m_blockPath;
        
        public BuildBlockSystem()
        { 
            m_blockPath = Resources.LoadAll<BlockPrefab>("Blocks");
        }

        public void Init()
        {
            SharedData.BlockData.Data = new FixedArray<long>(Enum.GetValues(typeof(BlockType)).Length);
            Array.ForEach(m_blockPath, prefab =>
            {
                var data = 0L;
                // Block shape buffer [0 - 256]
                data |= (UnsafeUtility.EnumToInt(prefab.shape) & 0xFFL) << 56;
                // Block hardness buffer [0 - 256]
                data |= (Mathf.RoundToInt(prefab.hardness * 16) & 0xFFL) << 48;
                // Block atlas [0 - 16] * 6
                for (int i = 0, currentBit = 40; i < 6; i++, currentBit -= 8)
                    data |= (prefab.atlasPositions[i].PackUVCoord() & 0xFFL) << currentBit;

                SharedData.BlockData.Data[UnsafeUtility.EnumToInt(prefab.block)] = data;
            });
        }

        public void Refresh() { }
    }
}