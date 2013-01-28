using Microsoft.Data.Edm;
using System;

namespace Microsoft.Data.Edm.Library
{
	internal class EdmEnumTypeReference : EdmTypeReference, IEdmEnumTypeReference, IEdmTypeReference, IEdmElement
	{
		public EdmEnumTypeReference(IEdmEnumType enumType, bool isNullable) : base(enumType, isNullable)
		{
		}
	}
}