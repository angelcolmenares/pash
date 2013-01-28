using System.Collections.Generic;

namespace Microsoft.Data.Edm
{
	internal interface IEdmEntityType : IEdmStructuredType, IEdmSchemaType, IEdmType, IEdmTerm, IEdmSchemaElement, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		IEnumerable<IEdmStructuralProperty> DeclaredKey
		{
			get;
		}

	}
}