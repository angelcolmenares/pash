using System;
using System.Collections;
using System.Globalization;
using System.Runtime.InteropServices;

namespace System.DirectoryServices.ActiveDirectory
{
	internal sealed class Locator
	{
		private Locator()
		{
		}

		private static Hashtable DnsGetDcWrapper(string domainName, string siteName, long dcFlags)
		{
			Hashtable hashtables = new Hashtable();
			int num = 0;
			IntPtr zero = IntPtr.Zero;
			IntPtr intPtr = IntPtr.Zero;
			int num1 = 0;
			IntPtr intPtr1 = new IntPtr(num1);
			IntPtr zero1 = IntPtr.Zero;
			int num2 = NativeMethods.DsGetDcOpen(domainName, num, siteName, IntPtr.Zero, null, (int)dcFlags, out zero);
			if (num2 != 0)
			{
				if (num2 != 0)
				{
					throw ExceptionHelper.GetExceptionFromErrorCode(num2);
				}
			}
			else
			{
				try
				{
					num2 = NativeMethods.DsGetDcNext(zero, out intPtr1, out zero1, out intPtr);
					if (num2 == 0 || num2 == 0x44d || num2 == 0x232b || num2 == 0x103)
					{
						while (num2 != 0x103)
						{
							if (num2 != 0x44d && num2 != 0x232b)
							{
								try
								{
									string stringUni = Marshal.PtrToStringUni(intPtr);
									string lower = stringUni.ToLower(CultureInfo.InvariantCulture);
									if (!hashtables.Contains(lower))
									{
										hashtables.Add(lower, null);
									}
								}
								finally
								{
									if (intPtr != IntPtr.Zero)
									{
										num2 = NativeMethods.NetApiBufferFree(intPtr);
									}
								}
							}
							num2 = NativeMethods.DsGetDcNext(zero, out intPtr1, out zero1, out intPtr);
							if (num2 == 0 || num2 == 0x44d || num2 == 0x232b || num2 == 0x103)
							{
								continue;
							}
							throw ExceptionHelper.GetExceptionFromErrorCode(num2);
						}
					}
					else
					{
						throw ExceptionHelper.GetExceptionFromErrorCode(num2);
					}
				}
				finally
				{
					NativeMethods.DsGetDcClose(zero);
				}
			}
			return hashtables;
		}

		private static Hashtable DnsQueryWrapper(string domainName, string siteName, long dcFlags)
		{
			PartialDnsRecord partialDnsRecord = null;
			Hashtable hashtables = new Hashtable();
			string str = "_ldap._tcp.";
			int num = 0;
			IntPtr zero = IntPtr.Zero;
			if (siteName != null && siteName.Length != 0)
			{
				str = string.Concat(str, siteName, "._sites.");
			}
			if ((dcFlags & (long)64) == (long)0)
			{
				if ((dcFlags & (long)0x1000) != (long)0)
				{
					str = string.Concat(str, "dc._msdcs.");
				}
			}
			else
			{
				str = string.Concat(str, "gc._msdcs.");
			}
			str = string.Concat(str, domainName);
			if ((dcFlags & (long)1) != (long)0)
			{
				num = num | 8;
			}
			int num1 = NativeMethods.DnsQuery(str, 33, num, IntPtr.Zero, out zero, IntPtr.Zero);
			if (num1 != 0)
			{
				if (num1 != 0)
				{
					throw ExceptionHelper.GetExceptionFromErrorCode(num1);
				}
			}
			else
			{
				try
				{
					for (IntPtr i = zero; i != IntPtr.Zero; i = partialDnsRecord.next)
					{
						partialDnsRecord = new PartialDnsRecord();
						Marshal.PtrToStructure(i, partialDnsRecord);
						if (partialDnsRecord.type == 33)
						{
							DnsRecord dnsRecord = new DnsRecord();
							Marshal.PtrToStructure(i, dnsRecord);
							string str1 = dnsRecord.data.targetName;
							string lower = str1.ToLower(CultureInfo.InvariantCulture);
							if (!hashtables.Contains(lower))
							{
								hashtables.Add(lower, null);
							}
						}
					}
				}
				finally
				{
					if (zero != IntPtr.Zero)
					{
						NativeMethods.DnsRecordListFree(zero, true);
					}
				}
			}
			return hashtables;
		}

		internal static int DsGetDcNameWrapper(string computerName, string domainName, string siteName, long flags, out DomainControllerInfo domainControllerInfo)
		{
			IntPtr zero = IntPtr.Zero;
			if (computerName != null && computerName.Length == 0)
			{
				computerName = null;
			}
			if (siteName != null && siteName.Length == 0)
			{
				siteName = null;
			}
			int num = NativeMethods.DsGetDcName(computerName, domainName, IntPtr.Zero, siteName, (int)(flags | (long)0x40000000), out zero);
			if (num != 0)
			{
				domainControllerInfo = new DomainControllerInfo();
			}
			else
			{
				try
				{
					domainControllerInfo = new DomainControllerInfo();
					Marshal.PtrToStructure(zero, domainControllerInfo);
				}
				finally
				{
					if (zero != IntPtr.Zero)
					{
						num = NativeMethods.NetApiBufferFree(zero);
					}
				}
			}
			return num;
		}

		internal static ArrayList EnumerateDomainControllers(DirectoryContext context, string domainName, string siteName, long dcFlags)
		{
			Hashtable hashtables;
			DomainControllerInfo domainControllerInfo = null;
			ArrayList arrayLists = new ArrayList();
			if (siteName == null)
			{
				int num = Locator.DsGetDcNameWrapper(null, domainName, null, dcFlags & (long)0x9040, out domainControllerInfo);
				if (num != 0)
				{
					if (num != 0x54b)
					{
						throw ExceptionHelper.GetExceptionFromErrorCode(num);
					}
					else
					{
						return arrayLists;
					}
				}
				else
				{
					siteName = domainControllerInfo.ClientSiteName;
				}
			}
			if (!DirectoryContext.DnsgetdcSupported)
			{
				hashtables = Locator.DnsQueryWrapper(domainName, null, dcFlags);
				if (siteName != null)
				{
					foreach (string key in Locator.DnsQueryWrapper(domainName, siteName, dcFlags).Keys)
					{
						if (hashtables.Contains(key))
						{
							continue;
						}
						hashtables.Add(key, null);
					}
				}
			}
			else
			{
				hashtables = Locator.DnsGetDcWrapper(domainName, siteName, dcFlags);
			}
			foreach (string str in hashtables.Keys)
			{
				DirectoryContext newDirectoryContext = Utils.GetNewDirectoryContext(str, DirectoryContextType.DirectoryServer, context);
				if ((dcFlags & (long)64) == (long)0)
				{
					arrayLists.Add(new DomainController(newDirectoryContext, str));
				}
				else
				{
					arrayLists.Add(new GlobalCatalog(newDirectoryContext, str));
				}
			}
			return arrayLists;
		}

		internal static DomainControllerInfo GetDomainControllerInfo(string computerName, string domainName, string siteName, long flags)
		{
			DomainControllerInfo domainControllerInfo = null;
			int num = Locator.DsGetDcNameWrapper(computerName, domainName, siteName, flags, out domainControllerInfo);
			if (num == 0)
			{
				return domainControllerInfo;
			}
			else
			{
				throw ExceptionHelper.GetExceptionFromErrorCode(num, domainName);
			}
		}
	}
}