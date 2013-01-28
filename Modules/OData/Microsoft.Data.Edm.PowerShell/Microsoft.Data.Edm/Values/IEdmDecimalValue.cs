using Microsoft.Data.Edm;
using System;

namespace Microsoft.Data.Edm.Values
{
	internal interface IEdmDecimalValue : IEdmPrimitiveValue, IEdmValue, IEdmElement
	{
		decimal Value
		{
			get;
		}

	}
}