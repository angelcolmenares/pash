using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Internal;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class CsdlSemanticsIsTypeExpression : CsdlSemanticsExpression, IEdmIsTypeExpression, IEdmExpression, IEdmElement
	{
		private readonly CsdlIsTypeExpression expression;

		private readonly IEdmEntityType bindingContext;

		private readonly Cache<CsdlSemanticsIsTypeExpression, IEdmExpression> operandCache;

		private readonly static Func<CsdlSemanticsIsTypeExpression, IEdmExpression> ComputeOperandFunc;

		private readonly Cache<CsdlSemanticsIsTypeExpression, IEdmTypeReference> typeCache;

		private readonly static Func<CsdlSemanticsIsTypeExpression, IEdmTypeReference> ComputeTypeFunc;

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
				return EdmExpressionKind.IsType;
			}
		}

		public IEdmExpression Operand
		{
			get
			{
				return this.operandCache.GetValue(this, CsdlSemanticsIsTypeExpression.ComputeOperandFunc, null);
			}
		}

		public IEdmTypeReference Type
		{
			get
			{
				return this.typeCache.GetValue(this, CsdlSemanticsIsTypeExpression.ComputeTypeFunc, null);
			}
		}

		static CsdlSemanticsIsTypeExpression()
		{
			CsdlSemanticsIsTypeExpression.ComputeOperandFunc = (CsdlSemanticsIsTypeExpression me) => me.ComputeOperand();
			CsdlSemanticsIsTypeExpression.ComputeTypeFunc = (CsdlSemanticsIsTypeExpression me) => me.ComputeType();
		}

		public CsdlSemanticsIsTypeExpression(CsdlIsTypeExpression expression, IEdmEntityType bindingContext, CsdlSemanticsSchema schema) : base(schema, expression)
		{
			this.operandCache = new Cache<CsdlSemanticsIsTypeExpression, IEdmExpression>();
			this.typeCache = new Cache<CsdlSemanticsIsTypeExpression, IEdmTypeReference>();
			this.expression = expression;
			this.bindingContext = bindingContext;
		}

		private IEdmExpression ComputeOperand()
		{
			return CsdlSemanticsModel.WrapExpression(this.expression.Operand, this.bindingContext, base.Schema);
		}

		private IEdmTypeReference ComputeType()
		{
			return CsdlSemanticsModel.WrapTypeReference(base.Schema, this.expression.Type);
		}
	}
}