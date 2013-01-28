using System;

namespace Microsoft.Management.PowerShellWebAccess
{
	public class TestRuleInvalidRuleEventArgs : EventArgs
	{
		public Exception Exception
		{
			get;
			private set;
		}

		public PswaAuthorizationRule Rule
		{
			get;
			private set;
		}

		public TestRuleInvalidRuleEventArgs(PswaAuthorizationRule rule, Exception exception)
		{
			this.Rule = rule;
			this.Exception = exception;
		}
	}
}