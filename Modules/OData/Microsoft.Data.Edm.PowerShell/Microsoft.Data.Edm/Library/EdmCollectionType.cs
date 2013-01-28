using Microsoft.Data.Edm;

namespace Microsoft.Data.Edm.Library
{
	internal class EdmCollectionType : EdmType, IEdmCollectionType, IEdmType, IEdmElement
	{
		private readonly IEdmTypeReference elementType;

		public IEdmTypeReference ElementType
		{
			get
			{
				return this.elementType;
			}
		}

		public override EdmTypeKind TypeKind
		{
			get
			{
				return EdmTypeKind.Collection;
			}
		}

		public EdmCollectionType(IEdmTypeReference elementType)
		{
			EdmUtil.CheckArgumentNull<IEdmTypeReference>(elementType, "elementType");
			this.elementType = elementType;
		}
	}
}