using Microsoft.Management.Odata.Common;
using System;
using System.Collections.Generic;
using System.Data.Services.Providers;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.Management.Odata.Core
{
	internal class ExpressionHelper
	{
		public readonly static MethodInfo GetValueMethodInfo;

		public readonly static MethodInfo GetSequenceValueMethodInfo;

		public static MethodInfo OfTypeMethodInfo;

		static ExpressionHelper()
		{
			Type[] typeArray = new Type[2];
			typeArray[0] = typeof(object);
			typeArray[1] = typeof(ResourceProperty);
			ExpressionHelper.GetValueMethodInfo = typeof(DataServiceProviderMethods).GetMethod("GetValue", BindingFlags.Static | BindingFlags.Public, null, typeArray, null);
			Type[] typeArray1 = new Type[2];
			typeArray1[0] = typeof(object);
			typeArray1[1] = typeof(ResourceProperty);
			ExpressionHelper.GetSequenceValueMethodInfo = typeof(DataServiceProviderMethods).GetMethod("GetSequenceValue", BindingFlags.Static | BindingFlags.Public, null, typeArray1, null);
			MethodInfo[] methods = typeof(DataServiceProviderMethods).GetMethods();
			Type[] typeArray2 = new Type[2];
			typeArray2[0] = typeof(DSResource);
			typeArray2[1] = typeof(DSResource);
			ExpressionHelper.OfTypeMethodInfo = methods.Where<MethodInfo>((MethodInfo it) => {
				if (!(it.Name == "OfType") || it.ReturnParameter == null || !it.ReturnParameter.ParameterType.IsGenericType)
				{
					return false;
				}
				else
				{
					return it.ReturnParameter.ParameterType.GetGenericTypeDefinition() == typeof(IQueryable<>);
				}
			}
			).First<MethodInfo>().MakeGenericMethod(typeArray2);
		}

		public ExpressionHelper()
		{
		}

		public static bool ContainsComparisonOperator(BinaryExpression binaryExpr)
		{
			if (binaryExpr.NodeType == ExpressionType.Equal || binaryExpr.NodeType == ExpressionType.GreaterThan || binaryExpr.NodeType == ExpressionType.GreaterThanOrEqual || binaryExpr.NodeType == ExpressionType.LessThan || binaryExpr.NodeType == ExpressionType.LessThanOrEqual || binaryExpr.NodeType == ExpressionType.NotEqual)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public static bool ContainsLogicalOperator(BinaryExpression binaryExpr)
		{
			if (binaryExpr.NodeType == ExpressionType.And || binaryExpr.NodeType == ExpressionType.AndAlso || binaryExpr.NodeType == ExpressionType.ExclusiveOr || binaryExpr.NodeType == ExpressionType.Or || binaryExpr.NodeType == ExpressionType.OrElse || binaryExpr.NodeType == ExpressionType.NotEqual)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public static bool GetPropertyNameAndValue(BinaryExpression binaryExpr, out ResourceProperty property, out object value)
		{
			property = null;
			value = null;
			UnaryExpression left = binaryExpr.Left as UnaryExpression;
			MethodCallExpression operand = null;
			if (left != null)
			{
				operand = left.Operand as MethodCallExpression;
			}
			if (operand != null && binaryExpr.Left.NodeType == ExpressionType.Convert && operand.NodeType == ExpressionType.Call && binaryExpr.Right.NodeType == ExpressionType.Constant && (operand.Method == ExpressionHelper.GetValueMethodInfo || operand.Method == ExpressionHelper.GetSequenceValueMethodInfo))
			{
				ConstantExpression item = operand.Arguments[1] as ConstantExpression;
				if (item != null)
				{
					property = item.Value as ResourceProperty;
				}
				ConstantExpression right = binaryExpr.Right as ConstantExpression;
				if (right != null)
				{
					value = right.Value;
				}
			}
			return property != null;
		}

		public static bool GetResourcePropertyAndValueFromLambda(Expression expression, out ResourceProperty property, out object value)
		{
			property = null;
			value = null;
			BinaryExpression body = null;
			UnaryExpression unaryExpression = expression as UnaryExpression;
			if (unaryExpression != null)
			{
				LambdaExpression operand = unaryExpression.Operand as LambdaExpression;
				if (operand != null)
				{
					body = operand.Body as BinaryExpression;
				}
			}
			if (body == null)
			{
				return false;
			}
			else
			{
				return ExpressionHelper.GetPropertyNameAndValue(body, out property, out value);
			}
		}

		public static ResourceProperty GetResourcePropertyFromSequence(Expression expression)
		{
			ResourceProperty value = null;
			UnaryExpression unaryExpression = expression as UnaryExpression;
			MethodCallExpression body = null;
			LambdaExpression operand = null;
			if (unaryExpression != null)
			{
				operand = unaryExpression.Operand as LambdaExpression;
			}
			if (operand != null)
			{
				body = operand.Body as MethodCallExpression;
			}
			if (body != null && body.Method.IsGenericMethod && body.Method.GetGenericMethodDefinition() == ExpressionHelper.GetSequenceValueMethodInfo)
			{
				ConstantExpression item = body.Arguments[1] as ConstantExpression;
				if (item != null)
				{
					value = item.Value as ResourceProperty;
				}
			}
			return value;
		}

		public static bool IsConstantTrue(Expression expression)
		{
			ConstantExpression constantExpression = expression as ConstantExpression;
			if (constantExpression == null || !(constantExpression.Type == typeof(bool)) || !(bool)constantExpression.Value)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		public static bool IsNestedWhereClause(Expression expression, IQueryable<DSResource> root)
		{
			MethodCallExpression methodCallExpression = expression as MethodCallExpression;
			if (methodCallExpression == null || !(methodCallExpression.Method.Name == "Where"))
			{
				return false;
			}
			else
			{
				if (!ExpressionHelper.IsResourceRoot(methodCallExpression.Arguments[0], root))
				{
					return ExpressionHelper.IsNestedWhereClause(methodCallExpression.Arguments[0], root);
				}
				else
				{
					return true;
				}
			}
		}

		public static bool IsPropertyComparisonPredicate(Expression expression)
		{
			ResourceProperty resourceProperty = null;
			object obj = null;
			UnaryExpression unaryExpression = expression as UnaryExpression;
			if (unaryExpression != null)
			{
				LambdaExpression operand = unaryExpression.Operand as LambdaExpression;
				if (operand != null)
				{
					BinaryExpression body = operand.Body as BinaryExpression;
					if (body != null)
					{
						return ExpressionHelper.IsPropertyEqualityCheck(body, out resourceProperty, out obj);
					}
					else
					{
						return false;
					}
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		public static bool IsPropertyEqualityCheck(BinaryExpression binaryExpr, out ResourceProperty property, out object value)
		{
			property = null;
			value = null;
			if (binaryExpr.NodeType == ExpressionType.Equal && !ExpressionHelper.GetPropertyNameAndValue(binaryExpr, out property, out value))
			{
				TraceHelper.Current.DebugMessage("Expression is too complex to execute");
			}
			return property != null;
		}

		public static bool IsResourceRoot(Expression expression, IQueryable<DSResource> root)
		{
			if (expression.NodeType == ExpressionType.Constant)
			{
				if ((expression as ConstantExpression).Value as IQueryable<DSResource> != root)
				{
					return false;
				}
				else
				{
					return true;
				}
			}
			else
			{
				return false;
			}
		}
	}
}