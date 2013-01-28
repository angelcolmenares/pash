using Microsoft.Management.Infrastructure.Native;
using Microsoft.Management.Infrastructure.Options;
using System;
using System.Globalization;

namespace Microsoft.Management.Infrastructure.Options.Internal
{
	internal static class ProxyTypeExtensionMethods
	{
		public static ProxyType FromNativeType(string proxyType)
		{
			if (string.Compare(proxyType, DestinationOptionsMethods.proxyType_None, true, CultureInfo.CurrentCulture) != 0)
			{
				if (string.Compare(proxyType, DestinationOptionsMethods.proxyType_WinHTTP, true, CultureInfo.CurrentCulture) != 0)
				{
					if (string.Compare(proxyType, DestinationOptionsMethods.proxyType_Auto, true, CultureInfo.CurrentCulture) != 0)
					{
						if (string.Compare(proxyType, DestinationOptionsMethods.proxyType_IE, true, CultureInfo.CurrentCulture) != 0)
						{
							throw new ArgumentOutOfRangeException("proxyType");
						}
						else
						{
							return ProxyType.InternetExplorer;
						}
					}
					else
					{
						return ProxyType.Auto;
					}
				}
				else
				{
					return ProxyType.WinHttp;
				}
			}
			else
			{
				return ProxyType.None;
			}
		}

		public static string ToNativeType(this ProxyType proxyType)
		{
			ProxyType proxyType1 = proxyType;
			switch (proxyType1)
			{
				case ProxyType.None:
				{
					return DestinationOptionsMethods.proxyType_None;
				}
				case ProxyType.WinHttp:
				{
					return DestinationOptionsMethods.proxyType_WinHTTP;
				}
				case ProxyType.Auto:
				{
					return DestinationOptionsMethods.proxyType_Auto;
				}
				case ProxyType.InternetExplorer:
				{
					return DestinationOptionsMethods.proxyType_IE;
				}
			}
			throw new ArgumentOutOfRangeException("proxyType");
		}
	}
}