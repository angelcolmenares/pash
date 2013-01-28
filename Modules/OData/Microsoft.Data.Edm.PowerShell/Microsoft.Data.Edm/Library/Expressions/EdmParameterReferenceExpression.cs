using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Library;

namespace Microsoft.Data.Edm.Library.Expressions
{
	internal class EdmParameterReferenceExpression : EdmElement, IEdmParameterReferenceExpression, IEdmExpression, IEdmElement
	{
		private readonly IEdmFunctionParameter referencedParameter;

		public EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.ParameterReference;
			}
		}

		public IEdmFunctionParameter ReferencedParameter
		{
			get
			{
				return this.referencedParameter;
			}
		}

		public EdmParameterReferenceExpression(IEdmFunctionParameter referencedParameter)
		{
			EdmUtil.CheckArgumentNull<IEdmFunctionParameter>(referencedParameter, "referencedParameter");
			this.referencedParameter = referencedParameter;
		}
	}
}