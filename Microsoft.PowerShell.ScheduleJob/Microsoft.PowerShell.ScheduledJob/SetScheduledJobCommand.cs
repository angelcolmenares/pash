using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace Microsoft.PowerShell.ScheduledJob
{
	[Cmdlet("Set", "ScheduledJob", DefaultParameterSetName="ScriptBlock", HelpUri="http://go.microsoft.com/fwlink/?LinkID=223924")]
	[OutputType(new Type[] { typeof(ScheduledJobDefinition) })]
	public sealed class SetScheduledJobCommand : ScheduleJobCmdletBase
	{
		private const string ExecutionParameterSet = "Execution";

		private const string ScriptBlockParameterSet = "ScriptBlock";

		private const string FilePathParameterSet = "FilePath";

		private string _name;

		private string _filePath;

		private ScriptBlock _scriptBlock;

		private ScheduledJobTrigger[] _triggers;

		private ScriptBlock _initializationScript;

		private SwitchParameter _runAs32;

		private PSCredential _credential;

		private AuthenticationMechanism _authenticationMechanism;

		private ScheduledJobOptions _options;

		private ScheduledJobDefinition _definition;

		private SwitchParameter _clearExecutionHistory;

		private int _executionHistoryLength;

		private SwitchParameter _passThru;

		private object[] _arguments;

		[Parameter(ParameterSetName="ScriptBlock")]
		[Parameter(ParameterSetName="FilePath")]
		[ValidateNotNullOrEmpty]
		public object[] ArgumentList
		{
			get
			{
				return this._arguments;
			}
			set
			{
				this._arguments = value;
			}
		}

		[Parameter(ParameterSetName="FilePath")]
		[Parameter(ParameterSetName="ScriptBlock")]
		public AuthenticationMechanism Authentication
		{
			get
			{
				return this._authenticationMechanism;
			}
			set
			{
				this._authenticationMechanism = value;
			}
		}

		[Parameter(ParameterSetName="Execution")]
		public SwitchParameter ClearExecutionHistory
		{
			get
			{
				return this._clearExecutionHistory;
			}
			set
			{
				this._clearExecutionHistory = value;
			}
		}

		[Credential]
		[Parameter(ParameterSetName="ScriptBlock")]
		[Parameter(ParameterSetName="FilePath")]
		public PSCredential Credential
		{
			get
			{
				return this._credential;
			}
			set
			{
				this._credential = value;
			}
		}

		[Parameter(ParameterSetName="FilePath")]
		[ValidateNotNullOrEmpty]
		public string FilePath
		{
			get
			{
				return this._filePath;
			}
			set
			{
				this._filePath = value;
			}
		}

		[Parameter(ParameterSetName="ScriptBlock")]
		[Parameter(ParameterSetName="FilePath")]
		[ValidateNotNull]
		public ScriptBlock InitializationScript
		{
			get
			{
				return this._initializationScript;
			}
			set
			{
				this._initializationScript = value;
			}
		}

		[Parameter(Position=0, Mandatory=true, ValueFromPipeline=true, ParameterSetName="ScriptBlock")]
		[Parameter(Position=0, Mandatory=true, ValueFromPipeline=true, ParameterSetName="Execution")]
		[Parameter(Position=0, Mandatory=true, ValueFromPipeline=true, ParameterSetName="FilePath")]
		[ValidateNotNull]
		public ScheduledJobDefinition InputObject
		{
			get
			{
				return this._definition;
			}
			set
			{
				this._definition = value;
			}
		}

		[Parameter(ParameterSetName="FilePath")]
		[Parameter(ParameterSetName="ScriptBlock")]
		public int MaxResultCount
		{
			get
			{
				return this._executionHistoryLength;
			}
			set
			{
				this._executionHistoryLength = value;
			}
		}

		[Parameter(ParameterSetName="ScriptBlock")]
		[Parameter(ParameterSetName="FilePath")]
		[ValidateNotNullOrEmpty]
		public string Name
		{
			get
			{
				return this._name;
			}
			set
			{
				this._name = value;
			}
		}

		[Parameter(ParameterSetName="ScriptBlock")]
		[Parameter(ParameterSetName="FilePath")]
		[Parameter(ParameterSetName="Execution")]
		public SwitchParameter PassThru
		{
			get
			{
				return this._passThru;
			}
			set
			{
				this._passThru = value;
			}
		}

		[Parameter(ParameterSetName="ScriptBlock")]
		[Parameter(ParameterSetName="FilePath")]
		public SwitchParameter RunAs32
		{
			get
			{
				return this._runAs32;
			}
			set
			{
				this._runAs32 = value;
			}
		}

		[Parameter(ParameterSetName="ScriptBlock")]
		[Parameter(ParameterSetName="FilePath")]
		[ValidateNotNull]
		public ScheduledJobOptions ScheduledJobOption
		{
			get
			{
				return this._options;
			}
			set
			{
				this._options = value;
			}
		}

		[Parameter(ParameterSetName="ScriptBlock")]
		[ValidateNotNull]
		public ScriptBlock ScriptBlock
		{
			get
			{
				return this._scriptBlock;
			}
			set
			{
				this._scriptBlock = value;
			}
		}

		[Parameter(ParameterSetName="FilePath")]
		[Parameter(ParameterSetName="ScriptBlock")]
		[ValidateNotNullOrEmpty]
		public ScheduledJobTrigger[] Trigger
		{
			get
			{
				return this._triggers;
			}
			set
			{
				this._triggers = value;
			}
		}

		public SetScheduledJobCommand()
		{
		}

		protected override void ProcessRecord()
		{
			ErrorRecord errorRecord;
			string parameterSetName = base.ParameterSetName;
			string str = parameterSetName;
			if (parameterSetName != null)
			{
				if (str == "Execution")
				{
					this.UpdateExecutionDefinition();
				}
				else
				{
					if (str == "ScriptBlock" || str == "FilePath")
					{
						this.UpdateDefinition();
					}
				}
			}
			try
			{
				if (this.Trigger != null || this.ScheduledJobOption != null || this.Credential != null)
				{
					this._definition.Save();
				}
				else
				{
					this._definition.SaveToStore();
				}
			}
			catch (ScheduledJobException scheduledJobException1)
			{
				ScheduledJobException scheduledJobException = scheduledJobException1;
				if (scheduledJobException.InnerException == null || scheduledJobException.InnerException as UnauthorizedAccessException == null)
				{
					if (scheduledJobException.InnerException == null || scheduledJobException.InnerException as IOException == null)
					{
						string str1 = StringUtil.Format(ScheduledJobErrorStrings.CantSetJobDefinition, this._definition.Name);
						errorRecord = new ErrorRecord(new RuntimeException(str1, scheduledJobException), "CantSetPropertiesToScheduledJobDefinition", ErrorCategory.InvalidOperation, this._definition);
					}
					else
					{
						string str2 = StringUtil.Format(ScheduledJobErrorStrings.IOFailureOnSetJobDefinition, this._definition.Name);
						errorRecord = new ErrorRecord(new RuntimeException(str2, scheduledJobException), "IOFailureOnSetJobDefinition", ErrorCategory.InvalidOperation, this._definition);
					}
				}
				else
				{
					string str3 = StringUtil.Format(ScheduledJobErrorStrings.NoAccessOnSetJobDefinition, this._definition.Name);
					errorRecord = new ErrorRecord(new RuntimeException(str3, scheduledJobException), "NoAccessFailureOnSetJobDefinition", ErrorCategory.InvalidOperation, this._definition);
				}
				base.WriteError(errorRecord);
			}
			if (this._passThru)
			{
				base.WriteObject(this._definition);
			}
		}

		private void UpdateDefinition()
		{
			if (this._name != null && string.Compare(this._name, this._definition.Name, StringComparison.OrdinalIgnoreCase) != 0)
			{
				this._definition.RenameAndSave(this._name);
			}
			this.UpdateJobInvocationInfo();
			if (base.MyInvocation.BoundParameters.ContainsKey("MaxResultCount"))
			{
				this._definition.SetExecutionHistoryLength(this.MaxResultCount, false);
			}
			if (this.Credential != null)
			{
				this._definition.Credential = this.Credential;
			}
			if (this.Trigger != null)
			{
				this._definition.SetTriggers(this.Trigger, false);
			}
			if (this.ScheduledJobOption != null)
			{
				this._definition.UpdateOptions(this.ScheduledJobOption, false);
			}
		}

		private void UpdateExecutionDefinition()
		{
			if (this._clearExecutionHistory)
			{
				this._definition.ClearExecutionHistory();
			}
		}

		private void UpdateJobInvocationInfo()
		{
			string command;
			Dictionary<string, object> strs = this.UpdateParameters();
			string name = this._definition.Name;
			if (this.ScriptBlock == null)
			{
				if (this.FilePath == null)
				{
					command = this._definition.InvocationInfo.Command;
				}
				else
				{
					command = this.FilePath;
				}
			}
			else
			{
				command = this.ScriptBlock.ToString();
			}
			JobDefinition jobDefinition = new JobDefinition(typeof(ScheduledJobSourceAdapter), command, name);
			jobDefinition.ModuleName = "PSScheduledJob";
			JobInvocationInfo scheduledJobInvocationInfo = new ScheduledJobInvocationInfo(jobDefinition, strs);
			this._definition.UpdateJobInvocationInfo(scheduledJobInvocationInfo, false);
		}

		private Dictionary<string, object> UpdateParameters()
		{
			Dictionary<string, object> strs = new Dictionary<string, object>();
			foreach (CommandParameter item in this._definition.InvocationInfo.Parameters[0])
			{
				strs.Add(item.Name, item.Value);
			}
			if (base.MyInvocation.BoundParameters.ContainsKey("RunAs32"))
			{
				if (!strs.ContainsKey("RunAs32"))
				{
					SwitchParameter runAs32 = this.RunAs32;
					strs.Add("RunAs32", runAs32.ToBool());
				}
				else
				{
					SwitchParameter switchParameter = this.RunAs32;
					strs["RunAs32"] = switchParameter.ToBool();
				}
			}
			if (base.MyInvocation.BoundParameters.ContainsKey("Authentication"))
			{
				if (!strs.ContainsKey("Authentication"))
				{
					strs.Add("Authentication", this.Authentication);
				}
				else
				{
					strs["Authentication"] = this.Authentication;
				}
			}
			if (this.InitializationScript != null)
			{
				if (!strs.ContainsKey("InitializationScript"))
				{
					strs.Add("InitializationScript", this.InitializationScript);
				}
				else
				{
					strs["InitializationScript"] = this.InitializationScript;
				}
			}
			else
			{
				if (strs.ContainsKey("InitializationScript"))
				{
					strs.Remove("InitializationScript");
				}
			}
			if (this.ScriptBlock != null)
			{
				if (strs.ContainsKey("FilePath"))
				{
					strs.Remove("FilePath");
				}
				if (!strs.ContainsKey("ScriptBlock"))
				{
					strs.Add("ScriptBlock", this.ScriptBlock);
				}
				else
				{
					strs["ScriptBlock"] = this.ScriptBlock;
				}
			}
			if (this.FilePath != null)
			{
				if (strs.ContainsKey("ScriptBlock"))
				{
					strs.Remove("ScriptBlock");
				}
				if (!strs.ContainsKey("FilePath"))
				{
					strs.Add("FilePath", this.FilePath);
				}
				else
				{
					strs["FilePath"] = this.FilePath;
				}
			}
			if (this.ArgumentList != null)
			{
				if (!strs.ContainsKey("ArgumentList"))
				{
					strs.Add("ArgumentList", this.ArgumentList);
				}
				else
				{
					strs["ArgumentList"] = this.ArgumentList;
				}
			}
			else
			{
				if (strs.ContainsKey("ArgumentList") && (this.ScriptBlock != null || this.FilePath != null))
				{
					strs.Remove("ArgumentList");
				}
			}
			return strs;
		}
	}
}