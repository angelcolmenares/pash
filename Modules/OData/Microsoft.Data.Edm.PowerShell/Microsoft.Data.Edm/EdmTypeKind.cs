namespace Microsoft.Data.Edm
{
	internal enum EdmTypeKind
	{
		None,
		Primitive,
		Entity,
		Complex,
		Row,
		Collection,
		EntityReference,
		Enum
	}
}