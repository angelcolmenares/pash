namespace Microsoft.Data.Edm
{
	internal interface IEdmNavigationTargetMapping
	{
		IEdmNavigationProperty NavigationProperty
		{
			get;
		}

		IEdmEntitySet TargetEntitySet
		{
			get;
		}

	}
}