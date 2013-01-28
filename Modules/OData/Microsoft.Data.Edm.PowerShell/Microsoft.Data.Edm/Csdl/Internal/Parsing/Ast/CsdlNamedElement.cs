using Microsoft.Data.Edm.Csdl;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal abstract class CsdlNamedElement : CsdlElementWithDocumentation
	{
		private readonly string name;

		public string Name
		{
			get
			{
				return this.name;
			}
		}

		protected CsdlNamedElement(string name, CsdlDocumentation documentation, CsdlLocation location) : base(documentation, location)
		{
			this.name = name;
		}
	}
}