using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Validation;
using Microsoft.Data.Edm.Values;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Library.Internal
{
	internal class BadPrimitiveValue : BadElement, IEdmPrimitiveValue, IEdmValue, IEdmElement
	{
		private readonly IEdmPrimitiveTypeReference type;

		public IEdmTypeReference Type
		{
			get
			{
				return this.type;
			}
		}

		public EdmValueKind ValueKind
		{
			get
			{
				return EdmValueKind.None;
			}
		}

		public BadPrimitiveValue(IEdmPrimitiveTypeReference type, IEnumerable<EdmError> errors) : base(errors)
		{
			this.type = type;
		}
	}
}