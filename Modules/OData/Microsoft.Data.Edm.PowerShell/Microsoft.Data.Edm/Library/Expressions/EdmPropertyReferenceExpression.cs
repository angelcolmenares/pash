using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Library;

namespace Microsoft.Data.Edm.Library.Expressions
{
	internal class EdmPropertyReferenceExpression : EdmElement, IEdmPropertyReferenceExpression, IEdmExpression, IEdmElement
	{
		private readonly IEdmExpression baseExpression;

		private readonly IEdmProperty referencedProperty;

		public IEdmExpression Base
		{
			get
			{
				return this.baseExpression;
			}
		}

		public EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.PropertyReference;
			}
		}

		public IEdmProperty ReferencedProperty
		{
			get
			{
				return this.referencedProperty;
			}
		}

		public EdmPropertyReferenceExpression(IEdmExpression baseExpression, IEdmProperty referencedProperty)
		{
			EdmUtil.CheckArgumentNull<IEdmExpression>(baseExpression, "baseExpression");
			EdmUtil.CheckArgumentNull<IEdmProperty>(referencedProperty, "referencedPropert");
			this.baseExpression = baseExpression;
			this.referencedProperty = referencedProperty;
		}
	}
}