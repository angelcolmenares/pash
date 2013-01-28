using System;
using System.Linq;
using System.Linq.Expressions;

namespace Microsoft.Management.Odata.Core
{
	internal class CompleteDataSetRequirementCheckVisitor : ExpressionVisitor
	{
		public bool DoesExpressionNeedCompleteDataSet
		{
			get;
			private set;
		}

		public CompleteDataSetRequirementCheckVisitor()
		{
			this.DoesExpressionNeedCompleteDataSet = false;
		}

		protected override Expression VisitMethodCall(MethodCallExpression node)
		{
			if (node.Method.ReturnType == typeof(IOrderedQueryable<DSResource>))
			{
				this.DoesExpressionNeedCompleteDataSet = true;
			}
			return base.VisitMethodCall(node);
		}
	}
}