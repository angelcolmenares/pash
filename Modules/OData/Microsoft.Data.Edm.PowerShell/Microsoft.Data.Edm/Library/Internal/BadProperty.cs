using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Internal;
using Microsoft.Data.Edm.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Library.Internal
{
	internal class BadProperty : BadElement, IEdmStructuralProperty, IEdmProperty, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		private readonly string name;

		private readonly IEdmStructuredType declaringType;

		private readonly Cache<BadProperty, IEdmTypeReference> type;

		private readonly static Func<BadProperty, IEdmTypeReference> ComputeTypeFunc;

		public EdmConcurrencyMode ConcurrencyMode
		{
			get
			{
				return EdmConcurrencyMode.None;
			}
		}

		public IEdmStructuredType DeclaringType
		{
			get
			{
				return this.declaringType;
			}
		}

		public string DefaultValueString
		{
			get
			{
				return null;
			}
		}

		public string Name
		{
			get
			{
				return this.name;
			}
		}

		public EdmPropertyKind PropertyKind
		{
			get
			{
				return EdmPropertyKind.None;
			}
		}

		public IEdmTypeReference Type
		{
			get
			{
				return this.type.GetValue(this, BadProperty.ComputeTypeFunc, null);
			}
		}

		static BadProperty()
		{
			BadProperty.ComputeTypeFunc = (BadProperty me) => me.ComputeType();
		}

		public BadProperty(IEdmStructuredType declaringType, string name, IEnumerable<EdmError> errors) : base(errors)
		{
			this.type = new Cache<BadProperty, IEdmTypeReference>();
			string str = name;
			string empty = str;
			if (str == null)
			{
				empty = string.Empty;
			}
			this.name = empty;
			this.declaringType = declaringType;
		}

		private IEdmTypeReference ComputeType()
		{
			return new BadTypeReference(new BadType(base.Errors), true);
		}

		public override string ToString()
		{
			string str;
			EdmError edmError = base.Errors.FirstOrDefault<EdmError>();
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