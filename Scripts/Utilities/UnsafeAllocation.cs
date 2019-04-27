using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace package.stormiumteam.shared
{
    public static class UnsafeAllocation
    {
        public static UnsafeAllocation<T> From<T>(ref T value)
            where T : struct
        {
            return new UnsafeAllocation<T>(ref value);
        }
    }
    
    [NativeContainerSupportsDeallocateOnJobCompletion]
    public unsafe struct UnsafeAllocation<T> : IDisposable
        where T : struct
    {
        [NativeDisableUnsafePtrRestriction]
        public void* Data;
        public Allocator Allocator;
        
        public UnsafeAllocation(Allocator allocator, T data)
        {
            Allocator = allocator;
            Data = UnsafeUtility.Malloc(UnsafeUtility.SizeOf<T>(), UnsafeUtility.AlignOf<T>(), allocator);
            
            UnsafeUtility.CopyStructureToPtr(ref data, Data);
        }
        
        public UnsafeAllocation(ref T data)
        {
            Allocator = Allocator.Invalid;
            Data = UnsafeUtility.AddressOf(ref data);
        }
        
        public void Dispose()
        {
            UnsafeUtility.Free(Data, Allocator);
        }

        public ref T AsRef()
        {
            return ref UnsafeUtilityEx.AsRef<T>(Data);
        }

        public T Value
        {
            get => AsRef();
            set => UnsafeUtility.MemCpy(Data, UnsafeUtility.AddressOf(ref value), UnsafeUtility.SizeOf<T>());
        }
    }
    
    public unsafe struct UnsafeAllocationLength<T> : IDisposable
        where T : struct
    {
        public void*     Data;
        public int Length;
        public Allocator Allocator;
        
        public UnsafeAllocationLength(Allocator allocator, int length)
        {
            Allocator = allocator;
            Data      = UnsafeUtility.Malloc(length, UnsafeUtility.AlignOf<T>(), allocator);
            Length = length;
        }

        public UnsafeAllocationLength(NativeList<T> list)
        {
            Allocator = Allocator.Invalid;
            Data = list.GetUnsafePtr();
            Length = list.Length;
        }

        public T this[int index]
        {
            get => UnsafeUtilityEx.ArrayElementAsRef<T>(Data, index);
        }
        
        public void Dispose()
        {
            if (Allocator == Allocator.Invalid)
                return;
            
            UnsafeUtility.Free(Data, Allocator);
        }
    }
}