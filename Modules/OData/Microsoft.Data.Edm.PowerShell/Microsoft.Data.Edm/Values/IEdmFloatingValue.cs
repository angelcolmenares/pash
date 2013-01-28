using Microsoft.Data.Edm;
using System;

namespace Microsoft.Data.Edm.Values
{
	internal interface IEdmFloatingValue : IEdmPrimitiveValue, IEdmValue, IEdmElement
	{
		double Value
		{
			get;
		}

	}
}