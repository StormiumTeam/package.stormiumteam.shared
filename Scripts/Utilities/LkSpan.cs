using System;

namespace package.stormiumteam.shared.utils
{
    // It's like Span<T>, but it's not Span<T> (as I didn't wanted to include a dll just for that...)
    public unsafe struct LkSpan<T>
        where T : unmanaged
    {
        public readonly int Length;
        public readonly IntPtr Pointer;
        
        public LkSpan(T* data, int length)
        {
            Length = length;
            Pointer = new IntPtr(data);
        }
    }
}