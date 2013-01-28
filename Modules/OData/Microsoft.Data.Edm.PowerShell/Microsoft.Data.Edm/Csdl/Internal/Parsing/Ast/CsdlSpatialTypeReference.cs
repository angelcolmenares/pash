using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlSpatialTypeReference : CsdlPrimitiveTypeReference
	{
		private readonly int? srid;

		public int? Srid
		{
			get
			{
				return this.srid;
			}
		}

		public CsdlSpatialTypeReference(EdmPrimitiveTypeKind kind, int? srid, string typeName, bool isNullable, CsdlLocation location) : base(kind, typeName, isNullable, location)
		{
			this.srid = srid;
		}
	}
}