using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.Edm.Expressions;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlPathExpression : CsdlExpressionBase
	{
		private readonly string path;

		public override EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.Path;
			}
		}

		public string Path
		{
			get
			{
				return this.path;
			}
		}

		public CsdlPathExpression(string path, CsdlLocation location) : base(location)
		{
			this.path = path;
		}
	}
}