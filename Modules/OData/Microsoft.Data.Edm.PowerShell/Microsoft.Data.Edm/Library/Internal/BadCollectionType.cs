using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Validation;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Library.Internal
{
	internal class BadCollectionType : BadType, IEdmCollectionType, IEdmType, IEdmElement
	{
		private readonly IEdmTypeReference elementType;

		public IEdmTypeReference ElementType
		{
			get
			{
				return this.elementType;
			}
		}

		public override EdmTypeKind TypeKind
		{
			get
			{
				return EdmTypeKind.Collection;
			}
		}

		public BadCollectionType(IEnumerable<EdmError> errors) : base(errors)
		{
			this.elementType = new BadTypeReference(new BadType(errors), true);
		}
	}
}