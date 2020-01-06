using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;

namespace Scripts.Utilities
{
	public static unsafe class UnsafeUtilityOp
	{
		public static bool AreEquals<T>(ref T left, ref T right)
			where T : struct
		{
			return UnsafeUtility.MemCmp(UnsafeUtility.AddressOf(ref left), UnsafeUtility.AddressOf(ref right), UnsafeUtility.SizeOf<T>()) == 0;
		}
		
		public static bool AreNotEquals<T>(ref T left, ref T right)
			where T : struct
		{
			return UnsafeUtility.MemCmp(UnsafeUtility.AddressOf(ref left), UnsafeUtility.AddressOf(ref right), UnsafeUtility.SizeOf<T>()) != 0;
		}
	}
}