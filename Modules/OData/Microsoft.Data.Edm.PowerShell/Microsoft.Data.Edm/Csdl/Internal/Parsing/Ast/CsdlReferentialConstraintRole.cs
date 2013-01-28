using Microsoft.Data.Edm.Csdl;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlReferentialConstraintRole : CsdlElementWithDocumentation
	{
		private readonly string role;

		private readonly List<CsdlPropertyReference> properties;

		public IEnumerable<CsdlPropertyReference> Properties
		{
			get
			{
				return this.properties;
			}
		}

		public string Role
		{
			get
			{
				return this.role;
			}
		}

		public CsdlReferentialConstraintRole(string role, IEnumerable<CsdlPropertyReference> properties, CsdlDocumentation documentation, CsdlLocation location) : base(documentation, location)
		{
			this.role = role;
			this.properties = new List<CsdlPropertyReference>(properties);
		}

		public int IndexOf(CsdlPropertyReference reference)
		{
			return this.properties.IndexOf(reference);
		}
	}
}