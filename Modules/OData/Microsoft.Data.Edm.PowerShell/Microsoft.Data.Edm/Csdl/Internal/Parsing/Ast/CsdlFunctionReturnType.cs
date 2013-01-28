using Microsoft.Data.Edm.Csdl;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlFunctionReturnType : CsdlElement
	{
		private readonly CsdlTypeReference returnType;

		public CsdlTypeReference ReturnType
		{
			get
			{
				return this.returnType;
			}
		}

		public CsdlFunctionReturnType(CsdlTypeReference returnType, CsdlLocation location) : base(location)
		{
			this.returnType = returnType;
		}
	}
}