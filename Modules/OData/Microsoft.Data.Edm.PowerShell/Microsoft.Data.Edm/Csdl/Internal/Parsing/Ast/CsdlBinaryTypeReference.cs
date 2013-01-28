using Microsoft.Data.Edm.Csdl;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlBinaryTypeReference : CsdlPrimitiveTypeReference
	{
		private readonly bool? isFixedLength;

		private readonly bool isUnbounded;

		private readonly int? maxLength;

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

		public int? MaxLength
		{
			get
			{
				return this.maxLength;
			}
		}

		public CsdlBinaryTypeReference(bool? isFixedLength, bool isUnbounded, int? maxLength, string typeName, bool isNullable, CsdlLocation location) : base((EdmPrimitiveTypeKind)1, typeName, isNullable, location)
		{
			this.isFixedLength = isFixedLength;
			this.isUnbounded = isUnbounded;
			this.maxLength = maxLength;
		}
	}
}