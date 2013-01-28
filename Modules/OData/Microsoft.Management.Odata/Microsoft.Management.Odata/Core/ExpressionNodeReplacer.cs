using System.Linq.Expressions;

namespace Microsoft.Management.Odata.Core
{
	internal class ExpressionNodeReplacer : ExpressionVisitor
	{
		private Expression oldNode;

		private Expression newNode;

		public ExpressionNodeReplacer(Expression oldNode, Expression newNode)
		{
			this.oldNode = oldNode;
			this.newNode = newNode;
		}

		public override Expression Visit(Expression expression)
		{
			if (expression != this.oldNode)
			{
				return base.Visit(expression);
			}
			else
			{
				return this.newNode;
			}
		}
	}
}