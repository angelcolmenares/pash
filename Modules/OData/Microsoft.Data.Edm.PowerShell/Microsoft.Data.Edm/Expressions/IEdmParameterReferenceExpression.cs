using Microsoft.Data.Edm;

namespace Microsoft.Data.Edm.Expressions
{
	internal interface IEdmParameterReferenceExpression : IEdmExpression, IEdmElement
	{
		IEdmFunctionParameter ReferencedParameter
		{
			get;
		}

	}
}