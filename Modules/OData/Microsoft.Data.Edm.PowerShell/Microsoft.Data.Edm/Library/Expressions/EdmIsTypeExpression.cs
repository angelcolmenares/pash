using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Library;

namespace Microsoft.Data.Edm.Library.Expressions
{
	internal class EdmIsTypeExpression : EdmElement, IEdmIsTypeExpression, IEdmExpression, IEdmElement
	{
		private readonly IEdmExpression operand;

		private readonly IEdmTypeReference type;

		public EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.IsType;
			}
		}

		public IEdmExpression Operand
		{
			get
			{
				return this.operand;
			}
		}

		public IEdmTypeReference Type
		{
			get
			{
				return this.type;
			}
		}

		public EdmIsTypeExpression(IEdmExpression operand, IEdmTypeReference type)
		{
			EdmUtil.CheckArgumentNull<IEdmExpression>(operand, "operand");
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(type, "type");
			this.operand = operand;
			this.type = type;
		}
	}
}