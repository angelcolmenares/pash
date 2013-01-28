using Microsoft.Data.Edm;
using System;

namespace Microsoft.Data.Edm.Library
{
	internal class EdmStructuralProperty : EdmProperty, IEdmStructuralProperty, IEdmProperty, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		private readonly string defaultValueString;

		private readonly EdmConcurrencyMode concurrencyMode;

		public EdmConcurrencyMode ConcurrencyMode
		{
			get
			{
				return this.concurrencyMode;
			}
		}

		public string DefaultValueString
		{
			get
			{
				return this.defaultValueString;
			}
		}

		public override EdmPropertyKind PropertyKind
		{
			get
			{
				return EdmPropertyKind.Structural;
			}
		}

		public EdmStructuralProperty(IEdmStructuredType declaringType, string name, IEdmTypeReference type) : this(declaringType, name, type, null, 0)
		{
		}

		public EdmStructuralProperty(IEdmStructuredType declaringType, string name, IEdmTypeReference type, string defaultValueString, EdmConcurrencyMode concurrencyMode) : base(declaringType, name, type)
		{
			this.defaultValueString = defaultValueString;
			this.concurrencyMode = concurrencyMode;
		}
	}
}