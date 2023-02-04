using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Utilities
{
    [BurstCompatible(GenericTypeArguments = new[] { typeof(int), typeof(int3), typeof(long), typeof(float2), typeof(float3) })]
    public unsafe struct FixedArray<T> where T : unmanaged
    {
        private void* m_buffer;

        private Allocator m_allocator;

        public T this[int index]
        {
            get => UnsafeUtility.ReadArrayElement<T>(m_buffer, index);
            set => UnsafeUtility.WriteArrayElement(m_buffer, index, value);
        }

        public FixedArray(int length, Allocator allocator = Allocator.Persistent)
        {
            m_buffer = UnsafeUtility.Malloc(UnsafeUtility.SizeOf<T>() * (long) length, UnsafeUtility.AlignOf<T>(), allocator);
            m_allocator = allocator;
        }
        
        public void CopyFrom(params T[] array) => Copy(array, this);

        private static void Copy(T[] src, FixedArray<T> dst)
        {
            var gcHandle = GCHandle.Alloc(src, GCHandleType.Pinned);
            UnsafeUtility.MemCpy(dst.m_buffer, (void*) gcHandle.AddrOfPinnedObject(), src.Length * UnsafeUtility.SizeOf<T>());
            gcHandle.Free();
        }

        public void Dispose()
        {
            if (m_allocator > Allocator.None)
            {
                UnsafeUtility.Free(m_buffer, m_allocator);
                m_allocator = Allocator.Invalid;
            }

            m_buffer = null;
        }
    }
}