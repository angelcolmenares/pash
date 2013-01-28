using Microsoft.PowerShell.Commands.Internal;
using Microsoft.PowerShell.Commands.Management;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Management;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Microsoft.PowerShell.Commands
{
	internal static class ComputerWMIHelper
	{
		internal const int NetBIOSNameMaxLength = 15;

		internal const string WMI_Class_SystemRestore = "SystemRestore";

		internal const string WMI_Class_OperatingSystem = "Win32_OperatingSystem";

		internal const string WMI_Class_Service = "Win32_Service";

		internal const string WMI_Class_ComputerSystem = "Win32_ComputerSystem";

		internal const string WMI_Class_PingStatus = "Win32_PingStatus";

		internal const string WMI_Path_CIM = "\\root\\cimv2";

		internal const string WMI_Path_Default = "\\root\\default";

		internal const int ErrorCode_Interface = 0x6b5;

		internal const int ErrorCode_Service = 0x420;

		internal const int TOKEN_ADJUST_PRIVILEGES = 32;

		internal const int TOKEN_QUERY = 8;

		internal const int TOKEN_ALL_ACCESS = 0x1f01ff;

		internal const string SE_SHUTDOWN_NAME = "SeShutdownPrivilege";

		internal const string SE_REMOTE_SHUTDOWN_NAME = "SeRemoteShutdownPrivilege";

		internal static bool ContainsSystemDrive(string[] drives, string sysdrive)
		{
			string str;
			string[] strArrays = drives;
			int num = 0;
			while (num < (int)strArrays.Length)
			{
				string str1 = strArrays[num];
				if (str1.EndsWith("\\", StringComparison.CurrentCultureIgnoreCase))
				{
					str = str1;
				}
				else
				{
					str = string.Concat(str1, "\\");
				}
				if (!str.Equals(sysdrive, StringComparison.CurrentCultureIgnoreCase))
				{
					num++;
				}
				else
				{
					bool flag = true;
					return flag;
				}
			}
			return false;
		}

		internal static bool EnableTokenPrivilege(string privilegeName, ref Win32Native.TOKEN_PRIVILEGE oldPrivilegeState)
		{
			bool flag = false;
			Win32Native.TOKEN_PRIVILEGE tOKENPRIVILEGE = new Win32Native.TOKEN_PRIVILEGE();
			if (Win32Native.LookupPrivilegeValue(null, privilegeName, ref tOKENPRIVILEGE.Privilege.Luid))
			{
				IntPtr currentProcess = Win32Native.GetCurrentProcess();
				if (currentProcess != IntPtr.Zero)
				{
					IntPtr zero = IntPtr.Zero;
					if (Win32Native.OpenProcessToken(currentProcess, 40, out zero))
					{
						Win32Native.PRIVILEGE_SET luid = new Win32Native.PRIVILEGE_SET();
						luid.Privilege.Luid = tOKENPRIVILEGE.Privilege.Luid;
						luid.PrivilegeCount = 1;
						luid.Control = 1;
						bool flag1 = false;
						if (!Win32Native.PrivilegeCheck(zero, ref luid, out flag1) || !flag1)
						{
							tOKENPRIVILEGE.PrivilegeCount = 1;
							tOKENPRIVILEGE.Privilege.Attributes = 2;
							int num = Marshal.SizeOf(typeof(Win32Native.TOKEN_PRIVILEGE));
							int num1 = 0;
							if (Win32Native.AdjustTokenPrivileges(zero, false, ref tOKENPRIVILEGE, num, out oldPrivilegeState, ref num1))
							{
								int lastWin32Error = Marshal.GetLastWin32Error();
								if (lastWin32Error != 0)
								{
									if (lastWin32Error == 0x514)
									{
										oldPrivilegeState.PrivilegeCount = 0;
										flag = true;
									}
								}
								else
								{
									flag = true;
								}
							}
						}
						else
						{
							oldPrivilegeState.PrivilegeCount = 0;
							flag = true;
						}
					}
					if (zero != IntPtr.Zero)
					{
						Win32Native.CloseHandle(zero);
					}
					Win32Native.CloseHandle(currentProcess);
				}
			}
			return flag;
		}

		internal static ComputerChangeInfo GetComputerStatusObject(int errorcode, string computername)
		{
			ComputerChangeInfo computerChangeInfo = new ComputerChangeInfo();
			computerChangeInfo.ComputerName = computername;
			if (errorcode == 0)
			{
				computerChangeInfo.HasSucceeded = true;
			}
			else
			{
				computerChangeInfo.HasSucceeded = false;
			}
			return computerChangeInfo;
		}

		internal static ConnectionOptions GetConnection(AuthenticationLevel Authentication, ImpersonationLevel Impersonation, PSCredential Credential)
		{
			ConnectionOptions connectionOption = new ConnectionOptions();
			connectionOption.Authentication = Authentication;
			connectionOption.EnablePrivileges = true;
			connectionOption.Impersonation = Impersonation;
			if (Credential != null)
			{
				connectionOption.Username = Credential.UserName;
				//connectionOption.SecurePassword = Credential.Password;
			}
			return connectionOption;
		}

		internal static string GetLocalAdminUserName(string computerName, PSCredential psLocalCredential)
		{
			string userName;
			if (!psLocalCredential.UserName.Contains("\\"))
			{
				int num = computerName.IndexOf(".", StringComparison.OrdinalIgnoreCase);
				if (num != -1)
				{
					userName = string.Concat(computerName.Substring(0, num), "\\", psLocalCredential.UserName);
				}
				else
				{
					userName = string.Concat(computerName, "\\", psLocalCredential.UserName);
				}
			}
			else
			{
				userName = psLocalCredential.UserName;
			}
			return userName;
		}

		internal static string GetMachineNames(string[] computerNames)
		{
			string hostName;
			StringBuilder stringBuilder = new StringBuilder();
			int num = 0;
			RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Control Panel\\International");
			string str = registryKey.GetValue("sList").ToString();
			string[] strArrays = computerNames;
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string str1 = strArrays[i];
				if (num >= 1)
				{
					stringBuilder.Append(str);
				}
				if (str1.Equals("localhost", StringComparison.CurrentCultureIgnoreCase) || str1.Equals(".", StringComparison.OrdinalIgnoreCase))
				{
					hostName = Dns.GetHostName();
				}
				else
				{
					hostName = str1;
				}
				stringBuilder.Append(hostName);
				num++;
			}
			return stringBuilder.ToString();
		}

		internal static string GetRandomPassword(int passwordLength)
		{
			byte[] numArray = new byte[passwordLength];
			char[] chrArray = new char[passwordLength];
			RNGCryptoServiceProvider rNGCryptoServiceProvider = new RNGCryptoServiceProvider();
			rNGCryptoServiceProvider.GetBytes(numArray);
			for (int i = 0; i < passwordLength; i++)
			{
				chrArray[i] = (char)((ushort)(numArray[i] % 91 + 32));
			}
			return new string(chrArray);
		}

		internal static RenameComputerChangeInfo GetRenameComputerStatusObject(int errorcode, string newcomputername, string oldcomputername)
		{
			RenameComputerChangeInfo renameComputerChangeInfo = new RenameComputerChangeInfo();
			renameComputerChangeInfo.OldComputerName = oldcomputername;
			renameComputerChangeInfo.NewComputerName = newcomputername;
			if (errorcode == 0)
			{
				renameComputerChangeInfo.HasSucceeded = true;
			}
			else
			{
				renameComputerChangeInfo.HasSucceeded = false;
			}
			return renameComputerChangeInfo;
		}

		internal static string GetScopeString(string computer, string namespaceParameter)
		{
			StringBuilder stringBuilder = new StringBuilder("\\\\");
			if (computer.Equals("::1", StringComparison.CurrentCultureIgnoreCase) || computer.Equals("[::1]", StringComparison.CurrentCultureIgnoreCase))
			{
				stringBuilder.Append("localhost");
			}
			else
			{
				stringBuilder.Append(computer);
			}
			stringBuilder.Append(namespaceParameter);
			return stringBuilder.ToString();
		}

		internal static bool IsComputerNameValid(string computerName)
		{
			bool flag = true;
			if (computerName.Length < 64)
			{
				string str = computerName;
				for (int i = 0; i < str.Length; i++)
				{
					char chr = str[i];
					if ((chr < 'A' || chr > 'Z') && (chr < 'a' || chr > 'z'))
					{
						if (chr < '0' || chr > '9')
						{
							if (chr != '-')
							{
								bool flag1 = false;
								return flag1;
							}
							else
							{
								flag = false;
							}
						}
					}
					else
					{
						flag = false;
					}
				}
				return !flag;
			}
			else
			{
				return false;
			}
		}

		internal static bool IsValidDrive(string drive)
		{
			DriveInfo[] drives = DriveInfo.GetDrives();
			DriveInfo[] driveInfoArray = drives;
			int num = 0;
			while (num < (int)driveInfoArray.Length)
			{
				DriveInfo driveInfo = driveInfoArray[num];
				if (!driveInfo.DriveType.Equals(DriveType.Fixed) || !drive.ToString().Equals(driveInfo.Name.ToString(), StringComparison.OrdinalIgnoreCase))
				{
					num++;
				}
				else
				{
					bool flag = true;
					return flag;
				}
			}
			return false;
		}

		internal static bool RestoreTokenPrivilege(string privilegeName, ref Win32Native.TOKEN_PRIVILEGE previousPrivilegeState)
		{
			if (previousPrivilegeState.PrivilegeCount != 0)
			{
				bool flag = false;
				Win32Native.TOKEN_PRIVILEGE tOKENPRIVILEGE = new Win32Native.TOKEN_PRIVILEGE();
				if (Win32Native.LookupPrivilegeValue(null, privilegeName, ref tOKENPRIVILEGE.Privilege.Luid) && tOKENPRIVILEGE.Privilege.Luid.HighPart == previousPrivilegeState.Privilege.Luid.HighPart && tOKENPRIVILEGE.Privilege.Luid.LowPart == previousPrivilegeState.Privilege.Luid.LowPart)
				{
					IntPtr currentProcess = Win32Native.GetCurrentProcess();
					if (currentProcess != IntPtr.Zero)
					{
						IntPtr zero = IntPtr.Zero;
						if (Win32Native.OpenProcessToken(currentProcess, 40, out zero))
						{
							int num = Marshal.SizeOf(typeof(Win32Native.TOKEN_PRIVILEGE));
							int num1 = 0;
							if (Win32Native.AdjustTokenPrivileges(zero, false, ref previousPrivilegeState, num, out tOKENPRIVILEGE, ref num1) && Marshal.GetLastWin32Error() == 0)
							{
								flag = true;
							}
						}
						if (zero != IntPtr.Zero)
						{
							Win32Native.CloseHandle(zero);
						}
						Win32Native.CloseHandle(currentProcess);
					}
				}
				return flag;
			}
			else
			{
				return true;
			}
		}

		internal static bool SkipSystemRestoreOperationForARMPlatform(PSCmdlet cmdlet)
		{
			bool flag = false;
			if (PsUtils.IsRunningOnProcessorArchitectureARM())
			{
				InvalidOperationException invalidOperationException = new InvalidOperationException(ComputerResources.SystemRestoreNotSupported);
				ErrorRecord errorRecord = new ErrorRecord(invalidOperationException, "SystemRestoreNotSupported", ErrorCategory.InvalidOperation, null);
				cmdlet.WriteError(errorRecord);
				flag = true;
			}
			return flag;
		}

		internal static void WriteNonTerminatingError(int errorcode, PSCmdlet cmdlet, string computername)
		{
			Win32Exception win32Exception = new Win32Exception(errorcode);
			string empty = string.Empty;
			int nativeErrorCode = win32Exception.NativeErrorCode;
			if (nativeErrorCode.Equals(53))
			{
				empty = StringUtil.Format(ComputerResources.NetworkPathNotFound, computername);
			}
			object[] message = new object[3];
			message[0] = win32Exception.Message;
			message[1] = computername;
			message[2] = empty;
			string str = StringUtil.Format(ComputerResources.OperationFailed, message);
			ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(str), "InvalidOperationException", ErrorCategory.InvalidOperation, computername);
			cmdlet.WriteError(errorRecord);
		}
	}
}