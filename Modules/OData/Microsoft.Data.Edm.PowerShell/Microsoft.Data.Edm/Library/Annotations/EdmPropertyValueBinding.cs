using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Annotations;
using Microsoft.Data.Edm.Expressions;
using Microsoft.Data.Edm.Library;

namespace Microsoft.Data.Edm.Library.Annotations
{
	internal class EdmPropertyValueBinding : EdmElement, IEdmPropertyValueBinding, IEdmElement
	{
		private readonly IEdmProperty boundProperty;

		private readonly IEdmExpression @value;

		public IEdmProperty BoundProperty
		{
			get
			{
				return this.boundProperty;
			}
		}

		public IEdmExpression Value
		{
			get
			{
				return this.@value;
			}
		}

		public EdmPropertyValueBinding(IEdmProperty boundProperty, IEdmExpression value)
		{
			EdmUtil.CheckArgumentNull<IEdmProperty>(boundProperty, "boundProperty");
			EdmUtil.CheckArgumentNull<IEdmExpression>(value, "value");
			this.boundProperty = boundProperty;
			this.@value = value;
		}
	}
}