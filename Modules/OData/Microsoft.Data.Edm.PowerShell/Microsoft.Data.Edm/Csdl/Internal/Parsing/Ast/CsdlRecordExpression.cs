using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.Edm.Expressions;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlRecordExpression : CsdlExpressionBase
	{
		private readonly CsdlTypeReference type;

		private readonly List<CsdlPropertyValue> propertyValues;

		public override EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.Record;
			}
		}

		public IEnumerable<CsdlPropertyValue> PropertyValues
		{
			get
			{
				return this.propertyValues;
			}
		}

		public CsdlTypeReference Type
		{
			get
			{
				return this.type;
			}
		}

		public CsdlRecordExpression(CsdlTypeReference type, IEnumerable<CsdlPropertyValue> propertyValues, CsdlLocation location) : base(location)
		{
			this.type = type;
			this.propertyValues = new List<CsdlPropertyValue>(propertyValues);
		}
	}
}