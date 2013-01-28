using Microsoft.Data.Edm;
using System;

namespace Microsoft.Data.Edm.Values
{
	internal interface IEdmBinaryValue : IEdmPrimitiveValue, IEdmValue, IEdmElement
	{
		byte[] Value
		{
			get;
		}

	}
}