using Microsoft.Data.Edm;

namespace Microsoft.Data.Edm.Expressions
{
	internal interface IEdmLabeledExpression : IEdmNamedElement, IEdmExpression, IEdmElement
	{
		IEdmExpression Expression
		{
			get;
		}

	}
}