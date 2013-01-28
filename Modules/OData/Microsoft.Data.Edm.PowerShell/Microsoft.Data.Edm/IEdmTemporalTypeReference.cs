using System;

namespace Microsoft.Data.Edm
{
	internal interface IEdmTemporalTypeReference : IEdmPrimitiveTypeReference, IEdmTypeReference, IEdmElement
	{
		int? Precision
		{
			get;
		}

	}
}