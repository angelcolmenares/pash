using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.Edm.Expressions;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlLabeledExpressionReferenceExpression : CsdlExpressionBase
	{
		private readonly string label;

		public override EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.LabeledExpressionReference;
			}
		}

		public string Label
		{
			get
			{
				return this.label;
			}
		}

		public CsdlLabeledExpressionReferenceExpression(string label, CsdlLocation location) : base(location)
		{
			this.label = label;
		}
	}
}