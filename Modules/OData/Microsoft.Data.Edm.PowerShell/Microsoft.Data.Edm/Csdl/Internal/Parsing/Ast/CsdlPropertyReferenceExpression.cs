using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.Edm.Expressions;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlPropertyReferenceExpression : CsdlExpressionBase
	{
		private readonly string property;

		private readonly CsdlExpressionBase baseExpression;

		public CsdlExpressionBase BaseExpression
		{
			get
			{
				return this.baseExpression;
			}
		}

		public override EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.PropertyReference;
			}
		}

		public string Property
		{
			get
			{
				return this.property;
			}
		}

		public CsdlPropertyReferenceExpression(string property, CsdlExpressionBase baseExpression, CsdlLocation location) : base(location)
		{
			this.property = property;
			this.baseExpression = baseExpression;
		}
	}
}