using Microsoft.Data.Edm;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Data.Edm.Library.Internal
{
	internal class AmbiguousEntityContainerBinding : AmbiguousBinding<IEdmEntityContainer>, IEdmEntityContainer, IEdmSchemaElement, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		private readonly string namespaceName;

		public IEnumerable<IEdmEntityContainerElement> Elements
		{
			get
			{
				return Enumerable.Empty<IEdmEntityContainerElement>();
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

		public AmbiguousEntityContainerBinding(IEdmEntityContainer first, IEdmEntityContainer second) : base(first, second)
		{
			string @namespace = first.Namespace;
			string empty = @namespace;
			if (@namespace == null)
			{
				empty = string.Empty;
			}
			this.namespaceName = empty;
		}

		public IEdmEntitySet FindEntitySet(string name)
		{
			return null;
		}

		public IEnumerable<IEdmFunctionImport> FindFunctionImports(string name)
		{
			return null;
		}
	}
}