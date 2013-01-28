using Microsoft.Data.Edm;
using System;

namespace Microsoft.Data.Edm.Values
{
	internal interface IEdmBooleanValue : IEdmPrimitiveValue, IEdmValue, IEdmElement
	{
		bool Value
		{
			get;
		}

	}
}