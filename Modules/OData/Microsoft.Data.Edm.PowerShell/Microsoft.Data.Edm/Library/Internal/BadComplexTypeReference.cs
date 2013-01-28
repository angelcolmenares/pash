using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using Microsoft.Data.Edm.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Library.Internal
{
	internal class BadComplexTypeReference : EdmComplexTypeReference, IEdmCheckable
	{
		private readonly IEnumerable<EdmError> errors;

		public IEnumerable<EdmError> Errors
		{
			get
			{
				return this.errors;
			}
		}

		public BadComplexTypeReference(string qualifiedName, bool isNullable, IEnumerable<EdmError> errors) : base(new BadComplexType(qualifiedName, errors), isNullable)
		{
			this.errors = errors;
		}

		public override string ToString()
		{
			string str;
			EdmError edmError = this.Errors.FirstOrDefault<EdmError>();
			if (edmError != null)
			{
				str = string.Concat(edmError.ErrorCode.ToString(), ":");
			}
			else
			{
				str = "";
			}
			string str1 = str;
			return string.Concat(str1, this.ToTraceString());
		}
	}
}