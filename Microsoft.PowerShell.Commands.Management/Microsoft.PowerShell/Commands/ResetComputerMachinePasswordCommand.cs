using Microsoft.PowerShell.Commands.Management;
using System;
using System.ComponentModel;
using System.DirectoryServices;
using System.Globalization;
using System.Management;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Reset", "ComputerMachinePassword", SupportsShouldProcess=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=135252")]
	public class ResetComputerMachinePasswordCommand : PSCmdlet
	{
		private const uint STATUS_ACCESS_DENIED = 0xc0000022;

		private const uint STATUS_OBJECT_NAME_NOT_FOUND = 0xc0000034;

		private const uint SECRET_SET_VALUE = 1;

		private const uint SECRET_QUERY_VALUE = 2;

		private const int PasswordLength = 120;

		private const string SecretKey = "$MACHINE.ACC";

		[Credential]
		[Parameter]
		[ValidateNotNullOrEmpty]
		public PSCredential Credential
		{
			get;
			set;
		}

		[Parameter]
		[ValidateNotNullOrEmpty]
		public string Server
		{
			get;
			set;
		}

		public ResetComputerMachinePasswordCommand()
		{
		}

		protected override void BeginProcessing()
		{
			if (this.Server != null)
			{
				if (this.Server.Length == 1 && this.Server[0] == '.')
				{
					this.Server = "localhost";
				}
				try
				{
                    //TODO: Review
                    this.Server = Dns.GetHostEntry(this.Server).HostName;
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					CommandProcessorBase.CheckForSevereException(exception);
					string str = StringUtil.Format(ComputerResources.CannotResolveComputerName, this.Server, exception.Message);
					ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(str), "AddressResolutionException", ErrorCategory.InvalidArgument, this.Server);
					base.ThrowTerminatingError(errorRecord);
				}
			}
		}

		protected override void ProcessRecord()
		{
			string hostName = Dns.GetHostName();
			string str = null;
			Exception exception = null;
			if (base.ShouldProcess(hostName))
			{
				try
				{
					ManagementObject managementObject = new ManagementObject(string.Concat("Win32_ComputerSystem.Name=\"", hostName, "\""));
					if (!(bool)managementObject["PartOfDomain"])
					{
						string resetComputerNotInDomain = ComputerResources.ResetComputerNotInDomain;
						ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(resetComputerNotInDomain), "ComputerNotInDomain", ErrorCategory.InvalidOperation, hostName);
						base.ThrowTerminatingError(errorRecord);
					}
					str = (string)LanguagePrimitives.ConvertTo(managementObject["Domain"], typeof(string), CultureInfo.InvariantCulture);
				}
				catch (ManagementException managementException1)
				{
					ManagementException managementException = managementException1;
					exception = managementException;
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					exception = cOMException;
				}
				catch (UnauthorizedAccessException unauthorizedAccessException1)
				{
					UnauthorizedAccessException unauthorizedAccessException = unauthorizedAccessException1;
					exception = unauthorizedAccessException;
				}
				if (exception != null)
				{
					string str1 = StringUtil.Format(ComputerResources.FailToGetDomainInformation, exception.Message);
					ErrorRecord errorRecord1 = new ErrorRecord(new InvalidOperationException(str1), "FailToGetDomainInformation", ErrorCategory.OperationStopped, hostName);
					base.ThrowTerminatingError(errorRecord1);
				}
				ResetComputerMachinePasswordCommand.ResetMachineAccountPassword(str, hostName, this.Server, this.Credential, this);
				return;
			}
			else
			{
				return;
			}
		}

		internal static void ResetMachineAccountPassword(string domain, string localMachineName, string server, PSCredential credential, PSCmdlet cmdlet)
		{
			SAMAPI.LSA_UNICODE_STRING structure;
			string userName;
			string stringFromSecureString;
			string cannotFindMachineAccountFromServer;
			DirectoryEntry directoryEntry = null;
			DirectoryEntry directoryEntry1 = null;
			DirectorySearcher directorySearcher = null;
			string randomPassword = null;
			string str = server;
			string str1 = str;
			if (str == null)
			{
				str1 = domain;
			}
			string str2 = str1;
			try
			{
				try
				{
					if (credential != null)
					{
						userName = credential.UserName;
					}
					else
					{
						userName = null;
					}
					string str3 = userName;
					if (credential != null)
					{
						stringFromSecureString = Utils.GetStringFromSecureString(credential.Password);
					}
					else
					{
						stringFromSecureString = null;
					}
					string str4 = stringFromSecureString;
					directoryEntry = new DirectoryEntry(string.Concat("LDAP://", str2), str3, str4, AuthenticationTypes.Secure);
					directorySearcher = new DirectorySearcher(directoryEntry);
					string[] strArrays = new string[5];
					strArrays[0] = "(&(objectClass=computer)(|(cn=";
					strArrays[1] = localMachineName;
					strArrays[2] = ")(dn=";
					strArrays[3] = localMachineName;
					strArrays[4] = ")))";
					directorySearcher.Filter = string.Concat(strArrays);
					SearchResult searchResult = directorySearcher.FindOne();
					if (searchResult != null)
					{
						directoryEntry1 = searchResult.GetDirectoryEntry();
						randomPassword = ComputerWMIHelper.GetRandomPassword(120);
						object[] objArray = new object[1];
						objArray[0] = randomPassword;
						directoryEntry1.Invoke("SetPassword", objArray);
						directoryEntry1.Properties["LockOutTime"].Value = 0;
					}
					else
					{
						if (server != null)
						{
							cannotFindMachineAccountFromServer = ComputerResources.CannotFindMachineAccountFromServer;
						}
						else
						{
							cannotFindMachineAccountFromServer = ComputerResources.CannotFindMachineAccountFromDomain;
						}
						string str5 = cannotFindMachineAccountFromServer;
						string str6 = StringUtil.Format(str5, str2);
						ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(str6), "CannotFindMachineAccount", ErrorCategory.OperationStopped, localMachineName);
						cmdlet.ThrowTerminatingError(errorRecord);
					}
				}
#if !MONO
				catch (DirectoryServicesCOMException directoryServicesCOMException1)
				{
					DirectoryServicesCOMException directoryServicesCOMException = directoryServicesCOMException1;
					string str7 = StringUtil.Format(ComputerResources.FailToResetPasswordOnDomain, directoryServicesCOMException.Message);
					ErrorRecord errorRecord1 = new ErrorRecord(new InvalidOperationException(str7), "FailToResetPasswordOnDomain", ErrorCategory.OperationStopped, localMachineName);
					cmdlet.ThrowTerminatingError(errorRecord1);
				}
#endif
				catch (TargetInvocationException targetInvocationException1)
				{
					TargetInvocationException targetInvocationException = targetInvocationException1;
					string str8 = StringUtil.Format(ComputerResources.FailToResetPasswordOnDomain, targetInvocationException.InnerException.Message);
					ErrorRecord errorRecord2 = new ErrorRecord(new InvalidOperationException(str8), "FailToResetPasswordOnDomain", ErrorCategory.OperationStopped, localMachineName);
					cmdlet.ThrowTerminatingError(errorRecord2);
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					string str9 = StringUtil.Format(ComputerResources.FailToResetPasswordOnDomain, cOMException.Message);
					ErrorRecord errorRecord3 = new ErrorRecord(new InvalidOperationException(str9), "FailToResetPasswordOnDomain", ErrorCategory.OperationStopped, localMachineName);
					cmdlet.ThrowTerminatingError(errorRecord3);
				}
			}
			finally
			{
				if (directoryEntry != null)
				{
					directoryEntry.Close();
					directoryEntry.Dispose();
				}
				if (directorySearcher != null)
				{
					directorySearcher.Dispose();
				}
				if (directoryEntry1 != null)
				{
					directoryEntry1.Close();
					directoryEntry1.Dispose();
				}
			}
			SAMAPI.LSA_OBJECT_ATTRIBUTES zero = new SAMAPI.LSA_OBJECT_ATTRIBUTES();
			zero.RootDirectory = IntPtr.Zero;
			zero.ObjectName = IntPtr.Zero;
			zero.Attributes = 0;
			zero.SecurityDescriptor = IntPtr.Zero;
			zero.SecurityQualityOfService = IntPtr.Zero;
			zero.Length = Marshal.SizeOf(typeof(SAMAPI.LSA_OBJECT_ATTRIBUTES));
			IntPtr intPtr = IntPtr.Zero;
			IntPtr zero1 = IntPtr.Zero;
			IntPtr intPtr1 = IntPtr.Zero;
			SAMAPI.LSA_UNICODE_STRING lSAUNICODESTRING = new SAMAPI.LSA_UNICODE_STRING();
			lSAUNICODESTRING.Buffer = IntPtr.Zero;
			SAMAPI.LSA_UNICODE_STRING lSAUNICODESTRING1 = lSAUNICODESTRING;
			SAMAPI.LSA_UNICODE_STRING zero2 = new SAMAPI.LSA_UNICODE_STRING();
			zero2.Buffer = IntPtr.Zero;
			SAMAPI.LSA_UNICODE_STRING lSAUNICODESTRING2 = zero2;
			SAMAPI.LSA_UNICODE_STRING zero3 = new SAMAPI.LSA_UNICODE_STRING();
			zero3.Buffer = IntPtr.Zero;
			zero3.Length = 0;
			zero3.MaximumLength = 0;
			try
			{
				uint num = SAMAPI.LsaOpenPolicy(ref zero3, ref zero, 0xf0fff, out intPtr);
				if (num == -1073741790)
				{
					string needAdminPrivilegeToResetPassword = ComputerResources.NeedAdminPrivilegeToResetPassword;
					ErrorRecord errorRecord4 = new ErrorRecord(new InvalidOperationException(needAdminPrivilegeToResetPassword), "UnauthorizedAccessException", ErrorCategory.InvalidOperation, localMachineName);
					cmdlet.ThrowTerminatingError(errorRecord4);
				}
				if (num != 0)
				{
					ResetComputerMachinePasswordCommand.ThrowOutLsaError(num, cmdlet);
				}
				SAMAPI.InitLsaString("$MACHINE.ACC", ref lSAUNICODESTRING1);
				SAMAPI.InitLsaString(randomPassword, ref lSAUNICODESTRING2);
				bool flag = false;
				num = SAMAPI.LsaOpenSecret(intPtr, ref lSAUNICODESTRING1, 3, out zero1);
				if (num == -1073741772)
				{
					num = SAMAPI.LsaCreateSecret(intPtr, ref lSAUNICODESTRING1, 1, out zero1);
					flag = true;
				}
				if (num != 0)
				{
					ResetComputerMachinePasswordCommand.ThrowOutLsaError(num, cmdlet);
				}
				if (!flag)
				{
					num = SAMAPI.LsaQuerySecret(zero1, out intPtr1, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
					if (num != 0)
					{
						ResetComputerMachinePasswordCommand.ThrowOutLsaError(num, cmdlet);
					}
					structure = (SAMAPI.LSA_UNICODE_STRING)Marshal.PtrToStructure(intPtr1, typeof(SAMAPI.LSA_UNICODE_STRING));
				}
				else
				{
					structure = lSAUNICODESTRING2;
				}
				num = SAMAPI.LsaSetSecret(zero1, ref lSAUNICODESTRING2, ref structure);
				if (num != 0)
				{
					ResetComputerMachinePasswordCommand.ThrowOutLsaError(num, cmdlet);
				}
			}
			finally
			{
				if (intPtr1 != IntPtr.Zero)
				{
					SAMAPI.LsaFreeMemory(intPtr1);
				}
				if (intPtr != IntPtr.Zero)
				{
					SAMAPI.LsaClose(intPtr);
				}
				if (zero1 != IntPtr.Zero)
				{
					SAMAPI.LsaClose(zero1);
				}
				SAMAPI.FreeLsaString(ref lSAUNICODESTRING1);
				SAMAPI.FreeLsaString(ref lSAUNICODESTRING2);
			}
		}

		private static void ThrowOutLsaError(uint ret, PSCmdlet cmdlet)
		{
			Win32Exception win32Exception = new Win32Exception(SAMAPI.LsaNtStatusToWinError(ret));
			string str = StringUtil.Format(ComputerResources.FailToResetPasswordOnLocalMachine, win32Exception.Message);
			ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(str), "FailToResetPasswordOnLocalMachine", ErrorCategory.OperationStopped, Dns.GetHostName());
			cmdlet.ThrowTerminatingError(errorRecord);
		}
	}
}