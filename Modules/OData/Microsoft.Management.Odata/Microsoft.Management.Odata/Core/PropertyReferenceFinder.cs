using System;
using System.Collections.Generic;
using System.Data.Services.Providers;
using System.Linq.Expressions;

namespace Microsoft.Management.Odata.Core
{
	internal class PropertyReferenceFinder : ExpressionVisitor
	{
		public List<ResourceProperty> Properties
		{
			get;
			private set;
		}

		public PropertyReferenceFinder()
		{
			this.Properties = new List<ResourceProperty>();
		}

		internal static bool IsReference(ResourceProperty property)
		{
			if (property.Kind == ResourcePropertyKind.ResourceReference)
			{
				return true;
			}
			else
			{
				return property.Kind == ResourcePropertyKind.ResourceSetReference;
			}
		}

		protected override Expression VisitConstant(ConstantExpression expression)
		{
			ResourceProperty value = expression.Value as ResourceProperty;
			if (value != null && PropertyReferenceFinder.IsReference(value))
			{
				this.Properties.Add(value);
			}
			return base.VisitConstant(expression);
		}
	}
}