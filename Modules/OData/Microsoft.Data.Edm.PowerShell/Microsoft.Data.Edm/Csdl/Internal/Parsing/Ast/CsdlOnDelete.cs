using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlOnDelete : CsdlElementWithDocumentation
	{
		private readonly EdmOnDeleteAction action;

		public EdmOnDeleteAction Action
		{
			get
			{
				return this.action;
			}
		}

		public CsdlOnDelete(EdmOnDeleteAction action, CsdlDocumentation documentation, CsdlLocation location) : base(documentation, location)
		{
			this.action = action;
		}
	}
}