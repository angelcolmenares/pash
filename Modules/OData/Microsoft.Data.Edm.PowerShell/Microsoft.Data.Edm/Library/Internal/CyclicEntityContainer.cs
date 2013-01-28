using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Validation;
using System;

namespace Microsoft.Data.Edm.Library.Internal
{
	internal class CyclicEntityContainer : BadEntityContainer
	{
		public CyclicEntityContainer(string name, EdmLocation location)
			: base(name, new EdmError[] { new EdmError(location, EdmErrorCode.BadCyclicEntityContainer, Strings.Bad_CyclicEntityContainer(name)) })
		{

		}
	}
}