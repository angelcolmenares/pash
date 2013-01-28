using Microsoft.PowerShell.Commands.Management;
using System;
using System.Globalization;
using System.Management;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("Checkpoint", "Computer", HelpUri="http://go.microsoft.com/fwlink/?LinkID=135197")]
	public class CheckpointComputerCommand : PSCmdlet, IDisposable
	{
		private string _description;

		private string _restorepointtype;

		private DateTime lastTimeProgressWasWritten;

		private int intRestorePoint;

		private int ret;

		private static Exception exceptionfromnewthread;

		private DateTime startUtcTime;

		private DateTime startLocalTime;

		[Parameter(Position=0, Mandatory=true)]
		[ValidateNotNullOrEmpty]
		public string Description
		{
			get
			{
				return this._description;
			}
			set
			{
				this._description = value;
			}
		}

		[Alias(new string[] { "RPT" })]
		[Parameter(Position=1)]
		[ValidateNotNullOrEmpty]
		[ValidateSet(new string[] { "APPLICATION_INSTALL", "APPLICATION_UNINSTALL", "DEVICE_DRIVER_INSTALL", "MODIFY_SETTINGS", "CANCELLED_OPERATION" })]
		public string RestorePointType
		{
			get
			{
				return this._restorepointtype;
			}
			set
			{
				if (value != null)
				{
					this._restorepointtype = value;
					if (!this._restorepointtype.Equals("APPLICATION_INSTALL", StringComparison.OrdinalIgnoreCase))
					{
						if (!this._restorepointtype.Equals("APPLICATION_UNINSTALL", StringComparison.OrdinalIgnoreCase))
						{
							if (!this._restorepointtype.Equals("DEVICE_DRIVER_INSTALL", StringComparison.OrdinalIgnoreCase))
							{
								if (!this._restorepointtype.Equals("MODIFY_SETTINGS", StringComparison.OrdinalIgnoreCase))
								{
									if (this._restorepointtype.Equals("CANCELLED_OPERATION", StringComparison.OrdinalIgnoreCase))
									{
										this.intRestorePoint = 13;
									}
									return;
								}
								else
								{
									this.intRestorePoint = 12;
									return;
								}
							}
							else
							{
								this.intRestorePoint = 10;
								return;
							}
						}
						else
						{
							this.intRestorePoint = 1;
							return;
						}
					}
					else
					{
						this.intRestorePoint = 0;
						return;
					}
				}
				else
				{
					throw PSTraceSource.NewArgumentNullException("value");
				}
			}
		}

		static CheckpointComputerCommand()
		{
		}

		public CheckpointComputerCommand()
		{
			this._restorepointtype = "APPLICATION_INSTALL";
			this.lastTimeProgressWasWritten = DateTime.UtcNow;
			this.ret = 0x7fffffff;
		}

		protected override void BeginProcessing()
		{
			if (!ComputerWMIHelper.SkipSystemRestoreOperationForARMPlatform(this))
			{
				CheckpointComputerCommand.exceptionfromnewthread = null;
				if (Environment.OSVersion.Version.Major >= 6 && this.intRestorePoint == 13)
				{
					ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(StringUtil.Format(ComputerResources.NotSupported, new object[0])), null, ErrorCategory.InvalidOperation, null);
					base.ThrowTerminatingError(errorRecord);
				}
				if (this.CanCreateNewRestorePoint(DateTime.Now))
				{
					this.startUtcTime = DateTime.UtcNow;
					this.startLocalTime = DateTime.Now;
					ThreadStart threadStart = new ThreadStart(this.CreateRestorePoint);
					Thread thread = new Thread(threadStart);
					thread.Start();
					this.WriteProgress();
					if (CheckpointComputerCommand.exceptionfromnewthread != null)
					{
						if (CheckpointComputerCommand.exceptionfromnewthread as COMException == null)
						{
							if (CheckpointComputerCommand.exceptionfromnewthread as ManagementException == null)
							{
								throw CheckpointComputerCommand.exceptionfromnewthread;
							}
							else
							{
								if (((ManagementException)CheckpointComputerCommand.exceptionfromnewthread).ErrorCode.Equals(ManagementStatus.NotFound) || ((ManagementException)CheckpointComputerCommand.exceptionfromnewthread).ErrorCode.Equals(ManagementStatus.InvalidClass))
								{
									Exception argumentException = new ArgumentException(StringUtil.Format(ComputerResources.NotSupported, new object[0]));
									base.WriteError(new ErrorRecord(argumentException, "CheckpointComputerNotSupported", ErrorCategory.InvalidOperation, null));
									return;
								}
								else
								{
									ErrorRecord errorRecord1 = new ErrorRecord(CheckpointComputerCommand.exceptionfromnewthread, "GetWMIManagementException", ErrorCategory.InvalidOperation, null);
									base.WriteError(errorRecord1);
									return;
								}
							}
						}
						else
						{
							if (!string.IsNullOrEmpty(CheckpointComputerCommand.exceptionfromnewthread.Message))
							{
								ErrorRecord errorRecord2 = new ErrorRecord(CheckpointComputerCommand.exceptionfromnewthread, "COMException", ErrorCategory.InvalidOperation, null);
								base.WriteError(errorRecord2);
								return;
							}
							else
							{
								Exception exception = new ArgumentException(StringUtil.Format(ComputerResources.SystemRestoreServiceDisabled, new object[0]));
								base.WriteError(new ErrorRecord(exception, "ServiceDisabled", ErrorCategory.InvalidOperation, null));
								return;
							}
						}
					}
					else
					{
						if (!this.ret.Equals(0x422))
						{
							if (this.ret.Equals(0) || this.ret.Equals(0x422))
							{
								return;
							}
							else
							{
								Exception argumentException1 = new ArgumentException(StringUtil.Format(ComputerResources.RestorePointNotCreated, new object[0]));
								base.WriteError(new ErrorRecord(argumentException1, "CheckpointComputerPointNotCreated", ErrorCategory.InvalidOperation, null));
								return;
							}
						}
						else
						{
							Exception exception1 = new ArgumentException(StringUtil.Format(ComputerResources.ServiceDisabled, new object[0]));
							base.WriteError(new ErrorRecord(exception1, "CheckpointComputerServiceDisabled", ErrorCategory.InvalidOperation, null));
							return;
						}
					}
				}
				else
				{
					base.WriteWarning(ComputerResources.CannotCreateRestorePointWarning);
					return;
				}
			}
			else
			{
				return;
			}
		}

		private bool CanCreateNewRestorePoint(DateTime startTime)
		{
			bool days;
			ManagementScope managementScope = new ManagementScope("\\root\\default");
			managementScope.Connect();
			ObjectQuery objectQuery = new ObjectQuery();
			objectQuery.QueryString = "select * from SystemRestore";
			ObjectQuery objectQuery1 = objectQuery;
			try
			{
				DateTime minValue = DateTime.MinValue;
				ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(managementScope, objectQuery1);
				foreach (ManagementObject managementObject in managementObjectSearcher.Get())
				{
					DateTime dateTime = ManagementDateTimeConverter.ToDateTime(managementObject.Properties["CreationTime"].Value.ToString());
					if (dateTime <= minValue)
					{
						continue;
					}
					minValue = dateTime;
				}
				TimeSpan timeSpan = startTime.Subtract(minValue);
				days = timeSpan.Days >= 1;
			}
			catch (ManagementException managementException)
			{
				days = true;
			}
			catch (COMException cOMException)
			{
				days = true;
			}
			return days;
		}

		private void CreateRestorePoint()
		{
			ManagementClass managementClass = null;
			using (managementClass)
			{
				try
				{
					ManagementScope managementScope = new ManagementScope("\\root\\default");
					managementScope.Connect();
					managementClass = new ManagementClass("SystemRestore");
					managementClass.Scope = managementScope;
					managementClass.GetMethodParameters("CreateRestorePoint");
					object[] objArray = new object[3];
					objArray[0] = this._description;
					objArray[1] = this.intRestorePoint;
					objArray[2] = 100;
					object[] objArray1 = objArray;
					this.ret = Convert.ToInt32(managementClass.InvokeMethod("CreateRestorePoint", objArray1), CultureInfo.CurrentCulture);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					CheckpointComputerCommand.exceptionfromnewthread = exception;
				}
			}
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}

		private bool IsRestorePointCreated(string description, DateTime starttime)
		{
			bool flag = false;
			ManagementScope managementScope = new ManagementScope("\\root\\default");
			managementScope.Connect();
			ObjectQuery objectQuery = new ObjectQuery();
			StringBuilder stringBuilder = new StringBuilder("select * from ");
			stringBuilder.Append("SystemRestore");
			stringBuilder.Append(" where  description = '");
			stringBuilder.Append(description.Replace("'", "\\'"));
			stringBuilder.Append("'");
			objectQuery.QueryString = stringBuilder.ToString();
			try
			{
				ManagementObjectSearcher managementObjectSearcher = new ManagementObjectSearcher(managementScope, objectQuery);
				if (managementObjectSearcher.Get().Count > 0)
				{
					foreach (ManagementObject managementObject in managementObjectSearcher.Get())
					{
						DateTime dateTime = ManagementDateTimeConverter.ToDateTime(managementObject.Properties["CreationTime"].Value.ToString());
						if (dateTime.AddSeconds(1) < starttime)
						{
							continue;
						}
						flag = true;
					}
				}
			}
			catch (ManagementException managementException)
			{
				flag = true;
			}
			catch (COMException cOMException)
			{
				flag = true;
			}
			return flag;
		}

		private void WriteProgress(string statusDescription, int? percentComplete)
		{
			ProgressRecordType progressRecordType;
			if (!percentComplete.HasValue || percentComplete.Value != 100)
			{
				progressRecordType = ProgressRecordType.Processing;
			}
			else
			{
				progressRecordType = ProgressRecordType.Completed;
			}
			if (progressRecordType == ProgressRecordType.Processing)
			{
				TimeSpan utcNow = DateTime.UtcNow - this.lastTimeProgressWasWritten;
				if (utcNow < TimeSpan.FromMilliseconds(200))
				{
					return;
				}
			}
			this.lastTimeProgressWasWritten = DateTime.UtcNow;
			string str = StringUtil.Format(ComputerResources.ProgressActivity, new object[0]);
			ProgressRecord progressRecord = new ProgressRecord(0x71914c8b, str, statusDescription);
			if (percentComplete.HasValue)
			{
				progressRecord.PercentComplete = percentComplete.Value;
			}
			progressRecord.RecordType = progressRecordType;
			base.WriteProgress(progressRecord);
		}

		private void WriteProgress(DateTime starttime)
		{
			int percentageComplete = ProgressRecord.GetPercentageComplete(starttime, TimeSpan.FromSeconds(90));
			if (percentageComplete < 100)
			{
				this.WriteProgress(StringUtil.Format(ComputerResources.ProgressStatusCreatingRestorePoint, percentageComplete), new int?(percentageComplete));
			}
		}

		private void WriteProgress()
		{
			do
			{
			Label0:
				this.WriteProgress(this.startUtcTime);
				Thread.Sleep(0x3e8);
				if (CheckpointComputerCommand.exceptionfromnewthread != null)
				{
					break;
				}
				if (this.ret != 0)
				{
					continue;
				}
				if (this.IsRestorePointCreated(this._description, this.startLocalTime))
				{
					break;
				}
				else
				{
					goto Label0;
				}
			}
			while (this.ret == 0x7fffffff);
			this.WriteProgress(StringUtil.Format(ComputerResources.ProgressStatusCompleted, new object[0]), new int?(100));
		}
	}
}