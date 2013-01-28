using Microsoft.Data.Edm;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Values
{
	internal interface IEdmStructuredValue : IEdmValue, IEdmElement
	{
		IEnumerable<IEdmPropertyValue> PropertyValues
		{
			get;
		}

		IEdmPropertyValue FindPropertyValue(string propertyName);
	}
}