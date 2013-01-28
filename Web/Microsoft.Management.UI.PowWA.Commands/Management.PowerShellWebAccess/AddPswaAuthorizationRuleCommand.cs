using Microsoft.Management.PowerShellWebAccess.Commands;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation;

namespace Microsoft.Management.PowerShellWebAccess
{
	[Cmdlet("Add", "PswaAuthorizationRule", HelpUri="http://go.microsoft.com/fwlink/?LinkID=254231")]
	public sealed class AddPswaAuthorizationRuleCommand : PSCmdlet
	{
		private readonly int batchAmount;

		private string computergroupname;

		private string computername;

		private string configurationname;

		private string rulename;

		private string[] usergroupname;

		private string[] username;

		private bool force;

		[Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="UserNameComputerGroupName")]
		[Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="UserGroupNameComputerGroupName")]
		public string ComputerGroupName
		{
			get
			{
				return this.computergroupname;
			}
			set
			{
				this.computergroupname = value;
			}
		}

		[Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="UserGroupNameComputerName")]
		[Parameter(Mandatory=true, Position=1, ValueFromPipelineByPropertyName=true, ParameterSetName="UserNameComputerName")]
		public string ComputerName
		{
			get
			{
				return this.computername;
			}
			set
			{
				this.computername = value;
			}
		}

		[Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="UserGroupNameComputerGroupName")]
		[Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="UserNameComputerGroupName")]
		[Parameter(Mandatory=true, Position=2, ValueFromPipelineByPropertyName=true, ParameterSetName="UserNameComputerName")]
		[Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="UserGroupNameComputerName")]
		public string ConfigurationName
		{
			get
			{
				return this.configurationname;
			}
			set
			{
				this.configurationname = value;
			}
		}

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

		[Parameter(ValueFromPipelineByPropertyName=true)]
		public string RuleName
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

		[Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="UserGroupNameComputerName")]
		[Parameter(Mandatory=true, ValueFromPipelineByPropertyName=true, ParameterSetName="UserGroupNameComputerGroupName")]
		public string[] UserGroupName
		{
			get
			{
				return this.usergroupname;
			}
			set
			{
				this.usergroupname = value;
			}
		}

		[Parameter(Mandatory=true, Position=0, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true, ParameterSetName="UserNameComputerGroupName")]
		[Parameter(Mandatory=true, Position=0, ValueFromPipeline=true, ValueFromPipelineByPropertyName=true, ParameterSetName="UserNameComputerName")]
		public string[] UserName
		{
			get
			{
				return this.username;
			}
			set
			{
				this.username = value;
			}
		}

		public AddPswaAuthorizationRuleCommand()
		{
			this.batchAmount = 5;
		}

		protected override void BeginProcessing()
		{
		}

		protected override void EndProcessing()
		{
		}

		protected override void ProcessRecord()
		{
			string str;
			PswaUserType pswaUserType;
			bool flag;
			bool flag1;
			bool flag2;
			string[] userName = new string[0];
			PswaUserType pswaUserType1 = PswaUserType.User;
			string empty = string.Empty;
			PswaDestinationType pswaDestinationType = PswaDestinationType.Computer;
			if (base.ParameterSetName == "UserNameComputerName" || base.ParameterSetName == "UserNameComputerGroupName")
			{
				userName = this.UserName;
				pswaUserType1 = PswaUserType.User;
				for (int i = 0; i < (int)userName.Length; i++)
				{
					userName[i] = PswaHelper.TranslateLocalAccountName(userName[i]);
				}
			}
			else
			{
				if (base.ParameterSetName == "UserGroupNameComputerName" || base.ParameterSetName == "UserGroupNameComputerGroupName")
				{
					userName = this.UserGroupName;
					pswaUserType1 = PswaUserType.UserGroup;
					for (int j = 0; j < (int)userName.Length; j++)
					{
						userName[j] = PswaHelper.TranslateLocalAccountName(userName[j]);
					}
				}
			}
			if (base.ParameterSetName == "UserNameComputerName" || base.ParameterSetName == "UserGroupNameComputerName")
			{
				empty = this.ComputerName;
				pswaDestinationType = PswaDestinationType.Computer;
			}
			else
			{
				if (base.ParameterSetName == "UserNameComputerGroupName" || base.ParameterSetName == "UserGroupNameComputerGroupName")
				{
					empty = PswaHelper.TranslateLocalAccountName(this.ComputerGroupName);
					pswaDestinationType = PswaDestinationType.ComputerGroup;
				}
			}
			if (empty == "*")
			{
				pswaDestinationType = PswaDestinationType.All;
			}
			SortedList<int, PswaAuthorizationRule> nums = PswaAuthorizationRuleCommandHelper.LoadFromFile(this, "Add");
			if (nums != null)
			{
				str = null;
				try
				{
					if (pswaDestinationType == PswaDestinationType.Computer)
					{
						string str1 = PswaAuthorizationRuleManager.Instance.TryParseDestinationIpAddress(empty);
						if (str1 == null)
						{
							if (!PswaAuthorizationRuleManager.Instance.IsCurrentComputerDomainJoined())
							{
								str = empty;
							}
							else
							{
								string str2 = null;
								PswaAuthorizationRuleManager.Instance.GetComputerFqdnAndSid(empty, out str2, out str);
								if (str != null)
								{
									if (string.Compare(str2, empty, StringComparison.OrdinalIgnoreCase) != 0)
									{
										if (this.Force)
										{
											flag = true;
										}
										else
										{
											object[] objArray = new object[1];
											objArray[0] = str2;
											flag = base.ShouldContinue(string.Format(CultureInfo.CurrentCulture, Resources.AuthorizationRule_UseFqdnQuery, objArray), "");
										}
										bool flag3 = flag;
										if (flag3)
										{
											empty = str2;
										}
										else
										{
											return;
										}
									}
								}
								else
								{
									if (this.Force)
									{
										flag1 = true;
									}
									else
									{
										object[] objArray1 = new object[1];
										objArray1[0] = empty;
										flag1 = base.ShouldContinue(string.Format(CultureInfo.CurrentCulture, Resources.AuthorizationRule_ForceComputerNameQuery, objArray1), "");
									}
									bool flag4 = flag1;
									if (flag4)
									{
										str = empty;
									}
									else
									{
										return;
									}
								}
							}
						}
						else
						{
							empty = str1;
							if (this.Force)
							{
								flag2 = true;
							}
							else
							{
								object[] objArray2 = new object[1];
								objArray2[0] = empty;
								flag2 = base.ShouldContinue(string.Format(CultureInfo.CurrentCulture, Resources.AuthorizationRule_UseIpAddressQuery, objArray2), "");
							}
							bool flag5 = flag2;
							if (flag5)
							{
								str = empty;
							}
							else
							{
								return;
							}
						}
					}
					goto Label0;
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					base.WriteError(new ErrorRecord(exception, "AddRuleError", ErrorCategory.InvalidOperation, null));
				}
				return;
			}
			else
			{
				return;
			}
		Label0:
			ArrayList arrayLists = new ArrayList();
			string[] strArrays = userName;
			int num = 0;
			while (num < (int)strArrays.Length)
			{
				string str3 = strArrays[num];
				try
				{
					PswaAuthorizationRuleManager instance = PswaAuthorizationRuleManager.Instance;
					SortedList<int, PswaAuthorizationRule> nums1 = nums;
					string ruleName = this.RuleName;
					string str4 = str3;
					if (str3 == "*")
					{
						pswaUserType = PswaUserType.All;
					}
					else
					{
						pswaUserType = pswaUserType1;
					}
					PswaAuthorizationRule pswaAuthorizationRule = instance.AddRule(nums1, ruleName, str4, pswaUserType, empty, pswaDestinationType, this.ConfigurationName, str);
					arrayLists.Add(pswaAuthorizationRule);
					if (arrayLists.Count >= this.batchAmount)
					{
						this.SaveCurrentBatch(nums, arrayLists);
					}
				}
				catch (Exception exception3)
				{
					Exception exception2 = exception3;
					this.SaveCurrentBatch(nums, arrayLists);
					base.WriteError(new ErrorRecord(exception2, "AddRuleError", ErrorCategory.InvalidOperation, null));
				}
				num++;
			}
			if (arrayLists.Count > 0)
			{
				this.SaveCurrentBatch(nums, arrayLists);
				return;
			}
			else
			{
				return;
			}
		}

		private void SaveCurrentBatch(SortedList<int, PswaAuthorizationRule> ruleList, ArrayList addedRules)
		{
			PswaAuthorizationRuleManager.Instance.SaveToFile(ruleList);
			foreach (PswaAuthorizationRule addedRule in addedRules)
			{
				base.WriteObject(addedRule);
			}
			addedRules.RemoveRange(0, addedRules.Count);
		}
	}
}