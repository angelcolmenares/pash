using Microsoft.PowerShell.Commands.Management;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Management;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Net;
using System.Runtime.InteropServices;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Test", "ComputerSecureChannel", SupportsShouldProcess=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=137749")]
	[OutputType(new Type[] { typeof(bool) })]
	public class TestComputerSecureChannelCommand : PSCmdlet
	{
		private const uint NETLOGON_CONTROL_REDISCOVER = 5;

		private const uint NETLOGON_CONTROL_TC_QUERY = 6;

		private const uint NETLOGON_INFO_2 = 2;

		[Credential]
		[Parameter]
		[ValidateNotNullOrEmpty]
		public PSCredential Credential
		{
			get;
			set;
		}

		[Parameter]
		public SwitchParameter Repair
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

		public TestComputerSecureChannelCommand()
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
					var hostname = Dns.GetHostEntry(this.Server).HostName;
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
			bool flag;
			string str;
			string str1;
			string str2;
			string hostName = Dns.GetHostName();
			string str3 = null;
			Exception exception = null;
			if (base.ShouldProcess(hostName))
			{
				try
				{
					ManagementObject managementObject = new ManagementObject(string.Concat("Win32_ComputerSystem.Name=\"", hostName, "\""));
					if (!(bool)managementObject["PartOfDomain"])
					{
						string testComputerNotInDomain = ComputerResources.TestComputerNotInDomain;
						ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(testComputerNotInDomain), "ComputerNotInDomain", ErrorCategory.InvalidOperation, hostName);
						base.ThrowTerminatingError(errorRecord);
					}
					str3 = (string)LanguagePrimitives.ConvertTo(managementObject["Domain"], typeof(string), CultureInfo.InvariantCulture);
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
					string str4 = StringUtil.Format(ComputerResources.FailToGetDomainInformation, exception.Message);
					ErrorRecord errorRecord1 = new ErrorRecord(new InvalidOperationException(str4), "FailToGetDomainInformation", ErrorCategory.OperationStopped, hostName);
					base.ThrowTerminatingError(errorRecord1);
				}
				if (!this.Repair)
				{
					flag = this.VerifySecureChannel(str3, hostName);
					if (flag)
					{
						str1 = StringUtil.Format(ComputerResources.SecureChannelAlive, str3);
					}
					else
					{
						str1 = StringUtil.Format(ComputerResources.SecureChannelBroken, str3);
					}
					str = str1;
				}
				else
				{
					ResetComputerMachinePasswordCommand.ResetMachineAccountPassword(str3, hostName, this.Server, this.Credential, this);
					flag = this.ResetSecureChannel(str3);
					if (flag)
					{
						str2 = StringUtil.Format(ComputerResources.RepairSecureChannelSucceed, str3);
					}
					else
					{
						str2 = StringUtil.Format(ComputerResources.RepairSecureChannelFail, str3);
					}
					str = str2;
				}
				base.WriteObject(flag);
				base.WriteVerbose(str);
				return;
			}
			else
			{
				return;
			}
		}

		private bool ResetSecureChannel(string domain)
		{
			IntPtr zero = IntPtr.Zero;
			IntPtr coTaskMemAuto = Marshal.StringToCoTaskMemAuto(domain);
			bool trustedDcName = false;
			try
			{
				int num = SAMAPI.I_NetLogonControl2(null, 5, 2, ref coTaskMemAuto, out zero);
				if (num == 0)
				{
					SAMAPI.NetLogonInfo2 structure = (SAMAPI.NetLogonInfo2)Marshal.PtrToStructure(zero, typeof(SAMAPI.NetLogonInfo2));
					trustedDcName = structure.TrustedDcName != null;
				}
			}
			finally
			{
				if (coTaskMemAuto != IntPtr.Zero)
				{
					Marshal.FreeCoTaskMem(coTaskMemAuto);
				}
				if (zero != IntPtr.Zero)
				{
					SAMAPI.NetApiBufferFree(zero);
				}
			}
			return trustedDcName;
		}

		private bool VerifySecureChannel(string domain, string localMachineName)
		{
			IntPtr zero = IntPtr.Zero;
			IntPtr coTaskMemAuto = Marshal.StringToCoTaskMemAuto(domain);
			bool pdcConnectionStatus = false;
			try
			{
				int num = SAMAPI.I_NetLogonControl2(null, 6, 2, ref coTaskMemAuto, out zero);
				if (num != 0)
				{
					Win32Exception win32Exception = new Win32Exception(num);
					string str = StringUtil.Format(ComputerResources.FailToTestSecureChannel, win32Exception.Message);
					ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(str), "FailToTestSecureChannel", ErrorCategory.OperationStopped, localMachineName);
					base.ThrowTerminatingError(errorRecord);
				}
				SAMAPI.NetLogonInfo2 structure = (SAMAPI.NetLogonInfo2)Marshal.PtrToStructure(zero, typeof(SAMAPI.NetLogonInfo2));
				pdcConnectionStatus = structure.PdcConnectionStatus == 0;
			}
			finally
			{
				if (coTaskMemAuto != IntPtr.Zero)
				{
					Marshal.FreeCoTaskMem(coTaskMemAuto);
				}
				if (zero != IntPtr.Zero)
				{
					SAMAPI.NetApiBufferFree(zero);
				}
			}
			return pdcConnectionStatus;
		}
	}
}