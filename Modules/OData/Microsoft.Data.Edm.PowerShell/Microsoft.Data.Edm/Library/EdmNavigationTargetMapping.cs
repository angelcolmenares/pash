using Microsoft.Data.Edm;

namespace Microsoft.Data.Edm.Library
{
	internal class EdmNavigationTargetMapping : IEdmNavigationTargetMapping
	{
		private IEdmNavigationProperty navigationProperty;

		private IEdmEntitySet targetEntitySet;

		public IEdmNavigationProperty NavigationProperty
		{
			get
			{
				return this.navigationProperty;
			}
		}

		public IEdmEntitySet TargetEntitySet
		{
			get
			{
				return this.targetEntitySet;
			}
		}

		public EdmNavigationTargetMapping(IEdmNavigationProperty navigationProperty, IEdmEntitySet targetEntitySet)
		{
			this.navigationProperty = navigationProperty;
			this.targetEntitySet = targetEntitySet;
		}
	}
}