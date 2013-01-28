using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlPrimitiveTypeReference : CsdlNamedTypeReference
	{
		private readonly EdmPrimitiveTypeKind kind;

		public EdmPrimitiveTypeKind Kind
		{
			get
			{
				return this.kind;
			}
		}

		public CsdlPrimitiveTypeReference(EdmPrimitiveTypeKind kind, string typeName, bool isNullable, CsdlLocation location) : base(typeName, isNullable, location)
		{
			this.kind = kind;
		}
	}
}