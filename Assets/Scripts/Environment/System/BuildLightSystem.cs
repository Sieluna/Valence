using Environment.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Environment.System
{
    [BurstCompile(DisableSafetyChecks = true, OptimizeFor = OptimizeFor.Performance)]
    public struct BuildLightSystem : IJobParallelFor
    {
        [ReadOnly] public NativeArray<long> blockData;
        [ReadOnly] public NativeArray<Block> blocksWithNeighbor;
        [ReadOnly] public int3 chunkSize;
        [ReadOnly] public int3 chunkPosition;
        [ReadOnly] public NativeHashMap<int3, int> neighborHashMap;

        [WriteOnly] public NativeArray<BlockLight> lightDatas;

        public unsafe void Execute(int index)
        {
            var gridPosition = index.To3DIndex(chunkSize);

            var blocks = blocksWithNeighbor.Slice(neighborHashMap[chunkPosition] * chunkSize.x * chunkSize.y * chunkSize.z, chunkSize.x * chunkSize.y * chunkSize.z);

            var block = blocks[index];

            var blockLight = new BlockLight();

            if (block.type == BlockType.Air) { lightDatas[index] = blockLight; return; }

            for (int direction = 0; direction < 6; direction++)
            {
                var down = gridPosition;
                var left = gridPosition;
                var top = gridPosition;
                var right = gridPosition;

                var leftDownCorner = gridPosition;
                var topLeftCorner = gridPosition;
                var topRightCorner = gridPosition;
                var rightDownCorner = gridPosition;

                down[Shared.DirectionAlignedY[direction]] -= 1;
                left[Shared.DirectionAlignedX[direction]] -= 1;
                top[Shared.DirectionAlignedY[direction]] += 1;
                right[Shared.DirectionAlignedX[direction]] += 1;

                leftDownCorner[Shared.DirectionAlignedX[direction]] -= 1;
                leftDownCorner[Shared.DirectionAlignedY[direction]] -= 1;

                topLeftCorner[Shared.DirectionAlignedX[direction]] -= 1;
                topLeftCorner[Shared.DirectionAlignedY[direction]] += 1;

                topRightCorner[Shared.DirectionAlignedX[direction]] += 1;
                topRightCorner[Shared.DirectionAlignedY[direction]] += 1;

                rightDownCorner[Shared.DirectionAlignedX[direction]] += 1;
                rightDownCorner[Shared.DirectionAlignedY[direction]] -= 1;

                int3* neighbors = stackalloc int3[] { down, leftDownCorner, left, topLeftCorner, top, topRightCorner, right, rightDownCorner };

                for (int i = 0; i < 8; i++)
                    neighbors[i][Shared.DirectionAlignedZ[direction]] += Shared.DirectionAlignedSign[direction];

                for (int i = 0; i < 4; i++)
                {
                    var side1 = TransparencyCheck(blocks, blockData, neighbors[Shared.AONeighborOffsets[i * 3]], chunkSize, chunkPosition, ref neighborHashMap, ref blocksWithNeighbor);
                    var corner = TransparencyCheck(blocks, blockData, neighbors[Shared.AONeighborOffsets[i * 3 + 1]], chunkSize, chunkPosition, ref neighborHashMap, ref blocksWithNeighbor);
                    var side2 = TransparencyCheck(blocks, blockData, neighbors[Shared.AONeighborOffsets[i * 3 + 2]], chunkSize, chunkPosition, ref neighborHashMap, ref blocksWithNeighbor);

                    if (side1 && side2)
                        blockLight.ambient[i + direction * 4] = 0f;
                    else
                        blockLight.ambient[i + direction * 4] = ((side1 ? 0f : 1f) + (side2 ? 0f : 1f) + (corner ? 0f : 1f)) / 3.0f;
                }
            }

            lightDatas[index] = blockLight;
        }

        
        private static bool TransparencyCheck(in NativeSlice<Block> blocks, in NativeArray<long> blockData,
            int3 gridPosition, int3 chunkSize, int3 chunkPosition,
            ref NativeHashMap<int3, int> neighborHashMap, ref NativeArray<Block> blocksWithNeighbor)
        {
            if (gridPosition.BoundaryCheck(chunkSize))
            {
                var shape = blockData[UnsafeUtility.EnumToInt(blocks[gridPosition.To1DIndex(chunkSize)].type)].GetBlockShape();
                return shape is not ((long) BlockShape.Empty or (long) BlockShape.Foliage or (long) BlockShape.Liquid);
            }

            var worldGridPosition = gridPosition + chunkPosition * chunkSize;
            var neighborChunkPosition = worldGridPosition.ToChunk(chunkSize);

            if (neighborHashMap.TryGetValue(neighborChunkPosition, out int blockIndex))
            {
                if (blockIndex == -1) return false;

                var position = worldGridPosition.ToGrid(neighborChunkPosition, chunkSize);
                var neighborBlocks = blocksWithNeighbor.Slice(blockIndex * chunkSize.x * chunkSize.y * chunkSize.z, chunkSize.x * chunkSize.y * chunkSize.z);
                var neighborShape = blockData[UnsafeUtility.EnumToInt(neighborBlocks[position.To1DIndex(chunkSize)].type)].GetBlockShape();
                return neighborShape is not ((long) BlockShape.Empty or (long) BlockShape.Foliage or (long) BlockShape.Liquid);
            }

            return false;
        }
    }
}