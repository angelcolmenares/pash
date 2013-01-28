using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlTemporalTypeReference : CsdlPrimitiveTypeReference
	{
		private readonly int? precision;

		public int? Precision
		{
			get
			{
				return this.precision;
			}
		}

		public CsdlTemporalTypeReference(EdmPrimitiveTypeKind kind, int? precision, string typeName, bool isNullable, CsdlLocation location) : base(kind, typeName, isNullable, location)
		{
			this.precision = precision;
		}
	}
}