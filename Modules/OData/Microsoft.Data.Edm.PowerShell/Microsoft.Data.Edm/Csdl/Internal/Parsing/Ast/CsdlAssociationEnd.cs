using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlAssociationEnd : CsdlNamedElement
	{
		private readonly CsdlTypeReference type;

		private readonly EdmMultiplicity multiplicity;

		private readonly CsdlOnDelete onDelete;

		public EdmMultiplicity Multiplicity
		{
			get
			{
				return this.multiplicity;
			}
		}

		public CsdlOnDelete OnDelete
		{
			get
			{
				return this.onDelete;
			}
		}

		public CsdlTypeReference Type
		{
			get
			{
				return this.type;
			}
		}

		public CsdlAssociationEnd(string role, CsdlTypeReference type, EdmMultiplicity multiplicity, CsdlOnDelete onDelete, CsdlDocumentation documentation, CsdlLocation location) : base(role, documentation, location)
		{
			this.type = type;
			this.multiplicity = multiplicity;
			this.onDelete = onDelete;
		}
	}
}