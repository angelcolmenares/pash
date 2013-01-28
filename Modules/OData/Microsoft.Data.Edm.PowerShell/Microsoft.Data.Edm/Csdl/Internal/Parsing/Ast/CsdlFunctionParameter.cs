using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl;
using System;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlFunctionParameter : CsdlNamedElement
	{
		private readonly CsdlTypeReference type;

		private readonly EdmFunctionParameterMode mode;

		public EdmFunctionParameterMode Mode
		{
			get
			{
				return this.mode;
			}
		}

		public CsdlTypeReference Type
		{
			get
			{
				return this.type;
			}
		}

		public CsdlFunctionParameter(string name, CsdlTypeReference type, EdmFunctionParameterMode mode, CsdlDocumentation documentation, CsdlLocation location) : base(name, documentation, location)
		{
			this.type = type;
			this.mode = mode;
		}
	}
}