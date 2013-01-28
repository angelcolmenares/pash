using Microsoft.Data.Edm;
using System;

namespace Microsoft.Data.Edm.Values
{
	internal interface IEdmStringValue : IEdmPrimitiveValue, IEdmValue, IEdmElement
	{
		string Value
		{
			get;
		}

	}
}