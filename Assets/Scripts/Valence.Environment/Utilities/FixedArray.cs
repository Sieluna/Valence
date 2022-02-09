using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Valence.Environment
{
    [StructLayout(LayoutKind.Sequential)]
    [BurstCompatible(GenericTypeArguments = new[] { typeof(int) })]
    public unsafe struct FixedArray<T> where T : unmanaged
    {
        [NativeDisableUnsafePtrRestriction]
        private void* m_Buffer;

        private int m_Length;
        private Allocator m_Allocator;

        public T this[int index]
        {
            get
            {
                CheckIndexInRange(index, m_Length);
                return UnsafeUtility.ReadArrayElement<T>(m_Buffer, index);
            }
            set
            {
                CheckIndexInRange(index, m_Length);
                UnsafeUtility.WriteArrayElement(m_Buffer, index, value);
            }
        }

        public FixedArray(int length, Allocator allocator = Allocator.Persistent)
        {
            m_Buffer = UnsafeUtility.Malloc(UnsafeUtility.SizeOf<T>() * (long)length, UnsafeUtility.AlignOf<T>(), allocator);
            m_Length = length;
            m_Allocator = allocator;
        }

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Length;
        }

        public void CopyFrom(params T[] array) => Copy(array, this);

        public T[] ToArray()
        {
            var dst = new T[m_Length];
            Copy(this, dst);
            return dst;
        }

        public void Dispose()
        {
            if (m_Allocator > Allocator.None)
            {
                UnsafeUtility.Free(m_Buffer, m_Allocator);
                m_Allocator = Allocator.Invalid;
            }
            m_Buffer = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Copy(T[] src, FixedArray<T> dst)
        {
            var gcHandle = GCHandle.Alloc(src, GCHandleType.Pinned);
            UnsafeUtility.MemCpy(dst.m_Buffer, (void*)gcHandle.AddrOfPinnedObject(), src.Length * UnsafeUtility.SizeOf<T>());
            gcHandle.Free();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Copy(FixedArray<T> src, T[] dst)
        {
            var gcHandle = GCHandle.Alloc(dst, GCHandleType.Pinned);
            UnsafeUtility.MemCpy((void*)gcHandle.AddrOfPinnedObject(), src.m_Buffer, src.Length * UnsafeUtility.SizeOf<T>());
            gcHandle.Free();
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private static void CheckIndexInRange(int index, int length)
        {
            if (index < 0)
                throw new IndexOutOfRangeException($"Index {index} must be positive.");

            if (index >= length)
                throw new IndexOutOfRangeException($"Index {index} is out of range in container of '{length}' Length.");
        }
    }
}