using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Validation;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Library.Internal
{
	internal class BadPrimitiveType : BadType, IEdmPrimitiveType, IEdmSchemaType, IEdmSchemaElement, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmType, IEdmElement
	{
		private readonly EdmPrimitiveTypeKind primitiveKind;

		private readonly string name;

		private readonly string namespaceName;

		public string Name
		{
			get
			{
				return this.name;
			}
		}

		public string Namespace
		{
			get
			{
				return this.namespaceName;
			}
		}

		public EdmPrimitiveTypeKind PrimitiveKind
		{
			get
			{
				return this.primitiveKind;
			}
		}

		public EdmSchemaElementKind SchemaElementKind
		{
			get
			{
				return EdmSchemaElementKind.TypeDefinition;
			}
		}

		public override EdmTypeKind TypeKind
		{
			get
			{
				return EdmTypeKind.Primitive;
			}
		}

		public BadPrimitiveType(string qualifiedName, EdmPrimitiveTypeKind primitiveKind, IEnumerable<EdmError> errors) : base(errors)
		{
			this.primitiveKind = primitiveKind;
			string str = qualifiedName;
			string empty = str;
			if (str == null)
			{
				empty = string.Empty;
			}
			qualifiedName = empty;
			EdmUtil.TryGetNamespaceNameFromQualifiedName(qualifiedName, out this.namespaceName, out this.name);
		}
	}
}