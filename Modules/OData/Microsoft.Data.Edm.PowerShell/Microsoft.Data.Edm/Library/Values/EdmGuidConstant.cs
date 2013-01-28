using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Values;
using System;

namespace Microsoft.Data.Edm.Library.Values
{
	internal class EdmGuidConstant : EdmValue, IEdmGuidConstantExpression, IEdmExpression, IEdmGuidValue, IEdmPrimitiveValue, IEdmValue, IEdmElement
	{
		private readonly Guid @value;

		public EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.GuidConstant;
			}
		}

		public Guid Value
		{
			get
			{
				return this.@value;
			}
		}

		public override EdmValueKind ValueKind
		{
			get
			{
				return EdmValueKind.Guid;
			}
		}

		public EdmGuidConstant(Guid value) : this(null, value)
		{
			this.@value = value;
		}

		public EdmGuidConstant(IEdmPrimitiveTypeReference type, Guid value) : base(type)
		{
			this.@value = value;
		}
	}
}