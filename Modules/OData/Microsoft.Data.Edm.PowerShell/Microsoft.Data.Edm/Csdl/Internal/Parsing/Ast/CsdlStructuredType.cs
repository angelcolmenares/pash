using Microsoft.Data.Edm.Csdl;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal abstract class CsdlStructuredType : CsdlElementWithDocumentation
	{
		protected List<CsdlProperty> properties;

		public IEnumerable<CsdlProperty> Properties
		{
			get
			{
				return this.properties;
			}
		}

		protected CsdlStructuredType(IEnumerable<CsdlProperty> properties, CsdlDocumentation documentation, CsdlLocation location) : base(documentation, location)
		{
			this.properties = new List<CsdlProperty>(properties);
		}
	}
}