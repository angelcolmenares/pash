using Microsoft.Data.Edm.Csdl;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlEntitySet : CsdlNamedElement
	{
		private readonly string entityType;

		public string EntityType
		{
			get
			{
				return this.entityType;
			}
		}

		public CsdlEntitySet(string name, string entityType, CsdlDocumentation documentation, CsdlLocation location) : base(name, documentation, location)
		{
			this.entityType = entityType;
		}
	}
}