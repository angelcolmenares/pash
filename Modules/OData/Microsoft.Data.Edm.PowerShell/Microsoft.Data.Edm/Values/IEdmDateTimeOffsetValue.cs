using Microsoft.Data.Edm;
using System;

namespace Microsoft.Data.Edm.Values
{
	internal interface IEdmDateTimeOffsetValue : IEdmPrimitiveValue, IEdmValue, IEdmElement
	{
		DateTimeOffset Value
		{
			get;
		}

	}
}