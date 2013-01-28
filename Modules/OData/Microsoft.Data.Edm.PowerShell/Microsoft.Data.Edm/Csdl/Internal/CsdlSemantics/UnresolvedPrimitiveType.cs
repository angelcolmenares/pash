using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library.Internal;
using Microsoft.Data.Edm.Validation;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class UnresolvedPrimitiveType : BadPrimitiveType, IUnresolvedElement
	{
		public UnresolvedPrimitiveType(string qualifiedName, EdmLocation location)
			: base(qualifiedName, 0, new EdmError[] { new EdmError(location, EdmErrorCode.BadUnresolvedPrimitiveType, Strings.Bad_UnresolvedPrimitiveType(qualifiedName)) })
		{

		}
	}
}