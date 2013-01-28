using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal static class IPUtil
	{
		private static IADOPathNode BuildHostFilterFromIP(string ipAddress, IPUtil.IPVersion ipVersion, string extendedAttribute, string directoryAttribute, ADOperator op)
		{
			IPHostEntry hostEntry;
			IPAddress pAddress;
			try
			{
				pAddress = IPAddress.Parse(ipAddress);
			}
			catch (Exception exception)
			{
				object[] objArray = new object[2];
				objArray[0] = ipAddress;
				objArray[1] = extendedAttribute;
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.SearchConverterInvalidValue, objArray));
			}
			if (pAddress.AddressFamily == (AddressFamily)ipVersion)
			{
				try
				{
					hostEntry = Dns.GetHostEntry(pAddress);
				}
				catch (SocketException socketException)
				{
					object[] objArray1 = new object[1];
					objArray1[0] = ipAddress;
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.CannotResolveIPAddressToHostName, objArray1));
				}
				return ADOPathUtil.CreateFilterClause(op, directoryAttribute, hostEntry.HostName);
			}
			else
			{
				object[] objArray2 = new object[2];
				objArray2[0] = ipAddress;
				objArray2[1] = extendedAttribute;
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.SearchConverterInvalidValue, objArray2));
			}
		}

		internal static IADOPathNode BuildIPFilter(string extendedAttribute, string directoryAttribute, IADOPathNode filterClause, IPUtil.IPVersion ipVersion)
		{
			BinaryADOPathNode binaryADOPathNode = filterClause as BinaryADOPathNode;
			if (binaryADOPathNode != null)
			{
				IDataNode rightNode = binaryADOPathNode.RightNode as IDataNode;
				if (rightNode != null)
				{
					if (rightNode.DataObject as string != null)
					{
						ADOperator @operator = binaryADOPathNode.Operator;
						if (@operator == ADOperator.Eq || @operator == ADOperator.Ne)
						{
							return IPUtil.BuildHostFilterFromIP(rightNode.DataObject as string, ipVersion, extendedAttribute, directoryAttribute, @operator);
						}
						else
						{
							object[] str = new object[2];
							ADOperator[] aDOperatorArray = new ADOperator[2];
							aDOperatorArray[1] = ADOperator.Ne;
							str[0] = SearchConverters.ConvertOperatorListToString(aDOperatorArray);
							str[1] = extendedAttribute;
							throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.SearchConverterSupportedOperatorListErrorMessage, str));
						}
					}
					else
					{
						object[] type = new object[2];
						type[0] = rightNode.DataObject.GetType();
						type[1] = extendedAttribute;
						throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, StringResources.SearchConverterRHSInvalidType, type));
					}
				}
				else
				{
					throw new ArgumentException(StringResources.SearchConverterRHSNotDataNode);
				}
			}
			else
			{
				throw new ArgumentException(StringResources.SearchConverterNotBinaryNode);
			}
		}

		internal static string GetIPAddress(string dnsHostName, IPUtil.IPVersion ipVersion)
		{
			string str;
			if (!string.IsNullOrEmpty(dnsHostName))
			{
				try
				{
					IPHostEntry hostEntry = Dns.GetHostEntry(dnsHostName);
					IPAddress[] addressList = hostEntry.AddressList;
					int num = 0;
					while (num < (int)addressList.Length)
					{
						IPAddress pAddress = addressList[num];
						if (pAddress.AddressFamily != (AddressFamily)ipVersion || ipVersion == IPUtil.IPVersion.IPv6 && (pAddress.IsIPv6LinkLocal || pAddress.IsIPv6SiteLocal))
						{
							num++;
						}
						else
						{
							str = pAddress.ToString();
							return str;
						}
					}
					str = null;
				}
				catch (SocketException socketException)
				{
					str = null;
				}
				return str;
			}
			else
			{
				return null;
			}
		}

		internal enum IPVersion
		{
			IPv4 = 2,
			IPv6 = 23
		}
	}
}