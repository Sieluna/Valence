using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

public static class Shared
{
    #region Constants

    public static readonly int2 AtlasSize = new int2(16, 16);

    /// <summary> x轴对齐 { 2, 2, 0, 0, 0, 0 } </summary>
    public static readonly int[] DirectionAlignedX = { 2, 2, 0, 0, 0, 0 };
        
    /// <summary> y轴对齐 { 1, 1, 2, 2, 1, 1 } </summary>
    public static readonly int[] DirectionAlignedY = { 1, 1, 2, 2, 1, 1 };
        
    /// <summary> y轴对齐 { 1, 1, 2, 2, 1, 1 } </summary>
    public static readonly int[] DirectionAlignedZ = { 0, 0, 1, 1, 2, 2 };
        
    public static readonly int[] DirectionAlignedSign = { 1, -1, 1, -1, 1, -1 };

    /// <summary> direction向量（指认方向）0 -> right, 1 -> left, 2 -> top, 3 -> bottom, 4 -> front, 5 -> back </summary>
    public static readonly int3[] CubeDirectionOffsets =
    {
        new int3( 1,  0,  0), // right
        new int3(-1,  0,  0), // left
        new int3( 0,  1,  0), // top
        new int3( 0, -1,  0), // bottom
        new int3( 0,  0,  1), // front
        new int3( 0,  0, -1), // back
    };

    /*      7 ------- 6
    *     /|        /|
    *    / |       / |
    *   4 ------- 5  |
    *   |  3 -----|- 2
    *   | /       | /
    *   |/        |/
    *   0 ------- 1
    */
    public static readonly float3[] CubeVertices =
    {
        new float3(0f, 0f, 0f), // 0
        new float3(1f, 0f, 0f), // 1
        new float3(1f, 0f, 1f), // 2
        new float3(0f, 0f, 1f), // 3
        new float3(0f, 1f, 0f), // 4
        new float3(1f, 1f, 0f), // 5
        new float3(1f, 1f, 1f), // 6
        new float3(0f, 1f, 1f)  // 7
    };

    /// <summary>
    /// [Right 0 - 3] 1,2,5,6 [Left 4-7] 0,3,4,7 [Top 8-11] 4,5,7,6 [Down 12-15] 0,1,3,2 [Front 16-19] 3,2,7,5 [Back 20-24] 0,1,4,5
    /// </summary>
    public static readonly int[] CubeFaces =
    {
        1, 2, 5, 6, // right
        0, 3, 4, 7, // left
        4, 5, 7, 6, // top
        0, 1, 3, 2, // bottom
        3, 2, 7, 6, // front
        0, 1, 4, 5, // back
    };

    /// <summary>
    /// UV Mapping (0, 0), (1, 0), (0, 1), (1, 1)
    /// </summary>
    public static readonly float2[] CubeUVs =
    {
        new float2(0f, 0f),
        new float2(1f, 0f),
        new float2(0f, 1f),
        new float2(1f, 1f)
    };

    //  2---3
    //  | / |
    //  0---1
    public static readonly byte[] CubeIndices =
    {
        0, 3, 1,
        0, 2, 3, //face right
        1, 3, 0,
        3, 2, 0, //face left
        0, 3, 1,
        0, 2, 3, //face top
        1, 3, 0,
        3, 2, 0, //face bottom
        1, 3, 0,
        3, 2, 0, //face front
        0, 3, 1,
        0, 2, 3, //face back
    };
        
    //  2---3
    //  | \ |
    //  0---1
    public static readonly byte[] CubeFlippedIndices =
    {
        0, 2, 1,
        1, 2, 3, //face right
        1, 2, 0,
        3, 2, 1, //face left
        0, 2, 1,
        1, 2, 3, //face top
        1, 2, 0,
        3, 2, 1, //face bottom
        1, 2, 0,
        3, 2, 1, //face front
        0, 2, 1,
        1, 2, 3, //face back
    };

    public static readonly byte[] CubeCrossIndices =
    {
        4, 3, 1,
        4, 6, 3,
        0, 7, 5,
        0, 2, 7,
    };
        
    public static readonly byte[] AONeighborOffsets =
    {
        0, 1, 2,
        6, 7, 0,
        2, 3, 4,
        4, 5, 6,
    };

    #endregion

    #region Functions
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int2 To2DIndex(this int index, int size)
        => new(index / size, index % size);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int To1DIndex(this int2 index, int size)
        => index.x * size + index.y;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int3 To3DIndex(this int index, int3 size)
        => new(index / (size.y * size.z), (index / size.z) % size.y, index % size.z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int To1DIndex(this int3 index, int3 size)
        => index.z + index.y * size.z + index.x * size.y * size.z;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int3 ToChunk(this int3 worldGridPosition, int3 chunkSize)
        => Floor((float3) worldGridPosition / chunkSize);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int3 ToChunk(this Vector3 worldGridPosition, int3 chunkSize)
        => Floor((float3) worldGridPosition / chunkSize);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float3 ToWorld(this int3 chunkPosition, int3 chunkSize)
        => chunkPosition * chunkSize;
        
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int3 ToGrid(this Vector3 worldPosition, int3 chunkPosition, int3 chunkSize)
        => ToGrid(Floor(worldPosition), chunkPosition, chunkSize);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int3 ToGrid(this int3 worldGridPosition, int3 chunkPosition, int3 chunkSize)
        => Mod(worldGridPosition - chunkPosition * chunkSize, chunkSize);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool BoundaryCheck(this int3 position, int3 chunkSize)
        => chunkSize.x > position.x && chunkSize.y > position.y && chunkSize.z > position.z &&
           position.x >= 0 && position.y >= 0 && position.z >= 0;
        
    public static int InvertDirection(int direction)
    {
        int axis = direction / 2; // 0(+x,-x), 1(+y,-y), 2(+z,-z)
        int invDirection = Mathf.Abs(direction - (axis * 2 + 1)) + (axis * 2);

        /*
                direction    x0    abs(x0)    abs(x) + axis * 2 => invDirection
                0            -1    1          1  
                1            0     0          0
                2            -1    1          3
                3            0     0          2
                4            -1    1          5
                5            0     0          4
             */

        return invDirection;
    }
        
    private static int3 Mod(int3 v, int3 m)
    {
        var r = (int3) math.fmod(v, m);
        return math.select(r, r + m, r < 0);
    }

    private static int3 Floor(float3 v)
    {
        var vi = (int3) v;
        return math.select(vi, vi - 1, v < vi);
    }

    #endregion
}