using Microsoft.Management.PowerShellWebAccess.Commands;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management.Automation;

namespace Microsoft.Management.PowerShellWebAccess
{
	[Cmdlet("Test", "PswaAuthorizationRule", DefaultParameterSetName="ComputerName", HelpUri="http://go.microsoft.com/fwlink/?LinkID=254234")]
	public sealed class TestPswaAuthorizationRuleCommand : PSCmdlet
	{
		private string computername;

		private string configurationname;

		private Uri connectionuri;

		private PswaAuthorizationRule[] rule;

		private string username;

		private int matches;

		private int warnings;

		[Parameter(Mandatory=true, Position=1, ParameterSetName="ComputerName")]
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

		[Parameter(Position=2)]
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

		[Parameter(Mandatory=true, Position=1, ParameterSetName="ConnectionUri")]
		public Uri ConnectionUri
		{
			get
			{
				return this.connectionuri;
			}
			set
			{
				this.connectionuri = value;
			}
		}

		[Parameter(ValueFromPipeline=true)]
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

		[Parameter(Mandatory=true, Position=0)]
		public string UserName
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

		public TestPswaAuthorizationRuleCommand()
		{
		}

		protected override void BeginProcessing()
		{
			PswaAuthorizationRuleManager.Instance.TestRuleRuleMatch += new EventHandler<TestRuleRuleMatchEventArgs>(this.OnRuleMatch);
			PswaAuthorizationRuleManager instance = PswaAuthorizationRuleManager.Instance;
			instance.TestRuleInvalidRule += new EventHandler<TestRuleInvalidRuleEventArgs>(this.OnInvalidRule);
		}

		protected override void EndProcessing()
		{
			PswaAuthorizationRuleManager.Instance.TestRuleRuleMatch -= new EventHandler<TestRuleRuleMatchEventArgs>(this.OnRuleMatch);
			PswaAuthorizationRuleManager instance = PswaAuthorizationRuleManager.Instance;
			instance.TestRuleInvalidRule -= new EventHandler<TestRuleInvalidRuleEventArgs>(this.OnInvalidRule);
		}

		private void OnInvalidRule(object sender, TestRuleInvalidRuleEventArgs e)
		{
			object[] ruleName = new object[3];
			ruleName[0] = e.Rule.RuleName;
			ruleName[1] = e.Rule.Id;
			ruleName[2] = e.Exception.Message;
			base.WriteWarning(string.Format(CultureInfo.CurrentCulture, Resources.TestRule_Warning, ruleName));
			TestPswaAuthorizationRuleCommand testPswaAuthorizationRuleCommand = this;
			testPswaAuthorizationRuleCommand.warnings = testPswaAuthorizationRuleCommand.warnings + 1;
		}

		private void OnRuleMatch(object sender, TestRuleRuleMatchEventArgs e)
		{
			base.WriteObject(e.Rule);
			TestPswaAuthorizationRuleCommand testPswaAuthorizationRuleCommand = this;
			testPswaAuthorizationRuleCommand.matches = testPswaAuthorizationRuleCommand.matches + 1;
		}

		protected override void ProcessRecord()
		{
			this.UserName = PswaHelper.TranslateLocalAccountName(this.UserName);
			PswaAuthorizationRule[] rule = this.Rule;
			if (rule == null)
			{
				SortedList<int, PswaAuthorizationRule> nums = PswaAuthorizationRuleCommandHelper.LoadFromFile(this, "Test");
				if (nums != null)
				{
					rule = nums.Values.ToArray<PswaAuthorizationRule>();
				}
				else
				{
					return;
				}
			}
			MatchingWildcard matchingWildcard = MatchingWildcard.None;
			if (this.UserName == "*")
			{
				matchingWildcard = matchingWildcard | MatchingWildcard.User;
			}
			if (this.ConfigurationName == "*")
			{
				matchingWildcard = matchingWildcard | MatchingWildcard.Configuration;
			}
			try
			{
				if (base.ParameterSetName != "ComputerName")
				{
					if (base.ParameterSetName == "ConnectionUri")
					{
						PswaAuthorizationRuleManager.Instance.TestRule(rule, this.UserName, this.ConnectionUri, this.ConfigurationName, true, matchingWildcard);
					}
				}
				else
				{
					if (this.ComputerName == "*")
					{
						matchingWildcard = matchingWildcard | MatchingWildcard.Destination;
					}
					PswaAuthorizationRuleManager.Instance.TestRule(rule, this.UserName, this.ComputerName, this.ConfigurationName, true, matchingWildcard);
				}
				if (this.matches == 0 && this.warnings > 0)
				{
					throw new Exception(Resources.TestRule_NoMatchWithWarnings);
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				base.WriteError(new ErrorRecord(exception, "TestRuleError", ErrorCategory.InvalidOperation, null));
			}
		}
	}
}