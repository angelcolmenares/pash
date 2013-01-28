using Microsoft.Data.Edm;
using System;

namespace Microsoft.Data.Edm.Library
{
	internal class EdmRowTypeReference : EdmTypeReference, IEdmRowTypeReference, IEdmStructuredTypeReference, IEdmTypeReference, IEdmElement
	{
		public EdmRowTypeReference(IEdmRowType rowType, bool isNullable) : base(rowType, isNullable)
		{
		}
	}
}