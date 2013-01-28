using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Library.Internal
{
	internal class BadEntityContainer : BadElement, IEdmEntityContainer, IEdmSchemaElement, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		private readonly string namespaceName;

		private readonly string name;

		public IEnumerable<IEdmEntityContainerElement> Elements
		{
			get
			{
				return Enumerable.Empty<IEdmEntityContainerElement>();
			}
		}

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
				return EdmSchemaElementKind.EntityContainer;
			}
		}

		public BadEntityContainer(string qualifiedName, IEnumerable<EdmError> errors) : base(errors)
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

		public IEdmEntitySet FindEntitySet(string setName)
		{
			return null;
		}

		public IEnumerable<IEdmFunctionImport> FindFunctionImports(string functionName)
		{
			return null;
		}
	}
}