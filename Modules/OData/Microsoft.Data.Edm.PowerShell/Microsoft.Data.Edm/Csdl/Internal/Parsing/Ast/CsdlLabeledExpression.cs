using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.Edm.Expressions;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlLabeledExpression : CsdlExpressionBase
	{
		private readonly string label;

		private readonly CsdlExpressionBase element;

		public CsdlExpressionBase Element
		{
			get
			{
				return this.element;
			}
		}

		public override EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.Labeled;
			}
		}

		public string Label
		{
			get
			{
				return this.label;
			}
		}

		public CsdlLabeledExpression(string label, CsdlExpressionBase element, CsdlLocation location) : base(location)
		{
			this.label = label;
			this.element = element;
		}
	}
}