using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.PowerShell.ScheduledJob
{
	[Cmdlet("Unregister", "ScheduledJob", SupportsShouldProcess=true, DefaultParameterSetName="Definition", HelpUri="http://go.microsoft.com/fwlink/?LinkID=223925")]
	public sealed class UnregisterScheduledJobCommand : ScheduleJobCmdletBase
	{
		private const string DefinitionIdParameterSet = "DefinitionId";

		private const string DefinitionNameParameterSet = "DefinitionName";

		private const string DefinitionParameterSet = "Definition";

		private int[] _definitionIds;

		private string[] _names;

		private ScheduledJobDefinition[] _definitions;

		private SwitchParameter _force;

		[Parameter(ParameterSetName="DefinitionName")]
		[Parameter(ParameterSetName="Definition")]
		[Parameter(ParameterSetName="DefinitionId")]
		public SwitchParameter Force
		{
			get
			{
				return this._force;
			}
			set
			{
				this._force = value;
			}
		}

		[Parameter(Position=0, Mandatory=true, ParameterSetName="DefinitionId")]
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

		[Parameter(Position=0, Mandatory=true, ValueFromPipeline=true, ParameterSetName="Definition")]
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

		[Parameter(Position=0, Mandatory=true, ParameterSetName="DefinitionName")]
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

		public UnregisterScheduledJobCommand()
		{
		}

		protected override void ProcessRecord()
		{
			List<ScheduledJobDefinition> scheduledJobDefinitions = null;
			string parameterSetName = base.ParameterSetName;
			string str = parameterSetName;
			if (parameterSetName != null)
			{
				if (str == "Definition")
				{
					scheduledJobDefinitions = new List<ScheduledJobDefinition>(this._definitions);
				}
				else
				{
					if (str == "DefinitionName")
					{
						scheduledJobDefinitions = base.GetJobDefinitionsByName(this._names, true);
					}
					else
					{
						if (str == "DefinitionId")
						{
							scheduledJobDefinitions = base.GetJobDefinitionsById(this._definitionIds, true);
						}
					}
				}
			}
			if (scheduledJobDefinitions != null)
			{
				foreach (ScheduledJobDefinition scheduledJobDefinition in scheduledJobDefinitions)
				{
					string str1 = StringUtil.Format(ScheduledJobErrorStrings.DefinitionWhatIf, scheduledJobDefinition.Name);
					if (!base.ShouldProcess(str1, "Unregister"))
					{
						continue;
					}
					try
					{
						scheduledJobDefinition.Remove(this._force);
					}
					catch (ScheduledJobException scheduledJobException1)
					{
						ScheduledJobException scheduledJobException = scheduledJobException1;
						string str2 = StringUtil.Format(ScheduledJobErrorStrings.CantUnregisterDefinition, scheduledJobDefinition.Name);
						Exception runtimeException = new RuntimeException(str2, scheduledJobException);
						ErrorRecord errorRecord = new ErrorRecord(runtimeException, "CantUnregisterScheduledJobDefinition", ErrorCategory.InvalidOperation, scheduledJobDefinition);
						base.WriteError(errorRecord);
					}
				}
			}
			if (this._names != null && (int)this._names.Length > 0 && (this._definitions == null || (int)this._definitions.Length < (int)this._names.Length))
			{
				using (ScheduledJobWTS scheduledJobWT = new ScheduledJobWTS())
				{
					string[] strArrays = this._names;
					for (int i = 0; i < (int)strArrays.Length; i++)
					{
						string str3 = strArrays[i];
						scheduledJobWT.RemoveTaskByName(str3, true, true);
					}
				}
			}
		}
	}
}