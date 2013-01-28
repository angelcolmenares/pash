using Microsoft.Data.Edm;
using System;

namespace Microsoft.Data.Edm.Library.Internal
{
	internal class AmbiguousTypeBinding : AmbiguousBinding<IEdmSchemaType>, IEdmSchemaType, IEdmSchemaElement, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmType, IEdmElement
	{
		private readonly string namespaceName;

		public string Namespace
		{
			get
			{
				return this.namespaceName;
			}
		}

		public EdmSchemaElementKind SchemaElementKind
		{
			get
			{
				return EdmSchemaElementKind.TypeDefinition;
			}
		}

		public EdmTypeKind TypeKind
		{
			get
			{
				return EdmTypeKind.None;
			}
		}

		public AmbiguousTypeBinding(IEdmSchemaType first, IEdmSchemaType second) : base(first, second)
		{
			string @namespace = first.Namespace;
			string empty = @namespace;
			if (@namespace == null)
			{
				empty = string.Empty;
			}
			this.namespaceName = empty;
		}
	}
}