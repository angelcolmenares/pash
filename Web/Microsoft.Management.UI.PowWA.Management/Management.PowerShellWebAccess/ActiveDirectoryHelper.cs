using Microsoft.Management.PowerShellWebAccess.Management;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Globalization;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.Management.PowerShellWebAccess
{
	internal class ActiveDirectoryHelper : IActiveDirectoryHelper
	{
		private const string BUILTIN_DOMAIN_SID_PREFIX = "S-1-5-32-";

		public ActiveDirectoryHelper()
		{

		}

		public bool CheckComputerTypeMatch(bool isLocal, string sid, PswaDestinationType type, string domain, out string errorMessage)
		{
			errorMessage = null;
			return true;
			/*
			bool flag;
			PrincipalContext principalContext;
			bool hasValue;
			errorMessage = string.Empty;
			try
			{
				if (isLocal)
				{
					principalContext = new PrincipalContext(ContextType.Machine);
				}
				else
				{
					principalContext = new PrincipalContext(ContextType.Domain, domain);
				}
				using (principalContext)
				{
					if (type != PswaDestinationType.Computer)
					{
						if (type != PswaDestinationType.ComputerGroup)
						{
							flag = false;
							return flag;
						}
						else
						{
							GroupPrincipal groupPrincipal = GroupPrincipal.FindByIdentity(V_0, sid);
							using (groupPrincipal)
							{
								if (groupPrincipal != null)
								{
									bool? isSecurityGroup = groupPrincipal.IsSecurityGroup;
									if (!isSecurityGroup.GetValueOrDefault())
									{
										hasValue = true;
									}
									else
									{
										hasValue = !isSecurityGroup.HasValue;
									}
									if (hasValue)
									{
										errorMessage = Resources.ComputerGroupIsNotSecurityGroup;
										flag = false;
										return flag;
									}
								}
								else
								{
									errorMessage = Resources.DestinationTypeDoesNotMatchComputerGroup;
									flag = false;
									return flag;
								}
							}
						}
					}
					else
					{
						ComputerPrincipal computerPrincipal = ComputerPrincipal.FindByIdentity(V_0, sid);
						using (computerPrincipal)
						{
							if (computerPrincipal == null)
							{
								errorMessage = Resources.DestinationTypeDoesNotMatchComputer;
								flag = false;
								return flag;
							}
						}
					}
				}
				return true;
			}
			catch (DirectoryServicesCOMException directoryServicesCOMException1)
			{
				DirectoryServicesCOMException directoryServicesCOMException = directoryServicesCOMException1;
				if (directoryServicesCOMException.ExtendedError == -2146893044 || directoryServicesCOMException.ExtendedError == 0x4dc)
				{
					errorMessage = Resources.NoActiveDirectoryPermission;
					flag = false;
				}
				else
				{
					throw;
				}
			}
			return flag;
			*/
		}

		public bool CheckUserTypeMatch(bool isLocal, string sid, PswaUserType type, string domain, out string errorMessage)
		{
			errorMessage = null;
			return true;
			/*
			bool flag;
			PrincipalContext principalContext;
			bool hasValue;
			errorMessage = string.Empty;
			try
			{
				if (isLocal)
				{
					principalContext = new PrincipalContext(ContextType.Machine);
				}
				else
				{
					principalContext = new PrincipalContext(ContextType.Domain, domain);
				}
				using (principalContext)
				{
					if (type != PswaUserType.User)
					{
						if (type != PswaUserType.UserGroup)
						{
							flag = false;
							return flag;
						}
						else
						{
							GroupPrincipal groupPrincipal = GroupPrincipal.FindByIdentity(V_0, sid);
							using (groupPrincipal)
							{
								if (groupPrincipal != null)
								{
									bool? isSecurityGroup = groupPrincipal.IsSecurityGroup;
									if (!isSecurityGroup.GetValueOrDefault())
									{
										hasValue = true;
									}
									else
									{
										hasValue = !isSecurityGroup.HasValue;
									}
									if (hasValue)
									{
										errorMessage = Resources.UserGroupIsNotSecurityGroup;
										flag = false;
										return flag;
									}
								}
								else
								{
									errorMessage = Resources.UserTypeDoesNotMatchUserGroup;
									flag = false;
									return flag;
								}
							}
						}
					}
					else
					{
						UserPrincipal userPrincipal = UserPrincipal.FindByIdentity(V_0, sid);
						using (userPrincipal)
						{
							if (userPrincipal == null)
							{
								errorMessage = Resources.UserTypeDoesNotMatchUser;
								flag = false;
								return flag;
							}
						}
					}
				}
				return true;
			}
			catch (DirectoryServicesCOMException directoryServicesCOMException1)
			{
				DirectoryServicesCOMException directoryServicesCOMException = directoryServicesCOMException1;
				if (directoryServicesCOMException.ExtendedError == -2146893044 || directoryServicesCOMException.ExtendedError == 0x4dc)
				{
					errorMessage = Resources.NoActiveDirectoryPermission;
					flag = false;
				}
				else
				{
					throw;
				}
			}
			return flag;
			*/
		}

		public string ConvertAccountNameToStringSid(string accountName, out bool isAccountLocal, out string domain)
		{
			string str = null;
			string str1 = null;
			string str2 = null;
			if (!ActiveDirectoryHelper.LookupAccountName(accountName, out str2, out domain))
			{
				char[] chrArray = new char[1];
				chrArray[0] = '\\';
				if (accountName.IndexOf (chrArray[0]) == -1)
				{
					domain = Environment.MachineName;
					isAccountLocal = true;
				}
				else {
					string[] strArrays = accountName.Split(chrArray);
					if ((int)strArrays.Length == 2 && string.Compare(strArrays[0], Environment.MachineName, StringComparison.OrdinalIgnoreCase) == 0 && ActiveDirectoryHelper.LookupAccountName(strArrays[1], out str, out str1))
					{
						str2 = str;
						if (str2.StartsWith(BUILTIN_DOMAIN_SID_PREFIX, StringComparison.OrdinalIgnoreCase))
						{
							domain = Environment.MachineName;
						}
					}
				}
			}
			if (str2 != null)
			{
				isAccountLocal = string.Compare(domain, Environment.MachineName, StringComparison.OrdinalIgnoreCase) == 0;
				if (string.IsNullOrEmpty (str2)) str2  = BUILTIN_DOMAIN_SID_PREFIX + "1";
				return str2;
			}
			else
			{
				object[] objArray = new object[1];
				objArray[0] = accountName;
				ArgumentException argumentException = new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.AccountToSidConvertError, objArray));
				argumentException.Data.Add("ErrorCode", 0x534);
				throw argumentException;
			}
		}

		public string ConvertComputerName(string computerName, bool enforceFqdn)
		{
			string str = string.Concat(computerName, (char)36);
			int num = computerName.IndexOf('.');
			if (num > 0 && num + 1 < computerName.Length)
			{
				str = string.Concat(computerName.Substring(num + 1), "\\", computerName.Substring(0, num), "$");
			}
			return str;
		}

		public string ConvertStringSidToAccountName(string sid, out string domain)
		{
			NativeMethods.SID_NAME_USE sIDNAMEUSE = 0;
			int lastWin32Error = 0;
			IntPtr intPtr = new IntPtr(0);
			byte[] numArray = null;
			try
			{
				if (NativeMethods.ConvertStringSidToSid(sid, out intPtr))
				{
					int lengthSid = NativeMethods.GetLengthSid(intPtr);
					numArray = new byte[lengthSid];
					Marshal.Copy(intPtr, numArray, 0, lengthSid);
				}
				else
				{
					lastWin32Error = Marshal.GetLastWin32Error();
					object[] systemErrorMessage = new object[1];
					systemErrorMessage[0] = ActiveDirectoryHelper.GetSystemErrorMessage(lastWin32Error);
					throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.SidToAccountConvertError, systemErrorMessage));
				}
			}
			finally
			{
				NativeMethods.LocalFree(intPtr);
			}
			StringBuilder stringBuilder = new StringBuilder();
			int capacity = stringBuilder.Capacity;
			StringBuilder stringBuilder1 = new StringBuilder();
			int num = stringBuilder1.Capacity;
			if (!NativeMethods.LookupAccountSid(null, numArray, stringBuilder, ref capacity, stringBuilder1, ref num, out sIDNAMEUSE))
			{
				lastWin32Error = Marshal.GetLastWin32Error();
				if (lastWin32Error == 122)
				{
					stringBuilder.EnsureCapacity(capacity);
					stringBuilder1.EnsureCapacity(num);
					lastWin32Error = 0;
					if (!NativeMethods.LookupAccountSid(null, numArray, stringBuilder, ref capacity, stringBuilder1, ref num, out sIDNAMEUSE))
					{
						lastWin32Error = Marshal.GetLastWin32Error();
					}
				}
			}
			if (lastWin32Error != 0)
			{
				object[] objArray = new object[1];
				objArray[0] = ActiveDirectoryHelper.GetSystemErrorMessage(lastWin32Error);
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.SidToAccountConvertError, objArray));
			}
			else
			{
				if (!sid.StartsWith(BUILTIN_DOMAIN_SID_PREFIX, StringComparison.OrdinalIgnoreCase))
				{
					domain = stringBuilder1.ToString();
				}
				else
				{
					domain = Environment.MachineName;
				}
				return stringBuilder.ToString();
			}
		}

		public List<string> GetAccountDomainGroupSid(string accountSid)
		{
			List<string> strs = new List<string>();
			object[] objArray = new object[1];
			objArray[0] = accountSid;
			using (DirectoryEntry directoryEntry = new DirectoryEntry(string.Format(CultureInfo.InvariantCulture, "LDAP://<Sid={0}>", objArray)))
			{
				string[] strArrays = new string[1];
				strArrays[0] = "tokenGroups";
				directoryEntry.RefreshCache(strArrays);
				foreach (byte[] item in directoryEntry.Properties["tokenGroups"])
				{
					IntPtr zero = IntPtr.Zero;
					try
					{
						try
						{
							if (NativeMethods.ConvertSidToStringSid(item, out zero))
							{
								strs.Add(Marshal.PtrToStringAuto(zero));
							}
						}
						catch (Exception exception)
						{
						}
					}
					finally
					{
						if (zero != IntPtr.Zero)
						{
							NativeMethods.LocalFree(zero);
						}
					}
				}
			}
			return strs;
		}

		public string GetFqdn(string computerName)
		{
			IPHostEntry hostEntry = null;
			try
			{
				hostEntry = Dns.GetHostEntry(computerName);
			}
			catch (Exception exception)
			{
				object[] objArray = new object[1];
				objArray[0] = computerName;
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.CannotFindComputer, objArray));
			}
			return hostEntry.HostName;
		}

		private static string GetSystemErrorMessage(int errcode)
		{
			IntPtr zero = IntPtr.Zero;
			uint num = NativeMethods.FormatMessage(0x1300, IntPtr.Zero, errcode, 0, ref zero, 0, IntPtr.Zero);
			if (num != 0)
			{
				string str = Marshal.PtrToStringAuto(zero).Trim();
				zero = NativeMethods.LocalFree(zero);
				return str;
			}
			else
			{
				Marshal.GetLastWin32Error();
				return string.Empty;
			}
		}

		public bool IsAccountInGroup(string groupSid, List<string> accountDomainGroup, string accountSid, Dictionary<string, string> checkedSid)
		{
			return true;
			/*
			bool flag;
			if (!checkedSid.ContainsKey(groupSid))
			{
				using (PrincipalContext principalContext = new PrincipalContext(ContextType.Machine))
				{
					GroupPrincipal groupPrincipal = GroupPrincipal.FindByIdentity(principalContext, groupSid);
					using (groupPrincipal)
					{
						if (groupPrincipal != null)
						{
							PrincipalCollection members = groupPrincipal.Members;
							foreach (Principal member in members)
							{
								if (string.Compare(member.Sid.Value, accountSid, StringComparison.OrdinalIgnoreCase) != 0)
								{
									if (member as GroupPrincipal == null)
									{
										continue;
									}
									if (accountDomainGroup.Contains(member.Sid.Value) || checkedSid.ContainsKey(member.Sid.Value) && checkedSid[member.Sid.Value] == "true")
									{
										checkedSid.Add(groupSid, "true");
										flag = true;
										return flag;
									}
									else
									{
										if (checkedSid.ContainsKey(member.Sid.Value) && checkedSid[member.Sid.Value] == "false")
										{
											continue;
										}
										bool flag1 = this.IsAccountInGroup(member.Sid.Value, accountDomainGroup, accountSid, checkedSid);
										if (!flag1)
										{
											continue;
										}
										checkedSid.Add(groupSid, "true");
										flag = true;
										return flag;
									}
								}
								else
								{
									checkedSid.Add(groupSid, "true");
									flag = true;
									return flag;
								}
							}
						}
					}
					checkedSid.Add(groupSid, "false");
					return false;
				}
				return flag;
			}
			else
			{
				if (checkedSid[groupSid] == "true")
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			*/
		}

		public bool IsCurrentComputerDomainJoined()
		{
			int num = 0;
			IntPtr zero = IntPtr.Zero;
			NativeMethods.NetJoinStatus netJoinStatu = NativeMethods.NetJoinStatus.NetSetupUnknownStatus;
			try
			{
				num = NativeMethods.NetGetJoinInformation(null, out zero, out netJoinStatu);
			}
			finally
			{
				if (zero != IntPtr.Zero)
				{
					NativeMethods.NetApiBufferFree(zero);
				}
			}
			if (num != 0)
			{
				return false;
			}
			else
			{
				return netJoinStatu == NativeMethods.NetJoinStatus.NetSetupDomainName;
			}
		}

		private static bool LookupAccountName(string accountName, out string sid, out string domain)
		{
			domain = Environment.MachineName;
			sid = BUILTIN_DOMAIN_SID_PREFIX + "1";
			return true;
			/*
			unsafe
			{
				NativeMethods.SID_NAME_USE sIDNAMEUSE = 0;
				bool flag;
				IntPtr zero = IntPtr.Zero;
				try
				{
					byte[] numArray = null;
					int num = 0;
					StringBuilder stringBuilder = new StringBuilder();
					int capacity = stringBuilder.Capacity;
					int lastWin32Error = 0;
					if (!NativeMethods.LookupAccountName(null, accountName, numArray, ref num, stringBuilder, ref capacity, out sIDNAMEUSE))
					{
						lastWin32Error = Marshal.GetLastWin32Error();
						if (lastWin32Error == 122)
						{
							numArray = new byte[num];
							stringBuilder.EnsureCapacity(capacity);
							lastWin32Error = 0;
							if (!NativeMethods.LookupAccountName(null, accountName, numArray, ref num, stringBuilder, ref capacity, out sIDNAMEUSE))
							{
								lastWin32Error = Marshal.GetLastWin32Error();
							}
						}
					}
					if (lastWin32Error != 0x534)
					{
						if (lastWin32Error == 0 && !NativeMethods.ConvertSidToStringSid(numArray, out zero))
						{
							lastWin32Error = Marshal.GetLastWin32Error();
						}
						if (lastWin32Error == 0)
						{
							sid = Marshal.PtrToStringAuto(zero);
							domain = stringBuilder.ToString();
							flag = true;
						}
						else
						{
							object[] systemErrorMessage = new object[2];
							systemErrorMessage[0] = accountName;
							systemErrorMessage[1] = ActiveDirectoryHelper.GetSystemErrorMessage(lastWin32Error);
							throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.AccountToSidConvertExtendedError, systemErrorMessage));
						}
					}
					else
					{
						sid = null;
						domain = null;
						flag = false;
					}
				}
				finally
				{
					if (zero != IntPtr.Zero)
					{
						NativeMethods.LocalFree(zero);
					}
				}
				return flag;
			}
			*/
		}
	}
}