using Microsoft.Data.Edm;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class UnresolvedTypeTerm : UnresolvedVocabularyTerm, IEdmEntityType, IEdmStructuredType, IEdmSchemaType, IEdmType, IEdmTerm, IEdmSchemaElement, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		public IEdmStructuredType BaseType
		{
			get
			{
				return null;
			}
		}

		public IEnumerable<IEdmStructuralProperty> DeclaredKey
		{
			get
			{
				return Enumerable.Empty<IEdmStructuralProperty>();
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

		public override EdmSchemaElementKind SchemaElementKind
		{
			get
			{
				return EdmSchemaElementKind.TypeDefinition;
			}
		}

		public override EdmTermKind TermKind
		{
			get
			{
				return EdmTermKind.Type;
			}
		}

		public EdmTypeKind TypeKind
		{
			get
			{
				return EdmTypeKind.Entity;
			}
		}

		public UnresolvedTypeTerm(string qualifiedName) : base(qualifiedName)
		{
		}

		public IEdmProperty FindProperty(string name)
		{
			return null;
		}
	}
}