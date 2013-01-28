using System;
using System.Globalization;
using System.IO;
using System.Management.Automation;

namespace Microsoft.PowerShell.Commands
{
	[Cmdlet("New", "PSWorkflowExecutionOption", HelpUri="http://go.microsoft.com/fwlink/?LinkID=210609")]
	[OutputType(new Type[] { typeof(PSWorkflowExecutionOption) })]
	public sealed class NewPSWorkflowExecutionOptionCommand : PSCmdlet
	{
		private PSWorkflowExecutionOption option;

		private bool enableValidationParamSpecified;

		[Parameter]
		[ValidateRange(1, 0x7fffffff)]
		public int ActivityProcessIdleTimeoutSec
		{
			get
			{
				return this.option.ActivityProcessIdleTimeoutSec;
			}
			set
			{
				this.option.ActivityProcessIdleTimeoutSec = value;
			}
		}

		[Parameter]
		public string[] AllowedActivity
		{
			get
			{
				return this.option.AllowedActivity;
			}
			set
			{
				this.option.AllowedActivity = value;
			}
		}

		[Parameter]
		public SwitchParameter EnableValidation
		{
			get
			{
				return this.option.EnableValidation;
			}
			set
			{
				this.option.EnableValidation = value;
				this.enableValidationParamSpecified = true;
			}
		}

		[Parameter]
		[ValidateRange(1, 0x7fffffff)]
		public int MaxActivityProcesses
		{
			get
			{
				return this.option.MaxActivityProcesses;
			}
			set
			{
				this.option.MaxActivityProcesses = value;
			}
		}

		[Parameter]
		[ValidateRange(1, 0x7fffffff)]
		public int MaxConnectedSessions
		{
			get
			{
				return this.option.MaxConnectedSessions;
			}
			set
			{
				this.option.MaxConnectedSessions = value;
			}
		}

		[Parameter]
		[ValidateRange(1, 0x7fffffff)]
		public int MaxDisconnectedSessions
		{
			get
			{
				return this.option.MaxDisconnectedSessions;
			}
			set
			{
				this.option.MaxDisconnectedSessions = value;
			}
		}

		[Parameter]
		public long MaxPersistenceStoreSizeGB
		{
			get
			{
				return this.option.MaxPersistenceStoreSizeGB;
			}
			set
			{
				this.option.MaxPersistenceStoreSizeGB = value;
			}
		}

		[Parameter]
		[ValidateRange(1, 0x7fffffff)]
		public int MaxRunningWorkflows
		{
			get
			{
				return this.option.MaxRunningWorkflows;
			}
			set
			{
				this.option.MaxRunningWorkflows = value;
			}
		}

		[Parameter]
		[ValidateRange(1, 0x7fffffff)]
		public int MaxSessionsPerRemoteNode
		{
			get
			{
				return this.option.MaxSessionsPerRemoteNode;
			}
			set
			{
				this.option.MaxSessionsPerRemoteNode = value;
			}
		}

		[Parameter]
		[ValidateRange(1, 0x7fffffff)]
		public int MaxSessionsPerWorkflow
		{
			get
			{
				return this.option.MaxSessionsPerWorkflow;
			}
			set
			{
				this.option.MaxSessionsPerWorkflow = value;
			}
		}

		[Parameter]
		public string[] OutOfProcessActivity
		{
			get
			{
				return this.option.OutOfProcessActivity;
			}
			set
			{
				this.option.OutOfProcessActivity = value;
			}
		}

		[Parameter]
		public string PersistencePath
		{
			get
			{
				return this.option.PersistencePath;
			}
			set
			{
				if (value != null)
				{
					string pathRoot = value;
					bool flag = false;
					if (!Path.IsPathRooted(value))
					{
						try
						{
							pathRoot = Path.GetPathRoot(value);
							if (string.IsNullOrEmpty(pathRoot))
							{
								pathRoot = value;
							}
						}
						catch (PathTooLongException pathTooLongException)
						{
							flag = true;
						}
					}
					if (flag || pathRoot != null && pathRoot.Length > 120)
					{
						object[] objArray = new object[2];
						objArray[0] = value;
						objArray[1] = 120;
						throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, Resources.PersistencePathToolLong, objArray));
					}
				}
				this.option.PersistencePath = value;
			}
		}

		[Parameter]
		public SwitchParameter PersistWithEncryption
		{
			get
			{
				return this.option.PersistWithEncryption;
			}
			set
			{
				this.option.PersistWithEncryption = value;
			}
		}

		[Parameter]
		[ValidateRange(30, 0x7530)]
		public int RemoteNodeSessionIdleTimeoutSec
		{
			get
			{
				return this.option.RemoteNodeSessionIdleTimeoutSec;
			}
			set
			{
				this.option.RemoteNodeSessionIdleTimeoutSec = value;
			}
		}

		[Parameter]
		[ValidateRange(1, 0x7fffffff)]
		public int SessionThrottleLimit
		{
			get
			{
				return this.option.SessionThrottleLimit;
			}
			set
			{
				this.option.SessionThrottleLimit = value;
			}
		}

		[Parameter]
		[ValidateRange(0, 0x1388)]
		public int WorkflowShutdownTimeoutMSec
		{
			get
			{
				return this.option.WorkflowShutdownTimeoutMSec;
			}
			set
			{
				this.option.WorkflowShutdownTimeoutMSec = value;
			}
		}

		public NewPSWorkflowExecutionOptionCommand()
		{
			this.option = new PSWorkflowExecutionOption();
		}

		protected override void ProcessRecord()
		{
			if (!this.enableValidationParamSpecified)
			{
				this.option.EnableValidation = true;
			}
			base.WriteObject(this.option);
		}
	}
}