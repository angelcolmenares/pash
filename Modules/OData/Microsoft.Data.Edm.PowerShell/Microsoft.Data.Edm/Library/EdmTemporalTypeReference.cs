using Microsoft.Data.Edm;
using System;

namespace Microsoft.Data.Edm.Library
{
	internal class EdmTemporalTypeReference : EdmPrimitiveTypeReference, IEdmTemporalTypeReference, IEdmPrimitiveTypeReference, IEdmTypeReference, IEdmElement
	{
		private readonly int? precision;

		public int? Precision
		{
			get
			{
				return this.precision;
			}
		}

		public EdmTemporalTypeReference(IEdmPrimitiveType definition, bool isNullable) : this(definition, isNullable, null)
		{
		}

		public EdmTemporalTypeReference(IEdmPrimitiveType definition, bool isNullable, int? precision) : base(definition, isNullable)
		{
			this.precision = precision;
		}
	}
}