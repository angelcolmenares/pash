using Microsoft.Data.Edm.Csdl;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlRowType : CsdlStructuredType, ICsdlTypeExpression
	{
		public CsdlRowType(IEnumerable<CsdlProperty> properties, CsdlLocation location) : base(properties, null, location)
		{
		}
	}
}