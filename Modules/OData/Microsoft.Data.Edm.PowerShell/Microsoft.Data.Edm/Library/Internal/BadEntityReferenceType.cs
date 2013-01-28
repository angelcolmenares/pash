using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Validation;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Library.Internal
{
	internal class BadEntityReferenceType : BadType, IEdmEntityReferenceType, IEdmType, IEdmElement
	{
		private readonly IEdmEntityType entityType;

		public IEdmEntityType EntityType
		{
			get
			{
				return this.entityType;
			}
		}

		public override EdmTypeKind TypeKind
		{
			get
			{
				return EdmTypeKind.EntityReference;
			}
		}

		public BadEntityReferenceType(IEnumerable<EdmError> errors) : base(errors)
		{
			this.entityType = new BadEntityType(string.Empty, base.Errors);
		}
	}
}