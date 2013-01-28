using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm
{
	internal interface IEdmEntityContainer : IEdmSchemaElement, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		IEnumerable<IEdmEntityContainerElement> Elements
		{
			get;
		}

		IEdmEntitySet FindEntitySet(string setName);

		IEnumerable<IEdmFunctionImport> FindFunctionImports(string functionName);
	}
}