using Microsoft.Data.Edm;

namespace Microsoft.Data.Edm.Expressions
{
	internal interface IEdmFunctionReferenceExpression : IEdmExpression, IEdmElement
	{
		IEdmFunction ReferencedFunction
		{
			get;
		}

	}
}