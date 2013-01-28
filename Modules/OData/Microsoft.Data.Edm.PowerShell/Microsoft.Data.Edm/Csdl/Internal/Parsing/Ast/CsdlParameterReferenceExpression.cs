using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.Edm.Expressions;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlParameterReferenceExpression : CsdlExpressionBase
	{
		private readonly string parameter;

		public override EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.ParameterReference;
			}
		}

		public string Parameter
		{
			get
			{
				return this.parameter;
			}
		}

		public CsdlParameterReferenceExpression(string parameter, CsdlLocation location) : base(location)
		{
			this.parameter = parameter;
		}
	}
}