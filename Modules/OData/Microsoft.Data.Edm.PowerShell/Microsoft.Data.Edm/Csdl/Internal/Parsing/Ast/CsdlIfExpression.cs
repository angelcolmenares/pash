using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.Edm.Expressions;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlIfExpression : CsdlExpressionBase
	{
		private readonly CsdlExpressionBase test;

		private readonly CsdlExpressionBase ifTrue;

		private readonly CsdlExpressionBase ifFalse;

		public override EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.If;
			}
		}

		public CsdlExpressionBase IfFalse
		{
			get
			{
				return this.ifFalse;
			}
		}

		public CsdlExpressionBase IfTrue
		{
			get
			{
				return this.ifTrue;
			}
		}

		public CsdlExpressionBase Test
		{
			get
			{
				return this.test;
			}
		}

		public CsdlIfExpression(CsdlExpressionBase test, CsdlExpressionBase ifTrue, CsdlExpressionBase ifFalse, CsdlLocation location) : base(location)
		{
			this.test = test;
			this.ifTrue = ifTrue;
			this.ifFalse = ifFalse;
		}
	}
}