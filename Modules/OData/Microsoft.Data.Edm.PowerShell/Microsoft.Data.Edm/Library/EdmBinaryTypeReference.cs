using Microsoft.Data.Edm;
using System;

namespace Microsoft.Data.Edm.Library
{
	internal class EdmBinaryTypeReference : EdmPrimitiveTypeReference, IEdmBinaryTypeReference, IEdmPrimitiveTypeReference, IEdmTypeReference, IEdmElement
	{
		private readonly bool isUnbounded;

		private readonly int? maxLength;

		private readonly bool? isFixedLength;

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

		public EdmBinaryTypeReference(IEdmPrimitiveType definition, bool isNullable) : this(definition, isNullable, false, null, null)
		{
		}

		public EdmBinaryTypeReference(IEdmPrimitiveType definition, bool isNullable, bool isUnbounded, int? maxLength, bool? isFixedLength) : base(definition, isNullable)
		{
			if (!isUnbounded || !maxLength.HasValue)
			{
				this.isUnbounded = isUnbounded;
				this.maxLength = maxLength;
				this.isFixedLength = isFixedLength;
				return;
			}
			else
			{
				throw new InvalidOperationException(Strings.EdmModel_Validator_Semantic_IsUnboundedCannotBeTrueWhileMaxLengthIsNotNull);
			}
		}
	}
}