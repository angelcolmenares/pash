using Microsoft.Data.Edm;
using System;

namespace Microsoft.Data.Edm.Library
{
	internal class EdmEntityTypeReference : EdmTypeReference, IEdmEntityTypeReference, IEdmStructuredTypeReference, IEdmTypeReference, IEdmElement
	{
		public EdmEntityTypeReference(IEdmEntityType entityType, bool isNullable) : base(entityType, isNullable)
		{
		}
	}
}