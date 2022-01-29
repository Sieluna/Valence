using Environment.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Utilities;

namespace Environment.System
{
    [BurstCompile]
    public struct BuildMeshSystem : IJob
    {
        [ReadOnly] public NativeArray<Block> blocks;
        [ReadOnly] public NativeArray<long> blockData;
        [ReadOnly] public int3 chunkSize;
        [ReadOnly] public NativeArray<BlockLight> lightData;
    
        [WriteOnly] [NativeDisableParallelForRestriction] public NativeArray<float3> vertices;
        [WriteOnly] [NativeDisableParallelForRestriction] public NativeArray<float3> normals;
        [WriteOnly] [NativeDisableParallelForRestriction] public NativeArray<float4> uvs;
        [WriteOnly] [NativeDisableParallelForRestriction] public NativeArray<Color> colors;
        [WriteOnly] [NativeDisableParallelForRestriction] public NativeList<int> indices;
        [WriteOnly] [NativeDisableParallelForRestriction] public NativeList<int> subIndices;
        [WriteOnly] [NativeDisableParallelForRestriction] public NativeList<int> morIndices;
    
        [WriteOnly] public NativeCounter counter;
    
        private struct Empty { }
    
        /* ---------------------------------------------------------------------------------------- *
         * 0/1. daX = 2 -> z   , daY = 1 -> y   , daZ = 0 -> x                                      *
         *   => x = chunkSize.z, y = chunkSize.y, d = chunkSize.x => gridPosition = (depth, y, x)   *
         *                                                                                          *
         * 2/3. daX = 0        , daY = 2        , daZ = 1                                           *
         *   => x = chunkSize.x, y = chunkSize.z, d = chunkSize.y => gridPosition = (x, depth, y)   *
         *                                                                                          *
         * 4/5. daX = 0        , daY = 1        , daZ = 2                                           *
         *   => x = chunkSize.x, y = chunkSize.y, d = chunkSize.z => gridPosition = (x, y, depth)   *
         * ---------------------------------------------------------------------------------------- */
        public void Execute()
        {
            for (int direction = 0; direction < 6; direction++)
            {
                var hashMap = new NativeHashMap<int3, Empty>(chunkSize[Shared.DirectionAlignedX[direction]] * chunkSize[Shared.DirectionAlignedY[direction]], Allocator.Temp);
                for (int depth = 0; depth < chunkSize[Shared.DirectionAlignedZ[direction]]; depth++)
                {
                    for (int x = 0; x < chunkSize[Shared.DirectionAlignedX[direction]]; x++)
                    {
                        for (int y = 0; y < chunkSize[Shared.DirectionAlignedY[direction]];)
                        {
                            int3 gridPosition = new int3
                            {
                                [Shared.DirectionAlignedX[direction]] = x,
                                [Shared.DirectionAlignedY[direction]] = y,
                                [Shared.DirectionAlignedZ[direction]] = depth
                            };
    
                            var block = blocks[gridPosition.To1DIndex(chunkSize)];
                            var shape = blockData[UnsafeUtility.EnumToInt(block.type)].GetBlockShape();
                            
                            #region Step1: Ignore Air block, calculated blocks and transparency blocks
                            // if no block at the position, skip it.
                            if (shape == (long) BlockShape.Empty) { y++; continue; }
                            // if already registered, skip it.
                            if (hashMap.ContainsKey(gridPosition)) { y++; continue; }
    
                            int3 neighborPosition = gridPosition + Shared.CubeDirectionOffsets[direction];
    
                            if (TransparencyCheck(blocks, blockData, neighborPosition, chunkSize)) { y++; continue; }
    
                            if (shape == (long) BlockShape.Liquid)
                            {
                                if (direction != 2) { y++; continue; }
                                if (blocks[neighborPosition.To1DIndex(chunkSize)].type == BlockType.Water) { y++; continue; }
                            }
    
                            if (shape == (long) BlockShape.Foliage)
                            {
                                if (direction != 2) { y++; continue; }
                            }
    
                            #endregion
    
                            #region Step2: Get the calculated light data
                            
                            // Get the light data results
                            var light = lightData[gridPosition.To1DIndex(chunkSize)];
                            
                            #endregion
    
                            hashMap.TryAdd(gridPosition, new Empty());
    
                            #region Step3: Mesh combine in xy direction
    
                            int height = 1, width = 1;
                            if (shape != UnsafeUtility.EnumToInt(BlockShape.Leaves))
                            {
                                for (height = 1; height + y < chunkSize[Shared.DirectionAlignedY[direction]]; height++)
                                {
                                    int3 nextPosition = gridPosition;
                                    nextPosition[Shared.DirectionAlignedY[direction]] += height;
    
                                    var nextBlock = blocks[nextPosition.To1DIndex(chunkSize)];
                                    var nextLight = lightData[nextPosition.To1DIndex(chunkSize)];
                                    
                                    if (nextBlock.type != block.type) break;
                                    if (!nextLight.CompareFace(light, direction)) break;
                                    if (hashMap.ContainsKey(nextPosition)) break;
    
                                    hashMap.TryAdd(nextPosition, new Empty());
                                }
                                
                                bool isDone = false;
                                
                                for (width = 1; width + x < chunkSize[Shared.DirectionAlignedX[direction]]; width++)
                                {
                                    for (int dy = 0; dy < height; dy++)
                                    {
                                        int3 nextPosition = gridPosition;
                                        nextPosition[Shared.DirectionAlignedX[direction]] += width;
                                        nextPosition[Shared.DirectionAlignedY[direction]] += dy;
    
                                        var nextBlock = blocks[nextPosition.To1DIndex(chunkSize)];
                                        var nextLight = lightData[nextPosition.To1DIndex(chunkSize)];
    
                                        if (nextBlock.type != block.type || hashMap.ContainsKey(nextPosition) || !nextLight.CompareFace(light, direction))
                                        {
                                            isDone = true;
                                            break;
                                        }
                                    }
    
                                    if (isDone) break;
    
                                    for (int dy = 0; dy < height; dy++)
                                    {
                                        int3 nextPosition = gridPosition;
                                        nextPosition[Shared.DirectionAlignedX[direction]] += width;
                                        nextPosition[Shared.DirectionAlignedY[direction]] += dy;
                                        hashMap.TryAdd(nextPosition, new Empty());
                                    }
                                }
                            }
    
                            #endregion
    
                            switch (shape)
                            {
                                case (long) BlockShape.Liquid:
                                    AddQuad(width, height, gridPosition, counter.Increment(),
                                        ref vertices, ref normals, ref uvs, ref colors, ref subIndices);
                                    break;
                                case (long) BlockShape.Foliage:
                                    AddCrossShape(gridPosition, counter.Increment(2), 1,
                                        ref vertices, ref normals, ref uvs, ref colors, ref morIndices);
                                    break;
                                case (long) BlockShape.Block:
                                    AddQuadByDirection(direction, blockData, block.type, light,
                                        width, height, gridPosition, counter.Increment(),
                                        ref vertices, ref normals, ref uvs, ref colors, ref indices);
                                    break;
                                case (long) BlockShape.Leaves:
                                    AddQuadByDirection(direction, blockData, block.type, light,
                                        width, height, gridPosition, counter.Increment(),
                                        ref vertices, ref normals, ref uvs, ref colors, ref indices);
                                    break;
                            }
                            
                            y += height;
                        }
                    }
                    hashMap.Clear();
                }
                hashMap.Dispose();
            }
        }
        
        #region Private Function (move outside?)

        private static bool TransparencyCheck(in NativeArray<Block> blocks, in NativeArray<long> blockData, int3 position, int3 chunkSize)
        {
            if (!position.BoundaryCheck(chunkSize)) return false;
            var neighborBlock = blockData[(byte) blocks[position.To1DIndex(chunkSize)].type].GetBlockShape();
            return neighborBlock is not ((long) BlockShape.Empty or (long) BlockShape.Foliage or (long) BlockShape.Liquid);
        }
    
        private static void AddCrossShape(int3 gridPosition, int numFace, int rand, 
            ref NativeArray<float3> vertices, ref NativeArray<float3> normals, ref NativeArray<float4> uvs, ref NativeArray<Color> colors, ref NativeList<int> indices)
        {
            int numVertices = numFace * 4;
            for (int i = 0; i < 8; i++)
            {
                /*      7 ------- 6
                 *     /|        /|
                 *    / |       / |
                 *   4 ------- 5  |
                 *   |  3 -----|- 2
                 *   | /       | /
                 *   |/        |/
                 *   0 ------- 1
                 *            1 -> 2 -> 5 -> 6 -> 0 -> 3 -> 4 -> 7
                 *   We want  0 -> 6 -> 2 -> 0 -> 4 -> 6 -> 1 -> 7 -> 3 -> 1 -> 5 -> 7
                 *   so       4 -> 3 -> 1 -> 4 -> 6 -> 3 -> 0 -> 7 -> 5 -> 0 -> 2 -> 7
                 */
    
                float3 vertex = Shared.CubeVertices[Shared.CubeFaces[i]];
    
                colors[numVertices + i] = new Color(0, 0, 0, 1);
                vertices[numVertices + i] = vertex + gridPosition + new float3(0, -0.04f, 0);
                normals[numVertices + i] = (i < 4) ? new float3(0.7f, 0, 0.7f) : new float3(-0.7f, 0, 0.7f);
                uvs[numVertices + i] = new float4
                {
                    x = Shared.CubeUVs[i % 4].x * 0.25f * rand,
                    y = 1 - (Shared.CubeUVs[i % 4].y * 0.25f * (rand % 4)),
                    z = 0.25f * (rand - 1),
                    w = 0.25f * ((rand % 4) - 1),
                };
            }
    
            for (int i = 0, iMax = Shared.CubeCrossIndices.Length; i < iMax; i++)
                indices.AddNoResize(numVertices + Shared.CubeCrossIndices[i]);
        }
    
        private static void AddQuad(float width, float height, int3 gridPosition, int numFace,
            ref NativeArray<float3> vertices, ref NativeArray<float3> normals, ref NativeArray<float4> uvs, ref NativeArray<Color> colors, ref NativeList<int> indices)
        {
            int numVertices = numFace * 4;
            for (int i = 0; i < 4; i++)
            {
                // 6(0,1,1) --> 7(1,1,1)
                //    |             |
                // 4(0,1,0) --> 5(1,1,0)
                float3 vertex = Shared.CubeVertices[Shared.CubeFaces[i + 2 * 4]];
    
                vertex[0] *= width;
                vertex[2] *= height;
    
                colors[numVertices + i] = new Color(0, 0, 0, 1);
                vertices[numVertices + i] = vertex + gridPosition + new float3(0, -0.13f, 0);
                normals[numVertices + i] = new float3(0, 1, 0);
                uvs[numVertices + i] = new float4(1, 1, 0, 0);
            }
    
            for (int i = 0; i < 6; i++)
                indices.AddNoResize(Shared.CubeFlippedIndices[12 + i] + numVertices);
        }
    
        private static unsafe void AddQuadByDirection(int direction, in NativeArray<long> blockData, BlockType type, in BlockLight blockLight,
            float width, float height, int3 gridPosition, int numFace, 
            ref NativeArray<float3> vertices, ref NativeArray<float3> normals, ref NativeArray<float4> uvs, ref NativeArray<Color> colors, ref NativeList<int> indices)
        {
            int numVertices = numFace * 4;
            for (int i = 0; i < 4; i++)
            {
                float3 vertex = Shared.CubeVertices[Shared.CubeFaces[i + direction /*0-5*/ * 4]];
    
                vertex[Shared.DirectionAlignedX[direction]] *= width;
                vertex[Shared.DirectionAlignedY[direction]] *= height;
    
                colors[numVertices + i] = new Color(0, 0, 0, blockLight.ambient[i + direction * 4]);
                vertices[numVertices + i] = vertex + gridPosition;
                normals[numVertices + i] = Shared.CubeDirectionOffsets[direction];
                uvs[numVertices + i] = new float4
                {
                    xy = new float2(Shared.CubeUVs[i].x * width, Shared.CubeUVs[i].y * height),
                    zw = blockData[(int) type].GetBlockUV(direction)
                };
            }
    
            for (int i = 0; i < 6; i++)
            {
                if (blockLight.ambient[direction * 4] + blockLight.ambient[direction * 4 + 3] < blockLight.ambient[direction * 4 + 1] + blockLight.ambient[direction * 4 + 2])
                    indices.AddNoResize(Shared.CubeFlippedIndices[direction * 6 + i] + numVertices);
                else
                    indices.AddNoResize(Shared.CubeIndices[direction * 6 + i] + numVertices);
            }
    
        }

        #endregion
    }
}