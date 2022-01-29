using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Utilities
{
    [StructLayout(LayoutKind.Sequential)]
    [NativeContainer]
    public unsafe struct NativeCounter
    {
        [NativeDisableUnsafePtrRestriction] private int* m_Counter;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        private AtomicSafetyHandle m_Safety;
        [NativeSetClassTypeToNullOnSchedule] private DisposeSentinel m_DisposeSentinel;
#endif

        private Allocator m_AllocatorLabel;

        public NativeCounter(Allocator label)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            if (!UnsafeUtility.IsBlittable<int>())
                throw new ArgumentException(string.Format("{0} used in NativeQueue<{0}> must be blittable", typeof(int)));
#endif
            m_AllocatorLabel = label;
            m_Counter = (int*)UnsafeUtility.Malloc(UnsafeUtility.SizeOf<int>(), 4, label);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, 0, label);
#endif
            Count = 0;
        }

        public int Increment(int number = 1)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
            (*m_Counter) += number;
            return *m_Counter - number;
        }

        public int Count
        {
            get
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckReadAndThrow(m_Safety);
#endif
                return *m_Counter;
            }
            set
            {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                AtomicSafetyHandle.CheckWriteAndThrow(m_Safety);
#endif
                *m_Counter = value;
            }
        }

        public bool IsCreated => m_Counter != null;

        public void Dispose()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Dispose(ref m_Safety, ref m_DisposeSentinel);
#endif

            UnsafeUtility.Free(m_Counter, m_AllocatorLabel);
            m_Counter = null;
        }
    }
}