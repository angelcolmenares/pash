using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Library;

namespace Microsoft.Data.Edm.Library.Expressions
{
	internal class EdmFunctionReferenceExpression : EdmElement, IEdmFunctionReferenceExpression, IEdmExpression, IEdmElement
	{
		private readonly IEdmFunction referencedFunction;

		public EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.FunctionReference;
			}
		}

		public IEdmFunction ReferencedFunction
		{
			get
			{
				return this.referencedFunction;
			}
		}

		public EdmFunctionReferenceExpression(IEdmFunction referencedFunction)
		{
			EdmUtil.CheckArgumentNull<IEdmFunction>(referencedFunction, "referencedFunction");
			this.referencedFunction = referencedFunction;
		}
	}
}