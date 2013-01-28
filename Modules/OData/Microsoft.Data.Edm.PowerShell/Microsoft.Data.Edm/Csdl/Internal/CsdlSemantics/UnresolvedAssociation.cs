using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Validation;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class UnresolvedAssociation : BadAssociation, IUnresolvedElement
	{
		public UnresolvedAssociation(string qualifiedName, EdmLocation location)
			: base(qualifiedName, new EdmError[] { new EdmError(location, EdmErrorCode.BadUnresolvedType, Strings.Bad_UnresolvedType(qualifiedName)) })
		{

		}
	}
}