using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.PowerShell.ScheduledJob
{
	[Cmdlet("Get", "JobTrigger", DefaultParameterSetName="JobDefinition", HelpUri="http://go.microsoft.com/fwlink/?LinkID=223915")]
	[OutputType(new Type[] { typeof(ScheduledJobTrigger) })]
	public sealed class GetJobTriggerCommand : ScheduleJobCmdletBase
	{
		private const string JobDefinitionParameterSet = "JobDefinition";

		private const string JobDefinitionIdParameterSet = "JobDefinitionId";

		private const string JobDefinitionNameParameterSet = "JobDefinitionName";

		private int[] _triggerIds;

		private ScheduledJobDefinition _definition;

		private int _definitionId;

		private string _name;

		[Parameter(Position=0, Mandatory=true, ParameterSetName="JobDefinitionId")]
		public int Id
		{
			get
			{
				return this._definitionId;
			}
			set
			{
				this._definitionId = value;
			}
		}

		[Parameter(Position=0, Mandatory=true, ValueFromPipeline=true, ParameterSetName="JobDefinition")]
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

		[Parameter(Position=0, Mandatory=true, ParameterSetName="JobDefinitionName")]
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

		[Parameter(Position=1, ParameterSetName="JobDefinition")]
		[Parameter(Position=1, ParameterSetName="JobDefinitionId")]
		[Parameter(Position=1, ParameterSetName="JobDefinitionName")]
		public int[] TriggerId
		{
			get
			{
				return this._triggerIds;
			}
			set
			{
				this._triggerIds = value;
			}
		}

		public GetJobTriggerCommand()
		{
		}

		protected override void ProcessRecord()
		{
			string parameterSetName = base.ParameterSetName;
			string str = parameterSetName;
			if (parameterSetName != null)
			{
				if (str == "JobDefinition")
				{
					this.WriteTriggers(this._definition);
					return;
				}
				else
				{
					if (str == "JobDefinitionId")
					{
						this.WriteTriggers(base.GetJobDefinitionById(this._definitionId, true));
						return;
					}
					else
					{
						if (str == "JobDefinitionName")
						{
							this.WriteTriggers(base.GetJobDefinitionByName(this._name, true));
						}
						else
						{
							return;
						}
					}
				}
			}
		}

		private void WriteTriggers(ScheduledJobDefinition definition)
		{
			List<int> nums = null;
			if (definition != null)
			{
				List<ScheduledJobTrigger> triggers = definition.GetTriggers(this._triggerIds, out nums);
				foreach (ScheduledJobTrigger trigger in triggers)
				{
					base.WriteObject(trigger);
				}
				foreach (int num in nums)
				{
					base.WriteTriggerNotFoundError(num, definition.Name, definition);
				}
				return;
			}
			else
			{
				return;
			}
		}
	}
}