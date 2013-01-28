using System;
using System.Collections.Generic;

namespace Microsoft.Management.Odata.Core
{
	internal class TestHookCommandInvocationData
	{
		public CommandType CommandType
		{
			get;
			set;
		}

		public Dictionary<string, object> Parameters
		{
			get;
			set;
		}

		public TestHookCommandInvocationData()
		{
		}
	}
}