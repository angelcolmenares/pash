using System;

namespace Microsoft.Data.Edm
{
	internal interface IEdmDecimalTypeReference : IEdmPrimitiveTypeReference, IEdmTypeReference, IEdmElement
	{
		int? Precision
		{
			get;
		}

		int? Scale
		{
			get;
		}

	}
}