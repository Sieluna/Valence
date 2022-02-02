using Environment.System;
using Unity.Burst;
using Unity.Mathematics;
using Utilities;

namespace Environment.Data
{
    public class SharedData
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

        public void Generate()
        {
            var system = new BuildSharedSystem();
            system.InitSharedStatic("Blocks");
        }
    }
}