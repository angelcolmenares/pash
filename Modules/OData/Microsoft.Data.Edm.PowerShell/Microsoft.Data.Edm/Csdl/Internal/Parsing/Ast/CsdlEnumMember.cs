using Microsoft.Data.Edm.Csdl;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlEnumMember : CsdlNamedElement
	{
		public long? Value
		{
			get;
			set;
		}

		public CsdlEnumMember(string name, long? value, CsdlDocumentation documentation, CsdlLocation location) : base(name, documentation, location)
		{
			this.Value = value;
		}
	}
}