using Microsoft.Data.Edm;
using System;

namespace Microsoft.Data.Edm.Library
{
	internal class EdmEntityReferenceTypeReference : EdmTypeReference, IEdmEntityReferenceTypeReference, IEdmTypeReference, IEdmElement
	{
		public IEdmEntityReferenceType EntityReferenceDefinition
		{
			get
			{
				return (IEdmEntityReferenceType)base.Definition;
			}
		}

		public EdmEntityReferenceTypeReference(IEdmEntityReferenceType entityReferenceType, bool isNullable) : base(entityReferenceType, isNullable)
		{
		}
	}
}