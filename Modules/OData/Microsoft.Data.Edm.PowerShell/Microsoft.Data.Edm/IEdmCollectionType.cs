namespace Microsoft.Data.Edm
{
	internal interface IEdmCollectionType : IEdmType, IEdmElement
	{
		IEdmTypeReference ElementType
		{
			get;
		}

	}
}