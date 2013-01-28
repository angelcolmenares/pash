using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.Edm.Expressions;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlAssertTypeExpression : CsdlExpressionBase
	{
		private readonly CsdlTypeReference type;

		private readonly CsdlExpressionBase operand;

		public override EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.AssertType;
			}
		}

		public CsdlExpressionBase Operand
		{
			get
			{
				return this.operand;
			}
		}

		public CsdlTypeReference Type
		{
			get
			{
				return this.type;
			}
		}

		public CsdlAssertTypeExpression(CsdlTypeReference type, CsdlExpressionBase operand, CsdlLocation location) : base(location)
		{
			this.type = type;
			this.operand = operand;
		}
	}
}