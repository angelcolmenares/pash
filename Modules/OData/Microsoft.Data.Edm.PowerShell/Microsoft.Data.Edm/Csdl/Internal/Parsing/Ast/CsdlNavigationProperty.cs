using Microsoft.Data.Edm.Csdl;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlNavigationProperty : CsdlNamedElement
	{
		private readonly string relationship;

		private readonly string toRole;

		private readonly string fromRole;

		private readonly bool containsTarget;

		public bool ContainsTarget
		{
			get
			{
				return this.containsTarget;
			}
		}

		public string FromRole
		{
			get
			{
				return this.fromRole;
			}
		}

		public string Relationship
		{
			get
			{
				return this.relationship;
			}
		}

		public string ToRole
		{
			get
			{
				return this.toRole;
			}
		}

		public CsdlNavigationProperty(string name, string relationship, string toRole, string fromRole, bool containsTarget, CsdlDocumentation documentation, CsdlLocation location) : base(name, documentation, location)
		{
			this.relationship = relationship;
			this.toRole = toRole;
			this.fromRole = fromRole;
			this.containsTarget = containsTarget;
		}
	}
}