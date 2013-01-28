using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.ScheduledJob
{
	[Cmdlet("Get", "ScheduledJobOption", DefaultParameterSetName="JobDefinition", HelpUri="http://go.microsoft.com/fwlink/?LinkID=223920")]
	[OutputType(new Type[] { typeof(ScheduledJobOptions) })]
	public sealed class GetScheduledJobOptionCommand : ScheduleJobCmdletBase
	{
		private const string JobDefinitionParameterSet = "JobDefinition";

		private const string JobDefinitionIdParameterSet = "JobDefinitionId";

		private const string JobDefinitionNameParameterSet = "JobDefinitionName";

		private int _id;

		private string _name;

		private ScheduledJobDefinition _definition;

		[Parameter(Position=0, Mandatory=true, ParameterSetName="JobDefinitionId")]
		public int Id
		{
			get
			{
				return this._id;
			}
			set
			{
				this._id = value;
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

		[Parameter(Position=0, Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="JobDefinitionName")]
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

		public GetScheduledJobOptionCommand()
		{
		}

		protected override void ProcessRecord()
		{
			ScheduledJobDefinition jobDefinitionById = null;
			string parameterSetName = base.ParameterSetName;
			string str = parameterSetName;
			if (parameterSetName != null)
			{
				if (str == "JobDefinition")
				{
					jobDefinitionById = this._definition;
				}
				else
				{
					if (str == "JobDefinitionId")
					{
						jobDefinitionById = base.GetJobDefinitionById(this._id, true);
					}
					else
					{
						if (str == "JobDefinitionName")
						{
							jobDefinitionById = base.GetJobDefinitionByName(this._name, true);
						}
					}
				}
			}
			if (jobDefinitionById != null)
			{
				base.WriteObject(jobDefinitionById.Options);
			}
		}
	}
}