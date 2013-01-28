using Microsoft.ActiveDirectory;
using Microsoft.ActiveDirectory.Management;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.ActiveDirectory.Management.Commands
{
	internal class Utils
	{
		private const string _debugCategory = "Utils";

		private const string GUID_REGEX_PATTERN_1 = "^[a-fA-F\\d]{8}-([a-fA-F\\d]{4}-){3}[a-fA-F\\d]{12}$";

		private const string GUID_REGEX_PATTERN_2 = "^[a-fA-F\\d]{32}$";

		private const string GUID_REGEX_PATTERN_3 = "^0(x|X)[a-fA-F\\d]{8}( )*,( )*0(x|X)[a-fA-F\\d]{4}( )*,( )*0(x|X)[a-fA-F\\d]{4}( )*,( )*\\{0(x|X)[a-fA-F\\d]{2}( )*,( )*0(x|X)[a-fA-F\\d]{2}( )*,( )*0(x|X)[a-fA-F\\d]{2}( )*,( )*0(x|X)[a-fA-F\\d]{2}( )*,( )*0(x|X)[a-fA-F\\d]{2}( )*,( )*0(x|X)[a-fA-F\\d]{2}( )*,( )*0(x|X)[a-fA-F\\d]{2}( )*,( )*0(x|X)[a-fA-F\\d]{2}( )*\\}$";

		private const string _escapedBackslashReplacement = "\\\\5c";

		private const string _escapedHexPairReplacement = "$1\\\\$2";

		private readonly static Regex _guidRegexPatternOne;

		private readonly static Regex _guidRegexPatternTwo;

		private readonly static Regex _guidRegexPatternThree;

		private readonly static Regex _sidRegex;

		private readonly static Regex _escapedBackslashRegex;

		private readonly static Regex _escapedHexPairRegex;

		static Utils()
		{
			Utils._guidRegexPatternOne = new Regex("^[a-fA-F\\d]{8}-([a-fA-F\\d]{4}-){3}[a-fA-F\\d]{12}$");
			Utils._guidRegexPatternTwo = new Regex("^[a-fA-F\\d]{32}$");
			Utils._guidRegexPatternThree = new Regex("^0(x|X)[a-fA-F\\d]{8}( )*,( )*0(x|X)[a-fA-F\\d]{4}( )*,( )*0(x|X)[a-fA-F\\d]{4}( )*,( )*\\{0(x|X)[a-fA-F\\d]{2}( )*,( )*0(x|X)[a-fA-F\\d]{2}( )*,( )*0(x|X)[a-fA-F\\d]{2}( )*,( )*0(x|X)[a-fA-F\\d]{2}( )*,( )*0(x|X)[a-fA-F\\d]{2}( )*,( )*0(x|X)[a-fA-F\\d]{2}( )*,( )*0(x|X)[a-fA-F\\d]{2}( )*,( )*0(x|X)[a-fA-F\\d]{2}( )*\\}$");
			Utils._sidRegex = new Regex("^[Ss](?:-[0-9]+){3,}$");
			Utils._escapedBackslashRegex = new Regex("\\\\\\\\");
			Utils._escapedHexPairRegex = new Regex("(^|[^\\\\])\\\\([0-9a-fA-F][0-9a-fA-F])");
		}

		public Utils()
		{
		}

		internal static ADServerType ADServerTypeFromRootDSE(ADRootDSE rootDSE)
		{
			if (rootDSE.ServerType == ADServerType.ADLDS)
			{
				return ADServerType.ADLDS;
			}
			else
			{
				return ADServerType.ADDS;
			}
		}

		internal static string ConvertSIDToStringizedSid(SecurityIdentifier sid)
		{
			if (sid != null)
			{
				StringBuilder stringBuilder = new StringBuilder("<SID=");
				stringBuilder.Append(sid.Value);
				stringBuilder.Append(">");
				return stringBuilder.ToString();
			}
			else
			{
				throw new ArgumentNullException("sid");
			}
		}

		public static ADObject CreateIdentityCopy(ADObject sourceObj)
		{
			object item;
			ADObject aDObject = new ADObject();
			string[] identityPropertyNames = ADObject.IdentityPropertyNames;
			for (int i = 0; i < (int)identityPropertyNames.Length; i++)
			{
				string str = identityPropertyNames[i];
				if (!aDObject.Contains(str))
				{
					item = sourceObj[str];
					if (item != null)
					{
						aDObject.Add(str, item);
					}
				}
			}
			foreach (string propertyName in sourceObj.InternalProperties.PropertyNames)
			{
				if (aDObject.InternalProperties.Contains(propertyName))
				{
					continue;
				}
				item = sourceObj.InternalProperties[propertyName].Value;
				if (item == null)
				{
					continue;
				}
				aDObject.InternalProperties.SetValue(propertyName, item);
			}
			return aDObject;
		}

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
						break;
					}
					else if (chr1 == '<')
					{
						stringBuilder.Append("\\<");
						break;
					}
					else if (chr1 == '=')
					{
						stringBuilder.Append("\\=");
						break;
					}
					else if (chr1 == '>')
					{
						stringBuilder.Append("\\>");
						break;
					}
					if (chr1 != '\\')
					{
						break;
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
							break;
						}
						else if (chr1 == ',')
						{
							stringBuilder.Append("\\,");
							break;
						}
						goto Label0;
					}
				}
			}
			if (stringBuilder[stringBuilder.Length - 1] == ' ')
			{
				stringBuilder.Remove(stringBuilder.Length - 1, 1);
				stringBuilder.Append("\\ ");
			}
			return stringBuilder.ToString();
		Label0:
			stringBuilder.Append(chr.ToString());
			if (stringBuilder[stringBuilder.Length - 1] == ' ')
			{
				stringBuilder.Remove(stringBuilder.Length - 1, 1);
				stringBuilder.Append("\\ ");
			}
			return stringBuilder.ToString();
		}

		internal static string EscapeDNForFilter(string dn)
		{
			if (!string.IsNullOrEmpty(dn))
			{
				dn = Utils._escapedBackslashRegex.Replace(dn, "\\\\5c");
				return Utils._escapedHexPairRegex.Replace(dn, "$1\\\\$2");
			}
			else
			{
				return dn;
			}
		}

		internal static ADObject GetDirectoryObject(string DN, string[] directoryAttributes, CmdletSessionInfo cmdletSessionInfo)
		{
			ADObject aDObject;
			using (ADObjectSearcher aDObjectSearcher = new ADObjectSearcher(cmdletSessionInfo.ADSessionInfo))
			{
				aDObjectSearcher.SearchRoot = DN;
				aDObjectSearcher.Scope = ADSearchScope.Base;
				if (directoryAttributes != null)
				{
					aDObjectSearcher.Properties.AddRange(directoryAttributes);
				}
				aDObject = aDObjectSearcher.FindOne();
			}
			return aDObject;
		}

		internal static string GetNtStatusMessage(int ntStatus)
		{
			string str;
			if (ntStatus != 0)
			{
				IntPtr zero = IntPtr.Zero;
				IntPtr intPtr = IntPtr.Zero;
				try
				{
					try
					{
						zero = UnsafeNativeMethods.LoadLibrary("Ntdll.dll");
						if (zero != IntPtr.Zero)
						{
							uint num = 0x1900;
							int lCID = CultureInfo.CurrentCulture.LCID;
							int num1 = UnsafeNativeMethods.FormatMessage(num, zero, ntStatus, lCID, out intPtr, 0, IntPtr.Zero);
							if (num1 != 0)
							{
								string stringAuto = Marshal.PtrToStringAuto(intPtr);
								char[] chrArray = new char[2];
								chrArray[0] = '\n';
								chrArray[1] = '\r';
								char[] chrArray1 = chrArray;
								str = stringAuto.TrimEnd(chrArray1);
								return str;
							}
						}
					}
					catch (Win32Exception win32Exception)
					{
					}
					return StringResources.UnspecifiedError;
				}
				finally
				{
					if (zero != IntPtr.Zero)
					{
						UnsafeNativeMethods.FreeLibrary(zero);
					}
					UnsafeNativeMethods.LocalFree(intPtr);
				}
				return str;
			}
			else
			{
				return StringResources.OperationSuccessful;
			}
		}

		internal static string GetWellKnownGuidDN(ADSessionInfo adSession, string partitionDN, string wellKnownGuid)
		{
			string distinguishedName;
			if (partitionDN != null)
			{
				ADObjectSearcher aDObjectSearcher = new ADObjectSearcher(adSession);
				try
				{
					try
					{
						aDObjectSearcher.SearchRoot = string.Format("<WKGUID={0},{1}>", wellKnownGuid, partitionDN);
						aDObjectSearcher.Scope = ADSearchScope.Base;
						aDObjectSearcher.Filter = ADOPathUtil.CreateFilterClause(ADOperator.Like, "objectClass", "*");
						ADObject aDObject = aDObjectSearcher.FindOne();
						if (aDObject == null)
						{
							distinguishedName = null;
						}
						else
						{
							distinguishedName = aDObject.DistinguishedName;
						}
					}
					catch (ADIdentityNotFoundException aDIdentityNotFoundException1)
					{
						ADIdentityNotFoundException aDIdentityNotFoundException = aDIdentityNotFoundException1;
						DebugLogger.LogError("Utils", string.Format("Error in searching for WKGUID {0}", aDIdentityNotFoundException.Message));
						distinguishedName = null;
					}
				}
				finally
				{
					aDObjectSearcher.Dispose();
				}
				return distinguishedName;
			}
			else
			{
				return null;
			}
		}

		internal static bool HasFlagsSet(int value, int flags)
		{
			return (value & flags) == flags;
		}

		internal static bool IsDeleted(ADEntity directoryObj)
		{
			if (directoryObj["isDeleted"].Value == null)
			{
				return false;
			}
			else
			{
				return (bool)directoryObj["isDeleted"].Value;
			}
		}

		internal static bool IsNamingContext(ADEntity directoryObj)
		{
			ADPropertyValueCollection item = directoryObj["instanceType"];
			int ret = directoryObj["instanceType"].Value is int ? (int)directoryObj["instanceType"].Value : 1;
			if (directoryObj["instanceType"].Value == null || ret == 0)
			{
				return false;
			}
			else
			{
				return Utils.HasFlagsSet((int)item.Value, 1);
			}
		}

		public static bool TryParseEnum<T>(string str, out T retValue)
		{
			object obj = null;
			if (Utils.TryParseEnum(typeof(T), str, out obj))
			{
				retValue = (T)obj;
				return true;
			}
			else
			{
				retValue = default(T);
				return false;
			}
		}

		internal static bool TryParseEnum(Type enumType, object inValue, out object retValue)
		{
			string str;
			int num;
			long num1;
			bool underlyingType = Enum.GetUnderlyingType(enumType) == typeof(int);
			if (inValue is int || inValue is long)
			{
				num1 = Convert.ToInt64(inValue, CultureInfo.InvariantCulture);
				if (!underlyingType)
				{
					if (Enum.IsDefined(enumType, num1))
					{
						retValue = Enum.ToObject(enumType, num1);
						return true;
					}
				}
				else
				{
					if (num1 >= (long)-2147483648 && num1 <= (long)0x7fffffff)
					{
						num = Convert.ToInt32(num1, CultureInfo.InvariantCulture);
						if (Enum.IsDefined(enumType, num))
						{
							retValue = Enum.ToObject(enumType, num);
							return true;
						}
					}
				}
			}
			else
			{
				if (inValue as string == null)
				{
					str = inValue.ToString();
				}
				else
				{
					str = (string)inValue;
				}
				if (!int.TryParse(str, out num))
				{
					if (!long.TryParse(str, out num1))
					{
						string[] names = Enum.GetNames(enumType);
						int num2 = 0;
						while (num2 < (int)names.Length)
						{
							string str1 = names[num2];
							if (!str1.Equals(str, StringComparison.OrdinalIgnoreCase))
							{
								num2++;
							}
							else
							{
								retValue = Enum.Parse(enumType, str, true);
								bool flag = true;
								return flag;
							}
						}
					}
					else
					{
						return Utils.TryParseEnum(enumType, num1, out retValue);
					}
				}
				else
				{
					return Utils.TryParseEnum(enumType, num, out retValue);
				}
			}
			retValue = null;
			return false;
		}

		internal static bool TryParseGuid(string inputStr, out Guid? guidObject)
		{
			bool flag;
			if (inputStr != null)
			{
				string str = inputStr.Trim();
				if (str.StartsWith("{") && str.EndsWith("}") || str.StartsWith("(") && str.EndsWith(")"))
				{
					str = str.Substring(1, str.Length - 2);
					str = str.Trim();
				}
				guidObject = null;
				bool flag1 = Utils._guidRegexPatternOne.IsMatch(str);
				if (!flag1)
				{
					flag1 = Utils._guidRegexPatternTwo.IsMatch(str);
				}
				if (!flag1)
				{
					flag1 = Utils._guidRegexPatternThree.IsMatch(str);
				}
				if (!flag1)
				{
					return false;
				}
				else
				{
					try
					{
						guidObject = new Guid?(new Guid(inputStr));
						flag = true;
					}
					catch (FormatException formatException)
					{
						flag = false;
					}
					return flag;
				}
			}
			else
			{
				throw new ArgumentNullException("inputStr");
			}
		}

		internal static bool TryParseSid(string inputStr, out SecurityIdentifier sidObject)
		{
			bool flag;
			if (inputStr != null)
			{
				sidObject = null;
				if (!Utils._sidRegex.IsMatch(inputStr))
				{
					return false;
				}
				else
				{
					try
					{
						sidObject = new SecurityIdentifier(inputStr);
						flag = true;
					}
					catch (ArgumentException argumentException)
					{
						flag = false;
					}
					return flag;
				}
			}
			else
			{
				throw new ArgumentNullException("inputStr");
			}
		}
	}
}