using Microsoft.Data.Edm;
using System;

namespace Microsoft.Data.Edm.Values
{
	internal interface IEdmIntegerValue : IEdmPrimitiveValue, IEdmValue, IEdmElement
	{
		long Value
		{
			get;
		}

	}
}