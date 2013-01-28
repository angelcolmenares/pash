using System;
using System.Collections;
using System.DirectoryServices;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using System.Text;

namespace System.DirectoryServices.ActiveDirectory
{
	internal sealed class Utils
	{
		private static int LOGON32_LOGON_NEW_CREDENTIALS;

		private static int LOGON32_PROVIDER_WINNT50;

		private static int POLICY_VIEW_LOCAL_INFORMATION;

		private static uint STANDARD_RIGHTS_REQUIRED;

		private static uint SYNCHRONIZE;

		private static uint THREAD_ALL_ACCESS;

		internal static AuthenticationTypes DefaultAuthType;

		private static uint LANG_ENGLISH;

		private static uint SUBLANG_ENGLISH_US;

		private static uint SORT_DEFAULT;

		private static uint LANGID;

		private static uint LCID;

		internal static uint NORM_IGNORECASE;

		internal static uint NORM_IGNORENONSPACE;

		internal static uint NORM_IGNOREKANATYPE;

		internal static uint NORM_IGNOREWIDTH;

		internal static uint SORT_STRINGSORT;

		internal static uint DEFAULT_CMP_FLAGS;

		private static string NTAuthorityString;

		static Utils()
		{
			Utils.LOGON32_LOGON_NEW_CREDENTIALS = 9;
			Utils.LOGON32_PROVIDER_WINNT50 = 3;
			Utils.POLICY_VIEW_LOCAL_INFORMATION = 1;
			Utils.STANDARD_RIGHTS_REQUIRED = 0xf0000;
			Utils.SYNCHRONIZE = 0x100000;
			Utils.THREAD_ALL_ACCESS = Utils.STANDARD_RIGHTS_REQUIRED | Utils.SYNCHRONIZE | 0x3ff;
			Utils.DefaultAuthType = AuthenticationTypes.Secure | AuthenticationTypes.Signing | AuthenticationTypes.Sealing;
			Utils.LANG_ENGLISH = 9;
			Utils.SUBLANG_ENGLISH_US = 1;
			Utils.SORT_DEFAULT = 0;
			Utils.LANGID = Convert.ToUInt32 ((ushort)Utils.SUBLANG_ENGLISH_US << 10 | (ushort)Utils.LANG_ENGLISH);
			Utils.LCID = Convert.ToUInt32 ((ushort)Utils.SORT_DEFAULT << 16 | (ushort)Utils.LANGID);
			Utils.NORM_IGNORECASE = 1;
			Utils.NORM_IGNORENONSPACE = 2;
			Utils.NORM_IGNOREKANATYPE = 0x10000;
			Utils.NORM_IGNOREWIDTH = 0x20000;
			Utils.SORT_STRINGSORT = 0x1000;
			Utils.DEFAULT_CMP_FLAGS = Utils.NORM_IGNORECASE | Utils.NORM_IGNOREKANATYPE | Utils.NORM_IGNORENONSPACE | Utils.NORM_IGNOREWIDTH | Utils.SORT_STRINGSORT;
			Utils.NTAuthorityString = null;
		}

		private Utils()
		{
		}

		internal static bool CheckCapability(DirectoryEntry rootDSE, Capability capability)
		{
			bool flag = false;
			if (rootDSE != null)
			{
				if (capability != Capability.ActiveDirectory)
				{
					if (capability != Capability.ActiveDirectoryApplicationMode)
					{
						if (capability == Capability.ActiveDirectoryOrADAM)
						{
							foreach (string item in rootDSE.Properties[PropertyManager.SupportedCapabilities])
							{
								if (string.Compare(item, SupportedCapability.ADAMOid, StringComparison.OrdinalIgnoreCase) != 0 && string.Compare(item, SupportedCapability.ADOid, StringComparison.OrdinalIgnoreCase) != 0)
								{
									continue;
								}
								flag = true;
								break;
							}
						}
					}
					else
					{
						foreach (string str in rootDSE.Properties[PropertyManager.SupportedCapabilities])
						{
							if (string.Compare(str, SupportedCapability.ADAMOid, StringComparison.OrdinalIgnoreCase) != 0)
							{
								continue;
							}
							flag = true;
							break;
						}
					}
				}
				else
				{
					foreach (string item1 in rootDSE.Properties[PropertyManager.SupportedCapabilities])
					{
						if (string.Compare(item1, SupportedCapability.ADOid, StringComparison.OrdinalIgnoreCase) != 0)
						{
							continue;
						}
						flag = true;
						break;
					}
				}
			}
			return flag;
		}

		[SecuritySafeCritical]
		internal static SidType ClassifySID(IntPtr pSid)
		{
			IntPtr sidIdentifierAuthority = UnsafeNativeMethods.GetSidIdentifierAuthority(pSid);
			SID_IDENTIFIER_AUTHORITY structure = (SID_IDENTIFIER_AUTHORITY)Marshal.PtrToStructure(sidIdentifierAuthority, typeof(SID_IDENTIFIER_AUTHORITY));
			IntPtr sidSubAuthority = UnsafeNativeMethods.GetSidSubAuthority(pSid, 0);
			int num = Marshal.ReadInt32(sidSubAuthority);
			if ((structure.b3 & 240) != 16)
			{
				if (structure.b1 == 0 || structure.b2 != 0 || structure.b3 != 0 || structure.b4 != 0 || structure.b5 != 0 || structure.b6 != 5)
				{
					int num1 = num;
					if (num1 == 21)
					{
						return SidType.RealObject;
					}
					else
					{
						if (num1 == 32)
						{
							return SidType.RealObjectFakeDomain;
						}
						else
						{
							return SidType.FakeObject;
						}
					}
				}
				else
				{
					return SidType.FakeObject;
				}
			}
			else
			{
				return SidType.RealObject;
			}
		}

		internal static int Compare(string s1, string s2, uint compareFlags)
		{
			if (s1 == null || s2 == null)
			{
				return string.Compare(s1, s2);
			}
			else
			{
				int num = 0;
				num = string.Compare (s1, s2, true);
				return num;
				/*
				IntPtr zero = IntPtr.Zero;
				IntPtr hGlobalUni = IntPtr.Zero;
				try
				{
					zero = Marshal.StringToHGlobalUni(s1);
					int length = s1.Length;
					hGlobalUni = Marshal.StringToHGlobalUni(s2);
					int length1 = s2.Length;
					num = NativeMethods.CompareString(Utils.LCID, compareFlags, zero, length, hGlobalUni, length1);
					if (num == 0)
					{
						throw ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
					}
				}
				finally
				{
					if (zero != IntPtr.Zero)
					{
						Marshal.FreeHGlobal(zero);
					}
					if (hGlobalUni != IntPtr.Zero)
					{
						Marshal.FreeHGlobal(hGlobalUni);
					}
				}
				return num - 2; 
				*/
			}
		}

		internal static int Compare(string s1, string s2)
		{
			return Utils.Compare(s1, s2, Utils.DEFAULT_CMP_FLAGS);
		}

		internal static int Compare(string s1, int offset1, int length1, string s2, int offset2, int length2)
		{
			if (s1 != null)
			{
				if (s2 != null)
				{
					return Utils.Compare(s1.Substring(offset1, length1), s2.Substring(offset2, length2));
				}
				else
				{
					throw new ArgumentNullException("s2");
				}
			}
			else
			{
				throw new ArgumentNullException("s1");
			}
		}

		internal static int Compare(string s1, int offset1, int length1, string s2, int offset2, int length2, uint compareFlags)
		{
			if (s1 != null)
			{
				if (s2 != null)
				{
					return Utils.Compare(s1.Substring(offset1, length1), s2.Substring(offset2, length2), compareFlags);
				}
				else
				{
					throw new ArgumentNullException("s2");
				}
			}
			else
			{
				throw new ArgumentNullException("s1");
			}
		}

		[SecurityCritical]
		internal static IntPtr ConvertByteArrayToIntPtr(byte[] bytes)
		{
			IntPtr intPtr = Marshal.AllocHGlobal((int)bytes.Length);
			try
			{
				Marshal.Copy(bytes, 0, intPtr, (int)bytes.Length);
			}
			catch (Exception exception)
			{
				Marshal.FreeHGlobal(intPtr);
				throw;
			}
			return intPtr;
		}

		internal static void FreeAuthIdentity(IntPtr authIdentity, LoadLibrarySafeHandle libHandle)
		{
			if (authIdentity != IntPtr.Zero)
			{
				IntPtr procAddress = UnsafeNativeMethods.GetProcAddress(libHandle, "DsFreePasswordCredentials");
				if (procAddress != (IntPtr)0)
				{
					NativeMethods.DsFreePasswordCredentials delegateForFunctionPointer = (NativeMethods.DsFreePasswordCredentials)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(NativeMethods.DsFreePasswordCredentials));
					delegateForFunctionPointer(authIdentity);
				}
				else
				{
					throw ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
				}
			}
		}

		internal static void FreeDSHandle(IntPtr dsHandle, LoadLibrarySafeHandle libHandle)
		{
			if (dsHandle != IntPtr.Zero)
			{
				IntPtr procAddress = UnsafeNativeMethods.GetProcAddress(libHandle, "DsUnBindW");
				if (procAddress != (IntPtr)0)
				{
					NativeMethods.DsUnBind delegateForFunctionPointer = (NativeMethods.DsUnBind)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(NativeMethods.DsUnBind));
					delegateForFunctionPointer(ref dsHandle);
				}
				else
				{
					throw ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
				}
			}
		}

		internal static string GetAdamDnsHostNameFromNTDSA(DirectoryContext context, string dn)
		{
			string searchResultPropertyValue = null;
			int num = -1;
			string str = dn;
			string partialDN = Utils.GetPartialDN(dn, 1);
			string partialDN1 = Utils.GetPartialDN(dn, 2);
			string str1 = "CN=NTDS-DSA";
			DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, partialDN1);
			string[] objectCategory = new string[13];
			objectCategory[0] = "(|(&(";
			objectCategory[1] = PropertyManager.ObjectCategory;
			objectCategory[2] = "=server)(";
			objectCategory[3] = PropertyManager.DistinguishedName;
			objectCategory[4] = "=";
			objectCategory[5] = Utils.GetEscapedFilterValue(partialDN);
			objectCategory[6] = "))(&(";
			objectCategory[7] = PropertyManager.ObjectCategory;
			objectCategory[8] = "=nTDSDSA)(";
			objectCategory[9] = PropertyManager.DistinguishedName;
			objectCategory[10] = "=";
			objectCategory[11] = Utils.GetEscapedFilterValue(str);
			objectCategory[12] = ")))";
			string str2 = string.Concat(objectCategory);
			string[] dnsHostName = new string[3];
			dnsHostName[0] = PropertyManager.DnsHostName;
			dnsHostName[1] = PropertyManager.MsDSPortLDAP;
			dnsHostName[2] = PropertyManager.ObjectCategory;
			ADSearcher aDSearcher = new ADSearcher(directoryEntry, str2, dnsHostName, SearchScope.Subtree, true, true);
			SearchResultCollection searchResultCollections = aDSearcher.FindAll();
			try
			{
				if (searchResultCollections.Count == 2)
				{
					foreach (SearchResult searchResult in searchResultCollections)
					{
						string searchResultPropertyValue1 = (string)PropertyManager.GetSearchResultPropertyValue(searchResult, PropertyManager.ObjectCategory);
						if (searchResultPropertyValue1.Length < str1.Length || Utils.Compare(searchResultPropertyValue1, 0, str1.Length, str1, 0, str1.Length) != 0)
						{
							searchResultPropertyValue = (string)PropertyManager.GetSearchResultPropertyValue(searchResult, PropertyManager.DnsHostName);
						}
						else
						{
							num = (int)PropertyManager.GetSearchResultPropertyValue(searchResult, PropertyManager.MsDSPortLDAP);
						}
					}
				}
				else
				{
					object[] objArray = new object[1];
					objArray[0] = dn;
					throw new ActiveDirectoryOperationException(Res.GetString("NoHostNameOrPortNumber", objArray));
				}
			}
			finally
			{
				searchResultCollections.Dispose();
				directoryEntry.Dispose();
			}
			if (num == -1 || searchResultPropertyValue == null)
			{
				object[] objArray1 = new object[1];
				objArray1[0] = dn;
				throw new ActiveDirectoryOperationException(Res.GetString("NoHostNameOrPortNumber", objArray1));
			}
			else
			{
				return string.Concat(searchResultPropertyValue, ":", num);
			}
		}

		internal static string GetAdamHostNameAndPortsFromNTDSA(DirectoryContext context, string dn)
		{
			string searchResultPropertyValue = null;
			int num = -1;
			int searchResultPropertyValue1 = -1;
			string str = dn;
			string partialDN = Utils.GetPartialDN(dn, 1);
			string partialDN1 = Utils.GetPartialDN(dn, 2);
			string str1 = "CN=NTDS-DSA";
			DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, partialDN1);
			string[] objectCategory = new string[13];
			objectCategory[0] = "(|(&(";
			objectCategory[1] = PropertyManager.ObjectCategory;
			objectCategory[2] = "=server)(";
			objectCategory[3] = PropertyManager.DistinguishedName;
			objectCategory[4] = "=";
			objectCategory[5] = Utils.GetEscapedFilterValue(partialDN);
			objectCategory[6] = "))(&(";
			objectCategory[7] = PropertyManager.ObjectCategory;
			objectCategory[8] = "=nTDSDSA)(";
			objectCategory[9] = PropertyManager.DistinguishedName;
			objectCategory[10] = "=";
			objectCategory[11] = Utils.GetEscapedFilterValue(str);
			objectCategory[12] = ")))";
			string str2 = string.Concat(objectCategory);
			string[] dnsHostName = new string[4];
			dnsHostName[0] = PropertyManager.DnsHostName;
			dnsHostName[1] = PropertyManager.MsDSPortLDAP;
			dnsHostName[2] = PropertyManager.MsDSPortSSL;
			dnsHostName[3] = PropertyManager.ObjectCategory;
			ADSearcher aDSearcher = new ADSearcher(directoryEntry, str2, dnsHostName, SearchScope.Subtree, true, true);
			SearchResultCollection searchResultCollections = aDSearcher.FindAll();
			try
			{
				if (searchResultCollections.Count == 2)
				{
					foreach (SearchResult searchResult in searchResultCollections)
					{
						string searchResultPropertyValue2 = (string)PropertyManager.GetSearchResultPropertyValue(searchResult, PropertyManager.ObjectCategory);
						if (searchResultPropertyValue2.Length < str1.Length || Utils.Compare(searchResultPropertyValue2, 0, str1.Length, str1, 0, str1.Length) != 0)
						{
							searchResultPropertyValue = (string)PropertyManager.GetSearchResultPropertyValue(searchResult, PropertyManager.DnsHostName);
						}
						else
						{
							num = (int)PropertyManager.GetSearchResultPropertyValue(searchResult, PropertyManager.MsDSPortLDAP);
							searchResultPropertyValue1 = (int)PropertyManager.GetSearchResultPropertyValue(searchResult, PropertyManager.MsDSPortSSL);
						}
					}
				}
				else
				{
					object[] objArray = new object[1];
					objArray[0] = dn;
					throw new ActiveDirectoryOperationException(Res.GetString("NoHostNameOrPortNumber", objArray));
				}
			}
			finally
			{
				searchResultCollections.Dispose();
				directoryEntry.Dispose();
			}
			if (num == -1 || searchResultPropertyValue1 == -1 || searchResultPropertyValue == null)
			{
				object[] objArray1 = new object[1];
				objArray1[0] = dn;
				throw new ActiveDirectoryOperationException(Res.GetString("NoHostNameOrPortNumber", objArray1));
			}
			else
			{
				object[] objArray2 = new object[5];
				objArray2[0] = searchResultPropertyValue;
				objArray2[1] = ":";
				objArray2[2] = num;
				objArray2[3] = ":";
				objArray2[4] = searchResultPropertyValue1;
				return string.Concat(objArray2);
			}
		}

		internal static IntPtr GetAuthIdentity(DirectoryContext context, LoadLibrarySafeHandle libHandle)
		{
			IntPtr intPtr;
			string str = null;
			string str1 = null;
			Utils.GetDomainAndUsername(context, out str, out str1);
			IntPtr procAddress = UnsafeNativeMethods.GetProcAddress(libHandle, "DsMakePasswordCredentialsW");
			if (procAddress != (IntPtr)0)
			{
				NativeMethods.DsMakePasswordCredentials delegateForFunctionPointer = (NativeMethods.DsMakePasswordCredentials)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(NativeMethods.DsMakePasswordCredentials));
				int password = delegateForFunctionPointer(str, str1, context.Password, out intPtr);
				if (password == 0)
				{
					return intPtr;
				}
				else
				{
					throw ExceptionHelper.GetExceptionFromErrorCode(password);
				}
			}
			else
			{
				throw ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
			}
		}

		internal static DirectoryEntry GetCrossRefEntry(DirectoryContext context, DirectoryEntry partitionsEntry, string partitionName)
		{
			StringBuilder stringBuilder = new StringBuilder(15);
			stringBuilder.Append("(&(");
			stringBuilder.Append(PropertyManager.ObjectCategory);
			stringBuilder.Append("=crossRef)(");
			stringBuilder.Append(PropertyManager.SystemFlags);
			stringBuilder.Append(":1.2.840.113556.1.4.804:=");
			stringBuilder.Append(1);
			stringBuilder.Append(")(!(");
			stringBuilder.Append(PropertyManager.SystemFlags);
			stringBuilder.Append(":1.2.840.113556.1.4.803:=");
			stringBuilder.Append(2);
			stringBuilder.Append("))(");
			stringBuilder.Append(PropertyManager.NCName);
			stringBuilder.Append("=");
			stringBuilder.Append(Utils.GetEscapedFilterValue(partitionName));
			stringBuilder.Append("))");
			string str = stringBuilder.ToString();
			string[] distinguishedName = new string[1];
			distinguishedName[0] = PropertyManager.DistinguishedName;
			ADSearcher aDSearcher = new ADSearcher(partitionsEntry, str, distinguishedName, SearchScope.OneLevel, false, false);
			SearchResult searchResult = null;
			try
			{
				searchResult = aDSearcher.FindOne();
				if (searchResult == null)
				{
					throw new ActiveDirectoryObjectNotFoundException(Res.GetString("AppNCNotFound"), typeof(ActiveDirectoryPartition), partitionName);
				}
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				throw ExceptionHelper.GetExceptionFromCOMException(context, cOMException);
			}
			return searchResult.GetDirectoryEntry();
		}

		[SecuritySafeCritical]
		internal static IntPtr GetCurrentUserSid()
		{
			IntPtr intPtr;
			IntPtr zero = IntPtr.Zero;
			IntPtr zero1 = IntPtr.Zero;
			try
			{
				if (!UnsafeNativeMethods.OpenThreadToken(UnsafeNativeMethods.GetCurrentThread(), 8, true, ref zero))
				{
					int lastWin32Error = Marshal.GetLastWin32Error();
					int num = lastWin32Error;
					if (lastWin32Error != 0x3f0)
					{
						object[] objArray = new object[1];
						objArray[0] = num;
						throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Res.GetString("UnableToOpenToken"), objArray));
					}
					else
					{
						if (!UnsafeNativeMethods.OpenProcessToken(UnsafeNativeMethods.GetCurrentProcess(), 8, ref zero))
						{
							int lastWin32Error1 = Marshal.GetLastWin32Error();
							object[] objArray1 = new object[1];
							objArray1[0] = lastWin32Error1;
							throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Res.GetString("UnableToOpenToken"), objArray1));
						}
					}
				}
				int num1 = 0;
				bool tokenInformation = UnsafeNativeMethods.GetTokenInformation(zero, 1, IntPtr.Zero, 0, ref num1);
				int lastWin32Error2 = Marshal.GetLastWin32Error();
				int num2 = lastWin32Error2;
				if (lastWin32Error2 == 122)
				{
					zero1 = Marshal.AllocHGlobal(num1);
					tokenInformation = UnsafeNativeMethods.GetTokenInformation(zero, 1, zero1, num1, ref num1);
					if (tokenInformation)
					{
						TOKEN_USER structure = (TOKEN_USER)Marshal.PtrToStructure(zero1, typeof(TOKEN_USER));
						IntPtr intPtr1 = structure.sidAndAttributes.pSid;
						int lengthSid = UnsafeNativeMethods.GetLengthSid(intPtr1);
						IntPtr intPtr2 = Marshal.AllocHGlobal(lengthSid);
						tokenInformation = UnsafeNativeMethods.CopySid(lengthSid, intPtr2, intPtr1);
						if (tokenInformation)
						{
							intPtr = intPtr2;
						}
						else
						{
							int lastWin32Error3 = Marshal.GetLastWin32Error();
							object[] objArray2 = new object[1];
							objArray2[0] = lastWin32Error3;
							throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Res.GetString("UnableToRetrieveTokenInfo"), objArray2));
						}
					}
					else
					{
						int num3 = Marshal.GetLastWin32Error();
						object[] objArray3 = new object[1];
						objArray3[0] = num3;
						throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Res.GetString("UnableToRetrieveTokenInfo"), objArray3));
					}
				}
				else
				{
					object[] objArray4 = new object[1];
					objArray4[0] = num2;
					throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Res.GetString("UnableToRetrieveTokenInfo"), objArray4));
				}
			}
			finally
			{
				if (zero != IntPtr.Zero)
				{
					UnsafeNativeMethods.CloseHandle(zero);
				}
				if (zero1 != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(zero1);
				}
			}
			return intPtr;
		}

		internal static Component[] GetDNComponents(string distinguishedName)
		{
			string[] strArrays = Utils.Split(distinguishedName, ',');
			Component[] componentArray = new Component[strArrays.GetLength(0)];
			int num = 0;
			while (num < strArrays.GetLength(0))
			{
				string[] strArrays1 = Utils.Split(strArrays[num], '=');
				if (strArrays1.GetLength(0) == 2)
				{
					componentArray[num].Name = strArrays1[0].Trim();
					if (componentArray[num].Name.Length != 0)
					{
						componentArray[num].Value = strArrays1[1].Trim();
						if (componentArray[num].Value.Length != 0)
						{
							num++;
						}
						else
						{
							throw new ArgumentException(Res.GetString("InvalidDNFormat"), "distinguishedName");
						}
					}
					else
					{
						throw new ArgumentException(Res.GetString("InvalidDNFormat"), "distinguishedName");
					}
				}
				else
				{
					throw new ArgumentException(Res.GetString("InvalidDNFormat"), "distinguishedName");
				}
			}
			return componentArray;
		}

		internal static string GetDNFromDnsName(string dnsName)
		{
			string str = null;
			IntPtr zero = IntPtr.Zero;
			IntPtr procAddress = UnsafeNativeMethods.GetProcAddress(DirectoryContext.ADHandle, "DsCrackNamesW");
			if (procAddress != (IntPtr)0)
			{
				NativeMethods.DsCrackNames delegateForFunctionPointer = (NativeMethods.DsCrackNames)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(NativeMethods.DsCrackNames));
				IntPtr hGlobalUni = Marshal.StringToHGlobalUni(string.Concat(dnsName, "/"));
				IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IntPtr)));
				Marshal.WriteIntPtr(intPtr, hGlobalUni);
				int num = delegateForFunctionPointer(IntPtr.Zero, 1, 7, 1, 1, intPtr, out zero);
				if (num != 0)
				{
					if (num != 6)
					{
						throw ExceptionHelper.GetExceptionFromErrorCode(num);
					}
					else
					{
						throw new ArgumentException(Res.GetString("InvalidDNFormat"));
					}
				}
				else
				{
					try
					{
						DsNameResult dsNameResult = new DsNameResult();
						Marshal.PtrToStructure(zero, dsNameResult);
						if (dsNameResult.itemCount >= 1 && dsNameResult.items != IntPtr.Zero)
						{
							DsNameResultItem dsNameResultItem = new DsNameResultItem();
							Marshal.PtrToStructure(dsNameResult.items, dsNameResultItem);
							str = dsNameResultItem.name;
						}
					}
					finally
					{
						if (intPtr != (IntPtr)0)
						{
							Marshal.FreeHGlobal(intPtr);
						}
						if (hGlobalUni != (IntPtr)0)
						{
							Marshal.FreeHGlobal(hGlobalUni);
						}
						if (zero != IntPtr.Zero)
						{
							procAddress = UnsafeNativeMethods.GetProcAddress(DirectoryContext.ADHandle, "DsFreeNameResultW");
							if (procAddress != (IntPtr)0)
							{
								UnsafeNativeMethods.DsFreeNameResultW dsFreeNameResultW = (UnsafeNativeMethods.DsFreeNameResultW)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(UnsafeNativeMethods.DsFreeNameResultW));
								dsFreeNameResultW(zero);
							}
							else
							{
								throw ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
							}
						}
					}
					return str;
				}
			}
			else
			{
				throw ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
			}
		}

		internal static string GetDNFromTransportType(ActiveDirectoryTransportType transport, DirectoryContext context)
		{
			string str = DirectoryEntryManager.ExpandWellKnownDN(context, WellKnownDN.SitesContainer);
			string str1 = string.Concat("CN=Inter-Site Transports,", str);
			if (transport != ActiveDirectoryTransportType.Rpc)
			{
				return string.Concat("CN=SMTP,", str1);
			}
			else
			{
				return string.Concat("CN=IP,", str1);
			}
		}

		internal static string GetDnsHostNameFromNTDSA(DirectoryContext context, string dn)
		{
			string propertyValue = null;
			int num = dn.IndexOf(',');
			if (num != -1)
			{
				string str = dn.Substring(num + 1);
				DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, str);
				try
				{
					propertyValue = (string)PropertyManager.GetPropertyValue(context, directoryEntry, PropertyManager.DnsHostName);
				}
				finally
				{
					directoryEntry.Dispose();
				}
				return propertyValue;
			}
			else
			{
				throw new ArgumentException(Res.GetString("InvalidDNFormat"), "dn");
			}
		}

		internal static string GetDnsNameFromDN(string distinguishedName)
		{
			return distinguishedName.Replace ("DC=", "").Replace ("CN=", "").Replace (",", ".").Replace (" ", "");
			/*
			string str = null;
			IntPtr zero = IntPtr.Zero;
			IntPtr procAddress = UnsafeNativeMethods.GetProcAddress(DirectoryContext.ADHandle, "DsCrackNamesW");
			if (procAddress != (IntPtr)0)
			{
				NativeMethods.DsCrackNames delegateForFunctionPointer = (NativeMethods.DsCrackNames)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(NativeMethods.DsCrackNames));
				IntPtr hGlobalUni = Marshal.StringToHGlobalUni(distinguishedName);
				IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IntPtr)));
				Marshal.WriteIntPtr(intPtr, hGlobalUni);
				int num = delegateForFunctionPointer(IntPtr.Zero, 1, 1, 7, 1, intPtr, out zero);
				if (num != 0)
				{
					if (num != 6)
					{
						throw ExceptionHelper.GetExceptionFromErrorCode(num);
					}
					else
					{
						throw new ArgumentException(Res.GetString("InvalidDNFormat"), "distinguishedName");
					}
				}
				else
				{
					try
					{
						DsNameResult dsNameResult = new DsNameResult();
						Marshal.PtrToStructure(zero, dsNameResult);
						if (dsNameResult.itemCount >= 1 && dsNameResult.items != IntPtr.Zero)
						{
							DsNameResultItem dsNameResultItem = new DsNameResultItem();
							Marshal.PtrToStructure(dsNameResult.items, dsNameResultItem);
							if (dsNameResultItem.status == 6 || dsNameResultItem.name == null)
							{
								throw new ArgumentException(Res.GetString("InvalidDNFormat"), "distinguishedName");
							}
							else
							{
								if (dsNameResultItem.status == 0)
								{
									if (dsNameResultItem.name.Length - 1 != dsNameResultItem.name.IndexOf('/'))
									{
										str = dsNameResultItem.name;
									}
									else
									{
										str = dsNameResultItem.name.Substring(0, dsNameResultItem.name.Length - 1);
									}
								}
								else
								{
									throw ExceptionHelper.GetExceptionFromErrorCode(num);
								}
							}
						}
					}
					finally
					{
						if (intPtr != (IntPtr)0)
						{
							Marshal.FreeHGlobal(intPtr);
						}
						if (hGlobalUni != (IntPtr)0)
						{
							Marshal.FreeHGlobal(hGlobalUni);
						}
						if (zero != IntPtr.Zero)
						{
							procAddress = UnsafeNativeMethods.GetProcAddress(DirectoryContext.ADHandle, "DsFreeNameResultW");
							if (procAddress != (IntPtr)0)
							{
								UnsafeNativeMethods.DsFreeNameResultW dsFreeNameResultW = (UnsafeNativeMethods.DsFreeNameResultW)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(UnsafeNativeMethods.DsFreeNameResultW));
								dsFreeNameResultW(zero);
							}
							else
							{
								throw ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
							}
						}
					}
					return str;
				}
			}
			else
			{
				throw ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
			}
			*/
		}

		internal static void GetDomainAndUsername(DirectoryContext context, out string username, out string domain)
		{
			if (context.UserName == null || context.UserName.Length <= 0)
			{
				username = context.UserName;
				domain = null;
				return;
			}
			else
			{
				string userName = context.UserName;
				int num = userName.IndexOf('\\');
				int num1 = num;
				if (num == -1)
				{
					username = userName;
					domain = null;
					return;
				}
				else
				{
					domain = userName.Substring(0, num1);
					username = userName.Substring(num1 + 1, userName.Length - num1 - 1);
					return;
				}
			}
		}

		internal static IntPtr GetDSHandle(string domainControllerName, string domainName, IntPtr authIdentity, LoadLibrarySafeHandle libHandle)
		{
			IntPtr intPtr;
			string str;
			IntPtr procAddress = UnsafeNativeMethods.GetProcAddress(libHandle, "DsBindWithCredW");
			if (procAddress != (IntPtr)0)
			{
				NativeMethods.DsBindWithCred delegateForFunctionPointer = (NativeMethods.DsBindWithCred)Marshal.GetDelegateForFunctionPointer(procAddress, typeof(NativeMethods.DsBindWithCred));
				int num = delegateForFunctionPointer(domainControllerName, domainName, authIdentity, out intPtr);
				if (num == 0)
				{
					return intPtr;
				}
				else
				{
					int num1 = num;
					if (domainControllerName != null)
					{
						str = domainControllerName;
					}
					else
					{
						str = domainName;
					}
					throw ExceptionHelper.GetExceptionFromErrorCode(num1, str);
				}
			}
			else
			{
				throw ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
			}
		}

		internal static string GetEscapedFilterValue(string filterValue)
		{
			char[] chrArray = new char[] { }; //TODO: Review:  <PrivateImplementationDetails>{C2A96791-9486-43D0-A676-E00CB6B83818}.$$method0x60007e6-1 };
			char[] chrArray1 = chrArray;
			int num = filterValue.IndexOfAny(chrArray1);
			if (num == -1)
			{
				return filterValue;
			}
			else
			{
				StringBuilder stringBuilder = new StringBuilder(2 * filterValue.Length);
				stringBuilder.Append(filterValue.Substring(0, num));
				for (int i = num; i < filterValue.Length; i++)
				{
					char chr = filterValue[i];
					switch (chr)
					{
						case '(':
						{
							stringBuilder.Append("\\28");
							break;
						}
						case ')':
						{
							stringBuilder.Append("\\29");
							break;
						}
						case '*':
						{
							stringBuilder.Append("\\2A");
							break;
						}
						default:
						{
							if (chr == '\\')
							{
								stringBuilder.Append("\\5C");
								break;
							}
							else
							{
								stringBuilder.Append(filterValue[i]);
								break;
							}
						}
					}
				}
				return stringBuilder.ToString();
			}
		}

		internal static string GetEscapedPath(string originalPath)
		{
			NativeComInterfaces.IAdsPathname pathname = (NativeComInterfaces.IAdsPathname)(new NativeComInterfaces.Pathname());
			return pathname.GetEscapedElement(0, originalPath);
		}

		[SecuritySafeCritical]
		internal static int GetLastRidFromSid(IntPtr pSid)
		{
			IntPtr sidSubAuthorityCount = UnsafeNativeMethods.GetSidSubAuthorityCount(pSid);
			int num = Marshal.ReadByte(sidSubAuthorityCount);
			IntPtr sidSubAuthority = UnsafeNativeMethods.GetSidSubAuthority(pSid, num - 1);
			int num1 = Marshal.ReadInt32(sidSubAuthority);
			return num1;
		}

		[SecurityCritical]
		internal static int GetLastRidFromSid(byte[] sid)
		{
			int num;
			IntPtr zero = IntPtr.Zero;
			try
			{
				zero = Utils.ConvertByteArrayToIntPtr(sid);
				int lastRidFromSid = Utils.GetLastRidFromSid(zero);
				num = lastRidFromSid;
			}
			finally
			{
				if (zero != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(zero);
				}
			}
			return num;
		}

		[SecuritySafeCritical]
		internal static IntPtr GetMachineDomainSid()
		{
			IntPtr intPtr;
			IntPtr zero = IntPtr.Zero;
			IntPtr zero1 = IntPtr.Zero;
			IntPtr intPtr1 = IntPtr.Zero;
			try
			{
				LSA_OBJECT_ATTRIBUTES lSAOBJECTATTRIBUTE = new LSA_OBJECT_ATTRIBUTES();
				intPtr1 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(LSA_OBJECT_ATTRIBUTES)));
				Marshal.StructureToPtr(lSAOBJECTATTRIBUTE, intPtr1, false);
				int num = UnsafeNativeMethods.LsaOpenPolicy(IntPtr.Zero, intPtr1, 1, ref zero);
				if (num == 0)
				{
					num = UnsafeNativeMethods.LsaQueryInformationPolicy(zero, 5, ref zero1);
					if (num == 0)
					{
						POLICY_ACCOUNT_DOMAIN_INFO structure = (POLICY_ACCOUNT_DOMAIN_INFO)Marshal.PtrToStructure(zero1, typeof(POLICY_ACCOUNT_DOMAIN_INFO));
						int lengthSid = UnsafeNativeMethods.GetLengthSid(structure.domainSid);
						IntPtr intPtr2 = Marshal.AllocHGlobal(lengthSid);
						bool flag = UnsafeNativeMethods.CopySid(lengthSid, intPtr2, structure.domainSid);
						if (flag)
						{
							intPtr = intPtr2;
						}
						else
						{
							int lastWin32Error = Marshal.GetLastWin32Error();
							object[] objArray = new object[1];
							objArray[0] = lastWin32Error;
							throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Res.GetString("UnableToRetrievePolicy"), objArray));
						}
					}
					else
					{
						object[] winError = new object[1];
						winError[0] = NativeMethods.LsaNtStatusToWinError(num);
						throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Res.GetString("UnableToRetrievePolicy"), winError));
					}
				}
				else
				{
					object[] winError1 = new object[1];
					winError1[0] = NativeMethods.LsaNtStatusToWinError(num);
					throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Res.GetString("UnableToRetrievePolicy"), winError1));
				}
			}
			finally
			{
				if (zero != IntPtr.Zero)
				{
					UnsafeNativeMethods.LsaClose(zero);
				}
				if (zero1 != IntPtr.Zero)
				{
					UnsafeNativeMethods.LsaFreeMemory(zero1);
				}
				if (intPtr1 != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(intPtr1);
				}
			}
			return intPtr;
		}

		internal static DirectoryContext GetNewDirectoryContext(string name, DirectoryContextType contextType, DirectoryContext context)
		{
			return new DirectoryContext(contextType, name, context);
		}

		internal static string GetNtAuthorityString()
		{
			if (Utils.NTAuthorityString == null)
			{
				SecurityIdentifier securityIdentifier = new SecurityIdentifier("S-1-5-18");
				NTAccount nTAccount = (NTAccount)securityIdentifier.Translate(typeof(NTAccount));
				int num = nTAccount.Value.IndexOf('\\');
				Utils.NTAuthorityString = nTAccount.Value.Substring(0, num);
			}
			return Utils.NTAuthorityString;
		}

		internal static string GetPartialDN(string distinguishedName, int startingIndex)
		{
			string str = "";
			Component[] dNComponents = Utils.GetDNComponents(distinguishedName);
			bool flag = true;
			for (int i = startingIndex; i < dNComponents.GetLength(0); i++)
			{
				if (!flag)
				{
					string str1 = str;
					string[] name = new string[5];
					name[0] = str1;
					name[1] = ",";
					name[2] = dNComponents[i].Name;
					name[3] = "=";
					name[4] = dNComponents[i].Value;
					str = string.Concat(name);
				}
				else
				{
					str = string.Concat(dNComponents[i].Name, "=", dNComponents[i].Value);
					flag = false;
				}
			}
			return str;
		}

		internal static IntPtr GetPolicyHandle(string serverName)
		{
			IntPtr intPtr;
			IntPtr intPtr1 = (IntPtr)0;
			LSA_OBJECT_ATTRIBUTES lSAOBJECTATTRIBUTE = new LSA_OBJECT_ATTRIBUTES();
			int pOLICYVIEWLOCALINFORMATION = Utils.POLICY_VIEW_LOCAL_INFORMATION;
			LSA_UNICODE_STRING lSAUNICODESTRING = new LSA_UNICODE_STRING();
			IntPtr hGlobalUni = Marshal.StringToHGlobalUni(serverName);
			UnsafeNativeMethods.RtlInitUnicodeString(lSAUNICODESTRING, hGlobalUni);
			try
			{
				int num = UnsafeNativeMethods.LsaOpenPolicy(lSAUNICODESTRING, lSAOBJECTATTRIBUTE, pOLICYVIEWLOCALINFORMATION, out intPtr1);
				if (num == 0)
				{
					intPtr = intPtr1;
				}
				else
				{
					throw ExceptionHelper.GetExceptionFromErrorCode(UnsafeNativeMethods.LsaNtStatusToWinError(num), serverName);
				}
			}
			finally
			{
				if (hGlobalUni != (IntPtr)0)
				{
					Marshal.FreeHGlobal(hGlobalUni);
				}
			}
			return intPtr;
		}

		internal static string GetPolicyServerName(DirectoryContext context, bool isForest, bool needPdc, string source)
		{
			string name;
			PrivateLocatorFlags privateLocatorFlag = PrivateLocatorFlags.DirectoryServicesRequired;
			if (!context.isDomain())
			{
				if (!isForest)
				{
					name = context.Name;
				}
				else
				{
					if (!needPdc)
					{
						if (context.ContextType != DirectoryContextType.DirectoryServer)
						{
							name = Locator.GetDomainControllerInfo(null, source, null, (long)privateLocatorFlag).DomainControllerName.Substring(2);
						}
						else
						{
							DirectoryEntry directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, WellKnownDN.RootDSE);
							string propertyValue = (string)PropertyManager.GetPropertyValue(context, directoryEntry, PropertyManager.DefaultNamingContext);
							string str = (string)PropertyManager.GetPropertyValue(context, directoryEntry, PropertyManager.RootDomainNamingContext);
							if (Utils.Compare(propertyValue, str) != 0)
							{
								name = Locator.GetDomainControllerInfo(null, source, null, (long)privateLocatorFlag).DomainControllerName.Substring(2);
							}
							else
							{
								name = context.Name;
							}
						}
					}
					else
					{
						privateLocatorFlag = privateLocatorFlag | PrivateLocatorFlags.PdcRequired;
						name = Locator.GetDomainControllerInfo(null, source, null, (long)privateLocatorFlag).DomainControllerName.Substring(2);
					}
				}
			}
			else
			{
				if (needPdc)
				{
					privateLocatorFlag = privateLocatorFlag | PrivateLocatorFlags.PdcRequired;
				}
				name = Locator.GetDomainControllerInfo(null, source, null, (long)privateLocatorFlag).DomainControllerName.Substring(2);
			}
			return name;
		}

		internal static int GetRandomIndex(int count)
		{
			Random random = new Random();
			int num = random.Next();
			return num % count;
		}

		internal static string GetRdnFromDN(string distinguishedName)
		{
			Component[] dNComponents = Utils.GetDNComponents(distinguishedName);
			string str = string.Concat(dNComponents[0].Name, "=", dNComponents[0].Value);
			return str;
		}

		internal static ArrayList GetReplicaList(DirectoryContext context, string partitionName, string siteName, bool isDefaultNC, bool isADAM, bool isGC)
		{
			string dnsHostNameFromNTDSA;
			string str = null;
			string str1 = null;
			string str2 = null;
			string str3 = null;
			ArrayList arrayLists;
			string[] objectCategory;
			IDisposable disposable;
			IEnumerator enumerator;
			object[] msDSHasInstantiatedNCs;
			ArrayList arrayLists1 = new ArrayList();
			ArrayList arrayLists2 = new ArrayList();
			Hashtable hashtables = new Hashtable();
			Hashtable hashtables1 = new Hashtable();
			StringBuilder stringBuilder = new StringBuilder(10);
			StringBuilder stringBuilder1 = new StringBuilder(10);
			StringBuilder stringBuilder2 = new StringBuilder(10);
			StringBuilder stringBuilder3 = new StringBuilder(10);
			bool flag = false;
			string str4 = null;
			try
			{
				str4 = DirectoryEntryManager.ExpandWellKnownDN(context, WellKnownDN.ConfigurationNamingContext);
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				throw ExceptionHelper.GetExceptionFromCOMException(context, cOMException);
			}
			if (partitionName != null && !isDefaultNC)
			{
				DistinguishedName distinguishedName = new DistinguishedName(partitionName);
				DistinguishedName distinguishedName1 = new DistinguishedName(str4);
				DistinguishedName distinguishedName2 = new DistinguishedName(string.Concat("CN=Schema,", str4));
				if (!distinguishedName1.Equals(distinguishedName) && !distinguishedName2.Equals(distinguishedName))
				{
					flag = true;
				}
			}
			if (flag)
			{
				DirectoryEntry directoryEntry = null;
				DirectoryEntry directoryEntry1 = null;
				try
				{
					try
					{
						directoryEntry = DirectoryEntryManager.GetDirectoryEntry(context, string.Concat("CN=Partitions,", str4));
						if (!isADAM)
						{
							dnsHostNameFromNTDSA = Utils.GetDnsHostNameFromNTDSA(context, (string)PropertyManager.GetPropertyValue(context, directoryEntry, PropertyManager.FsmoRoleOwner));
						}
						else
						{
							dnsHostNameFromNTDSA = Utils.GetAdamDnsHostNameFromNTDSA(context, (string)PropertyManager.GetPropertyValue(context, directoryEntry, PropertyManager.FsmoRoleOwner));
						}
						DirectoryContext newDirectoryContext = Utils.GetNewDirectoryContext(dnsHostNameFromNTDSA, DirectoryContextType.DirectoryServer, context);
						directoryEntry1 = DirectoryEntryManager.GetDirectoryEntry(newDirectoryContext, string.Concat("CN=Partitions,", str4));
						objectCategory = new string[7];
						objectCategory[0] = "(&(";
						objectCategory[1] = PropertyManager.ObjectCategory;
						objectCategory[2] = "=crossRef)(";
						objectCategory[3] = PropertyManager.NCName;
						objectCategory[4] = "=";
						objectCategory[5] = Utils.GetEscapedFilterValue(partitionName);
						objectCategory[6] = "))";
						string str5 = string.Concat(objectCategory);
						ArrayList arrayLists3 = new ArrayList();
						arrayLists3.Add(PropertyManager.MsDSNCReplicaLocations);
						arrayLists3.Add(PropertyManager.MsDSNCROReplicaLocations);
						Hashtable valuesWithRangeRetrieval = null;
						try
						{
							valuesWithRangeRetrieval = Utils.GetValuesWithRangeRetrieval(directoryEntry1, str5, arrayLists3, SearchScope.OneLevel);
						}
						catch (COMException cOMException3)
						{
							COMException cOMException2 = cOMException3;
							throw ExceptionHelper.GetExceptionFromCOMException(newDirectoryContext, cOMException2);
						}
						catch (ActiveDirectoryObjectNotFoundException activeDirectoryObjectNotFoundException)
						{
							arrayLists = arrayLists2;
							return arrayLists;
						}
						ArrayList item = (ArrayList)valuesWithRangeRetrieval[PropertyManager.MsDSNCReplicaLocations.ToLower(CultureInfo.InvariantCulture)];
						ArrayList item1 = (ArrayList)valuesWithRangeRetrieval[PropertyManager.MsDSNCROReplicaLocations.ToLower(CultureInfo.InvariantCulture)];
						if (item.Count != 0)
						{
							foreach (string str6 in item)
							{
								stringBuilder.Append("(");
								stringBuilder.Append(PropertyManager.DistinguishedName);
								stringBuilder.Append("=");
								stringBuilder.Append(Utils.GetEscapedFilterValue(str6));
								stringBuilder.Append(")");
								stringBuilder1.Append("(");
								stringBuilder1.Append(PropertyManager.DistinguishedName);
								stringBuilder1.Append("=");
								stringBuilder1.Append(Utils.GetEscapedFilterValue(Utils.GetPartialDN(str6, 1)));
								stringBuilder1.Append(")");
							}
							foreach (string str7 in item1)
							{
								stringBuilder2.Append("(");
								stringBuilder2.Append(PropertyManager.DistinguishedName);
								stringBuilder2.Append("=");
								stringBuilder2.Append(Utils.GetEscapedFilterValue(str7));
								stringBuilder2.Append(")");
								stringBuilder3.Append("(");
								stringBuilder3.Append(PropertyManager.DistinguishedName);
								stringBuilder3.Append("=");
								stringBuilder3.Append(Utils.GetEscapedFilterValue(Utils.GetPartialDN(str7, 1)));
								stringBuilder3.Append(")");
							}
						}
						else
						{
							arrayLists = arrayLists2;
							return arrayLists;
						}
					}
					catch (COMException cOMException5)
					{
						COMException cOMException4 = cOMException5;
						throw ExceptionHelper.GetExceptionFromCOMException(context, cOMException4);
					}
				}
				finally
				{
					if (directoryEntry != null)
					{
						directoryEntry.Dispose();
					}
					if (directoryEntry1 != null)
					{
						directoryEntry1.Dispose();
					}
				}
			}
			DirectoryEntry directoryEntry2 = null;
			using (directoryEntry2)
			{
				if (siteName == null)
				{
					str = string.Concat("CN=Sites,", str4);
				}
				else
				{
					str = string.Concat("CN=Servers,CN=", siteName, ",CN=Sites,", str4);
				}
				directoryEntry2 = DirectoryEntryManager.GetDirectoryEntry(context, str);
				if (stringBuilder.ToString().Length != 0)
				{
					if (!isGC)
					{
						if (stringBuilder2.Length <= 0)
						{
							objectCategory = new string[13];
							objectCategory[0] = "(|(&(";
							objectCategory[1] = PropertyManager.ObjectCategory;
							objectCategory[2] = "=nTDSDSA)(";
							objectCategory[3] = PropertyManager.MsDSHasMasterNCs;
							objectCategory[4] = "=";
							objectCategory[5] = Utils.GetEscapedFilterValue(partitionName);
							objectCategory[6] = ")(|";
							objectCategory[7] = stringBuilder.ToString();
							objectCategory[8] = "))(&(";
							objectCategory[9] = PropertyManager.ObjectCategory;
							objectCategory[10] = "=server)(|";
							objectCategory[11] = stringBuilder1.ToString();
							objectCategory[12] = ")))";
							str1 = string.Concat(objectCategory);
						}
						else
						{
							objectCategory = new string[21];
							objectCategory[0] = "(|(&(";
							objectCategory[1] = PropertyManager.ObjectCategory;
							objectCategory[2] = "=nTDSDSA)(";
							objectCategory[3] = PropertyManager.MsDSHasMasterNCs;
							objectCategory[4] = "=";
							objectCategory[5] = Utils.GetEscapedFilterValue(partitionName);
							objectCategory[6] = ")(|";
							objectCategory[7] = stringBuilder.ToString();
							objectCategory[8] = "))(&(";
							objectCategory[9] = PropertyManager.ObjectCategory;
							objectCategory[10] = "=nTDSDSARO)(|";
							objectCategory[11] = stringBuilder2.ToString();
							objectCategory[12] = "))(&(";
							objectCategory[13] = PropertyManager.ObjectCategory;
							objectCategory[14] = "=server)(|";
							objectCategory[15] = stringBuilder1.ToString();
							objectCategory[16] = "))(&(";
							objectCategory[17] = PropertyManager.ObjectCategory;
							objectCategory[18] = "=server)(|";
							objectCategory[19] = stringBuilder3.ToString();
							objectCategory[20] = ")))";
							str1 = string.Concat(objectCategory);
						}
					}
					else
					{
						if (stringBuilder2.Length <= 0)
						{
							objectCategory = new string[15];
							objectCategory[0] = "(|(&(";
							objectCategory[1] = PropertyManager.ObjectCategory;
							objectCategory[2] = "=nTDSDSA)(";
							objectCategory[3] = PropertyManager.Options;
							objectCategory[4] = ":1.2.840.113556.1.4.804:=1)(";
							objectCategory[5] = PropertyManager.MsDSHasMasterNCs;
							objectCategory[6] = "=";
							objectCategory[7] = Utils.GetEscapedFilterValue(partitionName);
							objectCategory[8] = ")(|";
							objectCategory[9] = stringBuilder.ToString();
							objectCategory[10] = "))(&(";
							objectCategory[11] = PropertyManager.ObjectCategory;
							objectCategory[12] = "=server)(|";
							objectCategory[13] = stringBuilder1.ToString();
							objectCategory[14] = ")))";
							str1 = string.Concat(objectCategory);
						}
						else
						{
							objectCategory = new string[25];
							objectCategory[0] = "(|(&(";
							objectCategory[1] = PropertyManager.ObjectCategory;
							objectCategory[2] = "=nTDSDSA)(";
							objectCategory[3] = PropertyManager.Options;
							objectCategory[4] = ":1.2.840.113556.1.4.804:=1)(";
							objectCategory[5] = PropertyManager.MsDSHasMasterNCs;
							objectCategory[6] = "=";
							objectCategory[7] = Utils.GetEscapedFilterValue(partitionName);
							objectCategory[8] = ")(|";
							objectCategory[9] = stringBuilder.ToString();
							objectCategory[10] = "))(&(";
							objectCategory[11] = PropertyManager.ObjectCategory;
							objectCategory[12] = "=nTDSDSARO)(";
							objectCategory[13] = PropertyManager.Options;
							objectCategory[14] = ":1.2.840.113556.1.4.804:=1)(|";
							objectCategory[15] = stringBuilder2.ToString();
							objectCategory[16] = "))(&(";
							objectCategory[17] = PropertyManager.ObjectCategory;
							objectCategory[18] = "=server)(|";
							objectCategory[19] = stringBuilder1.ToString();
							objectCategory[20] = "))(&(";
							objectCategory[21] = PropertyManager.ObjectCategory;
							objectCategory[22] = "=server)(|";
							objectCategory[23] = stringBuilder3.ToString();
							objectCategory[24] = ")))";
							str1 = string.Concat(objectCategory);
						}
					}
				}
				else
				{
					if (!isDefaultNC)
					{
						if (!isGC)
						{
							objectCategory = new string[7];
							objectCategory[0] = "(|(";
							objectCategory[1] = PropertyManager.ObjectCategory;
							objectCategory[2] = "=nTDSDSA)(";
							objectCategory[3] = PropertyManager.ObjectCategory;
							objectCategory[4] = "=nTDSDSARO)(";
							objectCategory[5] = PropertyManager.ObjectCategory;
							objectCategory[6] = "=server))";
							str1 = string.Concat(objectCategory);
						}
						else
						{
							objectCategory = new string[11];
							objectCategory[0] = "(|(&(";
							objectCategory[1] = PropertyManager.ObjectCategory;
							objectCategory[2] = "=nTDSDSA)(";
							objectCategory[3] = PropertyManager.Options;
							objectCategory[4] = ":1.2.840.113556.1.4.804:=1))(&(";
							objectCategory[5] = PropertyManager.ObjectCategory;
							objectCategory[6] = "=nTDSDSARO)(";
							objectCategory[7] = PropertyManager.Options;
							objectCategory[8] = ":1.2.840.113556.1.4.804:=1))(";
							objectCategory[9] = PropertyManager.ObjectCategory;
							objectCategory[10] = "=server))";
							str1 = string.Concat(objectCategory);
						}
					}
					else
					{
						objectCategory = new string[15];
						objectCategory[0] = "(|(&(";
						objectCategory[1] = PropertyManager.ObjectCategory;
						objectCategory[2] = "=nTDSDSA)(";
						objectCategory[3] = PropertyManager.HasMasterNCs;
						objectCategory[4] = "=";
						objectCategory[5] = Utils.GetEscapedFilterValue(partitionName);
						objectCategory[6] = "))(&(";
						objectCategory[7] = PropertyManager.ObjectCategory;
						objectCategory[8] = "=nTDSDSARO)(";
						objectCategory[9] = PropertyManager.MsDSHasFullReplicaNCs;
						objectCategory[10] = "=";
						objectCategory[11] = Utils.GetEscapedFilterValue(partitionName);
						objectCategory[12] = "))(";
						objectCategory[13] = PropertyManager.ObjectCategory;
						objectCategory[14] = "=server))";
						str1 = string.Concat(objectCategory);
					}
				}
				ADSearcher aDSearcher = new ADSearcher(directoryEntry2, str1, new string[0], SearchScope.Subtree);
				bool flag1 = false;
				ArrayList arrayLists4 = new ArrayList();
				int num = 0;
				string str8 = string.Concat(PropertyManager.MsDSHasInstantiatedNCs, ";range=0-*");
				aDSearcher.PropertiesToLoad.Add(PropertyManager.DistinguishedName);
				aDSearcher.PropertiesToLoad.Add(PropertyManager.DnsHostName);
				aDSearcher.PropertiesToLoad.Add(str8);
				aDSearcher.PropertiesToLoad.Add(PropertyManager.ObjectCategory);
				if (isADAM)
				{
					aDSearcher.PropertiesToLoad.Add(PropertyManager.MsDSPortLDAP);
				}
				try
				{
					string str9 = "CN=NTDS-DSA";
					string str10 = "CN=NTDS-DSA-RO";
					SearchResultCollection searchResultCollections = aDSearcher.FindAll();
					using (searchResultCollections)
					{
						foreach (SearchResult searchResult in searchResultCollections)
						{
							string searchResultPropertyValue = (string)PropertyManager.GetSearchResultPropertyValue(searchResult, PropertyManager.ObjectCategory);
							if (searchResultPropertyValue.Length < str9.Length || Utils.Compare(searchResultPropertyValue, 0, str9.Length, str9, 0, str9.Length) != 0)
							{
								if (!searchResult.Properties.Contains(PropertyManager.DnsHostName))
								{
									continue;
								}
								hashtables.Add(string.Concat("CN=NTDS Settings,", (string)PropertyManager.GetSearchResultPropertyValue(searchResult, PropertyManager.DistinguishedName)), (string)PropertyManager.GetSearchResultPropertyValue(searchResult, PropertyManager.DnsHostName));
							}
							else
							{
								string searchResultPropertyValue1 = (string)PropertyManager.GetSearchResultPropertyValue(searchResult, PropertyManager.DistinguishedName);
								if (!flag)
								{
									arrayLists1.Add(searchResultPropertyValue1);
									if (!isADAM)
									{
										continue;
									}
									hashtables1.Add(searchResultPropertyValue1, (int)PropertyManager.GetSearchResultPropertyValue(searchResult, PropertyManager.MsDSPortLDAP));
								}
								else
								{
									if (searchResultPropertyValue.Length < str10.Length || Utils.Compare(searchResultPropertyValue, 0, str10.Length, str10, 0, str10.Length) != 0)
									{
										if (searchResult.Properties.Contains(str8))
										{
											str2 = str8;
										}
										else
										{
											foreach (string propertyName in searchResult.Properties.PropertyNames)
											{
												if (propertyName.Length < PropertyManager.MsDSHasInstantiatedNCs.Length || Utils.Compare(propertyName, 0, PropertyManager.MsDSHasInstantiatedNCs.Length, PropertyManager.MsDSHasInstantiatedNCs, 0, PropertyManager.MsDSHasInstantiatedNCs.Length) != 0)
												{
													continue;
												}
												str2 = propertyName;
												break;
											}
										}
										if (str2 == null)
										{
											continue;
										}
										bool flag2 = false;
										int num1 = 0;
										foreach (string item2 in searchResult.Properties[str2])
										{
											if (item2.Length - 13 >= partitionName.Length && Utils.Compare(item2, 13, partitionName.Length, partitionName, 0, partitionName.Length) == 0)
											{
												flag2 = true;
												if (string.Compare(item2, 10, "0", 0, 1, StringComparison.OrdinalIgnoreCase) == 0)
												{
													arrayLists1.Add(searchResultPropertyValue1);
													if (!isADAM)
													{
														break;
													}
													hashtables1.Add(searchResultPropertyValue1, (int)PropertyManager.GetSearchResultPropertyValue(searchResult, PropertyManager.MsDSPortLDAP));
													break;
												}
											}
											num1++;
										}
										if (flag2 || str2.Length < str8.Length || Utils.Compare(str2, 0, str8.Length, str8, 0, str8.Length) == 0)
										{
											continue;
										}
										flag1 = true;
										arrayLists4.Add(searchResultPropertyValue1);
										num = num1;
									}
									else
									{
										arrayLists1.Add(searchResultPropertyValue1);
										if (!isADAM)
										{
											continue;
										}
										hashtables1.Add(searchResultPropertyValue1, (int)PropertyManager.GetSearchResultPropertyValue(searchResult, PropertyManager.MsDSPortLDAP));
									}
								}
							}
						}
					}
					if (flag1)
					{
						do
						{
							StringBuilder stringBuilder4 = new StringBuilder(20);
							if (arrayLists4.Count > 1)
							{
								stringBuilder4.Append("(|");
							}
							foreach (string arrayList in arrayLists4)
							{
								stringBuilder4.Append("(");
								stringBuilder4.Append(PropertyManager.NCName);
								stringBuilder4.Append("=");
								stringBuilder4.Append(Utils.GetEscapedFilterValue(arrayList));
								stringBuilder4.Append(")");
							}
							if (arrayLists4.Count > 1)
							{
								stringBuilder4.Append(")");
							}
							arrayLists4.Clear();
							flag1 = false;
							objectCategory = new string[5];
							objectCategory[0] = "(&(";
							objectCategory[1] = PropertyManager.ObjectCategory;
							objectCategory[2] = "=nTDSDSA)";
							objectCategory[3] = stringBuilder4.ToString();
							objectCategory[4] = ")";
							aDSearcher.Filter = string.Concat(objectCategory);
							msDSHasInstantiatedNCs = new object[4];
							msDSHasInstantiatedNCs[0] = PropertyManager.MsDSHasInstantiatedNCs;
							msDSHasInstantiatedNCs[1] = ";range=";
							msDSHasInstantiatedNCs[2] = num;
							msDSHasInstantiatedNCs[3] = "-*";
							string str11 = string.Concat(msDSHasInstantiatedNCs);
							aDSearcher.PropertiesToLoad.Clear();
							aDSearcher.PropertiesToLoad.Add(str11);
							aDSearcher.PropertiesToLoad.Add(PropertyManager.DistinguishedName);
							SearchResultCollection searchResultCollections1 = aDSearcher.FindAll();
							try
							{
								foreach (SearchResult searchResult1 in searchResultCollections1)
								{
									string searchResultPropertyValue2 = (string)PropertyManager.GetSearchResultPropertyValue(searchResult1, PropertyManager.DistinguishedName);
									str3 = null;
									if (searchResult1.Properties.Contains(str11))
									{
										str3 = str11;
									}
									else
									{
										enumerator = searchResult1.Properties.PropertyNames.GetEnumerator();
										try
										{
											while (enumerator.MoveNext())
											{
												string str12 = searchResult1.ToString ();
												if (string.Compare(str12, 0, PropertyManager.MsDSHasInstantiatedNCs, 0, PropertyManager.MsDSHasInstantiatedNCs.Length, StringComparison.OrdinalIgnoreCase) != 0)
												{
													continue;
												}
												str3 = str12;
												break;
											}
										}
										finally
										{
											disposable = enumerator as IDisposable;
											if (disposable != null)
											{
												disposable.Dispose();
											}
										}
									}
									if (str3 == null)
									{
										continue;
									}
									bool flag3 = false;
									int num2 = 0;
									enumerator = searchResult1.Properties[str3].GetEnumerator();
									try
									{
										while (enumerator.MoveNext())
										{
											string str13 = searchResult1.ToString ();
											if (str13.Length - 13 >= partitionName.Length && Utils.Compare(str13, 13, partitionName.Length, partitionName, 0, partitionName.Length) == 0)
											{
												if (string.Compare(str13, 10, "0", 0, 1, StringComparison.OrdinalIgnoreCase) == 0)
												{
													arrayLists1.Add(searchResultPropertyValue2);
													if (!isADAM)
													{
														break;
													}
													hashtables1.Add(searchResultPropertyValue2, (int)PropertyManager.GetSearchResultPropertyValue(searchResult1, PropertyManager.MsDSPortLDAP));
													break;
												}
											}
											num2++;
										}
									}
									finally
									{
										disposable = enumerator as IDisposable;
										if (disposable != null)
										{
											disposable.Dispose();
										}
									}
									if (flag3 || str3.Length < str11.Length || Utils.Compare(str3, 0, str11.Length, str11, 0, str11.Length) == 0)
									{
										continue;
									}
									flag1 = true;
									arrayLists4.Add(searchResultPropertyValue2);
									num = num + num2;
								}
							}
							finally
							{
								searchResultCollections1.Dispose();
							}
						}
						while (flag1);
					}
				}
				catch (COMException cOMException7)
				{
					COMException cOMException6 = cOMException7;
					if (cOMException6.ErrorCode != -2147016656 || siteName == null)
					{
						throw ExceptionHelper.GetExceptionFromCOMException(context, cOMException6);
					}
					else
					{
						arrayLists = arrayLists2;
						return arrayLists;
					}
				}
				foreach (string arrayList1 in arrayLists1)
				{
					string item3 = (string)hashtables[arrayList1];
					if (item3 != null)
					{
						if (!isADAM || hashtables1[arrayList1] != null)
						{
							if (!isADAM)
							{
								arrayLists2.Add(item3);
							}
							else
							{
								arrayLists2.Add(string.Concat(item3, ":", (object)((int)hashtables1[arrayList1])));
							}
						}
						else
						{
							msDSHasInstantiatedNCs = new object[1];
							msDSHasInstantiatedNCs[0] = arrayList1;
							throw new ActiveDirectoryOperationException(Res.GetString("NoHostNameOrPortNumber", msDSHasInstantiatedNCs));
						}
					}
					else
					{
						if (!isADAM)
						{
							msDSHasInstantiatedNCs = new object[1];
							msDSHasInstantiatedNCs[0] = arrayList1;
							throw new ActiveDirectoryOperationException(Res.GetString("NoHostName", msDSHasInstantiatedNCs));
						}
						else
						{
							msDSHasInstantiatedNCs = new object[1];
							msDSHasInstantiatedNCs[0] = arrayList1;
							throw new ActiveDirectoryOperationException(Res.GetString("NoHostNameOrPortNumber", msDSHasInstantiatedNCs));
						}
					}
				}
				return arrayLists2;
			}
			return arrayLists;
		}

		internal static string GetServerNameFromInvocationID(string serverObjectDN, Guid invocationID, DirectoryServer server)
		{
			DirectoryEntry directoryEntry;
			string str;
			string siteObjectName;
			string propertyValue = null;
			if (serverObjectDN != null)
			{
				directoryEntry = DirectoryEntryManager.GetDirectoryEntry(server.Context, serverObjectDN);
				try
				{
					propertyValue = (string)PropertyManager.GetPropertyValue(directoryEntry.Parent, PropertyManager.DnsHostName);
					goto Label0;
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					if (cOMException.ErrorCode != -2147016656)
					{
						throw ExceptionHelper.GetExceptionFromCOMException(server.Context, cOMException);
					}
					else
					{
						str = null;
					}
				}
				return str;
			}
			else
			{
				if (server as DomainController != null)
				{
					siteObjectName = ((DomainController)server).SiteObjectName;
				}
				else
				{
					siteObjectName = ((AdamInstance)server).SiteObjectName;
				}
				string str1 = siteObjectName;
				DirectoryEntry directoryEntry1 = DirectoryEntryManager.GetDirectoryEntry(server.Context, str1);
				byte[] byteArray = invocationID.ToByteArray();
				IntPtr intPtr = (IntPtr)0;
				string stringUni = null;
				int num = UnsafeNativeMethods.ADsEncodeBinaryData(byteArray, (int)byteArray.Length, ref intPtr);
				if (num != 0)
				{
					throw ExceptionHelper.GetExceptionFromCOMException(new COMException(ExceptionHelper.GetErrorMessage(num, true), num));
				}
				else
				{
					try
					{
						stringUni = Marshal.PtrToStringUni(intPtr);
					}
					finally
					{
						if (intPtr != (IntPtr)0)
						{
							UnsafeNativeMethods.FreeADsMem(intPtr);
						}
					}
					string[] strArrays = new string[1];
					strArrays[0] = "distinguishedName";
					ADSearcher aDSearcher = new ADSearcher(directoryEntry1, string.Concat("(&(objectClass=nTDSDSA)(invocationID=", stringUni, "))"), strArrays, SearchScope.Subtree, false, false);
					try
					{
						SearchResult searchResult = aDSearcher.FindOne();
						if (searchResult != null)
						{
							DirectoryEntry parent = searchResult.GetDirectoryEntry().Parent;
							propertyValue = (string)PropertyManager.GetPropertyValue(server.Context, parent, PropertyManager.DnsHostName);
						}
					}
					catch (COMException cOMException3)
					{
						COMException cOMException2 = cOMException3;
						throw ExceptionHelper.GetExceptionFromCOMException(server.Context, cOMException2);
					}
				}
			}
			return propertyValue;
		Label0:
			if (server as AdamInstance != null)
			{
				int propertyValue1 = (int)PropertyManager.GetPropertyValue(server.Context, directoryEntry, PropertyManager.MsDSPortLDAP);
				if (propertyValue1 != 0x185)
				{
					propertyValue = string.Concat(propertyValue, ":", propertyValue1);
					return propertyValue;
				}
				else
				{
					return propertyValue;
				}
			}
			else
			{
				return propertyValue;
			}
		}

		internal static ActiveDirectoryTransportType GetTransportTypeFromDN(string DN)
		{
			string rdnFromDN = Utils.GetRdnFromDN(DN);
			Component[] dNComponents = Utils.GetDNComponents(rdnFromDN);
			string value = dNComponents[0].Value;
			if (string.Compare(value, "IP", StringComparison.OrdinalIgnoreCase) != 0)
			{
				if (string.Compare(value, "SMTP", StringComparison.OrdinalIgnoreCase) != 0)
				{
					object[] objArray = new object[1];
					objArray[0] = value;
					string str = Res.GetString("UnknownTransport", objArray);
					throw new ActiveDirectoryOperationException(str);
				}
				else
				{
					return ActiveDirectoryTransportType.Smtp;
				}
			}
			else
			{
				return ActiveDirectoryTransportType.Rpc;
			}
		}

		internal static Hashtable GetValuesWithRangeRetrieval(DirectoryEntry searchRootEntry, string filter, ArrayList propertiesToLoad, SearchScope searchScope)
		{
			return Utils.GetValuesWithRangeRetrieval(searchRootEntry, filter, propertiesToLoad, new ArrayList(), searchScope);
		}

		internal static Hashtable GetValuesWithRangeRetrieval(DirectoryEntry searchRootEntry, string filter, ArrayList propertiesWithRangeRetrieval, ArrayList propertiesWithoutRangeRetrieval, SearchScope searchScope)
		{
			string str;
			ADSearcher aDSearcher = new ADSearcher(searchRootEntry, filter, new string[0], searchScope, false, false);
			int count = 0;
			Hashtable hashtables = new Hashtable();
			Hashtable hashtables1 = new Hashtable();
			ArrayList arrayLists = new ArrayList();
			ArrayList arrayLists1 = new ArrayList();
			foreach (string str1 in propertiesWithoutRangeRetrieval)
			{
				string lower = str1.ToLower(CultureInfo.InvariantCulture);
				arrayLists.Add(lower);
				hashtables.Add(lower, new ArrayList());
				aDSearcher.PropertiesToLoad.Add(str1);
			}
			foreach (string str2 in propertiesWithRangeRetrieval)
			{
				string lower1 = str2.ToLower(CultureInfo.InvariantCulture);
				arrayLists1.Add(lower1);
				hashtables.Add(lower1, new ArrayList());
			}
			do
			{
				foreach (string arrayList in arrayLists1)
				{
					object[] objArray = new object[4];
					objArray[0] = arrayList;
					objArray[1] = ";range=";
					objArray[2] = count;
					objArray[3] = "-*";
					string str3 = string.Concat(objArray);
					aDSearcher.PropertiesToLoad.Add(str3);
					hashtables1.Add(arrayList.ToLower(CultureInfo.InvariantCulture), str3);
				}
				arrayLists1.Clear();
				SearchResult searchResult = aDSearcher.FindOne();
				if (searchResult == null)
				{
					throw new ActiveDirectoryObjectNotFoundException(Res.GetString("DSNotFound"));
				}
				else
				{
					foreach (string propertyName in searchResult.Properties.PropertyNames)
					{
						int num = propertyName.IndexOf(';');
						if (num == -1)
						{
							str = propertyName;
						}
						else
						{
							str = propertyName.Substring(0, num);
						}
						if (!hashtables1.Contains(str) && !arrayLists.Contains(str))
						{
							continue;
						}
						ArrayList item = (ArrayList)hashtables[str];
						item.AddRange(searchResult.Properties[propertyName]);
						if (!hashtables1.Contains(str))
						{
							continue;
						}
						string item1 = (string)hashtables1[str];
						if (propertyName.Length < item1.Length || Utils.Compare(item1, 0, item1.Length, propertyName, 0, item1.Length) == 0)
						{
							continue;
						}
						arrayLists1.Add(str);
						count = count + searchResult.Properties[propertyName].Count;
					}
					aDSearcher.PropertiesToLoad.Clear();
					hashtables1.Clear();
				}
			}
			while (arrayLists1.Count > 0);
			return hashtables;
		}

		internal static bool Impersonate(DirectoryContext context)
		{
			string str = null;
			string str1 = null;
			IntPtr intPtr = (IntPtr)0;
			if (context.UserName != null || context.Password != null)
			{
				Utils.GetDomainAndUsername(context, out str, out str1);
				int lastWin32Error = UnsafeNativeMethods.LogonUserW(str, str1, context.Password, Utils.LOGON32_LOGON_NEW_CREDENTIALS, Utils.LOGON32_PROVIDER_WINNT50, ref intPtr);
				if (lastWin32Error != 0)
				{
					try
					{
						lastWin32Error = UnsafeNativeMethods.ImpersonateLoggedOnUser(intPtr);
						if (lastWin32Error == 0)
						{
							lastWin32Error = Marshal.GetLastWin32Error();
							throw ExceptionHelper.GetExceptionFromErrorCode(lastWin32Error);
						}
					}
					finally
					{
						if (intPtr != (IntPtr)0)
						{
							UnsafeNativeMethods.CloseHandle(intPtr);
						}
					}
					return true;
				}
				else
				{
					throw ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
				}
			}
			else
			{
				return false;
			}
		}

		internal static void ImpersonateAnonymous()
		{
			IntPtr intPtr = UnsafeNativeMethods.OpenThread(Utils.THREAD_ALL_ACCESS, false, UnsafeNativeMethods.GetCurrentThreadId());
			if (intPtr != (IntPtr)0)
			{
				try
				{
					int num = UnsafeNativeMethods.ImpersonateAnonymousToken(intPtr);
					if (num == 0)
					{
						throw ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
					}
				}
				finally
				{
					if (intPtr != (IntPtr)0)
					{
						UnsafeNativeMethods.CloseHandle(intPtr);
					}
				}
				return;
			}
			else
			{
				throw ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
			}
		}

		[SecuritySafeCritical]
		internal static bool IsMachineDC(string computerName)
		{
			int num;
			bool flag;
			bool machineRole;
			IntPtr zero = IntPtr.Zero;
			try
			{
				if (computerName != null)
				{
					num = UnsafeNativeMethods.DsRoleGetPrimaryDomainInformation(computerName, DSROLE_PRIMARY_DOMAIN_INFO_LEVEL.DsRolePrimaryDomainInfoBasic, out zero);
				}
				else
				{
					num = UnsafeNativeMethods.DsRoleGetPrimaryDomainInformation(IntPtr.Zero, DSROLE_PRIMARY_DOMAIN_INFO_LEVEL.DsRolePrimaryDomainInfoBasic, out zero);
				}
				if (num == 0)
				{
					DSROLE_PRIMARY_DOMAIN_INFO_BASIC structure = (DSROLE_PRIMARY_DOMAIN_INFO_BASIC)Marshal.PtrToStructure(zero, typeof(DSROLE_PRIMARY_DOMAIN_INFO_BASIC));
					if (structure.MachineRole == DSROLE_MACHINE_ROLE.DsRole_RoleBackupDomainController)
					{
						machineRole = true;
					}
					else
					{
						machineRole = structure.MachineRole == DSROLE_MACHINE_ROLE.DsRole_RolePrimaryDomainController;
					}
					flag = machineRole;
				}
				else
				{
					object[] objArray = new object[1];
					objArray[0] = num;
					throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Res.GetString("UnableToRetrieveDomainInfo"), objArray));
				}
			}
			finally
			{
				if (zero != IntPtr.Zero)
				{
					UnsafeNativeMethods.DsRoleFreeMemory(zero);
				}
			}
			return flag;
		}

		[SecurityCritical]
		internal static bool IsSamUser()
		{
			bool flag;
			bool flag1;
			IntPtr zero = IntPtr.Zero;
			IntPtr machineDomainSid = IntPtr.Zero;
			try
			{
				zero = Utils.GetCurrentUserSid();
				SidType sidType = Utils.ClassifySID(zero);
				if (sidType != SidType.RealObject)
				{
					flag = true;
				}
				else
				{
					machineDomainSid = Utils.GetMachineDomainSid();
					bool flag2 = false;
					UnsafeNativeMethods.EqualDomainSid(zero, machineDomainSid, ref flag2);
					if (flag2)
					{
						flag1 = !Utils.IsMachineDC(null);
					}
					else
					{
						flag1 = false;
					}
					flag = flag1;
				}
			}
			finally
			{
				if (zero != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(zero);
				}
				if (machineDomainSid != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(machineDomainSid);
				}
			}
			return flag;
		}

		internal static bool IsValidDNFormat(string distinguishedName)
		{
			string[] strArrays = Utils.Split(distinguishedName, ',');
			Component[] componentArray = new Component[strArrays.GetLength(0)];
			int num = 0;
			while (num < strArrays.GetLength(0))
			{
				string[] strArrays1 = Utils.Split(strArrays[num], '=');
				if (strArrays1.GetLength(0) == 2)
				{
					componentArray[num].Name = strArrays1[0].Trim();
					if (componentArray[num].Name.Length != 0)
					{
						componentArray[num].Value = strArrays1[1].Trim();
						if (componentArray[num].Value.Length != 0)
						{
							num++;
						}
						else
						{
							return false;
						}
					}
					else
					{
						return false;
					}
				}
				else
				{
					return false;
				}
			}
			return true;
		}

		internal static void Revert()
		{
			int self = UnsafeNativeMethods.RevertToSelf();
			if (self != 0)
			{
				return;
			}
			else
			{
				throw ExceptionHelper.GetExceptionFromErrorCode(Marshal.GetLastWin32Error());
			}
		}

		public static string[] Split(string distinguishedName, char delim)
		{
			bool flag = false;
			char chr = '\"';
			char chr1 = '\\';
			int num = 0;
			ArrayList arrayLists = new ArrayList();
			for (int i = 0; i < distinguishedName.Length; i++)
			{
				char chr2 = distinguishedName[i];
				if (chr2 != chr)
				{
					if (chr2 != chr1)
					{
						if (!flag && chr2 == delim)
						{
							arrayLists.Add(distinguishedName.Substring(num, i - num));
							num = i + 1;
						}
					}
					else
					{
						if (i < distinguishedName.Length - 1)
						{
							i++;
						}
					}
				}
				else
				{
					flag = !flag;
				}
				if (i == distinguishedName.Length - 1)
				{
					if (!flag)
					{
						arrayLists.Add(distinguishedName.Substring(num, i - num + 1));
					}
					else
					{
						throw new ArgumentException(Res.GetString("InvalidDNFormat"), "distinguishedName");
					}
				}
			}
			string[] item = new string[arrayLists.Count];
			for (int j = 0; j < arrayLists.Count; j++)
			{
				item[j] = (string)arrayLists[j];
			}
			return item;
		}

		internal static string SplitServerNameAndPortNumber(string serverName, out string portNumber)
		{
			portNumber = null;
			int num = serverName.LastIndexOf(':');
			if (num != -1)
			{
				bool flag = serverName.StartsWith("[");
				if (!flag)
				{
					try
					{
						IPAddress pAddress = IPAddress.Parse(serverName);
						if (pAddress.AddressFamily == AddressFamily.InterNetworkV6)
						{
							string str = serverName;
							return str;
						}
					}
					catch (FormatException formatException)
					{
					}
					portNumber = serverName.Substring(num + 1);
					serverName = serverName.Substring(0, num);
					return serverName;
				}
				else
				{
					if (!serverName.EndsWith("]"))
					{
						int num1 = serverName.LastIndexOf("]:");
						if (num1 == -1 || num1 + 1 != num)
						{
							return serverName;
						}
						else
						{
							portNumber = serverName.Substring(num + 1);
							serverName = serverName.Substring(1, num1 - 1);
							return serverName;
						}
					}
					else
					{
						serverName = serverName.Substring(1, serverName.Length - 2);
						return serverName;
					}
				}
			}
			else
			{
				return serverName;
			}
		}
	}
}