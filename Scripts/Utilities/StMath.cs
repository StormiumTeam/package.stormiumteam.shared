using System.Runtime.InteropServices;

namespace package.stormiumteam.shared
{
	[StructLayout(LayoutKind.Explicit)]
	public struct LongIntUnion
	{
		[FieldOffset(0)]
		public long LongValue;

		[FieldOffset(0)]
		public int Int0Value;

		[FieldOffset(sizeof(int))]
		public int Int1Value;
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct ULongUIntUnion
	{
		[FieldOffset(0)]
		public ulong LongValue;

		[FieldOffset(0)]
		public uint Int0Value;

		[FieldOffset(sizeof(uint))]
		public uint Int1Value;
	}
}