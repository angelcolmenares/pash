using Microsoft.Data.Edm.Csdl;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlPropertyValue : CsdlElement
	{
		private readonly CsdlExpressionBase expression;

		private readonly string property;

		public CsdlExpressionBase Expression
		{
			get
			{
				return this.expression;
			}
		}

		public string Property
		{
			get
			{
				return this.property;
			}
		}

		public CsdlPropertyValue(string property, CsdlExpressionBase expression, CsdlLocation location) : base(location)
		{
			this.property = property;
			this.expression = expression;
		}
	}
}