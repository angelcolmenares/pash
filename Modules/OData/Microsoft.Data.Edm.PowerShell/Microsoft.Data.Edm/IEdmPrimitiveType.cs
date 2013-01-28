namespace Microsoft.Data.Edm
{
	internal interface IEdmPrimitiveType : IEdmSchemaType, IEdmSchemaElement, IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmType, IEdmElement
	{
		EdmPrimitiveTypeKind PrimitiveKind
		{
			get;
		}

	}
}