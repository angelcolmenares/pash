using Microsoft.PowerShell.Commands.Management;
using System;
using System.Management;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Runtime.InteropServices;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Restore", "Computer", SupportsShouldProcess=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=135254")]
	public sealed class RestoreComputerCommand : PSCmdlet, IDisposable
	{
		private int _restorepoint;

		private ManagementClass WMIClass;

		[Alias(new string[] { "SequenceNumber", "SN", "RP" })]
		[Parameter(Position=0, Mandatory=true)]
		[ValidateNotNull]
		[ValidateRange(1, 0x7fffffff)]
		public int RestorePoint
		{
			get
			{
				return this._restorepoint;
			}
			set
			{
				this._restorepoint = value;
			}
		}

		public RestoreComputerCommand()
		{
		}

		protected override void BeginProcessing()
		{
			if (!ComputerWMIHelper.SkipSystemRestoreOperationForARMPlatform(this))
			{
				try
				{
					ConnectionOptions connection = ComputerWMIHelper.GetConnection(AuthenticationLevel.Packet, ImpersonationLevel.Impersonate, null);
					ManagementPath managementPath = new ManagementPath();
					managementPath.Path = "\\root\\default";
					ManagementScope managementScope = new ManagementScope(managementPath, connection);
					managementScope.Connect();
					this.WMIClass = new ManagementClass("SystemRestore");
					this.WMIClass.Scope = managementScope;
					ObjectQuery objectQuery = new ObjectQuery(string.Concat("select * from SystemRestore where SequenceNumber = ", this._restorepoint));
					ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(managementScope, objectQuery);
					if (managementObjectSearcher.Get().Count != 0)
					{
						string machineName = Environment.MachineName;
						if (base.ShouldProcess(machineName))
						{
							object[] objArray = new object[1];
							objArray[0] = this._restorepoint;
							object[] objArray1 = objArray;
							this.WMIClass.InvokeMethod("Restore", objArray1);
							managementPath.Path = "\\root\\cimv2";
							managementScope.Path = managementPath;
							ManagementClass managementClass = new ManagementClass("Win32_OperatingSystem");
							managementClass.Scope = managementScope;
							ObjectQuery objectQuery1 = new ObjectQuery("Select * from Win32_OperatingSystem");
							ManagementObjectSearcher managementObjectSearcher1 = new ManagementObjectSearcher(managementScope, objectQuery1);
							foreach (ManagementObject managementObject in managementObjectSearcher1.Get())
							{
								string[] strArrays = new string[1];
								strArrays[0] = "";
								string[] strArrays1 = strArrays;
								managementObject.InvokeMethod("Reboot", strArrays1);
							}
						}
					}
					else
					{
						string str = StringUtil.Format(ComputerResources.InvalidRestorePoint, this._restorepoint);
						ArgumentException argumentException = new ArgumentException(str);
						ErrorRecord errorRecord = new ErrorRecord(argumentException, "InvalidRestorePoint", ErrorCategory.InvalidArgument, null);
						base.WriteError(errorRecord);
					}
				}
				catch (ManagementException managementException1)
				{
					ManagementException managementException = managementException1;
					if (managementException.ErrorCode.ToString().Equals("NotFound") || managementException.ErrorCode.ToString().Equals("InvalidClass"))
					{
						Exception exception = new ArgumentException(StringUtil.Format(ComputerResources.NotSupported, new object[0]));
						base.WriteError(new ErrorRecord(exception, "RestoreComputerNotSupported", ErrorCategory.InvalidOperation, null));
					}
					else
					{
						ErrorRecord errorRecord1 = new ErrorRecord(managementException, "GetWMIManagementException", ErrorCategory.InvalidOperation, null);
						base.WriteError(errorRecord1);
					}
				}
				catch (COMException cOMException1)
				{
					COMException cOMException = cOMException1;
					if (!string.IsNullOrEmpty(cOMException.Message))
					{
						ErrorRecord errorRecord2 = new ErrorRecord(cOMException, "COMException", ErrorCategory.InvalidOperation, null);
						base.WriteError(errorRecord2);
					}
					else
					{
						Exception argumentException1 = new ArgumentException(StringUtil.Format(ComputerResources.SystemRestoreServiceDisabled, new object[0]));
						base.WriteError(new ErrorRecord(argumentException1, "ServiceDisabled", ErrorCategory.InvalidOperation, null));
					}
				}
				return;
			}
			else
			{
				return;
			}
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void Dispose(bool disposing)
		{
			if (disposing && this.WMIClass != null)
			{
				this.WMIClass.Dispose();
			}
		}

		protected override void StopProcessing()
		{
			if (this.WMIClass != null)
			{
				this.WMIClass.Dispose();
			}
		}
	}
}