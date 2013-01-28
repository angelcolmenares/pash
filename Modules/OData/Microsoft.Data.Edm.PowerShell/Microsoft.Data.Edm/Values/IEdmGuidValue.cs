using Microsoft.Data.Edm;
using System;

namespace Microsoft.Data.Edm.Values
{
	internal interface IEdmGuidValue : IEdmPrimitiveValue, IEdmValue, IEdmElement
	{
		Guid Value
		{
			get;
		}

	}
}