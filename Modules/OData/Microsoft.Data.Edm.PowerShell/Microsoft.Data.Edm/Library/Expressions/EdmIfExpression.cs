using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Library;

namespace Microsoft.Data.Edm.Library.Expressions
{
	internal class EdmIfExpression : EdmElement, IEdmIfExpression, IEdmExpression, IEdmElement
	{
		private readonly IEdmExpression testExpression;

		private readonly IEdmExpression trueExpression;

		private readonly IEdmExpression falseExpression;

		public EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.If;
			}
		}

		public IEdmExpression FalseExpression
		{
			get
			{
				return this.falseExpression;
			}
		}

		public IEdmExpression TestExpression
		{
			get
			{
				return this.testExpression;
			}
		}

		public IEdmExpression TrueExpression
		{
			get
			{
				return this.trueExpression;
			}
		}

		public EdmIfExpression(IEdmExpression testExpression, IEdmExpression trueExpression, IEdmExpression falseExpression)
		{
			EdmUtil.CheckArgumentNull<IEdmExpression>(testExpression, "testExpression");
			EdmUtil.CheckArgumentNull<IEdmExpression>(trueExpression, "trueExpression");
			EdmUtil.CheckArgumentNull<IEdmExpression>(falseExpression, "falseExpression");
			this.testExpression = testExpression;
			this.trueExpression = trueExpression;
			this.falseExpression = falseExpression;
		}
	}
}