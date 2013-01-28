using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Library;
using System;

namespace Microsoft.Data.Edm.Library.Expressions
{
	internal class EdmLabeledExpression : EdmElement, IEdmLabeledExpression, IEdmNamedElement, IEdmExpression, IEdmElement
	{
		private readonly string name;

		private readonly IEdmExpression expression;

		public IEdmExpression Expression
		{
			get
			{
				return this.expression;
			}
		}

		public EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.Labeled;
			}
		}

		public string Name
		{
			get
			{
				return this.name;
			}
		}

		public EdmLabeledExpression(string name, IEdmExpression expression)
		{
			EdmUtil.CheckArgumentNull<string>(name, "name");
			EdmUtil.CheckArgumentNull<IEdmExpression>(expression, "expression");
			this.name = name;
			this.expression = expression;
		}
	}
}