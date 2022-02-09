using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Valence.Environment
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Block
    {
        [FieldOffset(0)] public byte Id;

        [FieldOffset(1)] public Color32 Color;

        public static Block Empty => new Block
        {
            Id = 0,
            Color = new Color32(0, 0, 0, 0)
        };
    }

    public struct BlockData
    {
        public long AtlasUvs;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int PackUVCoord(int2 uv) => (uv.x & 0xF) << 4 | ((uv.y & 0xF) << 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2 UnpackUVCoord(int uvs) => new int2((uvs >> 4) & 0xF, (uvs >> 0) & 0xF);

        public static long PackAtlasPositions(int2[] atlasPositions, int direction)
        {
            long packedValue = 0;
            for (var i = 0; i < atlasPositions.Length; i++)
            {
                var packedCoord = PackUVCoord(atlasPositions[i]);
                packedValue |= ((long)packedCoord << (10 * ((direction == 0) ? i : 5 - i)));
            }
            return packedValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int2[] UnpackAtlasUv(long data, int direction)
        {
            var atlasPositions = new int2[6];
            for (var i = 0; i < 6; i++)
            {
                var packedCoord = (int)((data >> (10 * i)) & 0x3FF);
                atlasPositions[i] = UnpackUVCoord(packedCoord);
            }
            return atlasPositions;
        }
    }

    public partial class SharedData
    {
        public static readonly SharedStatic<FixedArray<BlockData>> BlockData =
            SharedStatic<FixedArray<BlockData>>.GetOrCreate<SharedData, BlockDataKey>();

        private class BlockDataKey {}
    }
}