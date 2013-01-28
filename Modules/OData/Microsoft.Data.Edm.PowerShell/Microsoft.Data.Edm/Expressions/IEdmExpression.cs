using Microsoft.Data.Edm;

namespace Microsoft.Data.Edm.Expressions
{
	internal interface IEdmExpression : IEdmElement
	{
		EdmExpressionKind ExpressionKind
		{
			get;
		}

	}
}