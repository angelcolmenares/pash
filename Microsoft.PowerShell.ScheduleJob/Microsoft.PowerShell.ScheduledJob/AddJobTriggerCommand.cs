using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.PowerShell.ScheduledJob
{
	[Cmdlet("Add", "JobTrigger", DefaultParameterSetName="JobDefinition", HelpUri="http://go.microsoft.com/fwlink/?LinkID=223913")]
	public sealed class AddJobTriggerCommand : ScheduleJobCmdletBase
	{
		private const string JobDefinitionParameterSet = "JobDefinition";

		private const string JobDefinitionIdParameterSet = "JobDefinitionId";

		private const string JobDefinitionNameParameterSet = "JobDefinitionName";

		private ScheduledJobTrigger[] _triggers;

		private int[] _ids;

		private string[] _names;

		private ScheduledJobDefinition[] _definitions;

		[Parameter(Position=0, Mandatory=true, ParameterSetName="JobDefinitionId")]
		[ValidateNotNullOrEmpty]
		public int[] Id
		{
			get
			{
				return this._ids;
			}
			set
			{
				this._ids = value;
			}
		}

		[Parameter(Position=0, Mandatory=true, ValueFromPipeline=true, ParameterSetName="JobDefinition")]
		[ValidateNotNullOrEmpty]
		public ScheduledJobDefinition[] InputObject
		{
			get
			{
				return this._definitions;
			}
			set
			{
				this._definitions = value;
			}
		}

		[Parameter(Position=0, Mandatory=true, ParameterSetName="JobDefinitionName")]
		[ValidateNotNullOrEmpty]
		public string[] Name
		{
			get
			{
				return this._names;
			}
			set
			{
				this._names = value;
			}
		}

		[Parameter(Position=1, Mandatory=true, ValueFromPipeline=true, ParameterSetName="JobDefinition")]
		[Parameter(Position=1, Mandatory=true, ValueFromPipeline=true, ParameterSetName="JobDefinitionId")]
		[Parameter(Position=1, Mandatory=true, ValueFromPipeline=true, ParameterSetName="JobDefinitionName")]
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

		public AddJobTriggerCommand()
		{
		}

		private void AddToJobDefinition(IEnumerable<ScheduledJobDefinition> jobDefinitions)
		{
			foreach (ScheduledJobDefinition jobDefinition in jobDefinitions)
			{
				try
				{
					jobDefinition.AddTriggers(this._triggers, true);
				}
				catch (ScheduledJobException scheduledJobException1)
				{
					ScheduledJobException scheduledJobException = scheduledJobException1;
					string str = StringUtil.Format(ScheduledJobErrorStrings.CantAddJobTriggersToDefinition, jobDefinition.Name);
					Exception runtimeException = new RuntimeException(str, scheduledJobException);
					ErrorRecord errorRecord = new ErrorRecord(runtimeException, "CantAddJobTriggersToScheduledJobDefinition", ErrorCategory.InvalidOperation, jobDefinition);
					base.WriteError(errorRecord);
				}
			}
		}

		protected override void ProcessRecord()
		{
			string parameterSetName = base.ParameterSetName;
			string str = parameterSetName;
			if (parameterSetName != null)
			{
				if (str == "JobDefinition")
				{
					this.AddToJobDefinition(this._definitions);
					return;
				}
				else
				{
					if (str == "JobDefinitionId")
					{
						this.AddToJobDefinition(base.GetJobDefinitionsById(this._ids, true));
						return;
					}
					else
					{
						if (str == "JobDefinitionName")
						{
							this.AddToJobDefinition(base.GetJobDefinitionsByName(this._names, true));
						}
						else
						{
							return;
						}
					}
				}
			}
		}
	}
}