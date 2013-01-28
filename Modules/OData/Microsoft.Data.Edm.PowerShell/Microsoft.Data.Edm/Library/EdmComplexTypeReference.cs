using Microsoft.Data.Edm;
using System;

namespace Microsoft.Data.Edm.Library
{
	internal class EdmComplexTypeReference : EdmTypeReference, IEdmComplexTypeReference, IEdmStructuredTypeReference, IEdmTypeReference, IEdmElement
	{
		public EdmComplexTypeReference(IEdmComplexType complexType, bool isNullable) : base(complexType, isNullable)
		{
		}
	}
}