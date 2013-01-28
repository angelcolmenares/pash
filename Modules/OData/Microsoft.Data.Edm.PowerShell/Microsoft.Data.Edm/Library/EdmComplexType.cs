using Microsoft.Data.Edm;
using System;

namespace Microsoft.Data.Edm.Library
{
	internal class EdmComplexType : EdmStructuredType, IEdmComplexType, IEdmStructuredType, IEdmSchemaType, IEdmType, IEdmTerm, IEdmSchemaElement, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		private readonly string namespaceName;

		private readonly string name;

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

		public EdmSchemaElementKind SchemaElementKind
		{
			get
			{
				return EdmSchemaElementKind.TypeDefinition;
			}
		}

		public EdmTermKind TermKind
		{
			get
			{
				return EdmTermKind.Type;
			}
		}

		public override EdmTypeKind TypeKind
		{
			get
			{
				return EdmTypeKind.Complex;
			}
		}

		public EdmComplexType(string namespaceName, string name) : this(namespaceName, name, null, false)
		{
		}

		public EdmComplexType(string namespaceName, string name, IEdmComplexType baseType, bool isAbstract) : base(isAbstract, false, baseType)
		{
			EdmUtil.CheckArgumentNull<string>(namespaceName, "namespaceName");
			EdmUtil.CheckArgumentNull<string>(name, "name");
			this.namespaceName = namespaceName;
			this.name = name;
		}
	}
}