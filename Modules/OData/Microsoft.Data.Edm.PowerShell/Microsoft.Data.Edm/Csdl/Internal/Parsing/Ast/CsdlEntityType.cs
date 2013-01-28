using Microsoft.Data.Edm.Csdl;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlEntityType : CsdlNamedStructuredType
	{
		private readonly CsdlKey key;

		private readonly bool isOpen;

		private readonly List<CsdlNavigationProperty> navigationProperties;

		public bool IsOpen
		{
			get
			{
				return this.isOpen;
			}
		}

		public CsdlKey Key
		{
			get
			{
				return this.key;
			}
		}

		public IEnumerable<CsdlNavigationProperty> NavigationProperties
		{
			get
			{
				return this.navigationProperties;
			}
		}

		public CsdlEntityType(string name, string baseTypeName, bool isAbstract, bool isOpen, CsdlKey key, IEnumerable<CsdlProperty> properties, IEnumerable<CsdlNavigationProperty> navigationProperties, CsdlDocumentation documentation, CsdlLocation location) : base(name, baseTypeName, isAbstract, properties, documentation, location)
		{
			this.isOpen = isOpen;
			this.key = key;
			this.navigationProperties = new List<CsdlNavigationProperty>(navigationProperties);
		}
	}
}