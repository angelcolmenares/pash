namespace Microsoft.Data.Edm
{
	internal interface IEdmEntityReferenceType : IEdmType, IEdmElement
	{
		IEdmEntityType EntityType
		{
			get;
		}

	}
}