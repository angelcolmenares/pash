using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.Edm.Validation;
using Microsoft.Data.Edm.Values;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Library.Internal
{
	internal class BadEnumMember : BadElement, IEdmEnumMember, IEdmNamedElement, IEdmElement
	{
		private readonly string name;

		private readonly IEdmEnumType declaringType;

		public IEdmEnumType DeclaringType
		{
			get
			{
				return this.declaringType;
			}
		}

		public string Name
		{
			get
			{
				return this.name;
			}
		}

		public IEdmPrimitiveValue Value
		{
			get
			{
				return new BadPrimitiveValue(new EdmPrimitiveTypeReference(this.declaringType.UnderlyingType, false), base.Errors);
			}
		}

		public BadEnumMember(IEdmEnumType declaringType, string name, IEnumerable<EdmError> errors) : base(errors)
		{
			string str = name;
			string empty = str;
			if (str == null)
			{
				empty = string.Empty;
			}
			this.name = empty;
			this.declaringType = declaringType;
		}
	}
}