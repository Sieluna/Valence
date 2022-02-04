using System;
using Environment.Data;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using Utilities;

namespace Environment.System
{
    public class BuildBlockSystem
    {
        private string m_BlockPath;
        
        public BuildBlockSystem(string path) => m_BlockPath = path;

        public void Init()
        {
            SharedData.BlockData.Data = new FixedArray<long>(Enum.GetValues(typeof(BlockType)).Length);
            Array.ForEach(Resources.LoadAll<BlockPrefab>(m_BlockPath), prefab =>
            {
                var data = 0L;
                data |= (UnsafeUtility.EnumToInt(prefab.shape) & 0xFFL) << 56; // blockTypeBuffer
                data |= (255 & 0xFFL) << 48; // empty byte
                for (int i = 0, currentBit = 40; i < 6; i++, currentBit -= 8)
                    data |= (prefab.atlasPositions[i].PackUVCoord() & 0xFFL) << currentBit;

                SharedData.BlockData.Data[UnsafeUtility.EnumToInt(prefab.block)] = data;
            });
        }

        public void Refresh() { return; }
    }
}