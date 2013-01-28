using System;

namespace Microsoft.Data.Edm
{
	internal interface IEdmStringTypeReference : IEdmPrimitiveTypeReference, IEdmTypeReference, IEdmElement
	{
		string Collation
		{
			get;
		}

		bool? IsFixedLength
		{
			get;
		}

		bool IsUnbounded
		{
			get;
		}

		bool? IsUnicode
		{
			get;
		}

		int? MaxLength
		{
			get;
		}

	}
}