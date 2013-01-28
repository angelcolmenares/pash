using Microsoft.Data.Edm.Csdl;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal abstract class CsdlElementWithDocumentation : CsdlElement
	{
		private readonly CsdlDocumentation documentation;

		public CsdlDocumentation Documentation
		{
			get
			{
				return this.documentation;
			}
		}

		public override bool HasDirectValueAnnotations
		{
			get
			{
				if (this.documentation != null)
				{
					return true;
				}
				else
				{
					return base.HasDirectValueAnnotations;
				}
			}
		}

		public CsdlElementWithDocumentation(CsdlDocumentation documentation, CsdlLocation location) : base(location)
		{
			this.documentation = documentation;
		}
	}
}