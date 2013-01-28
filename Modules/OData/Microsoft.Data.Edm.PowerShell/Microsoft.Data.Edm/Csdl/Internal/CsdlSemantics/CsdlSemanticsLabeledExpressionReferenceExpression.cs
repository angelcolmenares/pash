using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Internal;
using Microsoft.Data.Edm.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class CsdlSemanticsLabeledExpressionReferenceExpression : CsdlSemanticsExpression, IEdmLabeledExpressionReferenceExpression, IEdmExpression, IEdmElement, IEdmCheckable
	{
		private readonly CsdlLabeledExpressionReferenceExpression expression;

		private readonly IEdmEntityType bindingContext;

		private readonly Cache<CsdlSemanticsLabeledExpressionReferenceExpression, IEdmLabeledExpression> elementCache;

		private readonly static Func<CsdlSemanticsLabeledExpressionReferenceExpression, IEdmLabeledExpression> ComputeElementFunc;

		public override CsdlElement Element
		{
			get
			{
				return this.expression;
			}
		}

		public IEnumerable<EdmError> Errors
		{
			get
			{
				if (this.ReferencedLabeledExpression as IUnresolvedElement == null)
				{
					return Enumerable.Empty<EdmError>();
				}
				else
				{
					return this.ReferencedLabeledExpression.Errors();
				}
			}
		}

		public override EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.LabeledExpressionReference;
			}
		}

		public IEdmLabeledExpression ReferencedLabeledExpression
		{
			get
			{
				return this.elementCache.GetValue(this, CsdlSemanticsLabeledExpressionReferenceExpression.ComputeElementFunc, null);
			}
		}

		static CsdlSemanticsLabeledExpressionReferenceExpression()
		{
			CsdlSemanticsLabeledExpressionReferenceExpression.ComputeElementFunc = (CsdlSemanticsLabeledExpressionReferenceExpression me) => me.ComputeElement();
		}

		public CsdlSemanticsLabeledExpressionReferenceExpression(CsdlLabeledExpressionReferenceExpression expression, IEdmEntityType bindingContext, CsdlSemanticsSchema schema) : base(schema, expression)
		{
			this.elementCache = new Cache<CsdlSemanticsLabeledExpressionReferenceExpression, IEdmLabeledExpression>();
			this.expression = expression;
			this.bindingContext = bindingContext;
		}

		private IEdmLabeledExpression ComputeElement ()
		{
			var model = base.Schema.Model;
			if (model == null) {
				
			}
			IEdmLabeledExpression edmLabeledExpression = base.Schema.FindLabeledElement(this.expression.Label, this.bindingContext);
			if (edmLabeledExpression == null)
			{
				return new UnresolvedLabeledElement(this.expression.Label, base.Location);
			}
			else
			{
				return edmLabeledExpression;
			}
		}
	}
}