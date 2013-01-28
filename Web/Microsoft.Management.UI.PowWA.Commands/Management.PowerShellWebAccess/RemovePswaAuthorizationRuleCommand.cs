using Microsoft.Management.PowerShellWebAccess.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.Management.PowerShellWebAccess
{
	[Cmdlet("Remove", "PswaAuthorizationRule", DefaultParameterSetName="ID", SupportsShouldProcess=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=254233")]
	public sealed class RemovePswaAuthorizationRuleCommand : PSCmdlet
	{
		private readonly int batchAmount;

		private int[] id;

		private PswaAuthorizationRule[] rule;

		private bool force;

		[Parameter]
		public SwitchParameter Force
		{
			get
			{
				return this.force;
			}
			set
			{
				this.force = value;
			}
		}

		[Parameter(Mandatory=true, Position=1, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true, ParameterSetName="ID")]
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

		[Parameter(Mandatory=true, Position=1, ValueFromPipeline=true, ParameterSetName="Rule")]
		public PswaAuthorizationRule[] Rule
		{
			get
			{
				return this.rule;
			}
			set
			{
				this.rule = value;
			}
		}

		public RemovePswaAuthorizationRuleCommand()
		{
			this.batchAmount = 5;
		}

		protected override void BeginProcessing()
		{
			PswaAuthorizationRuleManager instance = PswaAuthorizationRuleManager.Instance;
			instance.GetRuleByIdNotFound += new EventHandler<GetRuleByIdNotFoundEventArgs>(this.OnGetRuleByIdNotFound);
		}

		protected override void EndProcessing()
		{
			PswaAuthorizationRuleManager instance = PswaAuthorizationRuleManager.Instance;
			instance.GetRuleByIdNotFound -= new EventHandler<GetRuleByIdNotFoundEventArgs>(this.OnGetRuleByIdNotFound);
		}

		private void OnGetRuleByIdNotFound(object sender, GetRuleByIdNotFoundEventArgs e)
		{
			this.ReportRuleNotFoundById(e.Id);
		}

		protected override void ProcessRecord()
		{
			PswaAuthorizationRule[] rule;
			SortedList<int, PswaAuthorizationRule> nums = PswaAuthorizationRuleCommandHelper.LoadFromFile(this, "Remove");
			if (nums != null)
			{
				if (string.Compare(base.ParameterSetName, "ID", StringComparison.OrdinalIgnoreCase) != 0)
				{
					rule = this.Rule;
				}
				else
				{
					rule = PswaAuthorizationRuleManager.Instance.GetRule(nums, this.Id);
				}
				ArrayList arrayLists = new ArrayList();
				PswaAuthorizationRule[] pswaAuthorizationRuleArray = rule;
				for (int i = 0; i < (int)pswaAuthorizationRuleArray.Length; i++)
				{
					PswaAuthorizationRule pswaAuthorizationRule = pswaAuthorizationRuleArray[i];
					try
					{
						if (!this.Force)
						{
							object[] ruleName = new object[2];
							ruleName[0] = pswaAuthorizationRule.RuleName;
							ruleName[1] = pswaAuthorizationRule.Id;
							if (!base.ShouldProcess(string.Format(CultureInfo.CurrentCulture, Resources.AuthorizationRuleIdName_DisplayFormat, ruleName)))
							{
								continue;
							}
						}
						PswaAuthorizationRule pswaAuthorizationRule1 = PswaAuthorizationRuleManager.Instance.RemoveRule(nums, pswaAuthorizationRule.Id);
						if (pswaAuthorizationRule1 != null)
						{
							arrayLists.Add(pswaAuthorizationRule1);
							if (arrayLists.Count >= this.batchAmount)
							{
								this.SaveCurrentBatch(nums, arrayLists);
							}
						}
						else
						{
							this.ReportRuleNotFoundById(pswaAuthorizationRule.Id);
						}
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						this.SaveCurrentBatch(nums, arrayLists);
						base.WriteError(new ErrorRecord(exception, "RemoveRuleError", ErrorCategory.InvalidOperation, null));
					}
				}
				if (arrayLists.Count > 0)
				{
					this.SaveCurrentBatch(nums, arrayLists);
				}
				return;
			}
			else
			{
				return;
			}
		}

		private void ReportRuleNotFoundById(int id)
		{
			object[] objArray = new object[1];
			objArray[0] = id;
			base.WriteError(new ErrorRecord(new Exception(string.Format(CultureInfo.CurrentCulture, Resources.Rule_NotFoundById, objArray)), "RemoveRuleError", ErrorCategory.InvalidOperation, null));
		}

		private void SaveCurrentBatch(SortedList<int, PswaAuthorizationRule> ruleList, ArrayList removedRules)
		{
			PswaAuthorizationRuleManager.Instance.SaveToFile(ruleList);
			removedRules.RemoveRange(0, removedRules.Count);
		}
	}
}