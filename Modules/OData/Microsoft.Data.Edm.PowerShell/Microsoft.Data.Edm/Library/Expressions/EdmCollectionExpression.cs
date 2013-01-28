using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Library;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Library.Expressions
{
	internal class EdmCollectionExpression : EdmElement, IEdmCollectionExpression, IEdmExpression, IEdmElement
	{
		private readonly IEdmTypeReference declaredType;

		private readonly IEnumerable<IEdmExpression> elements;

		public IEdmTypeReference DeclaredType
		{
			get
			{
				return this.declaredType;
			}
		}

		public IEnumerable<IEdmExpression> Elements
		{
			get
			{
				return this.elements;
			}
		}

		public EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.Collection;
			}
		}

		public EdmCollectionExpression(IEdmExpression[] elements) : this((IEnumerable<IEdmExpression>)elements)
		{
		}

		public EdmCollectionExpression(IEdmTypeReference declaredType, IEdmExpression[] elements) : this(declaredType, (IEnumerable<IEdmExpression>)elements)
		{
		}

		public EdmCollectionExpression(IEnumerable<IEdmExpression> elements) : this(null, elements)
		{
		}

		public EdmCollectionExpression(IEdmTypeReference declaredType, IEnumerable<IEdmExpression> elements)
		{
			EdmUtil.CheckArgumentNull<IEnumerable<IEdmExpression>>(elements, "elements");
			this.declaredType = declaredType;
			this.elements = elements;
		}
	}
}