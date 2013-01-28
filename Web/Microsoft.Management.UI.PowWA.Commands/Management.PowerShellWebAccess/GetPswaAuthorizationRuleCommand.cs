using Microsoft.Management.PowerShellWebAccess.Commands;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.Management.PowerShellWebAccess
{
	[Cmdlet("Get", "PswaAuthorizationRule", DefaultParameterSetName="ID", HelpUri="http://go.microsoft.com/fwlink/?LinkID=254232")]
	public sealed class GetPswaAuthorizationRuleCommand : PSCmdlet
	{
		private int[] id;

		private string[] rulename;

		[Parameter(Position=1, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true, ParameterSetName="ID")]
		public int[] Id
		{
			get
			{
				return this.id;
			}
			set
			{
				this.id = value;
			}
		}

		[Parameter(Mandatory=true, Position=1, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true, ParameterSetName="Name")]
		public string[] RuleName
		{
			get
			{
				return this.rulename;
			}
			set
			{
				this.rulename = value;
			}
		}

		public GetPswaAuthorizationRuleCommand()
		{
		}

		protected override void BeginProcessing()
		{
			PswaAuthorizationRuleManager instance = PswaAuthorizationRuleManager.Instance;
			instance.GetRuleByIdNotFound += new EventHandler<GetRuleByIdNotFoundEventArgs>(this.OnGetRuleByIdNotFound);
			PswaAuthorizationRuleManager pswaAuthorizationRuleManager = PswaAuthorizationRuleManager.Instance;
			pswaAuthorizationRuleManager.GetRuleByNameNotFound += new EventHandler<GetRuleByNameNotFoundEventArgs>(this.OnGetRuleByNameNotFound);
		}

		protected override void EndProcessing()
		{
			PswaAuthorizationRuleManager instance = PswaAuthorizationRuleManager.Instance;
			instance.GetRuleByIdNotFound -= new EventHandler<GetRuleByIdNotFoundEventArgs>(this.OnGetRuleByIdNotFound);
			PswaAuthorizationRuleManager pswaAuthorizationRuleManager = PswaAuthorizationRuleManager.Instance;
			pswaAuthorizationRuleManager.GetRuleByNameNotFound -= new EventHandler<GetRuleByNameNotFoundEventArgs>(this.OnGetRuleByNameNotFound);
		}

		private void OnGetRuleByIdNotFound(object sender, GetRuleByIdNotFoundEventArgs e)
		{
			object[] id = new object[1];
			id[0] = e.Id;
			base.WriteError(new ErrorRecord(new Exception(string.Format(CultureInfo.CurrentCulture, Resources.Rule_NotFoundById, id)), "GetRuleError", ErrorCategory.InvalidOperation, null));
		}

		private void OnGetRuleByNameNotFound(object sender, GetRuleByNameNotFoundEventArgs e)
		{
			object[] name = new object[1];
			name[0] = e.Name;
			base.WriteError(new ErrorRecord(new Exception(string.Format(CultureInfo.CurrentCulture, Resources.Rule_NotFoundByName, name)), "GetRuleError", ErrorCategory.InvalidOperation, null));
		}

		protected override void ProcessRecord()
		{
			SortedList<int, PswaAuthorizationRule> nums = PswaAuthorizationRuleCommandHelper.LoadFromFile(this, "Get");
			if (nums != null)
			{
				PswaAuthorizationRule[] rule = null;
				try
				{
					string parameterSetName = base.ParameterSetName;
					string str = parameterSetName;
					if (parameterSetName != null)
					{
						if (str == "ID")
						{
							rule = PswaAuthorizationRuleManager.Instance.GetRule(nums, this.Id);
						}
						else
						{
							if (str == "Name")
							{
								rule = PswaAuthorizationRuleManager.Instance.GetRule(nums, this.RuleName);
							}
						}
					}
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					base.WriteError(new ErrorRecord(exception, "GetRuleError", ErrorCategory.InvalidOperation, null));
				}
				if (rule != null)
				{
					base.WriteObject(rule, true);
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