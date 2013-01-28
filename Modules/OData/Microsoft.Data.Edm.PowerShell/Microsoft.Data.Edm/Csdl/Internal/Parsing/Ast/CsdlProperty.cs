using Microsoft.Data.Edm.Csdl;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlProperty : CsdlNamedElement
	{
		private readonly CsdlTypeReference type;

		private readonly string defaultValue;

		private readonly bool isFixedConcurrency;

		public string DefaultValue
		{
			get
			{
				return this.defaultValue;
			}
		}

		public bool IsFixedConcurrency
		{
			get
			{
				return this.isFixedConcurrency;
			}
		}

		public CsdlTypeReference Type
		{
			get
			{
				return this.type;
			}
		}

		public CsdlProperty(string name, CsdlTypeReference type, bool isFixedConcurrency, string defaultValue, CsdlDocumentation documentation, CsdlLocation location) : base(name, documentation, location)
		{
			this.type = type;
			this.isFixedConcurrency = isFixedConcurrency;
			this.defaultValue = defaultValue;
		}
	}
}