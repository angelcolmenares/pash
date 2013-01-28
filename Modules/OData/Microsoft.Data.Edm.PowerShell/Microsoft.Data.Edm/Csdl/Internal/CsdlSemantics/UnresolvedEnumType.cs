using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library.Internal;
using Microsoft.Data.Edm.Validation;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class UnresolvedEnumType : BadEnumType, IUnresolvedElement
	{
		public UnresolvedEnumType(string qualifiedName, EdmLocation location)
			:base(qualifiedName, new EdmError[] { new EdmError(location, EdmErrorCode.BadUnresolvedEnumType, Strings.Bad_UnresolvedEnumType(qualifiedName)) })
		{
		}
	}
}