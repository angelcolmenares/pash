using System;

namespace System.Runtime
{
	internal static class HashHelper
	{
		public static byte[] ComputeHash(byte[] buffer)
		{
			long num;
			long num1;
			long[] numArray = new long[] { 7, 12, 17, 22, 5, 9, 14, 20, 4, 11, 16, 23, 6, 10, 15, 21 };
			long[] numArray1 = numArray;
			long[] numArray2 = new long[] { 0xd76aa478, 0xe8c7b756, 0x242070db, 0xc1bdceee, 0xf57c0faf, 0x4787c62a, 0xa8304613, 0xfd469501, 0x698098d8, 0x8b44f7af, 0xffff5bb1, 0x895cd7be, 0x6b901122, 0xfd987193, 0xa679438e, 0x49b40821, 0xf61e2562, 0xc040b340, 0x265e5a51, 0xe9b6c7aa, 0xd62f105d, 0x2441453, 0xd8a1e681, 0xe7d3fbc8, 0x21e1cde6, 0xc33707d6, 0xf4d50d87, 0x455a14ed, 0xa9e3e905, 0xfcefa3f8, 0x676f02d9, 0x8d2a4c8a, 0xfffa3942, 0x8771f681, 0x6d9d6122, 0xfde5380c, 0xa4beea44, 0x4bdecfa9, 0xf6bb4b60, 0xbebfbc70, 0x289b7ec6, 0xeaa127fa, 0xd4ef3085, 0x4881d05, 0xd9d4d039, 0xe6db99e5, 0x1fa27cf8, 0xc4ac5665, 0xf4292244, 0x432aff97, 0xab9423a7, 0xfc93a039, 0x655b59c3, 0x8f0ccc92, 0xffeff47d, 0x85845dd1, 0x6fa87e4f, 0xfe2ce6e0, 0xa3014314, 0x4e0811a1, 0xf7537e82, 0xbd3af235, 0x2ad7d2bb, 0xeb86d391 };
			long[] numArray3 = numArray2;
			long length = ((long)buffer.Length + 8) / 64 + 1;
			long num2 = 0x67452301;
			long num3 = -271733879;
			long num4 = -1732584194;
			long num5 = 0x10325476;
			for (long i = 0; i < length; i++)
			{
				byte[] length1 = buffer;
				long num6 = i * 64;
				if (num6 + 64 > (long)buffer.Length)
				{
					length1 = new byte[64];
					for (long j = num6; j < (long)buffer.Length; j++)
					{
						length1[j - num6] = buffer[j];
					}
					if (num6 <= (long)buffer.Length)
					{
						length1[(long)buffer.Length - num6] = 128;
					}
					if (i == length - 1)
					{
						length1[56] = (byte)((long)buffer.Length << 3);
						length1[57] = (byte)((long)buffer.Length >> 5);
						length1[58] = (byte)((long)buffer.Length >> 13);
						length1[59] = (byte)((long)buffer.Length >> 21);
					}
					num6 = 0;
				}
				long num7 = num2;
				long num8 = num3;
				long num9 = num4;
				long num10 = num5;
				long num11 = 0;
				while (num11 < 64)
				{
					if (num11 >= 16)
					{
						if (num11 >= 32)
						{
							if (num11 >= 48)
							{
								num = num9 ^ (num8 | ~num10);
								num1 = 7 * num11;
							}
							else
							{
								num = num8 ^ num9 ^ num10;
								num1 = 3 * num11 + 5;
							}
						}
						else
						{
							num = num8 & num10 | num9 & ~num10;
							num1 = 5 * num11 + 1;
						}
					}
					else
					{
						num = num8 & num9 | ~num8 & num10;
						num1 = num11;
					}
					num1 = (num1 & 15) * 4 + num6;
					long num12 = num10;
					num10 = num9;
					num9 = num8;
					num8 = num7 + num + numArray3[num11] + length1[num1] + (length1[num1 + 1] << 8) + (length1[num1 + 2] << 16) + (length1[num1 + 3] << 24);
					num8 = num8 << (numArray1[num11 & 3 | num11 >> 2 & -4] & 31) | num8 >> (32 - numArray1[num11 & 3 | num11 >> 2 & -4] & 31);
					num8 = num8 + num9;
					num7 = num12;
					num11++;
				}
				num2 = num2 + num7;
				num3 = num3 + num8;
				num4 = num4 + num9;
				num5 = num5 + num10;
			}
			byte[] numArray4 = new byte[16];
			numArray4[0] = (byte)num2;
			numArray4[1] = (byte)(num2 >> 8);
			numArray4[2] = (byte)(num2 >> 16);
			numArray4[3] = (byte)(num2 >> 24);
			numArray4[4] = (byte)num3;
			numArray4[5] = (byte)(num3 >> 8);
			numArray4[6] = (byte)(num3 >> 16);
			numArray4[7] = (byte)(num3 >> 24);
			numArray4[8] = (byte)num4;
			numArray4[9] = (byte)(num4 >> 8);
			numArray4[10] = (byte)(num4 >> 16);
			numArray4[11] = (byte)(num4 >> 24);
			numArray4[12] = (byte)num5;
			numArray4[13] = (byte)(num5 >> 8);
			numArray4[14] = (byte)(num5 >> 16);
			numArray4[15] = (byte)(num5 >> 24);
			return numArray4;
		}
	}
}