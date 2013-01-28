using Microsoft.Data.Edm;
using System;

namespace Microsoft.Data.Edm.Library
{
	internal class EdmPrimitiveTypeReference : EdmTypeReference, IEdmPrimitiveTypeReference, IEdmTypeReference, IEdmElement
	{
		public EdmPrimitiveTypeReference(IEdmPrimitiveType definition, bool isNullable) : base(definition, isNullable)
		{
		}
	}
}