using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Validation;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class UnresolvedAssociationEnd : BadAssociationEnd, IUnresolvedElement
	{
		public UnresolvedAssociationEnd(IEdmAssociation declaringAssociation, string role, EdmLocation location)
			: base(declaringAssociation, role, new EdmError[] {  new EdmError(location, EdmErrorCode.BadNonComputableAssociationEnd, Strings.Bad_UncomputableAssociationEnd(role)) })
		{

		}
	}
}