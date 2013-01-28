using Microsoft.Data.Edm;
using System;

namespace Microsoft.Data.Edm.Library
{
	internal class EdmSpatialTypeReference : EdmPrimitiveTypeReference, IEdmSpatialTypeReference, IEdmPrimitiveTypeReference, IEdmTypeReference, IEdmElement
	{
		private readonly int? spatialReferenceIdentifier;

		public int? SpatialReferenceIdentifier
		{
			get
			{
				return this.spatialReferenceIdentifier;
			}
		}

		public EdmSpatialTypeReference(IEdmPrimitiveType definition, bool isNullable) : this(definition, isNullable, null)
		{
			EdmUtil.CheckArgumentNull<IEdmPrimitiveType>(definition, "definition");
			EdmPrimitiveTypeKind primitiveKind = definition.PrimitiveKind;
			switch (primitiveKind)
			{
				case EdmPrimitiveTypeKind.Geography:
				case EdmPrimitiveTypeKind.GeographyPoint:
				case EdmPrimitiveTypeKind.GeographyLineString:
				case EdmPrimitiveTypeKind.GeographyPolygon:
				case EdmPrimitiveTypeKind.GeographyCollection:
				case EdmPrimitiveTypeKind.GeographyMultiPolygon:
				case EdmPrimitiveTypeKind.GeographyMultiLineString:
				case EdmPrimitiveTypeKind.GeographyMultiPoint:
				{
					this.spatialReferenceIdentifier = new int?(0x10e6);
					return;
				}
			}
			this.spatialReferenceIdentifier = new int?(0);
		}

		public EdmSpatialTypeReference(IEdmPrimitiveType definition, bool isNullable, int? spatialReferenceIdentifier) : base(definition, isNullable)
		{
			this.spatialReferenceIdentifier = spatialReferenceIdentifier;
		}
	}
}