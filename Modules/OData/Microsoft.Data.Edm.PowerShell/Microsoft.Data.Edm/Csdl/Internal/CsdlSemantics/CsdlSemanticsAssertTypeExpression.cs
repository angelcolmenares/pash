using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Internal;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class CsdlSemanticsAssertTypeExpression : CsdlSemanticsExpression, IEdmAssertTypeExpression, IEdmExpression, IEdmElement
	{
		private readonly CsdlAssertTypeExpression expression;

		private readonly IEdmEntityType bindingContext;

		private readonly Cache<CsdlSemanticsAssertTypeExpression, IEdmExpression> operandCache;

		private readonly static Func<CsdlSemanticsAssertTypeExpression, IEdmExpression> ComputeOperandFunc;

		private readonly Cache<CsdlSemanticsAssertTypeExpression, IEdmTypeReference> typeCache;

		private readonly static Func<CsdlSemanticsAssertTypeExpression, IEdmTypeReference> ComputeTypeFunc;

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
				return EdmExpressionKind.AssertType;
			}
		}

		public IEdmExpression Operand
		{
			get
			{
				return this.operandCache.GetValue(this, CsdlSemanticsAssertTypeExpression.ComputeOperandFunc, null);
			}
		}

		public IEdmTypeReference Type
		{
			get
			{
				return this.typeCache.GetValue(this, CsdlSemanticsAssertTypeExpression.ComputeTypeFunc, null);
			}
		}

		static CsdlSemanticsAssertTypeExpression()
		{
			CsdlSemanticsAssertTypeExpression.ComputeOperandFunc = (CsdlSemanticsAssertTypeExpression me) => me.ComputeOperand();
			CsdlSemanticsAssertTypeExpression.ComputeTypeFunc = (CsdlSemanticsAssertTypeExpression me) => me.ComputeType();
		}

		public CsdlSemanticsAssertTypeExpression(CsdlAssertTypeExpression expression, IEdmEntityType bindingContext, CsdlSemanticsSchema schema) : base(schema, expression)
		{
			this.operandCache = new Cache<CsdlSemanticsAssertTypeExpression, IEdmExpression>();
			this.typeCache = new Cache<CsdlSemanticsAssertTypeExpression, IEdmTypeReference>();
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