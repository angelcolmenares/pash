using Microsoft.Data.Edm.Csdl;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlAssociationSet : CsdlNamedElement
	{
		private readonly string association;

		private readonly CsdlAssociationSetEnd end1;

		private readonly CsdlAssociationSetEnd end2;

		public string Association
		{
			get
			{
				return this.association;
			}
		}

		public CsdlAssociationSetEnd End1
		{
			get
			{
				return this.end1;
			}
		}

		public CsdlAssociationSetEnd End2
		{
			get
			{
				return this.end2;
			}
		}

		public CsdlAssociationSet(string name, string association, CsdlAssociationSetEnd end1, CsdlAssociationSetEnd end2, CsdlDocumentation documentation, CsdlLocation location) : base(name, documentation, location)
		{
			this.association = association;
			this.end1 = end1;
			this.end2 = end2;
		}
	}
}