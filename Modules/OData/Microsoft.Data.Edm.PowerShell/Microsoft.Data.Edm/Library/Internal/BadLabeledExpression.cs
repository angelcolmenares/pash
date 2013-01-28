using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Internal;
using Microsoft.Data.Edm.Library.Values;
using Microsoft.Data.Edm.Validation;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Library.Internal
{
	internal class BadLabeledExpression : BadElement, IEdmLabeledExpression, IEdmNamedElement, IEdmExpression, IEdmElement
	{
		private readonly string name;

		private readonly Cache<BadLabeledExpression, IEdmExpression> expressionCache;

		private readonly static Func<BadLabeledExpression, IEdmExpression> ComputeExpressionFunc;

		public IEdmExpression Expression
		{
			get
			{
				return this.expressionCache.GetValue(this, BadLabeledExpression.ComputeExpressionFunc, null);
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

		static BadLabeledExpression()
		{
			BadLabeledExpression.ComputeExpressionFunc = (BadLabeledExpression me) => me.ComputeExpression();
		}

		public BadLabeledExpression(string name, IEnumerable<EdmError> errors) : base(errors)
		{
			this.expressionCache = new Cache<BadLabeledExpression, IEdmExpression>();
			string str = name;
			string empty = str;
			if (str == null)
			{
				empty = string.Empty;
			}
			this.name = empty;
		}

		private IEdmExpression ComputeExpression()
		{
			return EdmNullExpression.Instance;
		}
	}
}