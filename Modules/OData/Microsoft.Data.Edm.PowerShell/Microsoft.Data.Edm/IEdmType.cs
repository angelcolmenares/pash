namespace Microsoft.Data.Edm
{
	internal interface IEdmType : IEdmElement
	{
		EdmTypeKind TypeKind
		{
			get;
		}

	}
}