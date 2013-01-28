using Microsoft.Data.Edm;

namespace Microsoft.Data.Edm.Library
{
	internal class EdmRowType : EdmStructuredType, IEdmRowType, IEdmStructuredType, IEdmType, IEdmElement
	{
		public override EdmTypeKind TypeKind
		{
			get
			{
				return EdmTypeKind.Row;
			}
		}

		public EdmRowType() : base(false, false, null)
		{
		}
	}
}