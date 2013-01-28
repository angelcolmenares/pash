using Microsoft.Data.Edm.Csdl;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlExpressionTypeReference : CsdlTypeReference
	{
		private readonly ICsdlTypeExpression typeExpression;

		public ICsdlTypeExpression TypeExpression
		{
			get
			{
				return this.typeExpression;
			}
		}

		public CsdlExpressionTypeReference(ICsdlTypeExpression typeExpression, bool isNullable, CsdlLocation location) : base(isNullable, location)
		{
			this.typeExpression = typeExpression;
		}
	}
}