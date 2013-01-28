using Microsoft.Data.Edm.Csdl;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlComplexType : CsdlNamedStructuredType
	{
		public CsdlComplexType(string name, string baseTypeName, bool isAbstract, IEnumerable<CsdlProperty> properties, CsdlDocumentation documentation, CsdlLocation location) : base(name, baseTypeName, isAbstract, properties, documentation, location)
		{
		}
	}
}