using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal abstract class UnresolvedVocabularyTerm : EdmElement, IEdmTerm, IEdmSchemaElement, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement, IUnresolvedElement
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

		public abstract EdmSchemaElementKind SchemaElementKind
		{
			get;
		}

		public abstract EdmTermKind TermKind
		{
			get;
		}

		protected UnresolvedVocabularyTerm(string qualifiedName)
		{
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