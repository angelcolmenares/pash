using System;

namespace Microsoft.Data.Edm
{
	internal interface IEdmSpatialTypeReference : IEdmPrimitiveTypeReference, IEdmTypeReference, IEdmElement
	{
		int? SpatialReferenceIdentifier
		{
			get;
		}

	}
}