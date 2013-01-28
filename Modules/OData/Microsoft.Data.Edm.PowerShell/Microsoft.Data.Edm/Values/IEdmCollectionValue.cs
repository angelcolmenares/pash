using Microsoft.Data.Edm;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Values
{
	internal interface IEdmCollectionValue : IEdmValue, IEdmElement
	{
		IEnumerable<IEdmDelayedValue> Elements
		{
			get;
		}

	}
}