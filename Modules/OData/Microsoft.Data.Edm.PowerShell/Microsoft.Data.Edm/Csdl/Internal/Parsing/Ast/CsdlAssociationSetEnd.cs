using Microsoft.Data.Edm.Csdl;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlAssociationSetEnd : CsdlElementWithDocumentation
	{
		private readonly string role;

		private readonly string entitySet;

		public string EntitySet
		{
			get
			{
				return this.entitySet;
			}
		}

		public string Role
		{
			get
			{
				return this.role;
			}
		}

		public CsdlAssociationSetEnd(string role, string entitySet, CsdlDocumentation documentation, CsdlLocation location) : base(documentation, location)
		{
			this.role = role;
			this.entitySet = entitySet;
		}
	}
}