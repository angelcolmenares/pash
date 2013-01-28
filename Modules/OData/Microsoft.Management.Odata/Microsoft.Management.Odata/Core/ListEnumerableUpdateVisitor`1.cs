using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Microsoft.Management.Odata.Core
{
	internal class ListEnumerableUpdateVisitor<TItem> : ExpressionVisitor
	{
		private List<TItem> newList;

		public ListEnumerableUpdateVisitor(List<TItem> newList)
		{
			this.newList = newList;
		}

		protected override Expression VisitConstant(ConstantExpression expression)
		{
			EnumerableQuery<TItem> value = expression.Value as EnumerableQuery<TItem>;
			if (value == null)
			{
				return base.VisitConstant(expression);
			}
			else
			{
				return Expression.Constant(this.newList.AsQueryable<TItem>(), expression.Type);
			}
		}
	}
}