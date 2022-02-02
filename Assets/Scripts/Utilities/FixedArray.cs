using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Utilities
{
    [BurstCompatible(GenericTypeArguments = new[] { typeof(int), typeof(int3), typeof(long), typeof(float2), typeof(float3) })]
    public unsafe struct FixedArray<T> where T : unmanaged
    {
        public IntPtr Ptr;

        public T this[int i]
        {
            get => *((T*) Ptr + i);
            set => *((T*)Ptr + i) = value;
        }

        public FixedArray(int length)
            => Ptr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(T)) * length);

        public void CopyFrom(params T[] array) => Copy(array, this);

        private static void Copy(T[] src, FixedArray<T> dst)
        {
            var gcHandle = GCHandle.Alloc(src, GCHandleType.Pinned);
            UnsafeUtility.MemCpy((void*) dst.Ptr, (void*) gcHandle.AddrOfPinnedObject(), src.Length * UnsafeUtility.SizeOf<T>());
            gcHandle.Free();
        }

        //public void Dispose() => Marshal.FreeHGlobal(Ptr);
    }
}