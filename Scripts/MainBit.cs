using System;
using Unity.Mathematics;

namespace package.stormiumteam.shared
{
	public static class MainBit
	{
		public static void SetBitAt(ref byte curr, byte pos, bool val)
		{
			SetBitAt(ref curr, pos, (byte) (val ? 1 : 0));
		}

		public static void SetBitAt(ref byte curr, byte pos, byte val)
		{
			var cd = (curr & (1 << pos)) == 0;
			if (val != 0)
			{
				// Set
				if (cd)
					curr = (byte) (curr | (1 << pos));
			}
			else
			{
				if (!cd)
					curr = (byte) (curr & ~(1 << pos));
			}
		}

		public static byte GetBitAt(byte curr, byte pos)
		{
			return (byte) math.select(0, 1, (curr & (1 << pos)) != 0);
		}

		public static void SetByteRangeAt(ref byte curr, byte pos, byte val, byte size)
		{
			if (size != 2)
				throw new NotImplementedException("Only support size of 2");

			SetBitAt(ref curr, pos, GetBitAt(val, 0));
			SetBitAt(ref curr, (byte) (pos + 1), GetBitAt(val, 1));
		}

		public static byte GetByteRangeAt(byte curr, byte pos, byte size)
		{
			if (size != 2)
				throw new NotImplementedException("Only support size of 2");

			byte newByte = default(byte);

			newByte |= (byte) (GetBitAt(curr, pos) << pos);
			newByte |= (byte) (GetBitAt(curr, (byte) (pos + 1)) << pos + 1);

			return (byte) (newByte >> pos);
		}
	} 
}