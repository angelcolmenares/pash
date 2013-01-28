using Microsoft.Data.Edm.Csdl;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlUsing : CsdlElementWithDocumentation
	{
		private readonly string @alias;

		private readonly string namespaceName;

		public string Alias
		{
			get
			{
				return this.@alias;
			}
		}

		public string Namespace
		{
			get
			{
				return this.namespaceName;
			}
		}

		public CsdlUsing(string namespaceName, string alias, CsdlDocumentation documentation, CsdlLocation location) : base(documentation, location)
		{
			this.@alias = alias;
			this.namespaceName = namespaceName;
		}
	}
}