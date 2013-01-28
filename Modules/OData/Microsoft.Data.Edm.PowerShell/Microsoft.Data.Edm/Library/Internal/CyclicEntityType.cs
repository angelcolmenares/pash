using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Validation;
using System;

namespace Microsoft.Data.Edm.Library.Internal
{
	internal class CyclicEntityType : BadEntityType
	{
		public CyclicEntityType(string qualifiedName, EdmLocation location)
			: base(qualifiedName, new EdmError[] { new EdmError(location, EdmErrorCode.BadCyclicEntity, Strings.Bad_CyclicEntity(qualifiedName)) })
		{

		}
	}
}