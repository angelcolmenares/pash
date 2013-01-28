using System;
using System.DirectoryServices;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Security.Permissions;

namespace System.DirectoryServices.AccountManagement
{
	[DirectoryServicesPermission(SecurityAction.Assert, Unrestricted=true)]
	[SecurityCritical(SecurityCriticalScope.Everything)]
	internal class SAMUtils
	{
		private SAMUtils()
		{
		}

		internal static Principal DirectoryEntryAsPrincipal(DirectoryEntry de, StoreCtx storeCtx)
		{
			if (SAMUtils.IsOfObjectClass(de, "Computer") || SAMUtils.IsOfObjectClass(de, "User") || SAMUtils.IsOfObjectClass(de, "Group"))
			{
				return storeCtx.GetAsPrincipal(de, null);
			}
			else
			{
				return null;
			}
		}

		internal static bool GetOSVersion(DirectoryEntry computerDE, out int versionMajor, out int versionMinor)
		{
			bool flag;
			versionMajor = 0;
			versionMinor = 0;
			string value = null;
			try
			{
				if (computerDE.Properties["OperatingSystemVersion"].Count > 0)
				{
					value = (string)computerDE.Properties["OperatingSystemVersion"].Value;
				}
			}
			catch (COMException cOMException1)
			{
				COMException cOMException = cOMException1;
				if (cOMException.ErrorCode != -2147024843)
				{
					throw;
				}
				else
				{
					flag = false;
					return flag;
				}
			}
			if (value == null || value.Length == 0)
			{
				return false;
			}
			else
			{
				char[] chrArray = new char[1];
				chrArray[0] = '.';
				string[] strArrays = value.Split(chrArray);
				try
				{
					versionMajor = int.Parse(strArrays[0], CultureInfo.InvariantCulture);
					if ((int)strArrays.Length > 1)
					{
						versionMinor = int.Parse(strArrays[1], CultureInfo.InvariantCulture);
					}
					if (versionMajor <= 0 || versionMinor < 0)
					{
						flag = false;
						return flag;
					}
				}
				catch (FormatException formatException)
				{
					flag = false;
					return flag;
				}
				catch (OverflowException overflowException)
				{
					flag = false;
					return flag;
				}
				return true;
			}
			return flag;
		}

		internal static bool IsOfObjectClass(DirectoryEntry de, string classToCompare)
		{
			return string.Compare(de.SchemaClassName, classToCompare, StringComparison.OrdinalIgnoreCase) == 0;
		}

		internal static string PAPIQueryToRegexString(string papiString)
		{
			StringBuilder stringBuilder = new StringBuilder(papiString.Length);
			stringBuilder.Append("\\G");
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
							stringBuilder.Append("\\(");
							break;
						}
						case ')':
						{
							stringBuilder.Append("\\)");
							break;
						}
						case '*':
						{
							stringBuilder.Append("\\*");
							break;
						}
						default:
						{
							if (chr1 == '\\')
							{
								stringBuilder.Append("\\\\\\\\");
								break;
							}
							else
							{
								stringBuilder.Append("\\\\");
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
							stringBuilder.Append("\\(");
							break;
						}
						case ')':
						{
							stringBuilder.Append("\\)");
							break;
						}
						case '*':
						{
							stringBuilder.Append(".*");
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
			if (flag)
			{
				stringBuilder.Append("\\\\");
			}
			stringBuilder.Append("\\z");
			return stringBuilder.ToString();
		}
	}
}