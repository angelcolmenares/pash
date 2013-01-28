using Microsoft.Data.Edm;
using System;

namespace Microsoft.Data.Edm.Values
{
	internal interface IEdmTimeValue : IEdmPrimitiveValue, IEdmValue, IEdmElement
	{
		TimeSpan Value
		{
			get;
		}

	}
}