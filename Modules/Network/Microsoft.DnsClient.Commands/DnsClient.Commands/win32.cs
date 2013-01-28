using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;

namespace Microsoft.DnsClient.Commands
{
	internal static class win32
	{
		internal const ushort DNS_CONFIG_FLAG_ALLOC = 1;

		[DllImport("dnsapi", CharSet=CharSet.Unicode)]
		private static extern void _DnsFree(IntPtr pData, DNS_FREE_TYPE type);

		[DllImport("dnsapi", CharSet=CharSet.Unicode)]
		private static extern int _DnsQueryConfig(DnsConfigType ConfigType, uint Flag, string AdapterName, IntPtr reserved, ref IntPtr Buffer, ref int BufferLength);

		[DllImport("dnsapi", CharSet=CharSet.Unicode)]
		private static extern int _DnsQueryW(string pszName, ushort wType, uint options, out IP4AddressArray pExtra, out IntPtr ppQueryResults, IntPtr pReserved);

		[DllImport("dnsapi", CharSet=CharSet.Unicode)]
		internal static extern int _DnsQueryW(string pszName, ushort wType, uint options, out DnsExtraInfo pExtra, out IntPtr ppQueryResults, IntPtr pReserved);

		[DllImport("dnsapi", CharSet=CharSet.Auto)]
		private static extern void _DnsRecordListFree(IntPtr pRecordList, int FreeType);

		[DllImport("Iphlpapi", CharSet=CharSet.Auto)]
		private static extern uint _GetAdaptersAddresses(uint Family, uint Flags, IntPtr Reserved, IntPtr AdapterAddresses, ref int SizePointer);

		[DllImport("kernel32", CharSet=CharSet.None)]
		private static extern IntPtr _LocalFree(IntPtr hMem);

		internal static void DnsFree(IntPtr pData, DNS_FREE_TYPE type)
		{
			win32._DnsFree(pData, type);
		}

		internal static int DnsQuery(string pszName, RecordType wType, QueryOptions options, ref IP4AddressArray pExtra, out IntPtr ppQueryResults, IntPtr pReserved)
		{
			return win32._DnsQueryW(pszName, (ushort)wType, (uint)options, out pExtra, out ppQueryResults, IntPtr.Zero);
		}

		internal static int DnsQuery(string pszName, RecordType wType, QueryOptions options, ref DnsExtraInfo pExtra, out IntPtr ppQueryResults)
		{
			IntPtr zero = IntPtr.Zero;
			return win32._DnsQueryW(pszName, (ushort)wType, (uint)options, out pExtra, out ppQueryResults, zero);
		}

		internal static int DnsQueryConfig(DnsConfigType ConfigType, uint Flag, string AdapterName, IntPtr reserved, ref IntPtr Buffer, ref int BufferLength)
		{
			return win32._DnsQueryConfig(ConfigType, Flag, AdapterName, reserved, ref Buffer, ref BufferLength);
		}

		internal static void DnsRecordListFree(IntPtr pRecordList, int FreeType)
		{
			win32._DnsRecordListFree(pRecordList, FreeType);
		}

		internal static List<AdapterInfo> GetAdaptersAddresses(FAMILY family, GetAdapterFlags flags)
		{
			IP_ADAPTER_ADDRESSES structure;
			int num = 0;
			List<AdapterInfo> adapterInfos = new List<AdapterInfo>();
			AdapterInfo strs = new AdapterInfo();
			win32._GetAdaptersAddresses((uint)family, (uint)flags, IntPtr.Zero, IntPtr.Zero, ref num);
			IntPtr intPtr = Marshal.AllocHGlobal(num);
			win32._GetAdaptersAddresses((uint)family, (uint)flags, IntPtr.Zero, intPtr, ref num);
			for (IntPtr i = intPtr; IntPtr.Zero != i; i = structure.Next)
			{
				strs.Ipv4 = new List<string>();
				strs.Ipv6 = new List<string>();
				structure = (IP_ADAPTER_ADDRESSES)Marshal.PtrToStructure(i, typeof(IP_ADAPTER_ADDRESSES));
				IntPtr firstUnicastAddress = structure.FirstUnicastAddress;
				strs.Description = structure.Description;
				strs.FriendlyName = structure.FriendlyName;
				strs.DNSSuffix = structure.DnsSuffix;
				strs.IfType = (InterfaceType)structure.IfType;
				uint num1 = 0;
				uint num2 = 0;
				while (IntPtr.Zero != firstUnicastAddress)
				{
					IP_ADAPTER_UNICAST_ADDRESS pADAPTERUNICASTADDREss = (IP_ADAPTER_UNICAST_ADDRESS)Marshal.PtrToStructure(firstUnicastAddress, typeof(IP_ADAPTER_UNICAST_ADDRESS));
					int address = pADAPTERUNICASTADDREss.Address.iSockaddrLength;
					if (address == 16)
					{
						IP_V4_ADDRESS pV4ADDREss = (IP_V4_ADDRESS)Marshal.PtrToStructure(pADAPTERUNICASTADDREss.Address.lpSockAddr, typeof(IP_V4_ADDRESS));
						object[] sinAddr = new object[7];
						sinAddr[0] = pV4ADDREss.sin_addr[0];
						sinAddr[1] = ".";
						sinAddr[2] = pV4ADDREss.sin_addr[1];
						sinAddr[3] = ".";
						sinAddr[4] = pV4ADDREss.sin_addr[2];
						sinAddr[5] = ".";
						sinAddr[6] = pV4ADDREss.sin_addr[3];
						string str = string.Concat(sinAddr);
						strs.Ipv4.Add(str);
						num1++;
					}
					else
					{
						if (address == 28)
						{
							IP_V6_ADDRESS pV6ADDREss = (IP_V6_ADDRESS)Marshal.PtrToStructure(pADAPTERUNICASTADDREss.Address.lpSockAddr, typeof(IP_V6_ADDRESS));
							strs.Ipv6.Add(utility.FormatAsIPv6Address(pV6ADDREss.sin6_addr));
							num2++;
						}
					}
					firstUnicastAddress = pADAPTERUNICASTADDREss.Next;
				}
				adapterInfos.Add(strs);
			}
			Marshal.FreeHGlobal(intPtr);
			return adapterInfos;
		}

		internal static List<AdapterInfo> GetAdaptersInformation()
		{
			return win32.GetAdaptersAddresses(FAMILY.AF_UNSPEC, GetAdapterFlags.GAA_FLAG_DEFAULT);
		}

		internal static List<DnsRecord> GetDNSRecords(string QueryName, string[] DnsServerIPs, QueryOptions Options, RecordType Type)
		{
			unsafe
			{
				PREMARSHAL_DNS_RECORD structure;
				DnsRecord_Default dnsRecordDefault = null;
				byte[] numArray = null;
				IntPtr zero = IntPtr.Zero;
				object[] typeADnsRecord;
				int size;
				IntPtr intPtr = IntPtr.Zero;
				new DnsRecord();
				List<DnsRecord> dnsRecords = new List<DnsRecord>();
				if (Environment.OSVersion.Platform == PlatformID.Win32NT)
				{
					DnsExtraInfo dnsExtraInfo = new DnsExtraInfo();
					if (DnsServerIPs != null && (int)DnsServerIPs.Length > 0)
					{
						IPAddress[] pAddressArray = new IPAddress[(int)DnsServerIPs.Length];
						for (int i = 0; i < (int)DnsServerIPs.Length; i++)
						{
							pAddressArray[i] = IPAddress.Parse(DnsServerIPs[i]);
						}
						dnsExtraInfo = new DnsExtraInfo(pAddressArray);
					}
					int num = win32.DnsQuery(QueryName, Type, Options, ref dnsExtraInfo, out intPtr);
					if (num == 0 || num == 0x251d)
					{
						for (IntPtr j = intPtr; !j.Equals(IntPtr.Zero); j = structure.pNext)
						{
							structure = (PREMARSHAL_DNS_RECORD)Marshal.PtrToStructure(j, typeof(PREMARSHAL_DNS_RECORD));
							RecordType recordType = structure.wType;
							switch (recordType)
							{
								case RecordType.A:
								{
									DnsRecord_A dnsRecordA = new DnsRecord_A();
									TYPEA_DNS_RECORD tYPEADNSRECORD = (TYPEA_DNS_RECORD)Marshal.PtrToStructure(j, typeof(TYPEA_DNS_RECORD));
									dnsRecordA.CharacterSet = tYPEADNSRECORD.flags.CharSet;
									dnsRecordA.Section = tYPEADNSRECORD.flags.Section;
									dnsRecordA.Name = Marshal.PtrToStringAuto(tYPEADNSRECORD.pName);
									dnsRecordA.Type = (RecordType)tYPEADNSRECORD.wType;
									dnsRecordA.TTL = tYPEADNSRECORD.dwTtl;
									dnsRecordA.DataLength = tYPEADNSRECORD.wDataLength;
									typeADnsRecord = new object[4];
									typeADnsRecord[0] = tYPEADNSRECORD.typeA.TypeA_DnsRecord[0];
									typeADnsRecord[1] = tYPEADNSRECORD.typeA.TypeA_DnsRecord[1];
									typeADnsRecord[2] = tYPEADNSRECORD.typeA.TypeA_DnsRecord[2];
									typeADnsRecord[3] = tYPEADNSRECORD.typeA.TypeA_DnsRecord[3];
									dnsRecordA.IP4Address = string.Format("{0}.{1}.{2}.{3}", typeADnsRecord);
									dnsRecords.Add(dnsRecordA);
									j = tYPEADNSRECORD.pNext;
									continue;
								}
								case RecordType.NS:
								case RecordType.MD:
								case RecordType.MF:
								case RecordType.CNAME:
								case RecordType.MB:
								case RecordType.MG:
								case RecordType.MR:
								case RecordType.PTR:
								case RecordType.DNAME:
								{
									DnsRecord_PTR dnsRecordPTR = new DnsRecord_PTR();
									TYPEPTR_DNS_RECORD tYPEPTRDNSRECORD = (TYPEPTR_DNS_RECORD)Marshal.PtrToStructure(j, typeof(TYPEPTR_DNS_RECORD));
									string stringAuto = Marshal.PtrToStringAuto(tYPEPTRDNSRECORD.NStype.TypeNS_DnsRecord);
									dnsRecordPTR.NameHost = stringAuto;
									dnsRecordPTR.CharacterSet = tYPEPTRDNSRECORD.flags.CharSet;
									dnsRecordPTR.Section = tYPEPTRDNSRECORD.flags.Section;
									dnsRecordPTR.Name = Marshal.PtrToStringAuto(tYPEPTRDNSRECORD.pName);
									dnsRecordPTR.Type = (RecordType)tYPEPTRDNSRECORD.wType;
									dnsRecordPTR.TTL = tYPEPTRDNSRECORD.dwTtl;
									dnsRecordPTR.DataLength = tYPEPTRDNSRECORD.wDataLength;
									dnsRecords.Add(dnsRecordPTR);
									j = tYPEPTRDNSRECORD.pNext;
									continue;
								}
								case RecordType.SOA:
								{
									DnsRecord_SOA dnsRecordSOA = new DnsRecord_SOA();
									TYPESOA_DNS_RECORD tYPESOADNSRECORD = (TYPESOA_DNS_RECORD)Marshal.PtrToStructure(j, typeof(TYPESOA_DNS_RECORD));
									dnsRecordSOA.PrimaryServer = Marshal.PtrToStringAuto(tYPESOADNSRECORD.SOAtype.NamePrimaryServer);
									dnsRecordSOA.NameAdministrator = Marshal.PtrToStringAuto(tYPESOADNSRECORD.SOAtype.NameAdministrator);
									dnsRecordSOA.DefaultTTL = tYPESOADNSRECORD.SOAtype.DefaultTtl;
									dnsRecordSOA.SerialNumber = tYPESOADNSRECORD.SOAtype.SerialNo;
									dnsRecordSOA.TimeToExpiration = tYPESOADNSRECORD.SOAtype.Expire;
									dnsRecordSOA.TimeToZoneFailureRetry = tYPESOADNSRECORD.SOAtype.Retry;
									dnsRecordSOA.TimeToZoneRefresh = tYPESOADNSRECORD.SOAtype.Refresh;
									dnsRecordSOA.CharacterSet = tYPESOADNSRECORD.flags.CharSet;
									dnsRecordSOA.Section = tYPESOADNSRECORD.flags.Section;
									dnsRecordSOA.Name = Marshal.PtrToStringAuto(tYPESOADNSRECORD.pName);
									dnsRecordSOA.Type = (RecordType)tYPESOADNSRECORD.wType;
									dnsRecordSOA.TTL = tYPESOADNSRECORD.dwTtl;
									dnsRecordSOA.DataLength = tYPESOADNSRECORD.wDataLength;
									dnsRecords.Add(dnsRecordSOA);
									j = tYPESOADNSRECORD.pNext;
									continue;
								}
								case RecordType.NULL:
								case RecordType.DHCID:
								{
									DnsRecord_DHCID dnsRecordDHCID = new DnsRecord_DHCID();
									PreMarshal_TYPEDHCID_DNS_RECORD preMarshalTYPEDHCIDDNSRECORD = (PreMarshal_TYPEDHCID_DNS_RECORD)Marshal.PtrToStructure(j, typeof(PreMarshal_TYPEDHCID_DNS_RECORD));
									IntPtr zero1 = IntPtr.Zero;
									size = IntPtr.Size;
									if (size == 4)
									{
										zero1 = new IntPtr(j.ToInt32() + Marshal.SizeOf(preMarshalTYPEDHCIDDNSRECORD));
									}
									else
									{
										if (size == 8)
										{
											zero1 = new IntPtr(j.ToInt64() + (long)Marshal.SizeOf(preMarshalTYPEDHCIDDNSRECORD));
										}
									}
									byte[] numArray1 = new byte[preMarshalTYPEDHCIDDNSRECORD.DHCIDtype.ByteCount];
									Marshal.Copy(zero1, numArray1, 0, (int)numArray1.Length);
									dnsRecordDHCID.Dhcid = numArray1;
									dnsRecordDHCID.CharacterSet = preMarshalTYPEDHCIDDNSRECORD.flags.CharSet;
									dnsRecordDHCID.Section = preMarshalTYPEDHCIDDNSRECORD.flags.Section;
									dnsRecordDHCID.Name = Marshal.PtrToStringAuto(preMarshalTYPEDHCIDDNSRECORD.pName);
									dnsRecordDHCID.Type = (RecordType)preMarshalTYPEDHCIDDNSRECORD.wType;
									dnsRecordDHCID.TTL = preMarshalTYPEDHCIDDNSRECORD.dwTtl;
									dnsRecordDHCID.DataLength = preMarshalTYPEDHCIDDNSRECORD.wDataLength;
									dnsRecords.Add(dnsRecordDHCID);
									j = preMarshalTYPEDHCIDDNSRECORD.pNext;
									continue;
								}
								case RecordType.WKS:
								{
									DnsRecord_WKS dnsRecordWK = new DnsRecord_WKS();
									TYPEWKS_DNS_RECORD tYPEWKSDNSRECORD = (TYPEWKS_DNS_RECORD)Marshal.PtrToStructure(j, typeof(TYPEWKS_DNS_RECORD));
									dnsRecordWK.Bitmask = new byte[128];
									dnsRecordWK.Bitmask = tYPEWKSDNSRECORD.WKStype.ByteArray;
									typeADnsRecord = new object[4];
									typeADnsRecord[0] = tYPEWKSDNSRECORD.WKStype.IP4Address[0];
									typeADnsRecord[1] = tYPEWKSDNSRECORD.WKStype.IP4Address[1];
									typeADnsRecord[2] = tYPEWKSDNSRECORD.WKStype.IP4Address[2];
									typeADnsRecord[3] = tYPEWKSDNSRECORD.WKStype.IP4Address[3];
									dnsRecordWK.IP4Address = string.Format("{0}.{1}.{2}.{3}", typeADnsRecord);
									dnsRecordWK.Protocol = (IpProtocol)tYPEWKSDNSRECORD.WKStype.Protocol;
									dnsRecordWK.CharacterSet = tYPEWKSDNSRECORD.flags.CharSet;
									dnsRecordWK.Section = tYPEWKSDNSRECORD.flags.Section;
									dnsRecordWK.Name = Marshal.PtrToStringAuto(tYPEWKSDNSRECORD.pName);
									dnsRecordWK.Type = (RecordType)tYPEWKSDNSRECORD.wType;
									dnsRecordWK.TTL = tYPEWKSDNSRECORD.dwTtl;
									dnsRecordWK.DataLength = tYPEWKSDNSRECORD.wDataLength;
									dnsRecords.Add(dnsRecordWK);
									j = tYPEWKSDNSRECORD.pNext;
									continue;
								}
								case RecordType.HINFO:
								case RecordType.TXT:
								case RecordType.X25:
								case RecordType.ISDN:
								{
									DnsRecord_TXT dnsRecordTXT = new DnsRecord_TXT();
									PreMarshal_TYPETXT_DNS_RECORD preMarshalTYPETXTDNSRECORD = (PreMarshal_TYPETXT_DNS_RECORD)Marshal.PtrToStructure(j, typeof(PreMarshal_TYPETXT_DNS_RECORD));
									dnsRecordTXT.Strings = new string[preMarshalTYPETXTDNSRECORD.TXTtype.StringCount];
									IntPtr[] intPtrArray = new IntPtr[preMarshalTYPETXTDNSRECORD.TXTtype.StringCount];
									TYPETXT_DNS_RECORD tYPETXTDNSRECORD = (TYPETXT_DNS_RECORD)Marshal.PtrToStructure(j, typeof(TYPETXT_DNS_RECORD));
									size = IntPtr.Size;
									if (size == 4)
									{
										IntPtr intPtr1 = new IntPtr(j.ToInt32() + Marshal.SizeOf(preMarshalTYPETXTDNSRECORD));
										Marshal.Copy(intPtr1, intPtrArray, 0, (int)intPtrArray.Length);
									}
									else
									{
										if (size == 8)
										{
											IntPtr intPtr2 = new IntPtr(j.ToInt64() + (long)Marshal.SizeOf(preMarshalTYPETXTDNSRECORD));
											Marshal.Copy(intPtr2, intPtrArray, 0, (int)intPtrArray.Length);
										}
									}
									for (int k = 0; (long)k < (long)preMarshalTYPETXTDNSRECORD.TXTtype.StringCount; k++)
									{
										dnsRecordTXT.Strings[k] = Marshal.PtrToStringAuto(intPtrArray[k]);
									}
									dnsRecordTXT.CharacterSet = tYPETXTDNSRECORD.flags.CharSet;
									dnsRecordTXT.Section = tYPETXTDNSRECORD.flags.Section;
									dnsRecordTXT.Name = Marshal.PtrToStringAuto(tYPETXTDNSRECORD.pName);
									dnsRecordTXT.Type = (RecordType)tYPETXTDNSRECORD.wType;
									dnsRecordTXT.TTL = tYPETXTDNSRECORD.dwTtl;
									dnsRecordTXT.DataLength = tYPETXTDNSRECORD.wDataLength;
									dnsRecords.Add(dnsRecordTXT);
									j = tYPETXTDNSRECORD.pNext;
									continue;
								}
								case RecordType.MINFO:
								case RecordType.RP:
								{
									DnsRecord_MINFO dnsRecordMINFO = new DnsRecord_MINFO();
									TYPEMINFO_DNS_RECORD tYPEMINFODNSRECORD = (TYPEMINFO_DNS_RECORD)Marshal.PtrToStructure(j, typeof(TYPEMINFO_DNS_RECORD));
									dnsRecordMINFO.NameErrorsMailbox = Marshal.PtrToStringAuto(tYPEMINFODNSRECORD.MINFOtype.NameErrorsMailBox);
									dnsRecordMINFO.NameMailbox = Marshal.PtrToStringAuto(tYPEMINFODNSRECORD.MINFOtype.NameMailbox);
									dnsRecordMINFO.CharacterSet = tYPEMINFODNSRECORD.flags.CharSet;
									dnsRecordMINFO.Section = tYPEMINFODNSRECORD.flags.Section;
									dnsRecordMINFO.Name = Marshal.PtrToStringAuto(tYPEMINFODNSRECORD.pName);
									dnsRecordMINFO.Type = (RecordType)tYPEMINFODNSRECORD.wType;
									dnsRecordMINFO.TTL = tYPEMINFODNSRECORD.dwTtl;
									dnsRecordMINFO.DataLength = tYPEMINFODNSRECORD.wDataLength;
									dnsRecords.Add(dnsRecordMINFO);
									j = tYPEMINFODNSRECORD.pNext;
									continue;
								}
								case RecordType.MX:
								case RecordType.AFSDB:
								case RecordType.RT:
								{
									DnsRecord_MX dnsRecordMX = new DnsRecord_MX();
									TYPEMX_DNS_RECORD tYPEMXDNSRECORD = (TYPEMX_DNS_RECORD)Marshal.PtrToStructure(j, typeof(TYPEMX_DNS_RECORD));
									dnsRecordMX.Preference = tYPEMXDNSRECORD.MXtype.wPreference;
									dnsRecordMX.NameExchange = Marshal.PtrToStringAuto(tYPEMXDNSRECORD.MXtype.pNameExchange);
									dnsRecordMX.CharacterSet = tYPEMXDNSRECORD.flags.CharSet;
									dnsRecordMX.Section = tYPEMXDNSRECORD.flags.Section;
									dnsRecordMX.Name = Marshal.PtrToStringAuto(tYPEMXDNSRECORD.pName);
									dnsRecordMX.Type = (RecordType)tYPEMXDNSRECORD.wType;
									dnsRecordMX.TTL = tYPEMXDNSRECORD.dwTtl;
									dnsRecordMX.DataLength = tYPEMXDNSRECORD.wDataLength;
									dnsRecords.Add(dnsRecordMX);
									j = tYPEMXDNSRECORD.pNext;
									continue;
								}
								case RecordType.NS | RecordType.MF | RecordType.SOA | RecordType.TXT | RecordType.AFSDB | RecordType.ISDN:
								case RecordType.A | RecordType.NS | RecordType.MD | RecordType.MF | RecordType.CNAME | RecordType.SOA | RecordType.MB | RecordType.TXT | RecordType.RP | RecordType.AFSDB | RecordType.X25 | RecordType.ISDN | RecordType.RT:
								case RecordType.MG | RecordType.TXT:
								case RecordType.A | RecordType.MG | RecordType.MR | RecordType.TXT | RecordType.RP:
								case RecordType.NS | RecordType.MG | RecordType.NULL | RecordType.TXT | RecordType.AFSDB:
								case RecordType.A | RecordType.NS | RecordType.MD | RecordType.MG | RecordType.MR | RecordType.NULL | RecordType.WKS | RecordType.TXT | RecordType.RP | RecordType.AFSDB | RecordType.X25:
								case RecordType.A | RecordType.AAAA | RecordType.MF | RecordType.CNAME | RecordType.MG | RecordType.MR | RecordType.PTR | RecordType.HINFO | RecordType.TXT | RecordType.RP | RecordType.ISDN | RecordType.RT:
								case RecordType.AAAA | RecordType.NS | RecordType.MF | RecordType.SOA | RecordType.MG | RecordType.NULL | RecordType.PTR | RecordType.MINFO | RecordType.TXT | RecordType.AFSDB | RecordType.ISDN:
								case RecordType.A | RecordType.AAAA | RecordType.NS | RecordType.MX | RecordType.MD | RecordType.MF | RecordType.CNAME | RecordType.SOA | RecordType.MB | RecordType.MG | RecordType.MR | RecordType.NULL | RecordType.WKS | RecordType.PTR | RecordType.HINFO | RecordType.MINFO | RecordType.TXT | RecordType.RP | RecordType.AFSDB | RecordType.X25 | RecordType.ISDN | RecordType.RT:
								/*case 32:*/
								/*case 34:*/
								case RecordType.A | RecordType.NS | RecordType.MD | RecordType.SRV:
								/*case 36:*/
								case RecordType.A | RecordType.MF | RecordType.CNAME | RecordType.SRV:
								/*case RecordType.NS | RecordType.MF | RecordType.SOA: */
								/*case 40:*/
								/*case RecordType.NS | RecordType.MG | RecordType.NULL:*/
								/*case RecordType.MF | RecordType.MG | RecordType.PTR:*/
								case RecordType.A | RecordType.MF | RecordType.CNAME | RecordType.MG | RecordType.MR | RecordType.PTR | RecordType.HINFO | RecordType.SRV | RecordType.OPT:
								{
									dnsRecordDefault = new DnsRecord_Default();
									dnsRecordDefault.CharacterSet = structure.flags.CharSet;
									dnsRecordDefault.DataLength = structure.wDataLength;
									dnsRecordDefault.Name = structure.pName;
									dnsRecordDefault.Section = structure.flags.Section;
									dnsRecordDefault.TTL = structure.dwTtl;
									dnsRecordDefault.Type = structure.wType;
									numArray = new byte[structure.wDataLength];
									zero = IntPtr.Zero;
									size = IntPtr.Size;
									if (size == 4)
									{
										zero = new IntPtr(j.ToInt32() + Marshal.SizeOf(structure));
										break;
									}
									else
									{
										if (size == 8)
										{
											zero = new IntPtr(j.ToInt64() + (long)Marshal.SizeOf(structure));
											break;
										}
										else
										{
											break;
										}
									}
								}
								case RecordType.AAAA:
								{
									DnsRecord_AAAA dnsRecordAAAA = new DnsRecord_AAAA();
									TYPEAAAA_DNS_RECORD tYPEAAAADNSRECORD = (TYPEAAAA_DNS_RECORD)Marshal.PtrToStructure(j, typeof(TYPEAAAA_DNS_RECORD));
									if (!utility.IsMappedIpv6Address(tYPEAAAADNSRECORD.AAAAtype.TypeAAAA_DnsRecord))
									{
										dnsRecordAAAA.CharacterSet = tYPEAAAADNSRECORD.flags.CharSet;
										dnsRecordAAAA.Section = tYPEAAAADNSRECORD.flags.Section;
										dnsRecordAAAA.Name = Marshal.PtrToStringAuto(tYPEAAAADNSRECORD.pName);
										dnsRecordAAAA.Type = (RecordType)tYPEAAAADNSRECORD.wType;
										dnsRecordAAAA.TTL = tYPEAAAADNSRECORD.dwTtl;
										dnsRecordAAAA.DataLength = tYPEAAAADNSRECORD.wDataLength;
										dnsRecordAAAA.IP6Address = utility.FormatAsIPv6Address(tYPEAAAADNSRECORD.AAAAtype.TypeAAAA_DnsRecord);
										dnsRecords.Add(dnsRecordAAAA);
										j = tYPEAAAADNSRECORD.pNext;
										continue;
									}
									else
									{
										DnsRecord_A charSet = new DnsRecord_A();
										charSet.CharacterSet = tYPEAAAADNSRECORD.flags.CharSet;
										charSet.Section = tYPEAAAADNSRECORD.flags.Section;
										charSet.Name = Marshal.PtrToStringAuto(tYPEAAAADNSRECORD.pName);
										charSet.Type = RecordType.A;
										charSet.TTL = tYPEAAAADNSRECORD.dwTtl;
										charSet.DataLength = 4;
										typeADnsRecord = new object[4];
										typeADnsRecord[0] = tYPEAAAADNSRECORD.AAAAtype.TypeAAAA_DnsRecord[12];
										typeADnsRecord[1] = tYPEAAAADNSRECORD.AAAAtype.TypeAAAA_DnsRecord[13];
										typeADnsRecord[2] = tYPEAAAADNSRECORD.AAAAtype.TypeAAAA_DnsRecord[14];
										typeADnsRecord[3] = tYPEAAAADNSRECORD.AAAAtype.TypeAAAA_DnsRecord[15];
										charSet.IP4Address = string.Format("{0}.{1}.{2}.{3}", typeADnsRecord);
										dnsRecords.Add(charSet);
										j = tYPEAAAADNSRECORD.pNext;
										continue;
									}
								}
								case RecordType.SRV:
								{
									DnsRecord_SRV dnsRecordSRV = new DnsRecord_SRV();
									TYPESRV_DNS_RECORD tYPESRVDNSRECORD = (TYPESRV_DNS_RECORD)Marshal.PtrToStructure(j, typeof(TYPESRV_DNS_RECORD));
									dnsRecordSRV.NameTarget = Marshal.PtrToStringAuto(tYPESRVDNSRECORD.SRVtype.NameTarget);
									dnsRecordSRV.Priority = tYPESRVDNSRECORD.SRVtype.Priority;
									dnsRecordSRV.Weight = tYPESRVDNSRECORD.SRVtype.Weight;
									dnsRecordSRV.Port = tYPESRVDNSRECORD.SRVtype.Port;
									dnsRecordSRV.CharacterSet = tYPESRVDNSRECORD.flags.CharSet;
									dnsRecordSRV.Section = tYPESRVDNSRECORD.flags.Section;
									dnsRecordSRV.Name = Marshal.PtrToStringAuto(tYPESRVDNSRECORD.pName);
									dnsRecordSRV.Type = (RecordType)tYPESRVDNSRECORD.wType;
									dnsRecordSRV.TTL = tYPESRVDNSRECORD.dwTtl;
									dnsRecordSRV.DataLength = tYPESRVDNSRECORD.wDataLength;
									dnsRecords.Add(dnsRecordSRV);
									j = tYPESRVDNSRECORD.pNext;
									continue;
								}
								case RecordType.OPT:
								{
									DnsRecord_OPT dnsRecordOPT = new DnsRecord_OPT();
									PreMarshal_TYPEOPT_DNS_RECORD preMarshalTYPEOPTDNSRECORD = (PreMarshal_TYPEOPT_DNS_RECORD)Marshal.PtrToStructure(j, typeof(PreMarshal_TYPEOPT_DNS_RECORD));
									IntPtr zero2 = IntPtr.Zero;
									size = IntPtr.Size;
									if (size == 4)
									{
										zero2 = new IntPtr(j.ToInt32() + Marshal.SizeOf(preMarshalTYPEOPTDNSRECORD));
									}
									else
									{
										if (size == 8)
										{
											zero2 = new IntPtr(j.ToInt64() + (long)Marshal.SizeOf(preMarshalTYPEOPTDNSRECORD));
										}
									}
									byte[] numArray2 = new byte[preMarshalTYPEOPTDNSRECORD.OPTtype.DataLength];
									Marshal.Copy(zero2, numArray2, 0, (int)numArray2.Length);
									dnsRecordOPT.Data = numArray2;
									dnsRecordOPT.CharacterSet = preMarshalTYPEOPTDNSRECORD.flags.CharSet;
									dnsRecordOPT.Section = preMarshalTYPEOPTDNSRECORD.flags.Section;
									dnsRecordOPT.Name = Marshal.PtrToStringAuto(preMarshalTYPEOPTDNSRECORD.pName);
									dnsRecordOPT.Type = (RecordType)preMarshalTYPEOPTDNSRECORD.wType;
									dnsRecordOPT.TTL = preMarshalTYPEOPTDNSRECORD.dwTtl;
									dnsRecordOPT.DataLength = preMarshalTYPEOPTDNSRECORD.wDataLength;
									dnsRecords.Add(dnsRecordOPT);
									j = preMarshalTYPEOPTDNSRECORD.pNext;
									continue;
								}
								case RecordType.DS:
								{
									DnsRecord_DS dnsRecordD = new DnsRecord_DS();
									PreMarshal_TYPEDS_DNS_RECORD preMarshalTYPEDSDNSRECORD = (PreMarshal_TYPEDS_DNS_RECORD)Marshal.PtrToStructure(j, typeof(PreMarshal_TYPEDS_DNS_RECORD));
									TYPEDS_DNS_RECORD tYPEDSDNSRECORD = (TYPEDS_DNS_RECORD)Marshal.PtrToStructure(j, typeof(TYPEDS_DNS_RECORD));
									byte[] numArray3 = new byte[preMarshalTYPEDSDNSRECORD.DStype.DigestLength];
									size = IntPtr.Size;
									if (size == 4)
									{
										IntPtr intPtr3 = new IntPtr(j.ToInt32() + Marshal.SizeOf(preMarshalTYPEDSDNSRECORD));
										Marshal.Copy(intPtr3, numArray3, 0, (int)numArray3.Length);
									}
									else
									{
										if (size == 8)
										{
											IntPtr intPtr4 = new IntPtr(j.ToInt64() + (long)Marshal.SizeOf(preMarshalTYPEDSDNSRECORD));
											Marshal.Copy(intPtr4, numArray3, 0, (int)numArray3.Length);
										}
									}
									dnsRecordD.Digest = new byte[tYPEDSDNSRECORD.DStype.DigestLength];
									for (int l = 0; l < tYPEDSDNSRECORD.DStype.DigestLength; l++)
									{
										dnsRecordD.Digest[l] = numArray3[l];
									}
									dnsRecordD.KeyTag = tYPEDSDNSRECORD.DStype.KeyTag;
									dnsRecordD.Algorithm = (EncryptionAlgorithm)tYPEDSDNSRECORD.DStype.Algorithm;
									dnsRecordD.DType = (DigestType)tYPEDSDNSRECORD.DStype.DigestType;
									dnsRecordD.CharacterSet = tYPEDSDNSRECORD.flags.CharSet;
									dnsRecordD.Section = tYPEDSDNSRECORD.flags.Section;
									dnsRecordD.Name = Marshal.PtrToStringAuto(tYPEDSDNSRECORD.pName);
									dnsRecordD.Type = (RecordType)tYPEDSDNSRECORD.wType;
									dnsRecordD.TTL = tYPEDSDNSRECORD.dwTtl;
									dnsRecordD.DataLength = tYPEDSDNSRECORD.wDataLength;
									dnsRecords.Add(dnsRecordD);
									j = tYPEDSDNSRECORD.pNext;
									continue;
								}
								case RecordType.RRSIG:
								{
									DnsRecord_RRSIG dnsRecordRRSIG = new DnsRecord_RRSIG();
									PreMarshal_TYPERRSIG_DNS_RECORD preMarshalTYPERRSIGDNSRECORD = (PreMarshal_TYPERRSIG_DNS_RECORD)Marshal.PtrToStructure(j, typeof(PreMarshal_TYPERRSIG_DNS_RECORD));
									IntPtr zero3 = IntPtr.Zero;
									size = IntPtr.Size;
									if (size == 4)
									{
										zero3 = new IntPtr(j.ToInt32() + Marshal.SizeOf(preMarshalTYPERRSIGDNSRECORD));
									}
									else
									{
										if (size == 8)
										{
											zero3 = new IntPtr(j.ToInt64() + (long)Marshal.SizeOf(preMarshalTYPERRSIGDNSRECORD));
										}
									}
									byte[] numArray4 = new byte[preMarshalTYPERRSIGDNSRECORD.RRSIGtype.SignatureLength];
									Marshal.Copy(zero3, numArray4, 0, (int)numArray4.Length);
									dnsRecordRRSIG.Signature = new byte[(int)numArray4.Length];
									for (int m = 0; m < (int)numArray4.Length; m++)
									{
										dnsRecordRRSIG.Signature[m] = numArray4[m];
									}
									dnsRecordRRSIG.Algorithm = (EncryptionAlgorithm)preMarshalTYPERRSIGDNSRECORD.RRSIGtype.Algorithm;
									dnsRecordRRSIG.TypeCovered = (RecordType)preMarshalTYPERRSIGDNSRECORD.RRSIGtype.TypeCovered;
									dnsRecordRRSIG.LabelCount = preMarshalTYPERRSIGDNSRECORD.RRSIGtype.LabelCount;
									dnsRecordRRSIG.OriginalTtl = preMarshalTYPERRSIGDNSRECORD.RRSIGtype.OriginalTtl;
									DateTime dateTime = new DateTime(0x7b2, 1, 1, 0, 0, 0, DateTimeKind.Utc);
									dnsRecordRRSIG.Expiration = dateTime.AddSeconds((double)((float)preMarshalTYPERRSIGDNSRECORD.RRSIGtype.Expiration));
									dnsRecordRRSIG.Signed = dateTime.AddSeconds((double)((float)preMarshalTYPERRSIGDNSRECORD.RRSIGtype.TimeSigned));
									dnsRecordRRSIG.KeyTag = preMarshalTYPERRSIGDNSRECORD.RRSIGtype.KeyTag;
									dnsRecordRRSIG.Signer = Marshal.PtrToStringAuto(preMarshalTYPERRSIGDNSRECORD.RRSIGtype.NameSigner);
									dnsRecordRRSIG.CharacterSet = preMarshalTYPERRSIGDNSRECORD.flags.CharSet;
									dnsRecordRRSIG.Section = preMarshalTYPERRSIGDNSRECORD.flags.Section;
									dnsRecordRRSIG.Name = Marshal.PtrToStringAuto(preMarshalTYPERRSIGDNSRECORD.pName);
									dnsRecordRRSIG.Type = (RecordType)preMarshalTYPERRSIGDNSRECORD.wType;
									dnsRecordRRSIG.TTL = preMarshalTYPERRSIGDNSRECORD.dwTtl;
									dnsRecordRRSIG.DataLength = preMarshalTYPERRSIGDNSRECORD.wDataLength;
									dnsRecords.Add(dnsRecordRRSIG);
									j = preMarshalTYPERRSIGDNSRECORD.pNext;
									continue;
								}
								case RecordType.NSEC:
								{
									DnsRecord_NSEC dnsRecordNSEC = new DnsRecord_NSEC();
									PreMarshal_TYPENSEC_DNS_RECORD preMarshalTYPENSECDNSRECORD = (PreMarshal_TYPENSEC_DNS_RECORD)Marshal.PtrToStructure(j, typeof(PreMarshal_TYPENSEC_DNS_RECORD));
									dnsRecordNSEC.NextDomainName = Marshal.PtrToStringAuto(preMarshalTYPENSECDNSRECORD.NSECtype.NextDomainName);
									IntPtr zero4 = IntPtr.Zero;
									size = IntPtr.Size;
									if (size == 4)
									{
										zero4 = new IntPtr(j.ToInt32() + Marshal.SizeOf(preMarshalTYPENSECDNSRECORD));
									}
									else
									{
										if (size == 8)
										{
											zero4 = new IntPtr(j.ToInt64() + (long)Marshal.SizeOf(preMarshalTYPENSECDNSRECORD));
										}
									}
									byte[] numArray5 = new byte[preMarshalTYPENSECDNSRECORD.NSECtype.BitMapLength];
									Marshal.Copy(zero4, numArray5, 0, (int)numArray5.Length);
									dnsRecordNSEC.TypeBitMap = numArray5;
									dnsRecordNSEC.CharacterSet = preMarshalTYPENSECDNSRECORD.flags.CharSet;
									dnsRecordNSEC.Section = preMarshalTYPENSECDNSRECORD.flags.Section;
									dnsRecordNSEC.Name = Marshal.PtrToStringAuto(preMarshalTYPENSECDNSRECORD.pName);
									dnsRecordNSEC.Type = (RecordType)preMarshalTYPENSECDNSRECORD.wType;
									dnsRecordNSEC.TTL = preMarshalTYPENSECDNSRECORD.dwTtl;
									dnsRecordNSEC.DataLength = preMarshalTYPENSECDNSRECORD.wDataLength;
									dnsRecords.Add(dnsRecordNSEC);
									j = preMarshalTYPENSECDNSRECORD.pNext;
									continue;
								}
								case RecordType.DNSKEY:
								{
									DnsRecord_DNSKEY dnsRecordDNSKEY = new DnsRecord_DNSKEY();
									PreMarshal_TYPEDNSKEY_DNS_RECORD preMarshalTYPEDNSKEYDNSRECORD = (PreMarshal_TYPEDNSKEY_DNS_RECORD)Marshal.PtrToStructure(j, typeof(PreMarshal_TYPEDNSKEY_DNS_RECORD));
									IntPtr zero5 = IntPtr.Zero;
									size = IntPtr.Size;
									if (size == 4)
									{
										zero5 = new IntPtr(j.ToInt32() + Marshal.SizeOf(preMarshalTYPEDNSKEYDNSRECORD));
									}
									else
									{
										if (size == 8)
										{
											zero5 = new IntPtr(j.ToInt64() + (long)Marshal.SizeOf(preMarshalTYPEDNSKEYDNSRECORD));
										}
									}
									byte[] numArray6 = new byte[preMarshalTYPEDNSKEYDNSRECORD.DNSKEYtype.KeyLength];
									Marshal.Copy(zero5, numArray6, 0, (int)numArray6.Length);
									dnsRecordDNSKEY.Key = numArray6;
									dnsRecordDNSKEY.Algorithm = (EncryptionAlgorithm)preMarshalTYPEDNSKEYDNSRECORD.DNSKEYtype.Algorithm;
									dnsRecordDNSKEY.Flags = preMarshalTYPEDNSKEYDNSRECORD.DNSKEYtype.Flags;
									dnsRecordDNSKEY.Protocol = (KeyProtocol)preMarshalTYPEDNSKEYDNSRECORD.DNSKEYtype.Protocol;
									dnsRecordDNSKEY.CharacterSet = preMarshalTYPEDNSKEYDNSRECORD.flags.CharSet;
									dnsRecordDNSKEY.Section = preMarshalTYPEDNSKEYDNSRECORD.flags.Section;
									dnsRecordDNSKEY.Name = Marshal.PtrToStringAuto(preMarshalTYPEDNSKEYDNSRECORD.pName);
									dnsRecordDNSKEY.Type = (RecordType)preMarshalTYPEDNSKEYDNSRECORD.wType;
									dnsRecordDNSKEY.TTL = preMarshalTYPEDNSKEYDNSRECORD.dwTtl;
									dnsRecordDNSKEY.DataLength = preMarshalTYPEDNSKEYDNSRECORD.wDataLength;
									dnsRecords.Add(dnsRecordDNSKEY);
									j = preMarshalTYPEDNSKEYDNSRECORD.pNext;
									continue;
								}
								case RecordType.NSEC3:
								{
									DnsRecord_NSEC3 dnsRecordNSEC3 = new DnsRecord_NSEC3();
									PreMarshal_TYPENSEC3_DNS_RECORD preMarshalTYPENSEC3DNSRECORD = (PreMarshal_TYPENSEC3_DNS_RECORD)Marshal.PtrToStructure(j, typeof(PreMarshal_TYPENSEC3_DNS_RECORD));
									dnsRecordNSEC3.Algorithm = preMarshalTYPENSEC3DNSRECORD.NSEC3type.Algorithm;
									dnsRecordNSEC3.Flags = preMarshalTYPENSEC3DNSRECORD.NSEC3type.Flags;
									dnsRecordNSEC3.Iterations = preMarshalTYPENSEC3DNSRECORD.NSEC3type.Iterations;
									dnsRecordNSEC3.SaltLength = preMarshalTYPENSEC3DNSRECORD.NSEC3type.SaltLength;
									dnsRecordNSEC3.HashLength = preMarshalTYPENSEC3DNSRECORD.NSEC3type.HashLength;
									IntPtr intPtr5 = IntPtr.Zero;
									size = IntPtr.Size;
									if (size == 4)
									{
										intPtr5 = new IntPtr(j.ToInt32() + Marshal.SizeOf(preMarshalTYPENSEC3DNSRECORD));
									}
									else
									{
										if (size == 8)
										{
											intPtr5 = new IntPtr(j.ToInt64() + (long)Marshal.SizeOf(preMarshalTYPENSEC3DNSRECORD));
										}
									}
									byte[] numArray7 = new byte[preMarshalTYPENSEC3DNSRECORD.NSEC3type.TypeBitMapsLength];
									Marshal.Copy(intPtr5, numArray7, 0, (int)numArray7.Length);
									dnsRecordNSEC3.Data = numArray7;
									dnsRecordNSEC3.CharacterSet = preMarshalTYPENSEC3DNSRECORD.flags.CharSet;
									dnsRecordNSEC3.Section = preMarshalTYPENSEC3DNSRECORD.flags.Section;
									dnsRecordNSEC3.Name = Marshal.PtrToStringAuto(preMarshalTYPENSEC3DNSRECORD.pName);
									dnsRecordNSEC3.Type = (RecordType)preMarshalTYPENSEC3DNSRECORD.wType;
									dnsRecordNSEC3.TTL = preMarshalTYPENSEC3DNSRECORD.dwTtl;
									dnsRecordNSEC3.DataLength = preMarshalTYPENSEC3DNSRECORD.wDataLength;
									dnsRecords.Add(dnsRecordNSEC3);
									j = preMarshalTYPENSEC3DNSRECORD.pNext;
									continue;
								}
								case RecordType.NSEC3PARAM:
								{
									DnsRecord_NSEC3PARAM dnsRecordNSEC3PARAM = new DnsRecord_NSEC3PARAM();
									PreMarshal_TYPENSEC3PARAM_DNS_RECORD preMarshalTYPENSEC3PARAMDNSRECORD = (PreMarshal_TYPENSEC3PARAM_DNS_RECORD)Marshal.PtrToStructure(j, typeof(PreMarshal_TYPENSEC3PARAM_DNS_RECORD));
									dnsRecordNSEC3PARAM.Algorithm = preMarshalTYPENSEC3PARAMDNSRECORD.NSEC3PARAMtype.Algorithm;
									dnsRecordNSEC3PARAM.Flags = preMarshalTYPENSEC3PARAMDNSRECORD.NSEC3PARAMtype.Flags;
									dnsRecordNSEC3PARAM.Iterations = preMarshalTYPENSEC3PARAMDNSRECORD.NSEC3PARAMtype.Iterations;
									dnsRecordNSEC3PARAM.SaltLength = preMarshalTYPENSEC3PARAMDNSRECORD.NSEC3PARAMtype.SaltLength;
									IntPtr zero6 = IntPtr.Zero;
									size = IntPtr.Size;
									if (size == 4)
									{
										zero6 = new IntPtr(j.ToInt32() + Marshal.SizeOf(preMarshalTYPENSEC3PARAMDNSRECORD));
									}
									else
									{
										if (size == 8)
										{
											zero6 = new IntPtr(j.ToInt64() + (long)Marshal.SizeOf(preMarshalTYPENSEC3PARAMDNSRECORD));
										}
									}
									byte[] numArray8 = new byte[preMarshalTYPENSEC3PARAMDNSRECORD.NSEC3PARAMtype.SaltLength];
									Marshal.Copy(zero6, numArray8, 0, (int)numArray8.Length);
									dnsRecordNSEC3PARAM.Salt = numArray8;
									dnsRecordNSEC3PARAM.CharacterSet = preMarshalTYPENSEC3PARAMDNSRECORD.flags.CharSet;
									dnsRecordNSEC3PARAM.Section = preMarshalTYPENSEC3PARAMDNSRECORD.flags.Section;
									dnsRecordNSEC3PARAM.Name = Marshal.PtrToStringAuto(preMarshalTYPENSEC3PARAMDNSRECORD.pName);
									dnsRecordNSEC3PARAM.Type = (RecordType)preMarshalTYPENSEC3PARAMDNSRECORD.wType;
									dnsRecordNSEC3PARAM.TTL = preMarshalTYPENSEC3PARAMDNSRECORD.dwTtl;
									dnsRecordNSEC3PARAM.DataLength = preMarshalTYPENSEC3PARAMDNSRECORD.wDataLength;
									dnsRecords.Add(dnsRecordNSEC3PARAM);
									j = preMarshalTYPENSEC3PARAMDNSRECORD.pNext;
									continue;
								}
								default:
								{
									if (recordType == RecordType.WINS)
									{
										DnsRecord_WINS dnsRecordWIN = new DnsRecord_WINS();
										uint num1 = 0;
										PreMarshal_TYPEWINS_DNS_RECORD preMarshalTYPEWINSDNSRECORD = (PreMarshal_TYPEWINS_DNS_RECORD)Marshal.PtrToStructure(j, typeof(PreMarshal_TYPEWINS_DNS_RECORD));
										new List<string>();
										IntPtr intPtr6 = IntPtr.Zero;
										byte[] numArray9 = new byte[4];
										dnsRecordWIN.IP4Addresses = new string[preMarshalTYPEWINSDNSRECORD.WINStype.WinsServerCount];
										while (num1 < preMarshalTYPEWINSDNSRECORD.WINStype.WinsServerCount)
										{
											size = IntPtr.Size;
											if (size == 4)
											{
												intPtr6 = new IntPtr((long)(j.ToInt32() + Marshal.SizeOf(preMarshalTYPEWINSDNSRECORD)) + (long)(num1 * 4));
											}
											else
											{
												if (size == 8)
												{
													intPtr6 = new IntPtr(j.ToInt64() + (long)Marshal.SizeOf(preMarshalTYPEWINSDNSRECORD) + (long)(num1 * 4));
												}
											}
											Marshal.Copy(intPtr6, numArray9, 0, Marshal.SizeOf(num1));
											typeADnsRecord = new object[4];
											typeADnsRecord[0] = numArray9[0];
											typeADnsRecord[1] = numArray9[1];
											typeADnsRecord[2] = numArray9[2];
											typeADnsRecord[3] = numArray9[3];
											dnsRecordWIN.IP4Addresses[num1] = string.Format("{0}.{1}.{2}.{3}", typeADnsRecord);
											num1++;
										}
										TYPEWINS_DNS_RECORD tYPEWINSDNSRECORD = (TYPEWINS_DNS_RECORD)Marshal.PtrToStructure(j, typeof(TYPEWINS_DNS_RECORD));
										dnsRecordWIN.MappingFlag = (WINSMappingFlag)tYPEWINSDNSRECORD.WINStype.MappingFlag;
										dnsRecordWIN.LookupTimeout = tYPEWINSDNSRECORD.WINStype.LookupTimeout;
										dnsRecordWIN.CacheTimeout = tYPEWINSDNSRECORD.WINStype.CacheTimeout;
										dnsRecordWIN.CharacterSet = tYPEWINSDNSRECORD.flags.CharSet;
										dnsRecordWIN.Section = tYPEWINSDNSRECORD.flags.Section;
										dnsRecordWIN.Name = Marshal.PtrToStringAuto(tYPEWINSDNSRECORD.pName);
										dnsRecordWIN.Type = (RecordType)tYPEWINSDNSRECORD.wType;
										dnsRecordWIN.TTL = tYPEWINSDNSRECORD.dwTtl;
										dnsRecordWIN.DataLength = tYPEWINSDNSRECORD.wDataLength;
										dnsRecords.Add(dnsRecordWIN);
										j = tYPEWINSDNSRECORD.pNext;
										continue;
									}
									else
									{
										dnsRecordDefault = new DnsRecord_Default();
										dnsRecordDefault.CharacterSet = structure.flags.CharSet;
										dnsRecordDefault.DataLength = structure.wDataLength;
										dnsRecordDefault.Name = structure.pName;
										dnsRecordDefault.Section = structure.flags.Section;
										dnsRecordDefault.TTL = structure.dwTtl;
										dnsRecordDefault.Type = structure.wType;
										numArray = new byte[structure.wDataLength];
										zero = IntPtr.Zero;
										size = IntPtr.Size;
										if (size == 4)
										{
											zero = new IntPtr(j.ToInt32() + Marshal.SizeOf(structure));
											break;
										}
										else
										{
											if (size == 8)
											{
												zero = new IntPtr(j.ToInt64() + (long)Marshal.SizeOf(structure));
												break;
											}
											else
											{
												break;
											}
										}
									}
								}
							}
							Marshal.Copy(zero, numArray, 0, structure.wDataLength);
							dnsRecordDefault.Data = new byte[structure.wDataLength];
							dnsRecordDefault.Data = numArray;
							dnsRecords.Add(dnsRecordDefault);
						}
						win32.DnsRecordListFree(intPtr, 0);
						return dnsRecords;
					}
					else
					{
						Win32Exception win32Exception = new Win32Exception(num);
						Win32Exception win32Exception1 = new Win32Exception(num, string.Concat(QueryName, " : ", win32Exception.Message));
						throw win32Exception1;
					}
				}
				else
				{
					throw new NotSupportedException();
				}
			}
		}

		internal static string GetPrimaryDomainName()
		{
			IntPtr zero = IntPtr.Zero;
			IntPtr intPtr = IntPtr.Zero;
			int num = 0;
			int num1 = win32.DnsQueryConfig(DnsConfigType.DnsConfigPrimaryDomainName_W, 0, null, zero, ref intPtr, ref num);
			if (num1 == 234)
			{
				intPtr = Marshal.AllocHGlobal(num);
				num1 = win32.DnsQueryConfig(DnsConfigType.DnsConfigPrimaryDomainName_W, 1, null, zero, ref intPtr, ref num);
				string stringAuto = Marshal.PtrToStringAuto(intPtr);
				if (intPtr != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(intPtr);
				}
				return stringAuto;
			}
			else
			{
				throw new Win32Exception(num1);
			}
		}

		internal static IntPtr LocalFree(IntPtr hMem)
		{
			return win32._LocalFree(hMem);
		}
	}
}