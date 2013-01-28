using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;

namespace System.DirectoryServices.AccountManagement
{
	internal class Utils
	{
		private Utils()
		{
		}

		internal static bool AreBytesEqual(byte[] src, byte[] tgt)
		{
			if ((int)src.Length == (int)tgt.Length)
			{
				int num = 0;
				while (num < (int)src.Length)
				{
					if (src[num] == tgt[num])
					{
						num++;
					}
					else
					{
						return false;
					}
				}
				return true;
			}
			else
			{
				return false;
			}
		}

		[SecurityCritical]
		internal static bool BeginImpersonation(NetCred credential, out IntPtr hUserToken)
		{
			hUserToken = IntPtr.Zero;
			IntPtr zero = IntPtr.Zero;
			if (credential != null)
			{
				string parsedUserName = credential.ParsedUserName;
				string password = credential.Password;
				string domain = credential.Domain;
				if (parsedUserName != null || password != null)
				{
					int num = UnsafeNativeMethods.LogonUser(parsedUserName, domain, password, 9, 3, ref zero);
					if (num != 0)
					{
						num = UnsafeNativeMethods.ImpersonateLoggedOnUser(zero);
						if (num != 0)
						{
							hUserToken = zero;
							return true;
						}
						else
						{
							int lastWin32Error = Marshal.GetLastWin32Error();
							UnsafeNativeMethods.CloseHandle(zero);
							object[] objArray = new object[1];
							objArray[0] = lastWin32Error;
							throw new PrincipalOperationException(string.Format(CultureInfo.CurrentCulture, StringResources.UnableToImpersonateCredentials, objArray));
						}
					}
					else
					{
						int lastWin32Error1 = Marshal.GetLastWin32Error();
						object[] objArray1 = new object[1];
						objArray1[0] = lastWin32Error1;
						throw new PrincipalOperationException(string.Format(CultureInfo.CurrentCulture, StringResources.UnableToImpersonateCredentials, objArray1));
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

		internal static string ByteArrayToString(byte[] byteArray)
		{
			StringBuilder stringBuilder = new StringBuilder();
			byte[] numArray = byteArray;
			for (int i = 0; i < (int)numArray.Length; i++)
			{
				byte num = numArray[i];
				stringBuilder.Append(num.ToString("x2", CultureInfo.InvariantCulture));
			}
			return stringBuilder.ToString();
		}

		[SecurityCritical]
		internal static SidType ClassifySID(byte[] sid)
		{
			SidType sidType;
			IntPtr zero = IntPtr.Zero;
			try
			{
				zero = Utils.ConvertByteArrayToIntPtr(sid);
				sidType = Utils.ClassifySID(zero);
			}
			finally
			{
				if (zero != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(zero);
				}
			}
			return sidType;
		}

		[SecuritySafeCritical]
		internal static SidType ClassifySID(IntPtr pSid)
		{
			IntPtr sidIdentifierAuthority = UnsafeNativeMethods.GetSidIdentifierAuthority(pSid);
			UnsafeNativeMethods.SID_IDENTIFIER_AUTHORITY structure = (UnsafeNativeMethods.SID_IDENTIFIER_AUTHORITY)Marshal.PtrToStructure(sidIdentifierAuthority, typeof(UnsafeNativeMethods.SID_IDENTIFIER_AUTHORITY));
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

		internal static void ClearBit(ref int value, uint bitmask)
		{
			value = value & ~bitmask;
		}

		[SecuritySafeCritical]
		internal static Principal ConstructFakePrincipalFromSID(byte[] sid, PrincipalContext ctx, string serverName, NetCred credentials, string authorityName)
		{
			string str = null;
			string str1 = null;
			string str2;
			string str3 = "";
			int num = 0;
			int num1 = Utils.LookupSid(serverName, credentials, sid, out str, out str1, out num);
			if (num1 == 0)
			{
				if (!string.IsNullOrEmpty(str1))
				{
					str2 = string.Concat(str1, "\\");
				}
				else
				{
					str2 = "";
				}
				str3 = string.Concat(str2, str);
			}
			GroupPrincipal groupPrincipal = GroupPrincipal.MakeGroup(ctx);
			groupPrincipal.fakePrincipal = true;
			groupPrincipal.unpersisted = false;
			groupPrincipal.LoadValueIntoProperty("Principal.DisplayName", str3);
			groupPrincipal.LoadValueIntoProperty("Principal.Name", str);
			groupPrincipal.LoadValueIntoProperty("Principal.SamAccountName", str);
			SecurityIdentifier securityIdentifier = new SecurityIdentifier(Utils.ConvertSidToSDDL(sid));
			groupPrincipal.LoadValueIntoProperty("Principal.Sid", securityIdentifier);
			groupPrincipal.LoadValueIntoProperty("GroupPrincipal.IsSecurityGroup", (bool)1);
			return groupPrincipal;
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

		[SecuritySafeCritical]
		internal static byte[] ConvertNativeSidToByteArray(IntPtr pSid)
		{
			int lengthSid = UnsafeNativeMethods.GetLengthSid(pSid);
			byte[] numArray = new byte[lengthSid];
			Marshal.Copy(pSid, numArray, 0, lengthSid);
			return numArray;
		}

		[SecuritySafeCritical]
		internal static string ConvertSidToSDDL(byte[] sid)
		{
			string str;
			string str1 = null;
			IntPtr zero = IntPtr.Zero;
			try
			{
				zero = Utils.ConvertByteArrayToIntPtr(sid);
				if (!UnsafeNativeMethods.ConvertSidToStringSid(zero, ref str1))
				{
					Marshal.GetLastWin32Error();
					str = null;
				}
				else
				{
					str = str1;
				}
			}
			finally
			{
				if (zero != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(zero);
				}
			}
			return str;
		}

		[SecurityCritical]
		internal static void EndImpersonation(IntPtr hUserToken)
		{
			UnsafeNativeMethods.RevertToSelf();
			UnsafeNativeMethods.CloseHandle(hUserToken);
		}

		[EnvironmentPermission(SecurityAction.Assert, Unrestricted=true)]
		[SecuritySafeCritical]
		internal static string GetComputerFlatName()
		{
			string machineName = Environment.MachineName;
			return machineName;
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
						throw new PrincipalOperationException(string.Format(CultureInfo.CurrentCulture, StringResources.UnableToOpenToken, objArray));
					}
					else
					{
						if (!UnsafeNativeMethods.OpenProcessToken(UnsafeNativeMethods.GetCurrentProcess(), 8, ref zero))
						{
							int lastWin32Error1 = Marshal.GetLastWin32Error();
							object[] objArray1 = new object[1];
							objArray1[0] = lastWin32Error1;
							throw new PrincipalOperationException(string.Format(CultureInfo.CurrentCulture, StringResources.UnableToOpenToken, objArray1));
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
						UnsafeNativeMethods.TOKEN_USER structure = (UnsafeNativeMethods.TOKEN_USER)Marshal.PtrToStructure(zero1, typeof(UnsafeNativeMethods.TOKEN_USER));
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
							throw new PrincipalOperationException(string.Format(CultureInfo.CurrentCulture, StringResources.UnableToRetrieveTokenInfo, objArray2));
						}
					}
					else
					{
						int num3 = Marshal.GetLastWin32Error();
						object[] objArray3 = new object[1];
						objArray3[0] = num3;
						throw new PrincipalOperationException(string.Format(CultureInfo.CurrentCulture, StringResources.UnableToRetrieveTokenInfo, objArray3));
					}
				}
				else
				{
					object[] objArray4 = new object[1];
					objArray4[0] = num2;
					throw new PrincipalOperationException(string.Format(CultureInfo.CurrentCulture, StringResources.UnableToRetrieveTokenInfo, objArray4));
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

		[SecuritySafeCritical]
		internal static UnsafeNativeMethods.DomainControllerInfo GetDcName(string computerName, string domainName, string siteName, int flags)
		{
			UnsafeNativeMethods.DomainControllerInfo domainControllerInfo;
			IntPtr zero = IntPtr.Zero;
			try
			{
				int num = UnsafeNativeMethods.DsGetDcName(computerName, domainName, IntPtr.Zero, siteName, flags, out zero);
				if (num == 0)
				{
					UnsafeNativeMethods.DomainControllerInfo structure = (UnsafeNativeMethods.DomainControllerInfo)Marshal.PtrToStructure(zero, typeof(UnsafeNativeMethods.DomainControllerInfo));
					domainControllerInfo = structure;
				}
				else
				{
					object[] objArray = new object[1];
					objArray[0] = num;
					throw new PrincipalOperationException(string.Format(CultureInfo.CurrentCulture, StringResources.UnableToRetrieveDomainInfo, objArray));
				}
			}
			finally
			{
				if (zero != IntPtr.Zero)
				{
					UnsafeNativeMethods.NetApiBufferFree(zero);
				}
			}
			return domainControllerInfo;
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
				UnsafeNativeMethods.LSA_OBJECT_ATTRIBUTES lSAOBJECTATTRIBUTE = new UnsafeNativeMethods.LSA_OBJECT_ATTRIBUTES();
				intPtr1 = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(UnsafeNativeMethods.LSA_OBJECT_ATTRIBUTES)));
				Marshal.StructureToPtr(lSAOBJECTATTRIBUTE, intPtr1, false);
				int num = UnsafeNativeMethods.LsaOpenPolicy(IntPtr.Zero, intPtr1, 1, ref zero);
				if (num == 0)
				{
					num = UnsafeNativeMethods.LsaQueryInformationPolicy(zero, 5, ref zero1);
					if (num == 0)
					{
						UnsafeNativeMethods.POLICY_ACCOUNT_DOMAIN_INFO structure = (UnsafeNativeMethods.POLICY_ACCOUNT_DOMAIN_INFO)Marshal.PtrToStructure(zero1, typeof(UnsafeNativeMethods.POLICY_ACCOUNT_DOMAIN_INFO));
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
							throw new PrincipalOperationException(string.Format(CultureInfo.CurrentCulture, StringResources.UnableToRetrievePolicy, objArray));
						}
					}
					else
					{
						object[] winError = new object[1];
						winError[0] = SafeNativeMethods.LsaNtStatusToWinError(num);
						throw new PrincipalOperationException(string.Format(CultureInfo.CurrentCulture, StringResources.UnableToRetrievePolicy, winError));
					}
				}
				else
				{
					object[] winError1 = new object[1];
					winError1[0] = SafeNativeMethods.LsaNtStatusToWinError(num);
					throw new PrincipalOperationException(string.Format(CultureInfo.CurrentCulture, StringResources.UnableToRetrievePolicy, winError1));
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

		[SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.ControlPrincipal)]
		[SecuritySafeCritical]
		internal static string GetNT4UserName()
		{
			string str;
			WindowsIdentity current = WindowsIdentity.GetCurrent();
			using (current)
			{
				string name = current.Name;
				str = name;
			}
			return str;
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
					num = UnsafeNativeMethods.DsRoleGetPrimaryDomainInformation(computerName, UnsafeNativeMethods.DSROLE_PRIMARY_DOMAIN_INFO_LEVEL.DsRolePrimaryDomainInfoBasic, out zero);
				}
				else
				{
					num = UnsafeNativeMethods.DsRoleGetPrimaryDomainInformation(IntPtr.Zero, UnsafeNativeMethods.DSROLE_PRIMARY_DOMAIN_INFO_LEVEL.DsRolePrimaryDomainInfoBasic, out zero);
				}
				if (num == 0)
				{
					UnsafeNativeMethods.DSROLE_PRIMARY_DOMAIN_INFO_BASIC structure = (UnsafeNativeMethods.DSROLE_PRIMARY_DOMAIN_INFO_BASIC)Marshal.PtrToStructure(zero, typeof(UnsafeNativeMethods.DSROLE_PRIMARY_DOMAIN_INFO_BASIC));
					if (structure.MachineRole == UnsafeNativeMethods.DSROLE_MACHINE_ROLE.DsRole_RoleBackupDomainController)
					{
						machineRole = true;
					}
					else
					{
						machineRole = structure.MachineRole == UnsafeNativeMethods.DSROLE_MACHINE_ROLE.DsRole_RolePrimaryDomainController;
					}
					flag = machineRole;
				}
				else
				{
					object[] objArray = new object[1];
					objArray[0] = num;
					throw new PrincipalOperationException(string.Format(CultureInfo.CurrentCulture, StringResources.UnableToRetrieveDomainInfo, objArray));
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

		[SecurityCritical]
		internal static int LookupSid(string serverName, NetCred credentials, byte[] sid, out string name, out string domainName, out int accountUsage)
		{
			int num;
			IntPtr zero = IntPtr.Zero;
			int num1 = 0;
			int num2 = 0;
			accountUsage = 0;
			name = null;
			domainName = null;
			IntPtr intPtr = IntPtr.Zero;
			try
			{
				zero = Utils.ConvertByteArrayToIntPtr(sid);
				Utils.BeginImpersonation(credentials, out intPtr);
				bool flag = UnsafeNativeMethods.LookupAccountSid(serverName, zero, null, ref num1, null, ref num2, ref accountUsage);
				int lastWin32Error = Marshal.GetLastWin32Error();
				if (lastWin32Error == 122)
				{
					StringBuilder stringBuilder = new StringBuilder(num1);
					StringBuilder stringBuilder1 = new StringBuilder(num2);
					flag = UnsafeNativeMethods.LookupAccountSid(serverName, zero, stringBuilder, ref num1, stringBuilder1, ref num2, ref accountUsage);
					if (flag)
					{
						name = stringBuilder.ToString();
						domainName = stringBuilder1.ToString();
						num = 0;
					}
					else
					{
						lastWin32Error = Marshal.GetLastWin32Error();
						num = lastWin32Error;
					}
				}
				else
				{
					num = lastWin32Error;
				}
			}
			finally
			{
				if (zero != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(zero);
				}
				if (intPtr != IntPtr.Zero)
				{
					Utils.EndImpersonation(intPtr);
				}
			}
			return num;
		}

		internal static string SecurityIdentifierToLdapHexBindingString(SecurityIdentifier sid)
		{
			byte[] numArray = new byte[sid.BinaryLength];
			sid.GetBinaryForm(numArray, 0);
			StringBuilder stringBuilder = new StringBuilder();
			byte[] numArray1 = numArray;
			for (int i = 0; i < (int)numArray1.Length; i++)
			{
				byte num = numArray1[i];
				stringBuilder.Append(num.ToString("x2", CultureInfo.InvariantCulture));
			}
			return stringBuilder.ToString();
		}

		internal static string SecurityIdentifierToLdapHexFilterString(SecurityIdentifier sid)
		{
			return ADUtils.HexStringToLdapHexString(Utils.SecurityIdentifierToLdapHexBindingString(sid));
		}

		internal static void SetBit(ref int value, uint bitmask)
		{
			value = value | bitmask;
		}

		internal static byte[] StringToByteArray(string s)
		{
			if (s.Length % 2 == 0)
			{
				byte[] numArray = new byte[s.Length / 2];
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
						byte num1 = byte.Parse(s.Substring(num * 2, 2), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
						numArray[num] = num1;
						num++;
					}
				}
				return numArray;
			}
			else
			{
				return null;
			}
		}
	}
}