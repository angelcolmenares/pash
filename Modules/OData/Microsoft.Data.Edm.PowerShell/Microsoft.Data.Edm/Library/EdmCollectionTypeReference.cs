using Microsoft.Data.Edm;
using System;

namespace Microsoft.Data.Edm.Library
{
	internal class EdmCollectionTypeReference : EdmTypeReference, IEdmCollectionTypeReference, IEdmTypeReference, IEdmElement
	{
		public EdmCollectionTypeReference(IEdmCollectionType collectionType, bool isNullable) : base(collectionType, isNullable)
		{
		}
	}
}