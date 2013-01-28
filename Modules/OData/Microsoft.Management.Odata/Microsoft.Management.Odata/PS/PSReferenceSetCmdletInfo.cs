using System;
using System.Collections.Generic;

namespace Microsoft.Management.Odata.PS
{
	internal class PSReferenceSetCmdletInfo : PSCmdletInfo
	{
		public Dictionary<string, string> ReferredObjectParameterMapping
		{
			get;
			private set;
		}

		public Dictionary<string, string> ReferringObjectParameterMapping
		{
			get;
			private set;
		}

		public PSReferenceSetCmdletInfo(string cmdletName) : base(cmdletName)
		{
			this.ReferringObjectParameterMapping = new Dictionary<string, string>();
			this.ReferredObjectParameterMapping = new Dictionary<string, string>();
		}
	}
}