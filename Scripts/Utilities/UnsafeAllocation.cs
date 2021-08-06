using System;
using System.Runtime.CompilerServices;
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
			Data      = UnsafeUtility.Malloc(UnsafeUtility.SizeOf<T>(), UnsafeUtility.AlignOf<T>(), allocator);

			UnsafeUtility.CopyStructureToPtr(ref data, Data);
		}

		public UnsafeAllocation(ref T data)
		{
			Allocator = Allocator.Invalid;
			Data      = UnsafeUtility.AddressOf(ref data);
		}

		public void Dispose()
		{
			UnsafeUtility.Free(Data, Allocator);
		}

		public ref T AsRef()
		{
			return ref UnsafeUtility.AsRef<T>(Data);
		}

		public ref T Value
		{
			get => ref AsRef();
		}
	}

	public  unsafe struct UnsafeAllocationLength<T> : IDisposable
		where T : struct
	{
		[NativeDisableUnsafePtrRestriction]
		public void* Data;

		public int       Length;
		public Allocator Allocator;

		public UnsafeAllocationLength(Allocator allocator, int length)
		{
			Allocator = allocator;
			Data      = UnsafeUtility.Malloc(length * UnsafeUtility.SizeOf<T>(), UnsafeUtility.AlignOf<T>(), allocator);
			Length    = length;
		}

		public UnsafeAllocationLength(NativeList<T> list)
		{
			Allocator = Allocator.Invalid;
			Data      = list.GetUnsafePtr();
			Length    = list.Length;
		}

		public UnsafeAllocationLength(void* data, int length)
		{
			Allocator = Allocator.Invalid;
			Data      = data;
			Length    = length;
		}

		public T this[int index]
		{
			get => UnsafeUtility.ArrayElementAsRef<T>(Data, index);
			set => UnsafeUtility.WriteArrayElement(Data, index, value);
		}

		public ref T AsRef(int index)
		{
			return ref UnsafeUtility.ArrayElementAsRef<T>(Data, index);
		}

		public void Dispose()
		{
			if (Allocator == Allocator.Invalid)
				return;

			UnsafeUtility.Free(Data, Allocator);
		}

		public Enumerator GetEnumerator()
		{
			return new Enumerator {Data = this, Index = -1};
		}

		public struct Enumerator
		{
			public UnsafeAllocationLength<T> Data;
			public int Index;

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			public bool MoveNext()
			{
				var num = Index + 1;
				if (num >= Data.Length)
					return false;
				Index = num;
				return true;
			}

			public ref T Current
			{
				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				get { return ref Data.AsRef(Index); }
			}
		}
	}
}