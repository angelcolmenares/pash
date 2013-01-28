using Microsoft.Data.Edm.Csdl;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlDocumentation : CsdlElement
	{
		private readonly string summary;

		private readonly string longDescription;

		public string LongDescription
		{
			get
			{
				return this.longDescription;
			}
		}

		public string Summary
		{
			get
			{
				return this.summary;
			}
		}

		public CsdlDocumentation(string summary, string longDescription, CsdlLocation location) : base(location)
		{
			this.summary = summary;
			this.longDescription = longDescription;
		}
	}
}