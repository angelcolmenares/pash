using System;
using System.Collections;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Globalization;
using System.Security;
using System.Security.Authentication;
using System.Security.Principal;
using System.Text;
using System.Security.Permissions;

namespace System.DirectoryServices.AccountManagement
{
	internal class ADUtils
	{
		private ADUtils()
		{
		}

		internal static DateTime ADFileTimeToDateTime(long filetime)
		{
			return DateTime.FromFileTimeUtc(filetime);
		}

		[SecurityCritical]
		internal static bool ArePrincipalsInSameForest(Principal p1, Principal p2)
		{
			string dnsForestName = ((ADStoreCtx)p1.GetStoreCtxToUse()).DnsForestName;
			string str = ((ADStoreCtx)p2.GetStoreCtxToUse()).DnsForestName;
			return string.Compare(dnsForestName, str, StringComparison.OrdinalIgnoreCase) == 0;
		}

		internal static bool AreSidsInSameDomain(SecurityIdentifier sid1, SecurityIdentifier sid2)
		{
			if (!sid1.IsAccountSid() || !sid2.IsAccountSid())
			{
				return false;
			}
			else
			{
				return sid1.AccountDomainSid.Equals(sid2.AccountDomainSid);
			}
		}

		internal static long DateTimeToADFileTime(DateTime dt)
		{
			return dt.ToFileTimeUtc();
		}

		internal static string DateTimeToADString(DateTime dateTime)
		{
			long fileTimeUtc = dateTime.ToFileTimeUtc();
			return fileTimeUtc.ToString(CultureInfo.InvariantCulture);
		}

		[DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
		[SecurityCritical]
		internal static Principal DirectoryEntryAsPrincipal(DirectoryEntry de, ADStoreCtx storeCtx)
		{
			if (ADUtils.IsOfObjectClass(de, "computer") || ADUtils.IsOfObjectClass(de, "user") || ADUtils.IsOfObjectClass(de, "group"))
			{
				return storeCtx.GetAsPrincipal(de, null);
			}
			else
			{
				if (!ADUtils.IsOfObjectClass(de, "foreignSecurityPrincipal"))
				{
					return storeCtx.GetAsPrincipal(de, null);
				}
				else
				{
					return storeCtx.ResolveCrossStoreRefToPrincipal(de);
				}
			}
		}

		internal static string EscapeBinaryValue(byte[] bytes)
		{
			StringBuilder stringBuilder = new StringBuilder((int)bytes.Length * 3);
			byte[] numArray = bytes;
			for (int i = 0; i < (int)numArray.Length; i++)
			{
				byte num = numArray[i];
				stringBuilder.Append("\\");
				stringBuilder.Append(num.ToString("x2", CultureInfo.InvariantCulture));
			}
			return stringBuilder.ToString();
		}

		[SecurityCritical]
		internal static string EscapeDNComponent(string dnComponent)
		{
			char chr;
			StringBuilder stringBuilder = new StringBuilder(dnComponent.Length);
			int num = 0;
			if (dnComponent[0] == ' ' || dnComponent[0] == '#')
			{
				stringBuilder.Append("\\");
				stringBuilder.Append(dnComponent[0]);
				num++;
			}
			for (int i = num; i < dnComponent.Length; i++)
			{
				chr = dnComponent[i];
				char chr1 = chr;
				if (chr1 > ',')
				{
					if (chr1 == ';')
					{
						stringBuilder.Append("\\;");
						goto Label1;
					}
					else if (chr1 == '<')
					{
						stringBuilder.Append("\\<");
						goto Label1;
					}
					else if (chr1 == '=')
					{
						stringBuilder.Append("\\=");
						goto Label1;
					}
					else if (chr1 == '>')
					{
						stringBuilder.Append("\\>");
						goto Label1;
					}
					if (chr1 != '\\')
					{
						goto Label0;
					}
					stringBuilder.Append("\\\\");
				}
				else
				{
					if (chr1 == '\"')
					{
						stringBuilder.Append("\\\"");
					}
					else
					{
						if (chr1 == '+')
						{
							stringBuilder.Append("\\+");
							goto Label1;
						}
						else if (chr1 == ',')
						{
							stringBuilder.Append("\\,");
							goto Label1;
						}
						stringBuilder.Append(chr.ToString());
						continue;
					}
				}
			}
			if (stringBuilder[stringBuilder.Length - 1] == ' ')
			{
				stringBuilder.Remove(stringBuilder.Length - 1, 1);
				stringBuilder.Append("\\ ");
			}
			return stringBuilder.ToString();
		}

		[SecurityCritical]
		internal static string EscapeRFC2254SpecialChars(string s)
		{
			StringBuilder stringBuilder = new StringBuilder(s.Length);
			string str = s;
			for (int i = 0; i < str.Length; i++)
			{
				char chr = str[i];
				char chr1 = chr;
				switch (chr1)
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
						stringBuilder.Append("\\2a");
						break;
					}
					default:
					{
						if (chr1 == '\\')
						{
							stringBuilder.Append("\\5c");
							break;
						}
						else
						{
							stringBuilder.Append(chr.ToString());
							break;
						}
					}
				}
			}
			return stringBuilder.ToString();
		}

		[DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
		[SecurityCritical]
		internal static string GetServerName(DirectoryEntry de)
		{
			UnsafeNativeMethods.IAdsObjectOptions nativeObject = (UnsafeNativeMethods.IAdsObjectOptions)de.NativeObject;
			return (string)nativeObject.GetOption(0);
		}

		internal static string HexStringToLdapHexString(string s)
		{
			if (s.Length % 2 == 0)
			{
				StringBuilder stringBuilder = new StringBuilder();
				int num = 0;
				while (num < s.Length / 2)
				{
					char chr = s[num * 2];
					char chr1 = s[num * 2 + 1];
					if ((chr < '0' || chr > '9') && (chr < 'A' || chr > 'F') && (chr < 'a' || chr > 'f') || (chr1 < '0' || chr1 > '9') && (chr1 < 'A' || chr1 > 'F') && (chr1 < 'a' || chr1 > 'f'))
					{
						return null;
					}
					else
					{
						stringBuilder.Append("\\");
						stringBuilder.Append(chr);
						stringBuilder.Append(chr1);
						num++;
					}
				}
				return stringBuilder.ToString();
			}
			else
			{
				return null;
			}
		}

		[DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
		[SecurityCritical]
		internal static bool IsOfObjectClass(DirectoryEntry de, string classToCompare)
		{
			return de.Properties["objectClass"].Contains(classToCompare);
		}

		[DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
		[SecurityCritical]
		internal static bool IsOfObjectClass(SearchResult sr, string classToCompare)
		{
			return sr.Properties["objectClass"].Contains(classToCompare);
		}

		internal static long LargeIntToInt64(UnsafeNativeMethods.IADsLargeInteger largeInt)
		{
			int lowPart = largeInt.LowPart;
			int highPart = largeInt.HighPart;
			long num = (long)lowPart | (long)highPart << 32;
			return num;
		}

		internal static string PAPIQueryToLdapQueryString(string papiString)
		{
			StringBuilder stringBuilder = new StringBuilder(papiString.Length);
			bool flag = false;
			string str = papiString;
			for (int i = 0; i < str.Length; i++)
			{
				char chr = str[i];
				if (flag)
				{
					flag = false;
					char chr1 = chr;
					switch (chr1)
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
							stringBuilder.Append("\\2a");
							break;
						}
						default:
						{
							if (chr1 == '\\')
							{
								stringBuilder.Append("\\5c");
								break;
							}
							else
							{
								stringBuilder.Append(chr.ToString());
								break;
							}
						}
					}
				}
				else
				{
					char chr2 = chr;
					switch (chr2)
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
						default:
						{
							if (chr2 == '\\')
							{
								flag = true;
								break;
							}
							else
							{
								stringBuilder.Append(chr.ToString());
								break;
							}
						}
					}
				}
			}
			return stringBuilder.ToString();
		}

		[SecurityCritical]
		internal static string RetriveWkDn(DirectoryEntry deBase, string defaultNamingContext, string serverName, byte[] wellKnownContainerGuid)
		{
			string str;
			PropertyValueCollection item = deBase.Properties["wellKnownObjects"];
			IEnumerator enumerator = item.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					UnsafeNativeMethods.IADsDNWithBinary current = (UnsafeNativeMethods.IADsDNWithBinary)enumerator.Current;
					if (!Utils.AreBytesEqual(wellKnownContainerGuid, (byte[])current.BinaryValue))
					{
						continue;
					}
					str = string.Concat("LDAP://", serverName, "/", current.DNString);
					return str;
				}
				return null;
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
			return str;
		}

		[DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
		[SecurityCritical]
		internal static Principal SearchResultAsPrincipal(SearchResult sr, ADStoreCtx storeCtx, object discriminant)
		{
			if (ADUtils.IsOfObjectClass(sr, "computer") || ADUtils.IsOfObjectClass(sr, "user") || ADUtils.IsOfObjectClass(sr, "group"))
			{
				return storeCtx.GetAsPrincipal(sr, discriminant);
			}
			else
			{
				if (!ADUtils.IsOfObjectClass(sr, "foreignSecurityPrincipal"))
				{
					return storeCtx.GetAsPrincipal(sr, discriminant);
				}
				else
				{
					return storeCtx.ResolveCrossStoreRefToPrincipal(sr.GetDirectoryEntry());
				}
			}
		}

		[SecurityCritical]
		internal static bool VerifyOutboundTrust(string targetDomain, string username, string password)
		{
			bool flag;
			Domain computerDomain = null;
			try
			{
				computerDomain = Domain.GetComputerDomain();
			}
			catch (ActiveDirectoryObjectNotFoundException activeDirectoryObjectNotFoundException)
			{
				flag = false;
				return flag;
			}
			catch (AuthenticationException authenticationException)
			{
				flag = false;
				return flag;
			}
			if (string.Compare(computerDomain.Name, targetDomain, StringComparison.OrdinalIgnoreCase) != 0)
			{
				try
				{
					TrustRelationshipInformation trustRelationship = computerDomain.GetTrustRelationship(targetDomain);
					if (TrustDirection.Outbound == trustRelationship.TrustDirection || TrustDirection.Bidirectional == trustRelationship.TrustDirection)
					{
						flag = true;
						return flag;
					}
				}
				catch (ActiveDirectoryObjectNotFoundException activeDirectoryObjectNotFoundException1)
				{
				}
				Forest currentForest = Forest.GetCurrentForest();
				Domain domain = Domain.GetDomain(new DirectoryContext(DirectoryContextType.Domain, targetDomain, username, password));
				try
				{
					ForestTrustRelationshipInformation forestTrustRelationshipInformation = currentForest.GetTrustRelationship(domain.Forest.Name);
					if (TrustDirection.Outbound == forestTrustRelationshipInformation.TrustDirection || TrustDirection.Bidirectional == forestTrustRelationshipInformation.TrustDirection)
					{
						flag = true;
						return flag;
					}
				}
				catch (ActiveDirectoryObjectNotFoundException activeDirectoryObjectNotFoundException2)
				{
				}
				return false;
			}
			else
			{
				return true;
			}
			return flag;
		}
	}
}