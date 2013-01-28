using Microsoft.Data.Edm.Csdl;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlAssociation : CsdlNamedElement
	{
		private readonly CsdlReferentialConstraint constraint;

		private readonly CsdlAssociationEnd end1;

		private readonly CsdlAssociationEnd end2;

		public CsdlReferentialConstraint Constraint
		{
			get
			{
				return this.constraint;
			}
		}

		public CsdlAssociationEnd End1
		{
			get
			{
				return this.end1;
			}
		}

		public CsdlAssociationEnd End2
		{
			get
			{
				return this.end2;
			}
		}

		public CsdlAssociation(string name, CsdlAssociationEnd end1, CsdlAssociationEnd end2, CsdlReferentialConstraint constraint, CsdlDocumentation documentation, CsdlLocation location) : base(name, documentation, location)
		{
			this.end1 = end1;
			this.end2 = end2;
			this.constraint = constraint;
		}
	}
}