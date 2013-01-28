using Microsoft.Data.Edm.Csdl;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlCollectionType : CsdlElement, ICsdlTypeExpression
	{
		private readonly CsdlTypeReference elementType;

		public CsdlTypeReference ElementType
		{
			get
			{
				return this.elementType;
			}
		}

		public CsdlCollectionType(CsdlTypeReference elementType, CsdlLocation location) : base(location)
		{
			this.elementType = elementType;
		}
	}
}