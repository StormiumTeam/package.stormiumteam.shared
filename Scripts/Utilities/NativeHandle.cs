using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace package.stormiumteam
{
    [StructLayout(LayoutKind.Sequential)]
    [NativeContainer]
    [NativeContainerSupportsDeallocateOnJobCompletion]
    public struct NativeHandle<TObj> : IDisposable
    {
        private GCHandle m_Handle;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        private AtomicSafetyHandle m_Safety;

        [NativeSetClassTypeToNullOnSchedule]
        private DisposeSentinel m_DisposeSentinel;
#endif

        private readonly Allocator m_AllocatorLabel;

        public NativeHandle(TObj obj, Allocator label)
        {
            m_AllocatorLabel = label;
            m_Handle         = GCHandle.Alloc(obj, GCHandleType.Pinned);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Create(out m_Safety, out m_DisposeSentinel, 0, label);
#endif
        }

        public bool IsCreated => m_Handle.AddrOfPinnedObject() != IntPtr.Zero;
        public TObj Object    => (TObj) m_Handle.Target;

        public void Dispose()
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            DisposeSentinel.Dispose(ref m_Safety, ref m_DisposeSentinel);
#endif

            m_Handle.Free();
        }
    }
}