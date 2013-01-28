using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.PowerShell.ScheduledJob
{
	[Cmdlet("Remove", "JobTrigger", DefaultParameterSetName="JobDefinition", HelpUri="http://go.microsoft.com/fwlink/?LinkID=223914")]
	public sealed class RemoveJobTriggerCommand : ScheduleJobCmdletBase
	{
		private const string JobDefinitionParameterSet = "JobDefinition";

		private const string JobDefinitionIdParameterSet = "JobDefinitionId";

		private const string JobDefinitionNameParameterSet = "JobDefinitionName";

		private int[] _triggerIds;

		private int[] _definitionIds;

		private string[] _names;

		private ScheduledJobDefinition[] _definitions;

		[Parameter(Position=0, Mandatory=true, ParameterSetName="JobDefinitionId")]
		[ValidateNotNullOrEmpty]
		public int[] Id
		{
			get
			{
				return this._definitionIds;
			}
			set
			{
				this._definitionIds = value;
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

		[Parameter(ParameterSetName="JobDefinitionId")]
		[Parameter(ParameterSetName="JobDefinition")]
		[Parameter(ParameterSetName="JobDefinitionName")]
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

		public RemoveJobTriggerCommand()
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
					this.RemoveFromJobDefinition(this._definitions);
					return;
				}
				else
				{
					if (str == "JobDefinitionId")
					{
						this.RemoveFromJobDefinition(base.GetJobDefinitionsById(this._definitionIds, true));
						return;
					}
					else
					{
						if (str == "JobDefinitionName")
						{
							this.RemoveFromJobDefinition(base.GetJobDefinitionsByName(this._names, true));
						}
						else
						{
							return;
						}
					}
				}
			}
		}

		private void RemoveFromJobDefinition(IEnumerable<ScheduledJobDefinition> definitions)
		{
			List<int> nums = null;
			foreach (ScheduledJobDefinition scheduledJobDefinition in definitions)
			{
				nums = new List<int>();
				try
				{
					nums = scheduledJobDefinition.RemoveTriggers(this._triggerIds, true);
				}
				catch (ScheduledJobException scheduledJobException1)
				{
					ScheduledJobException scheduledJobException = scheduledJobException1;
					string str = StringUtil.Format(ScheduledJobErrorStrings.CantRemoveTriggersFromDefinition, scheduledJobDefinition.Name);
					Exception runtimeException = new RuntimeException(str, scheduledJobException);
					ErrorRecord errorRecord = new ErrorRecord(runtimeException, "CantRemoveTriggersFromScheduledJobDefinition", ErrorCategory.InvalidOperation, scheduledJobDefinition);
					base.WriteError(errorRecord);
				}
				List<int>.Enumerator enumerator = nums.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						int num = enumerator.Current;
						base.WriteTriggerNotFoundError(num, scheduledJobDefinition.Name, scheduledJobDefinition);
					}
				}
				finally
				{
					enumerator.Dispose();
				}
			}
		}
	}
}