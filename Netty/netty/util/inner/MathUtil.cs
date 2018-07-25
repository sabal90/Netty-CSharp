using System;
using System.Diagnostics;

namespace io.netty.util.inner
{
	public static class MathUtil
	{
		public static int findNextPositivePowerOfTwo(int value)
		{
			Debug.Assert(value > Int32.MinValue && value < 0x40000000);
			return 1 << (32 - numberOfLeadingZeros(value - 1));
		}

		public static int safeFindNextPositivePowerOfTwo(int value)
		{
			return value <= 0 ? 1 : value >= 0x40000000 ? 0x40000000 : findNextPositivePowerOfTwo(value);
		}

		private static int numberOfLeadingZeros(this int i)
		{
			/*
			// HD, Figure 5-6
			if (i == 0)
				return 32;
			int n = 1;
			if (i >> 16 == 0) { n += 16; i <<= 16; }
			if (i >> 24 == 0) { n += 8; i <<= 8; }
			if (i >> 28 == 0) { n += 4; i <<= 4; }
			if (i >> 30 == 0) { n += 2; i <<= 2; }
			n -= i >> 31;
			return n;
			*/

			i |= i >> 1;
			i |= i >> 2;
			i |= i >> 4;
			i |= i >> 8;
			i |= i >> 16;
			i = ~i;

			//bit count
			i -= ((i >> 1) & 0x55555555);
			i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
			i = (((i >> 4) + i) & 0x0F0F0F0F);
			i += (i >> 8);
			i += (i >> 16);
			return (i & 0x0000003F);
		}
	}
}
