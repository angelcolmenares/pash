using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.ScheduledJob
{
	[Cmdlet("Get", "ScheduledJob", DefaultParameterSetName="DefinitionId", HelpUri="http://go.microsoft.com/fwlink/?LinkID=223923")]
	[OutputType(new Type[] { typeof(ScheduledJobDefinition) })]
	public sealed class GetScheduledJobCommand : ScheduleJobCmdletBase
	{
		private const string DefinitionIdParameterSet = "DefinitionId";

		private const string DefinitionNameParameterSet = "DefinitionName";

		private int[] _definitionIds;

		private string[] _definitionNames;

		[Parameter(Position=0, ParameterSetName="DefinitionId")]
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

		[Parameter(Position=0, Mandatory=true, ParameterSetName="DefinitionName")]
		[ValidateNotNullOrEmpty]
		public string[] Name
		{
			get
			{
				return this._definitionNames;
			}
			set
			{
				this._definitionNames = value;
			}
		}

		public GetScheduledJobCommand()
		{
		}

		protected override void ProcessRecord()
		{
			Action<ScheduledJobDefinition> action = null;
			Action<ScheduledJobDefinition> action1 = null;
			Action<ScheduledJobDefinition> action2 = null;
			string parameterSetName = base.ParameterSetName;
			string str = parameterSetName;
			if (parameterSetName != null)
			{
				if (str == "DefinitionId")
				{
					if (this._definitionIds != null)
					{
						GetScheduledJobCommand getScheduledJobCommand = this;
						int[] numArray = this._definitionIds;
						if (action1 == null)
						{
							action1 = (ScheduledJobDefinition definition) => base.WriteObject(definition);
						}
						getScheduledJobCommand.FindJobDefinitionsById(numArray, action1, true);
						return;
					}
					else
					{
						GetScheduledJobCommand getScheduledJobCommand1 = this;
						if (action == null)
						{
							action = (ScheduledJobDefinition definition) => base.WriteObject(definition);
						}
						getScheduledJobCommand1.FindAllJobDefinitions(action);
						return;
					}
				}
				else
				{
					if (str == "DefinitionName")
					{
						GetScheduledJobCommand getScheduledJobCommand2 = this;
						string[] strArrays = this._definitionNames;
						if (action2 == null)
						{
							action2 = (ScheduledJobDefinition definition) => base.WriteObject(definition);
						}
						getScheduledJobCommand2.FindJobDefinitionsByName(strArrays, action2, true);
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