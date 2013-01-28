using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Library.Internal
{
	internal abstract class BadStructuredType : BadType, IEdmStructuredType, IEdmType, IEdmElement, IEdmCheckable
	{
		public IEdmStructuredType BaseType
		{
			get
			{
				return null;
			}
		}

		public IEnumerable<IEdmProperty> DeclaredProperties
		{
			get
			{
				return Enumerable.Empty<IEdmProperty>();
			}
		}

		public bool IsAbstract
		{
			get
			{
				return false;
			}
		}

		public bool IsOpen
		{
			get
			{
				return false;
			}
		}

		protected BadStructuredType(IEnumerable<EdmError> errors) : base(errors)
		{
		}

		public IEdmProperty FindProperty(string name)
		{
			return null;
		}
	}
}