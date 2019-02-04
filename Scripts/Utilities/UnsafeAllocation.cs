using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace package.stormiumteam.shared
{
    public unsafe struct UnsafeAllocation<T> : IDisposable
        where T : struct
    {
        public void* Data;
        public Allocator Allocator;
        
        public UnsafeAllocation(Allocator allocator, T data)
        {
            Allocator = allocator;
            Data = UnsafeUtility.Malloc(UnsafeUtility.SizeOf<T>(), UnsafeUtility.AlignOf<T>(), allocator);
            
            UnsafeUtility.CopyStructureToPtr(ref data, Data);
        }
        
        public void Dispose()
        {
            UnsafeUtility.Free(Data, Allocator);
        }
    }
    
    public unsafe struct UnsafeAllocationLength<T> : IDisposable
        where T : struct
    {
        public void*     Data;
        public Allocator Allocator;
        
        public UnsafeAllocationLength(Allocator allocator, int length)
        {
            Allocator = allocator;
            Data      = UnsafeUtility.Malloc(length, UnsafeUtility.AlignOf<T>(), allocator);
        }
        
        public void Dispose()
        {
            UnsafeUtility.Free(Data, Allocator);
        }
    }
}