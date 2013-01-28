using System;

namespace Microsoft.Data.Edm
{
	internal interface IEdmBinaryTypeReference : IEdmPrimitiveTypeReference, IEdmTypeReference, IEdmElement
	{
		bool? IsFixedLength
		{
			get;
		}

		bool IsUnbounded
		{
			get;
		}

		int? MaxLength
		{
			get;
		}

	}
}