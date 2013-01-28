using Microsoft.Data.Edm;

namespace Microsoft.Data.Edm.Expressions
{
	internal interface IEdmLabeledExpressionReferenceExpression : IEdmExpression, IEdmElement
	{
		IEdmLabeledExpression ReferencedLabeledExpression
		{
			get;
		}

	}
}