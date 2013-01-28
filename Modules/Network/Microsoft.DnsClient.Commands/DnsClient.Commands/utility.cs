using System;
using System.Net;
using System.Net.Sockets;

namespace Microsoft.DnsClient.Commands
{
	internal class utility
	{
		public utility()
		{
		}

		internal static string ConvertNumericToReverseLookup(string query)
		{
			string str = query;
			IPAddress pAddress = IPAddress.Parse(query);
			if (pAddress.AddressFamily != AddressFamily.InterNetwork)
			{
				if (pAddress.AddressFamily != AddressFamily.InterNetworkV6)
				{
					throw new NotSupportedException();
				}
				else
				{
					str = "ip6.arpa";
					byte[] addressBytes = pAddress.GetAddressBytes();
					byte[] numArray = addressBytes;
					for (int i = 0; i < (int)numArray.Length; i++)
					{
						byte num = numArray[i];
						byte num1 = (byte)(num & 15);
						byte num2 = (byte)((num & 240) >> 4);
						str = string.Format("{0:X}.{1:X}.{2}", num1, num2, str);
					}
				}
			}
			else
			{
				char[] chrArray = new char[1];
				chrArray[0] = '.';
				string[] strArrays = str.Split(chrArray);
				string[] strArrays1 = new string[8];
				strArrays1[0] = strArrays[3];
				strArrays1[1] = ".";
				strArrays1[2] = strArrays[2];
				strArrays1[3] = ".";
				strArrays1[4] = strArrays[1];
				strArrays1[5] = ".";
				strArrays1[6] = strArrays[0];
				strArrays1[7] = ".in-addr.arpa";
				str = string.Concat(strArrays1);
			}
			return str;
		}

		internal static string FormatAsIPv6Address(byte[] InByte)
		{
			return (new IPAddress(InByte)).ToString();
		}

		internal static uint IPv4AddressToULong(string IPaddress)
		{
			IPAddress pAddress = IPAddress.Parse(IPaddress);
			byte[] addressBytes = pAddress.GetAddressBytes();
			if ((int)addressBytes.Length == 4)
			{
				uint num = BitConverter.ToUInt32(addressBytes, 0);
				return num;
			}
			else
			{
				throw new ArgumentException();
			}
		}

		internal static bool IsMappedIpv6Address(byte[] InByte)
		{
			if (InByte[0] != 0 || InByte[1] != 0 || InByte[2] != 0 || InByte[3] != 0 || InByte[4] != 0 || InByte[5] != 0 || InByte[6] != 0 || InByte[7] != 0 || InByte[8] != 0 || InByte[9] != 0 || InByte[10] != 0xff)
			{
				return false;
			}
			else
			{
				return InByte[11] == 0xff;
			}
		}
	}
}