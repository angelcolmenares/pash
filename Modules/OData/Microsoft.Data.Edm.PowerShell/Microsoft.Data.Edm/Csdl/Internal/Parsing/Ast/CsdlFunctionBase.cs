using Microsoft.Data.Edm.Csdl;
using System;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal abstract class CsdlFunctionBase : CsdlNamedElement
	{
		private readonly List<CsdlFunctionParameter> parameters;

		private readonly CsdlTypeReference returnType;

		public IEnumerable<CsdlFunctionParameter> Parameters
		{
			get
			{
				return this.parameters;
			}
		}

		public CsdlTypeReference ReturnType
		{
			get
			{
				return this.returnType;
			}
		}

		protected CsdlFunctionBase(string name, IEnumerable<CsdlFunctionParameter> parameters, CsdlTypeReference returnType, CsdlDocumentation documentation, CsdlLocation location) : base(name, documentation, location)
		{
			this.parameters = new List<CsdlFunctionParameter>(parameters);
			this.returnType = returnType;
		}
	}
}