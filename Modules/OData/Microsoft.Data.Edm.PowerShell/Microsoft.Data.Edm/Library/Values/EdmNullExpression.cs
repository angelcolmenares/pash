using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Values;

namespace Microsoft.Data.Edm.Library.Values
{
	internal class EdmNullExpression : EdmValue, IEdmNullExpression, IEdmExpression, IEdmNullValue, IEdmValue, IEdmElement
	{
		public static EdmNullExpression Instance;

		public EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.Null;
			}
		}

		public override EdmValueKind ValueKind
		{
			get
			{
				return EdmValueKind.Null;
			}
		}

		static EdmNullExpression()
		{
			EdmNullExpression.Instance = new EdmNullExpression();
		}

		private EdmNullExpression() : base(null)
		{
		}
	}
}