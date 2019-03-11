using System;
using Unity.Collections.LowLevel.Unsafe;

namespace package.stormiumteam.shared.utils
{
    // It's like Span<T>, but it's not Span<T> (as I didn't wanted to include a dll just for that...)
    public unsafe struct LkSpan<T>
        where T : unmanaged
    {
        public readonly int Length;
        public readonly IntPtr Pointer;
        
        public LkSpan(IntPtr data, int length)
        {
            Length  = length;
            Pointer = data;
        }
        
        public LkSpan(T* data, int length)
        {
            Length = length;
            Pointer = new IntPtr(data);
        }
    }
    
    public unsafe struct NmLkSpan<T>
        where T : struct
    {
        public readonly int    Length;
        public readonly IntPtr Pointer;
        
        public NmLkSpan(IntPtr data, int length)
        {
            Length  = length;
            Pointer = data;
        }

        public T this[int i]
        {
            get
            {
                if (i < 0) throw new IndexOutOfRangeException($"Argument {nameof(i)} is inferior to 0.");
                if (i >= Length) throw new IndexOutOfRangeException($"Argument {nameof(i)}={i} is superior to length={Length}");

                return UnsafeUtility.ReadArrayElement<T>((void*) Pointer, i);
            }
        }
    }
}