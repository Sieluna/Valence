using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace Utilities
{
    [DebuggerTypeProxy(typeof(FixedArrayDebugView<>))]
    [BurstCompatible(GenericTypeArguments = new[] { typeof(int), typeof(int3), typeof(long), typeof(float2), typeof(float3) })]
    public unsafe struct FixedArray<T> where T : unmanaged
    {
        private void* m_buffer;
        private int m_length;
        private Allocator m_allocator;

        public T this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get => UnsafeUtility.ReadArrayElement<T>(m_buffer, index);
            [MethodImpl(MethodImplOptions.AggressiveInlining)] set => UnsafeUtility.WriteArrayElement(m_buffer, index, value);
        }

        public FixedArray(int length, Allocator allocator = Allocator.Persistent)
        {
            m_buffer = UnsafeUtility.Malloc(UnsafeUtility.SizeOf<T>() * (long)length, UnsafeUtility.AlignOf<T>(), allocator);
            m_length = length;
            m_allocator = allocator;
        }

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)] get => m_length;
        }

        public void CopyFrom(params T[] array) => Copy(array, this);

        private static void Copy(T[] src, FixedArray<T> dst)
        {
            var gcHandle = GCHandle.Alloc(src, GCHandleType.Pinned);
            UnsafeUtility.MemCpy(dst.m_buffer, (void*)gcHandle.AddrOfPinnedObject(), src.Length * UnsafeUtility.SizeOf<T>());
            gcHandle.Free();
        }

        private static void Copy(FixedArray<T> src, T[] dst)
        {
            var gcHandle = GCHandle.Alloc(dst, GCHandleType.Pinned);
            UnsafeUtility.MemCpy((void*)gcHandle.AddrOfPinnedObject(), src.m_buffer, src.Length * UnsafeUtility.SizeOf<T>());
            gcHandle.Free();
        }

        public T[] ToArray()
        {
            var dst = new T[Length];
            Copy(this, dst);
            return dst;
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

    sealed class FixedArrayDebugView<T> where T : unmanaged
    {
        private FixedArray<T> m_array;

        public FixedArrayDebugView(FixedArray<T> array)
        {
            m_array = array;
        }

        public T[] Items => m_array.ToArray();
    }
}