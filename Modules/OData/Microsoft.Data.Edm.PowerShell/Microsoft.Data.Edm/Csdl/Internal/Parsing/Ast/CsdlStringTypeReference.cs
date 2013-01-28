using Microsoft.Data.Edm.Csdl;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlStringTypeReference : CsdlPrimitiveTypeReference
	{
		private readonly bool? isFixedLength;

		private readonly bool isUnbounded;

		private readonly int? maxLength;

		private readonly bool? isUnicode;

		private readonly string collation;

		public string Collation
		{
			get
			{
				return this.collation;
			}
		}

		public bool? IsFixedLength
		{
			get
			{
				return this.isFixedLength;
			}
		}

		public bool IsUnbounded
		{
			get
			{
				return this.isUnbounded;
			}
		}

		public bool? IsUnicode
		{
			get
			{
				return this.isUnicode;
			}
		}

		public int? MaxLength
		{
			get
			{
				return this.maxLength;
			}
		}

		public CsdlStringTypeReference(bool? isFixedLength, bool isUnbounded, int? maxLength, bool? isUnicode, string collation, string typeName, bool isNullable, CsdlLocation location) : base((EdmPrimitiveTypeKind)14, typeName, isNullable, location)
		{
			this.isFixedLength = isFixedLength;
			this.isUnbounded = isUnbounded;
			this.maxLength = maxLength;
			this.isUnicode = isUnicode;
			this.collation = collation;
		}
	}
}