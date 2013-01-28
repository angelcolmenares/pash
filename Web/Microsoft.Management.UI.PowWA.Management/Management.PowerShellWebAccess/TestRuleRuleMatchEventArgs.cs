using System;

namespace Microsoft.Management.PowerShellWebAccess
{
	internal class TestRuleRuleMatchEventArgs : EventArgs
	{
		public PswaAuthorizationRule Rule
		{
			get;
			private set;
		}

		public TestRuleRuleMatchEventArgs(PswaAuthorizationRule rule)
		{
			this.Rule = rule;
		}
	}
}