using Microsoft.Data.Edm.Csdl;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlKey : CsdlElement
	{
		private readonly List<CsdlPropertyReference> properties;

		public IEnumerable<CsdlPropertyReference> Properties
		{
			get
			{
				return this.properties;
			}
		}

		public CsdlKey(IEnumerable<CsdlPropertyReference> properties, CsdlLocation location) : base(location)
		{
			this.properties = new List<CsdlPropertyReference>(properties);
		}
	}
}