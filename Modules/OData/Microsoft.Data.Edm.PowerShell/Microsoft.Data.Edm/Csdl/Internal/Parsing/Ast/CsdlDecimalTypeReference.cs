using Microsoft.Data.Edm.Csdl;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlDecimalTypeReference : CsdlPrimitiveTypeReference
	{
		private readonly int? precision;

		private readonly int? scale;

		public int? Precision
		{
			get
			{
				return this.precision;
			}
		}

		public int? Scale
		{
			get
			{
				return this.scale;
			}
		}

		public CsdlDecimalTypeReference(int? precision, int? scale, string typeName, bool isNullable, CsdlLocation location) : base((EdmPrimitiveTypeKind)6, typeName, isNullable, location)
		{
			this.precision = precision;
			this.scale = scale;
		}
	}
}