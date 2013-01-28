using System;

namespace Microsoft.Data.Edm
{
	internal interface IEdmSchemaElement : IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		string Namespace
		{
			get;
		}

		EdmSchemaElementKind SchemaElementKind
		{
			get;
		}

	}
}