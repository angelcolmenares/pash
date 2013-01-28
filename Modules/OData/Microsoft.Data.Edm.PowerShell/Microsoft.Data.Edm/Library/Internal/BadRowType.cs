using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Validation;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Library.Internal
{
	internal class BadRowType : BadStructuredType, IEdmRowType, IEdmStructuredType, IEdmType, IEdmElement
	{
		public override EdmTypeKind TypeKind
		{
			get
			{
				return EdmTypeKind.Row;
			}
		}

		public BadRowType(IEnumerable<EdmError> errors) : base(errors)
		{
		}
	}
}