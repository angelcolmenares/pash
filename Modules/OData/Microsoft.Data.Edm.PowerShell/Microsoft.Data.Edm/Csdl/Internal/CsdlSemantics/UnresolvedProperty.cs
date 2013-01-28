using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library.Internal;
using Microsoft.Data.Edm.Validation;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class UnresolvedProperty : BadProperty, IUnresolvedElement
	{
		public UnresolvedProperty(IEdmStructuredType declaringType, string name, EdmLocation location)
			: base(declaringType, name, new EdmError[] { new EdmError(location, EdmErrorCode.BadUnresolvedProperty, Strings.Bad_UnresolvedProperty(name)) })
		{

		}
	}
}