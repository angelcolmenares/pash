using System;

namespace Microsoft.ActiveDirectory.TRLParser.LanguageParser
{
	[CLSCompliant(false)]
	public struct ExpressionStruct
	{
		public ExpressionType type;

		public ClaimPropertyAccessExpressStruct ReferenceExpression;

		public StringLiteralExpressionStruct LiteralExpression;

	}
}