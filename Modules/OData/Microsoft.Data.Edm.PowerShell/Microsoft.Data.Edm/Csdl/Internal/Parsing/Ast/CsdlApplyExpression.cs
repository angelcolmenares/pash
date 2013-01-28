using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.Edm.Expressions;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlApplyExpression : CsdlExpressionBase
	{
		private readonly string function;

		private readonly List<CsdlExpressionBase> arguments;

		public IEnumerable<CsdlExpressionBase> Arguments
		{
			get
			{
				return this.arguments;
			}
		}

		public override EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.FunctionApplication;
			}
		}

		public string Function
		{
			get
			{
				return this.function;
			}
		}

		public CsdlApplyExpression(string function, IEnumerable<CsdlExpressionBase> arguments, CsdlLocation location) : base(location)
		{
			this.function = function;
			this.arguments = new List<CsdlExpressionBase>(arguments);
		}
	}
}