using Microsoft.Data.Edm;

namespace Microsoft.Data.Edm.Library
{
	internal class EdmEntityReferenceType : EdmType, IEdmEntityReferenceType, IEdmType, IEdmElement
	{
		private readonly IEdmEntityType entityType;

		public IEdmEntityType EntityType
		{
			get
			{
				return this.entityType;
			}
		}

		public override EdmTypeKind TypeKind
		{
			get
			{
				return EdmTypeKind.EntityReference;
			}
		}

		public EdmEntityReferenceType(IEdmEntityType entityType)
		{
			EdmUtil.CheckArgumentNull<IEdmEntityType>(entityType, "entityType");
			this.entityType = entityType;
		}
	}
}