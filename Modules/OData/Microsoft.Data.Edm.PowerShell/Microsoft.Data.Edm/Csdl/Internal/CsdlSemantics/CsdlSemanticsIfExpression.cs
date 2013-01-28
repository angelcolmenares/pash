using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Internal;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class CsdlSemanticsIfExpression : CsdlSemanticsExpression, IEdmIfExpression, IEdmExpression, IEdmElement
	{
		private readonly CsdlIfExpression expression;

		private readonly IEdmEntityType bindingContext;

		private readonly Cache<CsdlSemanticsIfExpression, IEdmExpression> testCache;

		private readonly static Func<CsdlSemanticsIfExpression, IEdmExpression> ComputeTestFunc;

		private readonly Cache<CsdlSemanticsIfExpression, IEdmExpression> ifTrueCache;

		private readonly static Func<CsdlSemanticsIfExpression, IEdmExpression> ComputeIfTrueFunc;

		private readonly Cache<CsdlSemanticsIfExpression, IEdmExpression> ifFalseCache;

		private readonly static Func<CsdlSemanticsIfExpression, IEdmExpression> ComputeIfFalseFunc;

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
				return this.expression;
			}
		}

		public override EdmExpressionKind ExpressionKind
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
				return this.ifFalseCache.GetValue(this, CsdlSemanticsIfExpression.ComputeIfFalseFunc, null);
			}
		}

		public IEdmExpression TestExpression
		{
			get
			{
				return this.testCache.GetValue(this, CsdlSemanticsIfExpression.ComputeTestFunc, null);
			}
		}

		public IEdmExpression TrueExpression
		{
			get
			{
				return this.ifTrueCache.GetValue(this, CsdlSemanticsIfExpression.ComputeIfTrueFunc, null);
			}
		}

		static CsdlSemanticsIfExpression()
		{
			CsdlSemanticsIfExpression.ComputeTestFunc = (CsdlSemanticsIfExpression me) => me.ComputeTest();
			CsdlSemanticsIfExpression.ComputeIfTrueFunc = (CsdlSemanticsIfExpression me) => me.ComputeIfTrue();
			CsdlSemanticsIfExpression.ComputeIfFalseFunc = (CsdlSemanticsIfExpression me) => me.ComputeIfFalse();
		}

		public CsdlSemanticsIfExpression(CsdlIfExpression expression, IEdmEntityType bindingContext, CsdlSemanticsSchema schema) : base(schema, expression)
		{
			this.testCache = new Cache<CsdlSemanticsIfExpression, IEdmExpression>();
			this.ifTrueCache = new Cache<CsdlSemanticsIfExpression, IEdmExpression>();
			this.ifFalseCache = new Cache<CsdlSemanticsIfExpression, IEdmExpression>();
			this.expression = expression;
			this.bindingContext = bindingContext;
		}

		private IEdmExpression ComputeIfFalse()
		{
			return CsdlSemanticsModel.WrapExpression(this.expression.IfFalse, this.BindingContext, base.Schema);
		}

		private IEdmExpression ComputeIfTrue()
		{
			return CsdlSemanticsModel.WrapExpression(this.expression.IfTrue, this.BindingContext, base.Schema);
		}

		private IEdmExpression ComputeTest()
		{
			return CsdlSemanticsModel.WrapExpression(this.expression.Test, this.BindingContext, base.Schema);
		}
	}
}