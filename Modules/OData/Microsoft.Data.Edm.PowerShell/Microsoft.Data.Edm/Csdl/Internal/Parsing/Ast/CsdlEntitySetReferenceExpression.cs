using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.Edm.Expressions;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlEntitySetReferenceExpression : CsdlExpressionBase
	{
		private readonly string entitySetPath;

		public string EntitySetPath
		{
			get
			{
				return this.entitySetPath;
			}
		}

		public override EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.EntitySetReference;
			}
		}

		public CsdlEntitySetReferenceExpression(string entitySetPath, CsdlLocation location) : base(location)
		{
			this.entitySetPath = entitySetPath;
		}
	}
}