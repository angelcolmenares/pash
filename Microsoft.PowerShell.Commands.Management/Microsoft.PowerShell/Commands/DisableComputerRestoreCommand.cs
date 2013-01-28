using Microsoft.PowerShell.Commands.Management;
using System;
using System.Globalization;
using System.Management;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Runtime.InteropServices;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Disable", "ComputerRestore", SupportsShouldProcess=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=135207")]
	public sealed class DisableComputerRestoreCommand : PSCmdlet, IDisposable
	{
		private const string ErrorBase = "ComputerResources";

		private string[] _drive;

		private ManagementClass WMIClass;

		[Parameter(Position=0, Mandatory=true)]
		[ValidateNotNullOrEmpty]
		public string[] Drive
		{
			get
			{
				return this._drive;
			}
			set
			{
				this._drive = value;
			}
		}

		public DisableComputerRestoreCommand()
		{
		}

		protected override void BeginProcessing()
		{
			string str;
			if (!ComputerWMIHelper.SkipSystemRestoreOperationForARMPlatform(this))
			{
				ManagementScope managementScope = new ManagementScope("\\root\\default");
				managementScope.Connect();
				this.WMIClass = new ManagementClass("SystemRestore");
				this.WMIClass.Scope = managementScope;
				string[] strArrays = this._drive;
				for (int i = 0; i < (int)strArrays.Length; i++)
				{
					string str1 = strArrays[i];
					if (base.ShouldProcess(str1))
					{
						if (str1.EndsWith("\\", StringComparison.CurrentCultureIgnoreCase))
						{
							str = str1;
						}
						else
						{
							str = string.Concat(str1, "\\");
						}
						if (ComputerWMIHelper.IsValidDrive(str))
						{
							try
							{
								object[] objArray = new object[1];
								objArray[0] = str;
								object[] objArray1 = objArray;
								int num = Convert.ToInt32(this.WMIClass.InvokeMethod("Disable", objArray1), CultureInfo.CurrentCulture);
								if (!num.Equals(0) && !num.Equals(0x6b5))
								{
									ErrorRecord errorRecord = new ErrorRecord(new ArgumentException(StringUtil.Format(ComputerResources.NotDisabled, str1)), null, ErrorCategory.InvalidOperation, null);
									base.WriteError(errorRecord);
								}
							}
							catch (ManagementException managementException1)
							{
								ManagementException managementException = managementException1;
								if (managementException.ErrorCode.Equals(ManagementStatus.NotFound) || managementException.ErrorCode.Equals(ManagementStatus.InvalidClass))
								{
									ErrorRecord errorRecord1 = new ErrorRecord(new ArgumentException(StringUtil.Format(ComputerResources.NotSupported, new object[0])), null, ErrorCategory.InvalidOperation, null);
									base.WriteError(errorRecord1);
								}
								else
								{
									ErrorRecord errorRecord2 = new ErrorRecord(managementException, "GetWMIManagementException", ErrorCategory.InvalidOperation, null);
									base.WriteError(errorRecord2);
								}
							}
							catch (COMException cOMException1)
							{
								COMException cOMException = cOMException1;
								if (!string.IsNullOrEmpty(cOMException.Message))
								{
									ErrorRecord errorRecord3 = new ErrorRecord(cOMException, "COMException", ErrorCategory.InvalidOperation, null);
									base.WriteError(errorRecord3);
								}
								else
								{
									Exception argumentException = new ArgumentException(StringUtil.Format(ComputerResources.SystemRestoreServiceDisabled, new object[0]));
									base.WriteError(new ErrorRecord(argumentException, "ServiceDisabled", ErrorCategory.InvalidOperation, null));
								}
							}
						}
						else
						{
							ErrorRecord errorRecord4 = new ErrorRecord(new ArgumentException(StringUtil.Format(ComputerResources.NotValidDrive, str1)), null, ErrorCategory.InvalidData, null);
							base.WriteError(errorRecord4);
						}
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