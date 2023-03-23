using System;
using System.Runtime.InteropServices;
using System.Threading;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Utilities
{
    [StructLayout(LayoutKind.Sequential)]
    [NativeContainer]
    public unsafe struct NativeCounter
    {
        [NativeDisableUnsafePtrRestriction] private int* m_counter;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        private AtomicSafetyHandle m_Safety;
        [NativeSetClassTypeToNullOnSchedule] private DisposeSentinel m_disposeSentinel;
#endif

        private Allocator m_allocatorLabel;

        public NativeCounter(Allocator label)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (!UnsafeUtility.IsBlittable<int>())
                throw new ArgumentException(string.Format("{0} used in NativeQueue<{0}> must be blittable", typeof(int)));
#endif
            m_allocatorLabel = label;
            m_counter = (int*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<int>(), 4, label);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Create(out m_Safety, out m_disposeSentinel, 0, label);
#endif
            Count = 0;
        }

        public int Increment(int number = 1)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
            (*m_counter) += number;
            return *m_counter - number;
        }

        public int Count
        {
            get
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif
                return *m_counter;
            }
            set
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
                *m_counter = value;
            }
        }

        public bool IsCreated => m_counter != null;

        public void Dispose()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Dispose(ref m_Safety, ref m_disposeSentinel);
#endif

            UnsafeUtility.Free(m_counter, m_allocatorLabel);
            m_counter = null;
        }

        [NativeContainer]
        [NativeContainerIsAtomicWriteOnly]
        public unsafe struct ParallelWriter
        {
            [NativeDisableUnsafePtrRestriction] private int* m_counter;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            private AtomicSafetyHandle m_safety;
#endif
            
            public static implicit operator NativeCounter.ParallelWriter(NativeCounter cnt)
            {
                NativeCounter.ParallelWriter parallelWriter;
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckWriteAndThrow(cnt.m_Safety);
                parallelWriter.m_safety = cnt.m_Safety;
                AtomicSafetyHandle.UseSecondaryVersion(ref parallelWriter.m_safety);
#endif

                parallelWriter.m_counter = cnt.m_counter;
                return parallelWriter;
            }

            public int Increment(int number = 1)
            {
                // Increment still needs to check for write permissions
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckWriteAndThrow(m_safety);
#endif
                // The actual increment is implemented with an atomic, since it can be incremented by multiple threads at the same time
                Interlocked.Add(ref *m_counter, number);
                return *m_counter - number;
            }
        }
    }
}