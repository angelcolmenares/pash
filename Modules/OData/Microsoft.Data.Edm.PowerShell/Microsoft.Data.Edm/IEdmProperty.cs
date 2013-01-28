namespace Microsoft.Data.Edm
{
	internal interface IEdmProperty : IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		IEdmStructuredType DeclaringType
		{
			get;
		}

		EdmPropertyKind PropertyKind
		{
			get;
		}

		IEdmTypeReference Type
		{
			get;
		}

	}
}