using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Library;
using System;

namespace Microsoft.Data.Edm.Library.Expressions
{
	internal class EdmValueTermReferenceExpression : EdmElement, IEdmValueTermReferenceExpression, IEdmExpression, IEdmElement
	{
		private readonly IEdmExpression baseExpression;

		private readonly IEdmValueTerm term;

		private readonly string qualifier;

		public IEdmExpression Base
		{
			get
			{
				return this.baseExpression;
			}
		}

		public EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.ValueTermReference;
			}
		}

		public string Qualifier
		{
			get
			{
				return this.qualifier;
			}
		}

		public IEdmValueTerm Term
		{
			get
			{
				return this.term;
			}
		}

		public EdmValueTermReferenceExpression(IEdmExpression baseExpression, IEdmValueTerm term) : this(baseExpression, term, null)
		{
		}

		public EdmValueTermReferenceExpression(IEdmExpression baseExpression, IEdmValueTerm term, string qualifier)
		{
			EdmUtil.CheckArgumentNull<IEdmExpression>(baseExpression, "baseExpression");
			EdmUtil.CheckArgumentNull<IEdmValueTerm>(term, "term");
			this.baseExpression = baseExpression;
			this.term = term;
			this.qualifier = qualifier;
		}
	}
}