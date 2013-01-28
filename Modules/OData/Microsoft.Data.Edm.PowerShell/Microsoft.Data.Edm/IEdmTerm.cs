namespace Microsoft.Data.Edm
{
	internal interface IEdmTerm : IEdmSchemaElement, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		EdmTermKind TermKind
		{
			get;
		}

	}
}