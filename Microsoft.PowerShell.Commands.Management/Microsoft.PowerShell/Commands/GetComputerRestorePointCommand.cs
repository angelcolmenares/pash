using Microsoft.PowerShell.Commands.Management;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Management;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Runtime.InteropServices;
using System.Text;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Get", "ComputerRestorePoint", DefaultParameterSetName="ID", HelpUri="http://go.microsoft.com/fwlink/?LinkID=135215")]
	[OutputType(new string[] { "System.Management.ManagementObject#root\\default\\SystemRestore" })]
	public sealed class GetComputerRestorePointCommand : PSCmdlet, IDisposable
	{
		private int[] _restorepoint;

		private SwitchParameter _laststatus;

		private ManagementClass WMIClass;

		[Parameter(ParameterSetName="LastStatus", Mandatory=true)]
		[ValidateNotNull]
		public SwitchParameter LastStatus
		{
			get
			{
				return this._laststatus;
			}
			set
			{
				this._laststatus = value;
			}
		}

		[Parameter(Position=0, ParameterSetName="ID")]
		[ValidateNotNullOrEmpty]
		[ValidateRange(1, 0x7fffffff)]
		public int[] RestorePoint
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

		public GetComputerRestorePointCommand()
		{
		}

		protected override void BeginProcessing()
		{
			if (!ComputerWMIHelper.SkipSystemRestoreOperationForARMPlatform(this))
			{
				try
				{
					ManagementScope managementScope = new ManagementScope("\\root\\default");
					managementScope.Connect();
					this.WMIClass = new ManagementClass("SystemRestore");
					this.WMIClass.Scope = managementScope;
					if (base.ParameterSetName.Equals("LastStatus"))
					{
						int num = Convert.ToInt32(this.WMIClass.InvokeMethod("GetLastRestoreStatus", null), CultureInfo.CurrentCulture);
						if (!num.Equals(0))
						{
							if (!num.Equals(1))
							{
								if (num.Equals(2))
								{
									base.WriteObject(ComputerResources.RestoreInterrupted);
								}
							}
							else
							{
								base.WriteObject(ComputerResources.RestoreSuceess);
							}
						}
						else
						{
							base.WriteObject(ComputerResources.RestoreFailed);
						}
					}
					Dictionary<int, string> nums = new Dictionary<int, string>();
					if (base.ParameterSetName.Equals("ID"))
					{
						ObjectQuery objectQuery = new ObjectQuery();
						if (this._restorepoint != null)
						{
							StringBuilder stringBuilder = new StringBuilder("select * from ");
							stringBuilder.Append("SystemRestore");
							stringBuilder.Append(" where  SequenceNumber = ");
							for (int i = 0; i <= (int)this._restorepoint.Length - 1; i++)
							{
								stringBuilder.Append(this._restorepoint[i]);
								if (i < (int)this._restorepoint.Length - 1)
								{
									stringBuilder.Append(" OR SequenceNumber = ");
								}
								if (!nums.ContainsKey(this._restorepoint[i]))
								{
									nums.Add(this._restorepoint[i], "true");
								}
							}
							objectQuery.QueryString = stringBuilder.ToString();
						}
						else
						{
							objectQuery.QueryString = "select * from SystemRestore";
						}
						ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(managementScope, objectQuery);
						foreach (ManagementObject managementObject in managementObjectSearcher.Get())
						{
							base.WriteObject(managementObject);
							if (this._restorepoint == null)
							{
								continue;
							}
							int num1 = Convert.ToInt32(managementObject.Properties["SequenceNumber"].Value, CultureInfo.CurrentCulture);
							if (!nums.ContainsKey(num1))
							{
								continue;
							}
							nums.Remove(num1);
						}
						if (nums != null && nums.Count > 0)
						{
							foreach (int key in nums.Keys)
							{
								string str = StringUtil.Format(ComputerResources.NoResorePoint, key);
								ArgumentException argumentException = new ArgumentException(str);
								ErrorRecord errorRecord = new ErrorRecord(argumentException, "NoResorePoint", ErrorCategory.InvalidArgument, null);
								base.WriteError(errorRecord);
							}
						}
					}
				}
				catch (ManagementException managementException1)
				{
					ManagementException managementException = managementException1;
					if (managementException.ErrorCode.Equals(ManagementStatus.NotFound) || managementException.ErrorCode.Equals(ManagementStatus.InvalidClass))
					{
						Exception exception = new ArgumentException(StringUtil.Format(ComputerResources.NotSupported, new object[0]));
						base.WriteError(new ErrorRecord(exception, "GetComputerRestorePointNotSupported", ErrorCategory.InvalidOperation, null));
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