using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.Edm.Expressions;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlEnumMemberReferenceExpression : CsdlExpressionBase
	{
		private readonly string enumMemberPath;

		public string EnumMemberPath
		{
			get
			{
				return this.enumMemberPath;
			}
		}

		public override EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.EnumMemberReference;
			}
		}

		public CsdlEnumMemberReferenceExpression(string enumMemberPath, CsdlLocation location) : base(location)
		{
			this.enumMemberPath = enumMemberPath;
		}
	}
}