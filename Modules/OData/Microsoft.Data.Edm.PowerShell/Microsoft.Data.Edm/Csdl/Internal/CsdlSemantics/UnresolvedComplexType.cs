using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library.Internal;
using Microsoft.Data.Edm.Validation;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class UnresolvedComplexType : BadComplexType, IUnresolvedElement
	{
		public UnresolvedComplexType(string qualifiedName, EdmLocation location)
			: base(qualifiedName, new EdmError[] { new EdmError(location, EdmErrorCode.BadUnresolvedComplexType, Strings.Bad_UnresolvedComplexType(qualifiedName)) })
		{

		}
	}
}