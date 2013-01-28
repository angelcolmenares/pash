using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl.Internal.Parsing.Ast;

namespace Microsoft.Data.Edm.Csdl.Internal.CsdlSemantics
{
	internal class CsdlSemanticsRowTypeDefinition : CsdlSemanticsStructuredTypeDefinition, IEdmRowType, IEdmStructuredType, IEdmType, IEdmElement
	{
		private readonly CsdlRowType row;

		public override IEdmStructuredType BaseType
		{
			get
			{
				return null;
			}
		}

		protected override CsdlStructuredType MyStructured
		{
			get
			{
				return this.row;
			}
		}

		public override EdmTypeKind TypeKind
		{
			get
			{
				return EdmTypeKind.Row;
			}
		}

		public CsdlSemanticsRowTypeDefinition(CsdlSemanticsSchema context, CsdlRowType row) : base(context, row)
		{
			this.row = row;
		}
	}
}