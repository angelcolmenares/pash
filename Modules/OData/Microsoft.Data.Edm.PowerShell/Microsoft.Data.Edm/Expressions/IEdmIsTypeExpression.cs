using Microsoft.Data.Edm;

namespace Microsoft.Data.Edm.Expressions
{
	internal interface IEdmIsTypeExpression : IEdmExpression, IEdmElement
	{
		IEdmExpression Operand
		{
			get;
		}

		IEdmTypeReference Type
		{
			get;
		}

	}
}