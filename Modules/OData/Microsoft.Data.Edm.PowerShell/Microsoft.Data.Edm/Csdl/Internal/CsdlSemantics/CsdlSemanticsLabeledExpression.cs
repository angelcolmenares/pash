using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Internal;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class CsdlSemanticsLabeledExpression : CsdlSemanticsElement, IEdmLabeledExpression, IEdmNamedElement, IEdmExpression, IEdmElement
	{
		private readonly string name;

		private readonly CsdlExpressionBase sourceElement;

		private readonly CsdlSemanticsSchema schema;

		private readonly IEdmEntityType bindingContext;

		private readonly Cache<CsdlSemanticsLabeledExpression, IEdmExpression> expressionCache;

		private readonly static Func<CsdlSemanticsLabeledExpression, IEdmExpression> ComputeExpressionFunc;

		public IEdmEntityType BindingContext
		{
			get
			{
				return this.bindingContext;
			}
		}

		public override CsdlElement Element
		{
			get
			{
				return this.sourceElement;
			}
		}

		public IEdmExpression Expression
		{
			get
			{
				return this.expressionCache.GetValue(this, CsdlSemanticsLabeledExpression.ComputeExpressionFunc, null);
			}
		}

		public EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.Labeled;
			}
		}

		public override CsdlSemanticsModel Model
		{
			get
			{
				return this.schema.Model;
			}
		}

		public string Name
		{
			get
			{
				return this.name;
			}
		}

		static CsdlSemanticsLabeledExpression()
		{
			CsdlSemanticsLabeledExpression.ComputeExpressionFunc = (CsdlSemanticsLabeledExpression me) => me.ComputeExpression();
		}

		public CsdlSemanticsLabeledExpression(string name, CsdlExpressionBase element, IEdmEntityType bindingContext, CsdlSemanticsSchema schema) : base(element)
		{
			this.expressionCache = new Cache<CsdlSemanticsLabeledExpression, IEdmExpression>();
			this.name = name;
			this.sourceElement = element;
			this.bindingContext = bindingContext;
			this.schema = schema;
		}

		private IEdmExpression ComputeExpression()
		{
			return CsdlSemanticsModel.WrapExpression(this.sourceElement, this.BindingContext, this.schema);
		}
	}
}