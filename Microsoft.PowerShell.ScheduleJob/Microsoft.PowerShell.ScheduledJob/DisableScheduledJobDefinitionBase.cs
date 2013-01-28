using System;
using System.Management.Automation;

namespace Microsoft.PowerShell.ScheduledJob
{
	public abstract class DisableScheduledJobDefinitionBase : ScheduleJobCmdletBase
	{
		protected const string DefinitionIdParameterSet = "DefinitionId";

		protected const string DefinitionNameParameterSet = "DefinitionName";

		protected const string DefinitionParameterSet = "Definition";

		private ScheduledJobDefinition _definition;

		private int _definitionId;

		private string _definitionName;

		private SwitchParameter _passThru;

		protected abstract bool Enabled
		{
			get;
		}

		[Parameter(Position=0, Mandatory=true, ParameterSetName="DefinitionId")]
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

		[Parameter(Position=0, Mandatory=true, ValueFromPipeline=true, ParameterSetName="Definition")]
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

		[Parameter(Position=0, Mandatory=true, ParameterSetName="DefinitionName")]
		[ValidateNotNullOrEmpty]
		public string Name
		{
			get
			{
				return this._definitionName;
			}
			set
			{
				this._definitionName = value;
			}
		}

		[Parameter(ParameterSetName="DefinitionId")]
		[Parameter(ParameterSetName="DefinitionName")]
		[Parameter(ParameterSetName="Definition")]
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

		protected DisableScheduledJobDefinitionBase()
		{
		}

		protected override void ProcessRecord()
		{
			string str;
			ScheduledJobDefinition jobDefinitionById = null;
			string parameterSetName = base.ParameterSetName;
			string str1 = parameterSetName;
			if (parameterSetName != null)
			{
				if (str1 == "Definition")
				{
					jobDefinitionById = this._definition;
				}
				else
				{
					if (str1 == "DefinitionId")
					{
						jobDefinitionById = base.GetJobDefinitionById(this._definitionId, true);
					}
					else
					{
						if (str1 == "DefinitionName")
						{
							jobDefinitionById = base.GetJobDefinitionByName(this._definitionName, true);
						}
					}
				}
			}
			if (this.Enabled)
			{
				str = "Enable";
			}
			else
			{
				str = "Disable";
			}
			string str2 = str;
			if (jobDefinitionById != null && base.ShouldProcess(jobDefinitionById.Name, str2))
			{
				try
				{
					jobDefinitionById.SetEnabled(this.Enabled, true);
				}
				catch (ScheduledJobException scheduledJobException1)
				{
					ScheduledJobException scheduledJobException = scheduledJobException1;
					string str3 = StringUtil.Format(ScheduledJobErrorStrings.CantSetEnableOnJobDefinition, jobDefinitionById.Name);
					Exception runtimeException = new RuntimeException(str3, scheduledJobException);
					ErrorRecord errorRecord = new ErrorRecord(runtimeException, "CantSetEnableOnScheduledJobDefinition", ErrorCategory.InvalidOperation, jobDefinitionById);
					base.WriteError(errorRecord);
				}
				if (this._passThru)
				{
					base.WriteObject(jobDefinitionById);
				}
			}
		}
	}
}