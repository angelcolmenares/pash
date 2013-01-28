using Microsoft.Management.Infrastructure.Native;
using Microsoft.Management.Infrastructure.Options;
using System;
using System.Globalization;

namespace Microsoft.Management.Infrastructure.Options.Internal
{
	internal static class PacketEncodingExtensionMethods
	{
		public static PacketEncoding FromNativeType(string packetEncoding)
		{
			if (string.Compare(packetEncoding, DestinationOptionsMethods.packetEncoding_Default, true, CultureInfo.CurrentCulture) != 0)
			{
				if (string.Compare(packetEncoding, DestinationOptionsMethods.packetEncoding_UTF8, true, CultureInfo.CurrentCulture) != 0)
				{
					if (string.Compare(packetEncoding, DestinationOptionsMethods.packetEncoding_UTF16, true, CultureInfo.CurrentCulture) != 0)
					{
						throw new ArgumentOutOfRangeException("packetEncoding");
					}
					else
					{
						return PacketEncoding.Utf16;
					}
				}
				else
				{
					return PacketEncoding.Utf8;
				}
			}
			else
			{
				return PacketEncoding.Default;
			}
		}

		public static string ToNativeType(this PacketEncoding packetEncoding)
		{
			PacketEncoding packetEncoding1 = packetEncoding;
			switch (packetEncoding1)
			{
				case PacketEncoding.Default:
				{
					return DestinationOptionsMethods.packetEncoding_Default;
				}
				case PacketEncoding.Utf8:
				{
					return DestinationOptionsMethods.packetEncoding_UTF8;
				}
				case PacketEncoding.Utf16:
				{
					return DestinationOptionsMethods.packetEncoding_UTF16;
				}
			}
			throw new ArgumentOutOfRangeException("packetEncoding");
		}
	}
}