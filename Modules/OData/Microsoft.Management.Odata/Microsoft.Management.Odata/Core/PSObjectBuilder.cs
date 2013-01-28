using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;

namespace Microsoft.Management.Odata.Core
{
	internal static class PSObjectBuilder
	{
		public static PSObject Build(Dictionary<string, object> properties)
		{
			return new PSObject(new Hashtable(properties));
		}
	}
}