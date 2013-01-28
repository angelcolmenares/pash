using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library.Internal;
using Microsoft.Data.Edm.Validation;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class UnresolvedEntityContainer : BadEntityContainer, IUnresolvedElement
	{
		public UnresolvedEntityContainer(string name, EdmLocation location)
			: base(name, new EdmError[] { new EdmError(location, EdmErrorCode.BadUnresolvedEntityContainer, Strings.Bad_UnresolvedEntityContainer(name)) })
		{
		}
	}
}