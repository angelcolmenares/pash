using Microsoft.PowerShell.Commands.Management;
using System;
using System.Globalization;
using System.Management;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Runtime.InteropServices;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Enable", "ComputerRestore", SupportsShouldProcess=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=135209")]
	public sealed class EnableComputerRestoreCommand : PSCmdlet, IDisposable
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

		public EnableComputerRestoreCommand()
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
				string str1 = Environment.ExpandEnvironmentVariables("%SystemDrive%");
				string[] strArrays = new string[2];
				strArrays[0] = str1;
				strArrays[1] = "\\";
				str1 = string.Concat(strArrays);
				if (!ComputerWMIHelper.ContainsSystemDrive(this._drive, str1))
				{
					ArgumentException argumentException = new ArgumentException(StringUtil.Format(ComputerResources.NoSystemDrive, new object[0]));
					base.WriteError(new ErrorRecord(argumentException, "EnableComputerNoSystemDrive", ErrorCategory.InvalidArgument, null));
				}
				else
				{
					object[] objArray = new object[1];
					objArray[0] = str1;
					object[] objArray1 = objArray;
					try
					{
						int num = Convert.ToInt32(this.WMIClass.InvokeMethod("Enable", objArray1), CultureInfo.CurrentCulture);
						if (num.Equals(0) || num.Equals(0x420))
						{
							string[] strArrays1 = this._drive;
							for (int i = 0; i < (int)strArrays1.Length; i++)
							{
								string str2 = strArrays1[i];
								if (base.ShouldProcess(str2))
								{
									if (str2.EndsWith("\\", StringComparison.CurrentCultureIgnoreCase))
									{
										str = str2;
									}
									else
									{
										str = string.Concat(str2, "\\");
									}
									if (ComputerWMIHelper.IsValidDrive(str))
									{
										if (!str.Equals(str1, StringComparison.OrdinalIgnoreCase))
										{
											object[] objArray2 = new object[1];
											objArray2[0] = str;
											object[] objArray3 = objArray2;
											num = Convert.ToInt32(this.WMIClass.InvokeMethod("Enable", objArray3), CultureInfo.CurrentCulture);
											if (num.Equals(0x6b5))
											{
												num = Convert.ToInt32(this.WMIClass.InvokeMethod("Enable", objArray3), CultureInfo.CurrentCulture);
											}
										}
										if (!num.Equals(0) && !num.Equals(0x420) && !num.Equals(0x6b5))
										{
											Exception exception = new ArgumentException(StringUtil.Format(ComputerResources.NotEnabled, str2));
											base.WriteError(new ErrorRecord(exception, "EnableComputerRestoreNotEnabled", ErrorCategory.InvalidOperation, null));
										}
									}
									else
									{
										Exception argumentException1 = new ArgumentException(StringUtil.Format(ComputerResources.InvalidDrive, str2));
										base.WriteError(new ErrorRecord(argumentException1, "EnableComputerRestoreInvalidDrive", ErrorCategory.InvalidData, null));
									}
								}
							}
						}
						else
						{
							ArgumentException argumentException2 = new ArgumentException(StringUtil.Format(ComputerResources.NotEnabled, str1));
							base.WriteError(new ErrorRecord(argumentException2, "EnableComputerRestoreNotEnabled", ErrorCategory.InvalidOperation, null));
						}
					}
					catch (ManagementException managementException1)
					{
						ManagementException managementException = managementException1;
						if (managementException.ErrorCode.Equals(ManagementStatus.NotFound) || managementException.ErrorCode.Equals(ManagementStatus.InvalidClass))
						{
							ErrorRecord errorRecord = new ErrorRecord(new ArgumentException(StringUtil.Format(ComputerResources.NotSupported, new object[0])), null, ErrorCategory.InvalidOperation, null);
							base.WriteError(errorRecord);
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
							Exception exception1 = new ArgumentException(StringUtil.Format(ComputerResources.SystemRestoreServiceDisabled, new object[0]));
							base.WriteError(new ErrorRecord(exception1, "ServiceDisabled", ErrorCategory.InvalidOperation, null));
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