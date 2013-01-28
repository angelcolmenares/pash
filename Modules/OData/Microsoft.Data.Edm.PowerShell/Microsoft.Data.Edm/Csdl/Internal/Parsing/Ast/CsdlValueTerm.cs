using Microsoft.Data.Edm.Csdl;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlValueTerm : CsdlNamedElement
	{
		private readonly CsdlTypeReference type;

		public CsdlTypeReference Type
		{
			get
			{
				return this.type;
			}
		}

		public CsdlValueTerm(string name, CsdlTypeReference type, CsdlDocumentation documentation, CsdlLocation location) : base(name, documentation, location)
		{
			this.type = type;
		}
	}
}