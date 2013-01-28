using Microsoft.Data.Edm.Csdl;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlPropertyReference : CsdlElement
	{
		private readonly string propertyName;

		public string PropertyName
		{
			get
			{
				return this.propertyName;
			}
		}

		public CsdlPropertyReference(string propertyName, CsdlLocation location) : base(location)
		{
			this.propertyName = propertyName;
		}
	}
}