using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Library;
using System;

namespace Microsoft.Data.Edm.Library.Expressions
{
	internal class EdmLabeledExpressionReferenceExpression : EdmElement, IEdmLabeledExpressionReferenceExpression, IEdmExpression, IEdmElement
	{
		private IEdmLabeledExpression referencedLabeledExpression;

		public EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.LabeledExpressionReference;
			}
		}

		public IEdmLabeledExpression JustDecompileGenerated_get_ReferencedLabeledExpression()
		{
			return this.referencedLabeledExpression;
		}

		public void JustDecompileGenerated_set_ReferencedLabeledExpression(IEdmLabeledExpression value)
		{
			EdmUtil.CheckArgumentNull<IEdmLabeledExpression>(value, "value");
			if (this.referencedLabeledExpression == null)
			{
				this.referencedLabeledExpression = value;
				return;
			}
			else
			{
				throw new InvalidOperationException(Strings.ValueHasAlreadyBeenSet);
			}
		}

		public IEdmLabeledExpression ReferencedLabeledExpression
		{
			get
			{
				return JustDecompileGenerated_get_ReferencedLabeledExpression();
			}
			set
			{
				JustDecompileGenerated_set_ReferencedLabeledExpression(value);
			}
		}

		public EdmLabeledExpressionReferenceExpression()
		{
		}

		public EdmLabeledExpressionReferenceExpression(IEdmLabeledExpression referencedLabeledExpression)
		{
			EdmUtil.CheckArgumentNull<IEdmLabeledExpression>(referencedLabeledExpression, "referencedLabeledExpression");
			this.referencedLabeledExpression = referencedLabeledExpression;
		}
	}
}