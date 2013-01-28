using Microsoft.Data.Edm;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Library
{
	internal sealed class EdmNavigationPropertyInfo
	{
		public bool ContainsTarget
		{
			get;
			set;
		}

		public IEnumerable<IEdmStructuralProperty> DependentProperties
		{
			get;
			set;
		}

		public string Name
		{
			get;
			set;
		}

		public EdmOnDeleteAction OnDelete
		{
			get;
			set;
		}

		public IEdmEntityType Target
		{
			get;
			set;
		}

		public EdmMultiplicity TargetMultiplicity
		{
			get;
			set;
		}

		public EdmNavigationPropertyInfo()
		{
		}

		public EdmNavigationPropertyInfo Clone()
		{
			EdmNavigationPropertyInfo edmNavigationPropertyInfo = new EdmNavigationPropertyInfo();
			edmNavigationPropertyInfo.Name = this.Name;
			edmNavigationPropertyInfo.Target = this.Target;
			edmNavigationPropertyInfo.TargetMultiplicity = this.TargetMultiplicity;
			edmNavigationPropertyInfo.DependentProperties = this.DependentProperties;
			edmNavigationPropertyInfo.ContainsTarget = this.ContainsTarget;
			edmNavigationPropertyInfo.OnDelete = this.OnDelete;
			return edmNavigationPropertyInfo;
		}
	}
}