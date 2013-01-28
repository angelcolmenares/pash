using System.Data.Services.Providers;
using System.Linq;
using System.Linq.Expressions;

namespace Microsoft.Management.Odata.Core
{
	internal class WhereExpressionOptimizer
	{
		public WhereExpressionOptimizer()
		{
		}

		public static Expression GetOptimizedExpression(Expression tree, IQueryable<DSResource> resourceRoot, ResourceType baseResourceType)
		{
			while (true)
			{
				MovableWhereFinder movableWhereFinder = new MovableWhereFinder(tree, resourceRoot, baseResourceType);
				if (movableWhereFinder.MovableWhereExpression == null)
				{
					break;
				}
				MethodCallExpression item = movableWhereFinder.MovableWhereExpression.Arguments[0] as MethodCallExpression;
				Expression[] expressionArray = new Expression[2];
				expressionArray[0] = item.Arguments[0];
				expressionArray[1] = movableWhereFinder.MovableWhereExpression.Arguments[1];
				MethodCallExpression methodCallExpression = Expression.Call(movableWhereFinder.MovableWhereExpression.Method, expressionArray);
				Expression[] item1 = new Expression[2];
				item1[0] = methodCallExpression;
				item1[1] = item.Arguments[1];
				MethodCallExpression methodCallExpression1 = Expression.Call(item.Method, item1);
				ExpressionNodeReplacer expressionNodeReplacer = new ExpressionNodeReplacer(movableWhereFinder.MovableWhereExpression, methodCallExpression1);
				tree = expressionNodeReplacer.Visit(tree);
			}
			return tree;
		}
	}
}