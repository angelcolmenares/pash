namespace Microsoft.Data.Edm
{
	internal interface IEdmEntityContainerElement : IEdmNamedElement, IEdmVocabularyAnnotatable, IEdmElement
	{
		IEdmEntityContainer Container
		{
			get;
		}

		EdmContainerElementKind ContainerElementKind
		{
			get;
		}

	}
}