namespace Microsoft.Data.Edm
{
	internal interface IEdmValueTerm : IEdmTerm, IEdmSchemaElement, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		IEdmTypeReference Type
		{
			get;
		}

	}
}