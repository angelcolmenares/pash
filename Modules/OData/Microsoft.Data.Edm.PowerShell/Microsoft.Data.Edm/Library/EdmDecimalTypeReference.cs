using Microsoft.Data.Edm;
using System;

namespace Microsoft.Data.Edm.Library
{
	internal class EdmDecimalTypeReference : EdmPrimitiveTypeReference, IEdmDecimalTypeReference, IEdmPrimitiveTypeReference, IEdmTypeReference, IEdmElement
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

		public EdmDecimalTypeReference(IEdmPrimitiveType definition, bool isNullable) : this(definition, isNullable, null, null)
		{
		}

		public EdmDecimalTypeReference(IEdmPrimitiveType definition, bool isNullable, int? precision, int? scale) : base(definition, isNullable)
		{
			this.precision = precision;
			this.scale = scale;
		}
	}
}