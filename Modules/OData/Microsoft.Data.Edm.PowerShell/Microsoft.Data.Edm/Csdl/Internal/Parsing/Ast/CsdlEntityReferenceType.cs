using Microsoft.Data.Edm.Csdl;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlEntityReferenceType : CsdlElement, ICsdlTypeExpression
	{
		private readonly CsdlTypeReference entityType;

		public CsdlTypeReference EntityType
		{
			get
			{
				return this.entityType;
			}
		}

		public CsdlEntityReferenceType(CsdlTypeReference entityType, CsdlLocation location) : base(location)
		{
			this.entityType = entityType;
		}
	}
}