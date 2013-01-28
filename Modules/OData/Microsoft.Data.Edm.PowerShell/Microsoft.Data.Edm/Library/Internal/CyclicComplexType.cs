using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Validation;
using System;

namespace Microsoft.Data.Edm.Library.Internal
{
	internal class CyclicComplexType : BadComplexType
	{
		public CyclicComplexType(string qualifiedName, EdmLocation location)
			: base(qualifiedName, new EdmError[] { new EdmError(location, EdmErrorCode.BadCyclicComplex, Strings.Bad_CyclicComplex(qualifiedName)) })
		{

		}
	}
}