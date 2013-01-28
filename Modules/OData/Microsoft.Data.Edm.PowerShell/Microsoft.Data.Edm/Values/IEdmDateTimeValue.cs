using Microsoft.Data.Edm;
using System;

namespace Microsoft.Data.Edm.Values
{
	internal interface IEdmDateTimeValue : IEdmPrimitiveValue, IEdmValue, IEdmElement
	{
		DateTime Value
		{
			get;
		}

	}
}