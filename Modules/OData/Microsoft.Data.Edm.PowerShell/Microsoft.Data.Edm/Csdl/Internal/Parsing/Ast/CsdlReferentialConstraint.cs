using Microsoft.Data.Edm.Csdl;

namespace Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast
{
	internal class CsdlReferentialConstraint : CsdlElementWithDocumentation
	{
		private readonly CsdlReferentialConstraintRole principal;

		private readonly CsdlReferentialConstraintRole dependent;

		public CsdlReferentialConstraintRole Dependent
		{
			get
			{
				return this.dependent;
			}
		}

		public CsdlReferentialConstraintRole Principal
		{
			get
			{
				return this.principal;
			}
		}

		public CsdlReferentialConstraint(CsdlReferentialConstraintRole principal, CsdlReferentialConstraintRole dependent, CsdlDocumentation documentation, CsdlLocation location) : base(documentation, location)
		{
			this.principal = principal;
			this.dependent = dependent;
		}
	}
}