using Microsoft.Data.Edm;

namespace Microsoft.Data.Edm.Expressions
{
	internal interface IEdmIfExpression : IEdmExpression, IEdmElement
	{
		IEdmExpression FalseExpression
		{
			get;
		}

		IEdmExpression TestExpression
		{
			get;
		}

		IEdmExpression TrueExpression
		{
			get;
		}

	}
}