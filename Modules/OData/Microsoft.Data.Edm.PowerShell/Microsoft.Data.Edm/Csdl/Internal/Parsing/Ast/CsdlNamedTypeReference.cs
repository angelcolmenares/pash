using Microsoft.Data.Edm.Csdl;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlNamedTypeReference : CsdlTypeReference
	{
		private readonly string fullName;

		public string FullName
		{
			get
			{
				return this.fullName;
			}
		}

		public CsdlNamedTypeReference(string fullName, bool isNullable, CsdlLocation location) : base(isNullable, location)
		{
			this.fullName = fullName;
		}
	}
}