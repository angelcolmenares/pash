using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.Edm.Expressions;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlFunctionReferenceExpression : CsdlExpressionBase
	{
		private readonly string function;

		public override EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.FunctionReference;
			}
		}

		public string Function
		{
			get
			{
				return this.function;
			}
		}

		public CsdlFunctionReferenceExpression(string function, CsdlLocation location) : base(location)
		{
			this.function = function;
		}
	}
}