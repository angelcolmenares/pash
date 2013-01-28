using System;
using System.Collections.Generic;

namespace Microsoft.Management.Odata.Core
{
	internal interface IReferencedResourceSet
	{
		List<DSResource> Get(Dictionary<string, object> parameters);
	}
}