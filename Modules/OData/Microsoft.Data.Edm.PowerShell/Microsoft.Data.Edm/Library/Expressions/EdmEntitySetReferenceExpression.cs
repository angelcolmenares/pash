using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Library;

namespace Microsoft.Data.Edm.Library.Expressions
{
	internal class EdmEntitySetReferenceExpression : EdmElement, IEdmEntitySetReferenceExpression, IEdmExpression, IEdmElement
	{
		private readonly IEdmEntitySet referencedEntitySet;

		public EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.EntitySetReference;
			}
		}

		public IEdmEntitySet ReferencedEntitySet
		{
			get
			{
				return this.referencedEntitySet;
			}
		}

		public EdmEntitySetReferenceExpression(IEdmEntitySet referencedEntitySet)
		{
			EdmUtil.CheckArgumentNull<IEdmEntitySet>(referencedEntitySet, "referencedEntitySet");
			this.referencedEntitySet = referencedEntitySet;
		}
	}
}