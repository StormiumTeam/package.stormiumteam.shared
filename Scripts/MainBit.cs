namespace package.stormiumteam.shared
{
	public static class MainBit
	{
		public static byte SetBitAt(byte curr, byte pos, byte val)
		{
			var cd = (curr & (1 << pos)) == 0;
			if (val != 0)
			{
				// Set
				if (cd)
					return (byte) (curr | (1 << pos));
			}
			else
			{
				if (!cd)
					return (byte) (curr & ~(1 << pos));
			}

			return curr;
		}
	}
}