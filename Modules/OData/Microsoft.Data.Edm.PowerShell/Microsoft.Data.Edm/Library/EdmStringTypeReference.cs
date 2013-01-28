using Microsoft.Data.Edm;
using System;

namespace Microsoft.Data.Edm.Library
{
	internal class EdmStringTypeReference : EdmPrimitiveTypeReference, IEdmStringTypeReference, IEdmPrimitiveTypeReference, IEdmTypeReference, IEdmElement
	{
		private readonly bool isUnbounded;

		private readonly int? maxLength;

		private readonly bool? isFixedLength;

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

		public EdmStringTypeReference(IEdmPrimitiveType definition, bool isNullable) : this(definition, isNullable, false, null, null, null, null)
		{
		}

		public EdmStringTypeReference(IEdmPrimitiveType definition, bool isNullable, bool isUnbounded, int? maxLength, bool? isFixedLength, bool? isUnicode, string collation) : base(definition, isNullable)
		{
			if (!isUnbounded || !maxLength.HasValue)
			{
				this.isUnbounded = isUnbounded;
				this.maxLength = maxLength;
				this.isFixedLength = isFixedLength;
				this.isUnicode = isUnicode;
				this.collation = collation;
				return;
			}
			else
			{
				throw new InvalidOperationException(Strings.EdmModel_Validator_Semantic_IsUnboundedCannotBeTrueWhileMaxLengthIsNotNull);
			}
		}
	}
}