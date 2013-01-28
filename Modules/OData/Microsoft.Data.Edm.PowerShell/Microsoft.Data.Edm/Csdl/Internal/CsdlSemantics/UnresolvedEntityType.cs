using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library.Internal;
using Microsoft.Data.Edm.Validation;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class UnresolvedEntityType : BadEntityType, IUnresolvedElement
	{
		public UnresolvedEntityType(string qualifiedName, EdmLocation location)
			: base(qualifiedName, new EdmError[] { new EdmError(location, EdmErrorCode.BadUnresolvedEntityType, Strings.Bad_UnresolvedEntityType(qualifiedName)) })
		{

		}
	}
}