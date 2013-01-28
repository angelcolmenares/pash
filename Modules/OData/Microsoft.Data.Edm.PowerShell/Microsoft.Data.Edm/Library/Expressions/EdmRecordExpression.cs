using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Library;
using System.Collections.Generic;

namespace Microsoft.Data.Edm.Library.Expressions
{
	internal class EdmRecordExpression : EdmElement, IEdmRecordExpression, IEdmExpression, IEdmElement
	{
		private readonly IEdmStructuredTypeReference declaredType;

		private readonly IEnumerable<IEdmPropertyConstructor> properties;

		public IEdmStructuredTypeReference DeclaredType
		{
			get
			{
				return this.declaredType;
			}
		}

		public EdmExpressionKind ExpressionKind
		{
			get
			{
				return EdmExpressionKind.Record;
			}
		}

		public IEnumerable<IEdmPropertyConstructor> Properties
		{
			get
			{
				return this.properties;
			}
		}

		public EdmRecordExpression(IEdmPropertyConstructor[] properties) : this((IEnumerable<IEdmPropertyConstructor>)properties)
		{
		}

		public EdmRecordExpression(IEdmStructuredTypeReference declaredType, IEdmPropertyConstructor[] properties) : this(declaredType, (IEnumerable<IEdmPropertyConstructor>)properties)
		{
		}

		public EdmRecordExpression(IEnumerable<IEdmPropertyConstructor> properties) : this(null, properties)
		{
		}

		public EdmRecordExpression(IEdmStructuredTypeReference declaredType, IEnumerable<IEdmPropertyConstructor> properties)
		{
			EdmUtil.CheckArgumentNull<IEnumerable<IEdmPropertyConstructor>>(properties, "properties");
			this.declaredType = declaredType;
			this.properties = properties;
		}
	}
}