using Microsoft.Data.Edm.Csdl;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlFunction : CsdlFunctionBase
	{
		private readonly string definingExpression;

		public string DefiningExpression
		{
			get
			{
				return this.definingExpression;
			}
		}

		public CsdlFunction(string name, IEnumerable<CsdlFunctionParameter> parameters, string definingExpression, CsdlTypeReference returnType, CsdlDocumentation documentation, CsdlLocation location) : base(name, parameters, returnType, documentation, location)
		{
			this.definingExpression = definingExpression;
		}
	}
}