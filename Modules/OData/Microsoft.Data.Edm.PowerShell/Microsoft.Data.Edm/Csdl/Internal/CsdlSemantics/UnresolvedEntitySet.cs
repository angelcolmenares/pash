using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library.Internal;
using Microsoft.Data.Edm.Validation;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class UnresolvedEntitySet : BadEntitySet, IUnresolvedElement
	{
		public UnresolvedEntitySet(string name, IEdmEntityContainer container, EdmLocation location)
			: base(name, container, new EdmError[] { new EdmError(location, EdmErrorCode.BadUnresolvedEntitySet, Strings.Bad_UnresolvedEntitySet(name)) })
		{

		}
	}
}