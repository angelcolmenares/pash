using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Internal;
using Microsoft.Data.Edm.Library.Values;
using System;

namespace Microsoft.Data.Edm.Library.Internal
{
	internal class AmbiguousLabeledExpressionBinding : AmbiguousBinding<IEdmLabeledExpression>, IEdmLabeledExpression, IEdmNamedElement, IEdmExpression, IEdmElement
	{
		private readonly IEdmLabeledExpression first;

		private readonly Cache<AmbiguousLabeledExpressionBinding, IEdmExpression> expressionCache;

		private readonly static Func<AmbiguousLabeledExpressionBinding, IEdmExpression> ComputeExpressionFunc;

		public IEdmExpression Expression
		{
			get
			{
				return this.expressionCache.GetValue(this, AmbiguousLabeledExpressionBinding.ComputeExpressionFunc, null);
			}
		}

		public EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.Labeled;
			}
		}

		static AmbiguousLabeledExpressionBinding()
		{
			AmbiguousLabeledExpressionBinding.ComputeExpressionFunc = (AmbiguousLabeledExpressionBinding me) => me.ComputeExpression();
		}

		public AmbiguousLabeledExpressionBinding(IEdmLabeledExpression first, IEdmLabeledExpression second) : base(first, second)
		{
			this.expressionCache = new Cache<AmbiguousLabeledExpressionBinding, IEdmExpression>();
			this.first = first;
		}

		private IEdmExpression ComputeExpression()
		{
			return EdmNullExpression.Instance;
		}
	}
}