using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library.Internal;
using Microsoft.Data.Edm.Validation;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class UnresolvedLabeledElement : BadLabeledExpression, IUnresolvedElement
	{
		public UnresolvedLabeledElement(string label, EdmLocation location)
			: base(label, new EdmError[] { new EdmError(location, EdmErrorCode.BadUnresolvedLabeledElement, Strings.Bad_UnresolvedLabeledElement(label)) })
		{

		}
	}
}