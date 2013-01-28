using System;
using System.Collections.Generic;
using System.Data.Services;
using System.Data.Services.Providers;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Data.Services.Parsing
{
	internal static class ExpressionUtils
	{
		private static readonly MethodInfo asEnumerableMethod = methodof<IEnumerable<object>, IEnumerable<object>>(Enumerable.AsEnumerable);
		private static readonly MethodInfo asEnumerableTypeMethod = methodof<IEnumerable<Type>, IEnumerable<Type>>(Enumerable.AsEnumerable);
		private static readonly MethodInfo allMethod = methodof<IQueryable<object>, Expression<Func<object, bool>>, bool>(Queryable.All);
		private static readonly MethodInfo anyMethod = methodof<IQueryable<object>, Expression<Func<object, bool>>, bool>(Queryable.Any);
		private static readonly MethodInfo asQueryableMethod = methodof<IEnumerable<object>, IQueryable<object>>(Queryable.AsQueryable);
		private static readonly MethodInfo asQueryableTypeMethod = methodof<IEnumerable<Type>, IQueryable<Type>>(Queryable.AsQueryable);
		private static readonly MethodInfo createQueryMethod = typeof(IQueryProvider).GetMethods().FirstOrDefault (x => x.Name == "CreateQuery" && x.IsGenericMethod).MakeGenericMethod (new Type[] { typeof(object) });
		private static readonly MethodInfo getProviderMethod = typeof(IQueryable).GetProperty ("Provider").GetGetMethod ();
		private static readonly MethodInfo constantMethod = methodof<object, Type, ConstantExpression>(Expression.Constant);
		private static readonly MethodInfo castMethod = EnumerableMethods.GetMethod (typeof(Queryable), "Cast");
		private static readonly MethodInfo emptyMethod = EnumerableMethods.GetMethod (typeof(Enumerable), "Empty");
		private static readonly MethodInfo ofTypeMethod = EnumerableMethods.GetMethod (typeof(Queryable), "OfType");
		private static readonly MethodInfo skipMethod = EnumerableMethods.GetMethod (typeof(Queryable), "Skip");
		private static readonly MethodInfo takeMethod = EnumerableMethods.GetMethod (typeof(Queryable), "Take");
		private static readonly MethodInfo getHashCodeMethod = typeof(object).GetMethod("GetHashCode");
		private static readonly MethodInfo toStringMethod = typeof(object).GetMethod("ToString");
		private static readonly MethodInfo getMembersMethod = typeof(Type).GetMethods().FirstOrDefault(x => x.Name == "GetMembers" && x.GetParameters ().Count () == 0);
		private static readonly MethodInfo selectStringMethod = GetSelectStringMethod();
		private static readonly MethodInfo selectManyTypeMethod = typeof(Queryable).GetMethods().FirstOrDefault (x => x.Name == "SelectMany").MakeGenericMethod (new Type[] { typeof(Type), typeof(MemberInfo) });
		private static readonly MethodInfo orderByDescMethod = typeof(Queryable).GetMethods().FirstOrDefault (x => x.Name == "OrderByDescending").MakeGenericMethod (new Type[] { typeof(object), typeof(int) });
		private static readonly MethodInfo orderByMethod = typeof(Queryable).GetMethods().FirstOrDefault (x => x.Name == "OrderBy").MakeGenericMethod (new Type[] { typeof(object), typeof(int) });
		private static readonly MethodInfo thenByDescMethod = typeof(Queryable).GetMethods().FirstOrDefault (x => x.Name == "ThenByDescending").MakeGenericMethod (new Type[] { typeof(object), typeof(string) });
		private static readonly MethodInfo thenByMethod = typeof(Queryable).GetMethods().FirstOrDefault (x => x.Name == "ThenBy").MakeGenericMethod (new Type[] { typeof(object), typeof(string) });
		private static readonly MethodInfo whereMethod = typeof(Queryable).GetMethods().FirstOrDefault (x => x.Name == "Where").MakeGenericMethod (new Type[] { typeof(object) });
		private static readonly MethodInfo longCountMethod = typeof(Queryable).GetMethods().FirstOrDefault (x => x.Name == "LongCount").MakeGenericMethod (new Type[] { typeof(object) });

		private static MethodInfo GetSelectStringMethod ()
		{
			return typeof(Queryable).GetMethods ().FirstOrDefault (x => x.Name == "Select" && x.GetParameters ().Last ().ParameterType.AssemblyQualifiedName.Contains ("Func`2")).MakeGenericMethod (new Type[] { typeof(object), typeof(string) });
		}



		private static MethodInfo methodof<T1> (Func<T1> func)
		{
			return func.Method;
		}

		private static MethodInfo methodof<T1, T2> (Func<T1, T2> func)
		{
			return func.Method;
		}

		private static MethodInfo methodof<T1, T2, T3> (Func<T1, T2, T3> func)
		{
			return func.Method;
		}

		private static MethodInfo methodof<T1, T2, T3, T4> (Func<T1, T2, T3, T4> func)
		{
			return func.Method;
		}

		internal readonly static ConstantExpression NullLiteral;
		
		private static MethodInfo queryableWhereMethodInfo;
		
		private static MethodInfo queryableOfTypeMethodInfo;
		
		private static MethodInfo queryableSelectMethodInfo;
		
		private static MethodInfo queryableSelectManyMethodInfo;
		
		private static MethodInfo queryableOrderByMethodInfo;
		
		private static MethodInfo queryableOrderByDescendingMethodInfo;
		
		private static MethodInfo queryableThenByMethodInfo;
		
		private static MethodInfo queryableThenByDescendingMethodInfo;
		
		private static MethodInfo queryableTakeMethodInfo;
		
		private static MethodInfo queryableSkipMethodInfo;
		
		private static MethodInfo queryableLongCountMethodInfo;
		
		private static MethodInfo enumerableWhereMethodInfo;
		
		private static MethodInfo enumerableOfTypeMethodInfo;
		
		private static MethodInfo enumerableSelectMethodInfo;
		
		private static MethodInfo enumerableSelectManyMethodInfo;
		
		private static MethodInfo enumerableOrderByMethodInfo;
		
		private static MethodInfo enumerableOrderByDescendingMethodInfo;
		
		private static MethodInfo enumerableThenByMethodInfo;
		
		private static MethodInfo enumerableThenByDescendingMethodInfo;
		
		private static MethodInfo enumerableTakeMethodInfo;
		
		private static MethodInfo enumerableSkipMethodInfo;
		
		private static MethodInfo enumerableCastMethodInfo;
		
		private static MethodInfo enumerableAllMethodInfo;
		
		private static MethodInfo enumerableAnyWithNoPredicateMethodInfo;
		
		private static MethodInfo enumerableAnyWithPredicateMethodInfo;
		
		private static MethodInfo enumerableEmptyMethodInfo;
		
		private static MethodInfo createQueryMethodInfo;
		
		private static MethodInfo CreateQueryMethodInfo
		{
			get
			{
				MethodInfo methodInfo = ExpressionUtils.createQueryMethodInfo;
				MethodInfo methodInfo1 = methodInfo;
				if (methodInfo == null)
				{
					Expression[] expressionArray = new Expression[1];
					expressionArray[0] = Expression.NewArrayInit(typeof(object), new Expression[0]);
					Expression[] expressionArray1 = new Expression[1];
					Expression[] expressionArray2 = new Expression[1];
					expressionArray2[0] = Expression.Convert(Expression.Constant(0, typeof(int)), typeof(object));
					expressionArray1[0] = Expression.Call(null, constantMethod, expressionArray2);
					MethodInfo methodInfoFromLambdaBody = ExpressionUtils.GetMethodInfoFromLambdaBody<IQueryable<object>>(Expression.Lambda<Func<IQueryable<object>>>(Expression.Call(Expression.Property(Expression.Call(null, asQueryableMethod, expressionArray), getProviderMethod), createQueryMethod, expressionArray1), new ParameterExpression[0]));
					methodInfo1 = methodInfoFromLambdaBody;
					ExpressionUtils.createQueryMethodInfo = methodInfoFromLambdaBody;
				}
				return methodInfo1;
			}
		}
		
		private static MethodInfo EnumerableAllMethodInfo
		{
			get
			{
				MethodInfo methodInfo = ExpressionUtils.enumerableAllMethodInfo;
				MethodInfo methodInfo1 = methodInfo;
				if (methodInfo == null)
				{
					Expression[] expressionArray = new Expression[2];
					Expression[] expressionArray1 = new Expression[1];
					expressionArray1[0] = Expression.NewArrayInit(typeof(object), new Expression[0]);
					expressionArray[0] = Expression.Call(null, asEnumerableMethod, expressionArray1);
					ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "o");
					ParameterExpression[] parameterExpressionArray = new ParameterExpression[1];
					parameterExpressionArray[0] = parameterExpression;
					expressionArray[1] = Expression.Lambda<Func<object, bool>>(Expression.Constant(true, typeof(bool)), parameterExpressionArray);
					var ex =  Expression.Call(null, allMethod, expressionArray);
					MethodInfo methodInfoFromLambdaBody = ExpressionUtils.GetMethodInfoFromLambdaBody<bool>(Expression.Lambda<Func<bool>>(ex, new ParameterExpression[0]));
					methodInfo1 = methodInfoFromLambdaBody;
					ExpressionUtils.enumerableAllMethodInfo = methodInfoFromLambdaBody;
				}
				return methodInfo1;
			}
		}
		
		private static MethodInfo EnumerableAnyWithNoPredicateMethodInfo
		{
			get
			{
				MethodInfo methodInfo = ExpressionUtils.enumerableAnyWithNoPredicateMethodInfo;
				MethodInfo methodInfo1 = methodInfo;
				if (methodInfo == null)
				{
					Expression[] expressionArray = new Expression[1];
					Expression[] expressionArray1 = new Expression[1];
					expressionArray1[0] = Expression.NewArrayInit(typeof(object), new Expression[0]);
					expressionArray[0] = Expression.Call(null, asEnumerableMethod, expressionArray1);
					MethodInfo methodInfoFromLambdaBody = ExpressionUtils.GetMethodInfoFromLambdaBody<bool>(Expression.Lambda<Func<bool>>(Expression.Call(null, anyMethod, expressionArray), new ParameterExpression[0]));
					methodInfo1 = methodInfoFromLambdaBody;
					ExpressionUtils.enumerableAnyWithNoPredicateMethodInfo = methodInfoFromLambdaBody;
				}
				return methodInfo1;
			}
		}
		
		private static MethodInfo EnumerableAnyWithPredicateMethodInfo
		{
			get
			{
				MethodInfo methodInfo = ExpressionUtils.enumerableAnyWithPredicateMethodInfo;
				MethodInfo methodInfo1 = methodInfo;
				if (methodInfo == null)
				{
					Expression[] expressionArray = new Expression[2];
					Expression[] expressionArray1 = new Expression[1];
					expressionArray1[0] = Expression.NewArrayInit(typeof(object), new Expression[0]);
					expressionArray[0] = Expression.Call(null, asEnumerableMethod, expressionArray1);
					ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "o");
					ParameterExpression[] parameterExpressionArray = new ParameterExpression[1];
					parameterExpressionArray[0] = parameterExpression;
					expressionArray[1] = Expression.Lambda<Func<object, bool>>(Expression.Constant(true, typeof(bool)), parameterExpressionArray);
					MethodInfo methodInfoFromLambdaBody = ExpressionUtils.GetMethodInfoFromLambdaBody<bool>(Expression.Lambda<Func<bool>>(Expression.Call(null, anyMethod, expressionArray), new ParameterExpression[0]));
					methodInfo1 = methodInfoFromLambdaBody;
					ExpressionUtils.enumerableAnyWithPredicateMethodInfo = methodInfoFromLambdaBody;
				}
				return methodInfo1;
			}
		}
		
		private static MethodInfo EnumerableCastMethodInfo
		{
			get
			{
				MethodInfo methodInfo = ExpressionUtils.enumerableCastMethodInfo;
				MethodInfo methodInfo1 = methodInfo;
				if (methodInfo == null)
				{
					Expression[] expressionArray = new Expression[1];
					Expression[] expressionArray1 = new Expression[1];
					expressionArray1[0] = Expression.NewArrayInit(typeof(object), new Expression[0]);
					expressionArray[0] = Expression.Call(null, asEnumerableMethod, expressionArray1);
					MethodInfo methodInfoFromLambdaBody = ExpressionUtils.GetMethodInfoFromLambdaBody<IEnumerable<object>>(Expression.Lambda<Func<IEnumerable<object>>>(Expression.Call(null, castMethod, expressionArray), new ParameterExpression[0]));
					methodInfo1 = methodInfoFromLambdaBody;
					ExpressionUtils.enumerableCastMethodInfo = methodInfoFromLambdaBody;
				}
				return methodInfo1;
			}
		}
		
		private static MethodInfo EnumerableEmptyMethodInfo
		{
			get
			{
				MethodInfo methodInfo = ExpressionUtils.enumerableEmptyMethodInfo;
				MethodInfo methodInfo1 = methodInfo;
				if (methodInfo == null)
				{
					MethodInfo methodInfoFromLambdaBody = ExpressionUtils.GetMethodInfoFromLambdaBody<IEnumerable<object>>(Expression.Lambda<Func<IEnumerable<object>>>(Expression.Call(null, emptyMethod, new Expression[0]), new ParameterExpression[0]));
					methodInfo1 = methodInfoFromLambdaBody;
					ExpressionUtils.enumerableEmptyMethodInfo = methodInfoFromLambdaBody;
				}
				return methodInfo1;
			}
		}
		
		private static MethodInfo EnumerableOfTypeMethodInfo
		{
			get
			{
				MethodInfo methodInfo = ExpressionUtils.enumerableOfTypeMethodInfo;
				MethodInfo methodInfo1 = methodInfo;
				if (methodInfo == null)
				{
					Expression[] expressionArray = new Expression[1];
					Expression[] expressionArray1 = new Expression[1];
					expressionArray1[0] = Expression.NewArrayInit(typeof(object), new Expression[0]);
					expressionArray[0] = Expression.Call(null, asEnumerableMethod, expressionArray1);
					MethodInfo methodInfoFromLambdaBody = ExpressionUtils.GetMethodInfoFromLambdaBody<IEnumerable<object>>(Expression.Lambda<Func<IEnumerable<object>>>(Expression.Call(null, ofTypeMethod, expressionArray), new ParameterExpression[0]));
					methodInfo1 = methodInfoFromLambdaBody;
					ExpressionUtils.enumerableOfTypeMethodInfo = methodInfoFromLambdaBody;
				}
				return methodInfo1;
			}
		}
		
		private static MethodInfo EnumerableOrderByDescendingMethodInfo
		{
			get
			{
				MethodInfo methodInfo = ExpressionUtils.enumerableOrderByDescendingMethodInfo;
				MethodInfo methodInfo1 = methodInfo;
				if (methodInfo == null)
				{
					Expression[] expressionArray = new Expression[2];
					Expression[] expressionArray1 = new Expression[1];
					expressionArray1[0] = Expression.NewArrayInit(typeof(object), new Expression[0]);
					expressionArray[0] = Expression.Call(null, asEnumerableMethod, expressionArray1);
					ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "o");
					ParameterExpression[] parameterExpressionArray = new ParameterExpression[1];
					parameterExpressionArray[0] = parameterExpression;
					expressionArray[1] = Expression.Lambda<Func<object, int>>(Expression.Call(parameterExpression, getHashCodeMethod, new Expression[0]), parameterExpressionArray);
					MethodInfo methodInfoFromLambdaBody = ExpressionUtils.GetMethodInfoFromLambdaBody<IOrderedEnumerable<object>>(Expression.Lambda<Func<IOrderedEnumerable<object>>>(Expression.Call(null, orderByDescMethod, expressionArray), new ParameterExpression[0]));
					methodInfo1 = methodInfoFromLambdaBody;
					ExpressionUtils.enumerableOrderByDescendingMethodInfo = methodInfoFromLambdaBody;
				}
				return methodInfo1;
			}
		}
		
		private static MethodInfo EnumerableOrderByMethodInfo
		{
			get
			{
				MethodInfo methodInfo = ExpressionUtils.enumerableOrderByMethodInfo;
				MethodInfo methodInfo1 = methodInfo;
				if (methodInfo == null)
				{
					Expression[] expressionArray = new Expression[2];
					Expression[] expressionArray1 = new Expression[1];
					expressionArray1[0] = Expression.NewArrayInit(typeof(object), new Expression[0]);
					expressionArray[0] = Expression.Call(null, asEnumerableMethod, expressionArray1);
					ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "o");
					ParameterExpression[] parameterExpressionArray = new ParameterExpression[1];
					parameterExpressionArray[0] = parameterExpression;
					expressionArray[1] = Expression.Lambda<Func<object, int>>(Expression.Call(parameterExpression, getHashCodeMethod, new Expression[0]), parameterExpressionArray);
					MethodInfo methodInfoFromLambdaBody = ExpressionUtils.GetMethodInfoFromLambdaBody<IOrderedEnumerable<object>>(Expression.Lambda<Func<IOrderedEnumerable<object>>>(Expression.Call(null, orderByMethod, expressionArray), new ParameterExpression[0]));
					methodInfo1 = methodInfoFromLambdaBody;
					ExpressionUtils.enumerableOrderByMethodInfo = methodInfoFromLambdaBody;
				}
				return methodInfo1;
			}
		}
		
		private static MethodInfo EnumerableSelectManyMethodInfo
		{
			get
			{
				MethodInfo methodInfo = ExpressionUtils.enumerableSelectManyMethodInfo;
				MethodInfo methodInfo1 = methodInfo;
				if (methodInfo == null)
				{
					Expression[] expressionArray = new Expression[2];
					Expression[] expressionArray1 = new Expression[1];
					expressionArray1[0] = Expression.NewArrayInit(typeof(Type), new Expression[0]);
					expressionArray[0] = Expression.Call(null, asEnumerableTypeMethod, expressionArray1);
					ParameterExpression parameterExpression = Expression.Parameter(typeof(Type), "t");
					ParameterExpression[] parameterExpressionArray = new ParameterExpression[1];
					parameterExpressionArray[0] = parameterExpression;
					expressionArray[1] = Expression.Lambda<Func<Type, IEnumerable<MemberInfo>>>(Expression.Call(parameterExpression, getMembersMethod, new Expression[0]), parameterExpressionArray);
					MethodInfo methodInfoFromLambdaBody = ExpressionUtils.GetMethodInfoFromLambdaBody<IEnumerable<MemberInfo>>(Expression.Lambda<Func<IEnumerable<MemberInfo>>>(Expression.Call(null, selectManyTypeMethod, expressionArray), new ParameterExpression[0]));
					methodInfo1 = methodInfoFromLambdaBody;
					ExpressionUtils.enumerableSelectManyMethodInfo = methodInfoFromLambdaBody;
				}
				return methodInfo1;
			}
		}
		
		private static MethodInfo EnumerableSelectMethodInfo
		{
			get
			{
				MethodInfo methodInfo = ExpressionUtils.enumerableSelectMethodInfo;
				MethodInfo methodInfo1 = methodInfo;
				if (methodInfo == null)
				{
					Expression[] expressionArray = new Expression[2];
					Expression[] expressionArray1 = new Expression[1];
					expressionArray1[0] = Expression.NewArrayInit(typeof(object), new Expression[0]);
					expressionArray[0] = Expression.Call(null, asEnumerableMethod, expressionArray1);
					ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "o");
					ParameterExpression[] parameterExpressionArray = new ParameterExpression[1];
					parameterExpressionArray[0] = parameterExpression;
					expressionArray[1] = Expression.Lambda<Func<object, string>>(Expression.Call(parameterExpression, toStringMethod, new Expression[0]), parameterExpressionArray);
					MethodInfo methodInfoFromLambdaBody = ExpressionUtils.GetMethodInfoFromLambdaBody<IEnumerable<string>>(Expression.Lambda<Func<IEnumerable<string>>>(Expression.Call(null, selectStringMethod, expressionArray), new ParameterExpression[0]));
					methodInfo1 = methodInfoFromLambdaBody;
					ExpressionUtils.enumerableSelectMethodInfo = methodInfoFromLambdaBody;
				}
				return methodInfo1;
			}
		}
		
		private static MethodInfo EnumerableSkipMethodInfo
		{
			get
			{
				MethodInfo methodInfo = ExpressionUtils.enumerableSkipMethodInfo;
				MethodInfo methodInfo1 = methodInfo;
				if (methodInfo == null)
				{
					Expression[] expressionArray = new Expression[2];
					Expression[] expressionArray1 = new Expression[1];
					expressionArray1[0] = Expression.NewArrayInit(typeof(object), new Expression[0]);
					expressionArray[0] = Expression.Call(null, asEnumerableMethod, expressionArray1);
					expressionArray[1] = Expression.Constant(1, typeof(int));
					MethodInfo methodInfoFromLambdaBody = ExpressionUtils.GetMethodInfoFromLambdaBody<IEnumerable<object>>(Expression.Lambda<Func<IEnumerable<object>>>(Expression.Call(null, skipMethod, expressionArray), new ParameterExpression[0]));
					methodInfo1 = methodInfoFromLambdaBody;
					ExpressionUtils.enumerableSkipMethodInfo = methodInfoFromLambdaBody;
				}
				return methodInfo1;
			}
		}
		
		private static MethodInfo EnumerableTakeMethodInfo
		{
			get
			{
				MethodInfo methodInfo = ExpressionUtils.enumerableTakeMethodInfo;
				MethodInfo methodInfo1 = methodInfo;
				if (methodInfo == null)
				{
					Expression[] expressionArray = new Expression[2];
					Expression[] expressionArray1 = new Expression[1];
					expressionArray1[0] = Expression.NewArrayInit(typeof(object), new Expression[0]);
					expressionArray[0] = Expression.Call(null, asEnumerableMethod, expressionArray1);
					expressionArray[1] = Expression.Constant(1, typeof(int));
					MethodInfo methodInfoFromLambdaBody = ExpressionUtils.GetMethodInfoFromLambdaBody<IEnumerable<object>>(Expression.Lambda<Func<IEnumerable<object>>>(Expression.Call(null, takeMethod, expressionArray), new ParameterExpression[0]));
					methodInfo1 = methodInfoFromLambdaBody;
					ExpressionUtils.enumerableTakeMethodInfo = methodInfoFromLambdaBody;
				}
				return methodInfo1;
			}
		}
		
		private static MethodInfo EnumerableThenByDescendingMethodInfo
		{
			get
			{
				MethodInfo methodInfo = ExpressionUtils.enumerableThenByDescendingMethodInfo;
				MethodInfo methodInfo1 = methodInfo;
				if (methodInfo == null)
				{
					Expression[] expressionArray = new Expression[2];
					Expression[] expressionArray1 = new Expression[2];
					Expression[] expressionArray2 = new Expression[1];
					expressionArray2[0] = Expression.NewArrayInit(typeof(object), new Expression[0]);
					expressionArray1[0] = Expression.Call(null, asEnumerableMethod, expressionArray2);
					ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "o");
					ParameterExpression[] parameterExpressionArray = new ParameterExpression[1];
					parameterExpressionArray[0] = parameterExpression;
					expressionArray1[1] = Expression.Lambda<Func<object, int>>(Expression.Call(parameterExpression, getHashCodeMethod, new Expression[0]), parameterExpressionArray);
					expressionArray[0] = Expression.Call(null, orderByMethod, expressionArray1);
					ParameterExpression parameterExpression1 = Expression.Parameter(typeof(object), "o");
					ParameterExpression[] parameterExpressionArray1 = new ParameterExpression[1];
					parameterExpressionArray1[0] = parameterExpression1;
					expressionArray[1] = Expression.Lambda<Func<object, string>>(Expression.Call(parameterExpression1, toStringMethod, new Expression[0]), parameterExpressionArray1);
					MethodInfo methodInfoFromLambdaBody = ExpressionUtils.GetMethodInfoFromLambdaBody<IOrderedEnumerable<object>>(Expression.Lambda<Func<IOrderedEnumerable<object>>>(Expression.Call(null, thenByDescMethod, expressionArray), new ParameterExpression[0]));
					methodInfo1 = methodInfoFromLambdaBody;
					ExpressionUtils.enumerableThenByDescendingMethodInfo = methodInfoFromLambdaBody;
				}
				return methodInfo1;
			}
		}
		
		private static MethodInfo EnumerableThenByMethodInfo
		{
			get
			{
				MethodInfo methodInfo = ExpressionUtils.enumerableThenByMethodInfo;
				MethodInfo methodInfo1 = methodInfo;
				if (methodInfo == null)
				{
					Expression[] expressionArray = new Expression[2];
					Expression[] expressionArray1 = new Expression[2];
					Expression[] expressionArray2 = new Expression[1];
					expressionArray2[0] = Expression.NewArrayInit(typeof(object), new Expression[0]);
					expressionArray1[0] = Expression.Call(null, asEnumerableMethod, expressionArray2);
					ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "o");
					ParameterExpression[] parameterExpressionArray = new ParameterExpression[1];
					parameterExpressionArray[0] = parameterExpression;
					expressionArray1[1] = Expression.Lambda<Func<object, int>>(Expression.Call(parameterExpression, getHashCodeMethod, new Expression[0]), parameterExpressionArray);
					expressionArray[0] = Expression.Call(null, orderByMethod, expressionArray1);
					ParameterExpression parameterExpression1 = Expression.Parameter(typeof(object), "o");
					ParameterExpression[] parameterExpressionArray1 = new ParameterExpression[1];
					parameterExpressionArray1[0] = parameterExpression1;
					expressionArray[1] = Expression.Lambda<Func<object, string>>(Expression.Call(parameterExpression1, toStringMethod, new Expression[0]), parameterExpressionArray1);
					MethodInfo methodInfoFromLambdaBody = ExpressionUtils.GetMethodInfoFromLambdaBody<IOrderedEnumerable<object>>(Expression.Lambda<Func<IOrderedEnumerable<object>>>(Expression.Call(null, thenByMethod, expressionArray), new ParameterExpression[0]));
					methodInfo1 = methodInfoFromLambdaBody;
					ExpressionUtils.enumerableThenByMethodInfo = methodInfoFromLambdaBody;
				}
				return methodInfo1;
			}
		}
		
		private static MethodInfo EnumerableWhereMethodInfo
		{
			get
			{
				MethodInfo methodInfo = ExpressionUtils.enumerableWhereMethodInfo;
				MethodInfo methodInfo1 = methodInfo;
				if (methodInfo == null)
				{
					Expression[] expressionArray = new Expression[2];
					Expression[] expressionArray1 = new Expression[1];
					expressionArray1[0] = Expression.NewArrayInit(typeof(object), new Expression[0]);
					expressionArray[0] = Expression.Call(null, asEnumerableMethod, expressionArray1);
					ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "o");
					ParameterExpression[] parameterExpressionArray = new ParameterExpression[1];
					parameterExpressionArray[0] = parameterExpression;
					expressionArray[1] = Expression.Lambda<Func<object, bool>>(Expression.Constant(true, typeof(bool)), parameterExpressionArray);
					MethodInfo methodInfoFromLambdaBody = ExpressionUtils.GetMethodInfoFromLambdaBody<IEnumerable<object>>(Expression.Lambda<Func<IEnumerable<object>>>(Expression.Call(null, whereMethod, expressionArray), new ParameterExpression[0]));
					methodInfo1 = methodInfoFromLambdaBody;
					ExpressionUtils.enumerableWhereMethodInfo = methodInfoFromLambdaBody;
				}
				return methodInfo1;
			}
		}
		
		private static MethodInfo QueryableLongCountMethodInfo
		{
			get
			{
				MethodInfo methodInfo = ExpressionUtils.queryableLongCountMethodInfo;
				MethodInfo methodInfo1 = methodInfo;
				if (methodInfo == null)
				{
					Expression[] expressionArray = new Expression[1];
					Expression[] expressionArray1 = new Expression[1];
					expressionArray1[0] = Expression.NewArrayInit(typeof(object), new Expression[0]);
					expressionArray[0] = Expression.Call(null, asQueryableMethod, expressionArray1);
					MethodInfo methodInfoFromLambdaBody = ExpressionUtils.GetMethodInfoFromLambdaBody<long>(Expression.Lambda<Func<long>>(Expression.Call(null, longCountMethod, expressionArray), new ParameterExpression[0]));
					methodInfo1 = methodInfoFromLambdaBody;
					ExpressionUtils.queryableLongCountMethodInfo = methodInfoFromLambdaBody;
				}
				return methodInfo1;
			}
		}
		
		private static MethodInfo QueryableOfTypeMethodInfo
		{
			get
			{
				MethodInfo methodInfo = ExpressionUtils.queryableOfTypeMethodInfo;
				MethodInfo methodInfo1 = methodInfo;
				if (methodInfo == null)
				{
					Expression[] expressionArray = new Expression[1];
					Expression[] expressionArray1 = new Expression[1];
					expressionArray1[0] = Expression.NewArrayInit(typeof(object), new Expression[0]);
					expressionArray[0] = Expression.Call(null, asQueryableMethod, expressionArray1);
					MethodInfo methodInfoFromLambdaBody = ExpressionUtils.GetMethodInfoFromLambdaBody<IQueryable<object>>(Expression.Lambda<Func<IQueryable<object>>>(Expression.Call(null, ofTypeMethod, expressionArray), new ParameterExpression[0]));
					methodInfo1 = methodInfoFromLambdaBody;
					ExpressionUtils.queryableOfTypeMethodInfo = methodInfoFromLambdaBody;
				}
				return methodInfo1;
			}
		}
		
		private static MethodInfo QueryableOrderByDescendingMethodInfo
		{
			get
			{
				MethodInfo methodInfo = ExpressionUtils.queryableOrderByDescendingMethodInfo;
				MethodInfo methodInfo1 = methodInfo;
				if (methodInfo == null)
				{
					Expression[] expressionArray = new Expression[2];
					Expression[] expressionArray1 = new Expression[1];
					expressionArray1[0] = Expression.NewArrayInit(typeof(object), new Expression[0]);
					expressionArray[0] = Expression.Call(null, asQueryableMethod, expressionArray1);
					ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "o");
					ParameterExpression[] parameterExpressionArray = new ParameterExpression[1];
					parameterExpressionArray[0] = parameterExpression;
					expressionArray[1] = Expression.Quote(Expression.Lambda<Func<object, int>>(Expression.Call(parameterExpression, getHashCodeMethod, new Expression[0]), parameterExpressionArray));
					MethodInfo methodInfoFromLambdaBody = ExpressionUtils.GetMethodInfoFromLambdaBody<IOrderedQueryable<object>>(Expression.Lambda<Func<IOrderedQueryable<object>>>(Expression.Call(null, orderByDescMethod, expressionArray), new ParameterExpression[0]));
					methodInfo1 = methodInfoFromLambdaBody;
					ExpressionUtils.queryableOrderByDescendingMethodInfo = methodInfoFromLambdaBody;
				}
				return methodInfo1;
			}
		}
		
		private static MethodInfo QueryableOrderByMethodInfo
		{
			get
			{
				MethodInfo methodInfo = ExpressionUtils.queryableOrderByMethodInfo;
				MethodInfo methodInfo1 = methodInfo;
				if (methodInfo == null)
				{
					Expression[] expressionArray = new Expression[2];
					Expression[] expressionArray1 = new Expression[1];
					expressionArray1[0] = Expression.NewArrayInit(typeof(object), new Expression[0]);
					expressionArray[0] = Expression.Call(null, asQueryableMethod, expressionArray1);
					ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "o");
					ParameterExpression[] parameterExpressionArray = new ParameterExpression[1];
					parameterExpressionArray[0] = parameterExpression;
					expressionArray[1] = Expression.Quote(Expression.Lambda<Func<object, int>>(Expression.Call(parameterExpression, getHashCodeMethod, new Expression[0]), parameterExpressionArray));
					MethodInfo methodInfoFromLambdaBody = ExpressionUtils.GetMethodInfoFromLambdaBody<IOrderedQueryable<object>>(Expression.Lambda<Func<IOrderedQueryable<object>>>(Expression.Call(null, orderByMethod, expressionArray), new ParameterExpression[0]));
					methodInfo1 = methodInfoFromLambdaBody;
					ExpressionUtils.queryableOrderByMethodInfo = methodInfoFromLambdaBody;
				}
				return methodInfo1;
			}
		}
		
		private static MethodInfo QueryableSelectManyMethodInfo
		{
			get
			{
				MethodInfo methodInfo = ExpressionUtils.queryableSelectManyMethodInfo;
				MethodInfo methodInfo1 = methodInfo;
				if (methodInfo == null)
				{
					Expression[] expressionArray = new Expression[2];
					Expression[] expressionArray1 = new Expression[1];
					expressionArray1[0] = Expression.NewArrayInit(typeof(Type), new Expression[0]);
					expressionArray[0] = Expression.Call(null, asQueryableTypeMethod, expressionArray1);
					ParameterExpression parameterExpression = Expression.Parameter(typeof(Type), "t");
					ParameterExpression[] parameterExpressionArray = new ParameterExpression[1];
					parameterExpressionArray[0] = parameterExpression;
					expressionArray[1] = Expression.Quote(Expression.Lambda<Func<Type, IEnumerable<MemberInfo>>>(Expression.Call(parameterExpression, getMembersMethod, new Expression[0]), parameterExpressionArray));
					MethodInfo methodInfoFromLambdaBody = ExpressionUtils.GetMethodInfoFromLambdaBody<IQueryable<MemberInfo>>(Expression.Lambda<Func<IQueryable<MemberInfo>>>(Expression.Call(null, selectManyTypeMethod, expressionArray), new ParameterExpression[0]));
					methodInfo1 = methodInfoFromLambdaBody;
					ExpressionUtils.queryableSelectManyMethodInfo = methodInfoFromLambdaBody;
				}
				return methodInfo1;
			}
		}
		
		private static MethodInfo QueryableSelectMethodInfo
		{
			get
			{
				MethodInfo methodInfo = ExpressionUtils.queryableSelectMethodInfo;
				MethodInfo methodInfo1 = methodInfo;
				if (methodInfo == null)
				{
					Expression[] expressionArray = new Expression[2];
					Expression[] expressionArray1 = new Expression[1];
					expressionArray1[0] = Expression.NewArrayInit(typeof(object), new Expression[0]);
					expressionArray[0] = Expression.Call(null, asQueryableMethod, expressionArray1);
					ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "o");
					ParameterExpression[] parameterExpressionArray = new ParameterExpression[1];
					parameterExpressionArray[0] = parameterExpression;
					expressionArray[1] = Expression.Quote(Expression.Lambda<Func<object, string>>(Expression.Call(parameterExpression, toStringMethod, new Expression[0]), parameterExpressionArray));
					MethodInfo methodInfoFromLambdaBody = ExpressionUtils.GetMethodInfoFromLambdaBody<IQueryable<string>>(Expression.Lambda<Func<IQueryable<string>>>(Expression.Call(null, selectStringMethod, expressionArray), new ParameterExpression[0]));
					methodInfo1 = methodInfoFromLambdaBody;
					ExpressionUtils.queryableSelectMethodInfo = methodInfoFromLambdaBody;
				}
				return methodInfo1;
			}
		}
		
		private static MethodInfo QueryableSkipMethodInfo
		{
			get
			{
				MethodInfo methodInfo = ExpressionUtils.queryableSkipMethodInfo;
				MethodInfo methodInfo1 = methodInfo;
				if (methodInfo == null)
				{
					Expression[] expressionArray = new Expression[2];
					Expression[] expressionArray1 = new Expression[1];
					expressionArray1[0] = Expression.NewArrayInit(typeof(object), new Expression[0]);
					expressionArray[0] = Expression.Call(null, asQueryableMethod, expressionArray1);
					expressionArray[1] = Expression.Constant(1, typeof(int));
					MethodInfo methodInfoFromLambdaBody = ExpressionUtils.GetMethodInfoFromLambdaBody<IQueryable<object>>(Expression.Lambda<Func<IQueryable<object>>>(Expression.Call(null, skipMethod, expressionArray), new ParameterExpression[0]));
					methodInfo1 = methodInfoFromLambdaBody;
					ExpressionUtils.queryableSkipMethodInfo = methodInfoFromLambdaBody;
				}
				return methodInfo1;
			}
		}
		
		private static MethodInfo QueryableTakeMethodInfo
		{
			get
			{
				MethodInfo methodInfo = ExpressionUtils.queryableTakeMethodInfo;
				MethodInfo methodInfo1 = methodInfo;
				if (methodInfo == null)
				{
					Expression[] expressionArray = new Expression[2];
					Expression[] expressionArray1 = new Expression[1];
					expressionArray1[0] = Expression.NewArrayInit(typeof(object), new Expression[0]);
					expressionArray[0] = Expression.Call(null, asQueryableMethod, expressionArray1);
					expressionArray[1] = Expression.Constant(1, typeof(int));
					MethodInfo methodInfoFromLambdaBody = ExpressionUtils.GetMethodInfoFromLambdaBody<IQueryable<object>>(Expression.Lambda<Func<IQueryable<object>>>(Expression.Call(null, takeMethod, expressionArray), new ParameterExpression[0]));
					methodInfo1 = methodInfoFromLambdaBody;
					ExpressionUtils.queryableTakeMethodInfo = methodInfoFromLambdaBody;
				}
				return methodInfo1;
			}
		}
		
		private static MethodInfo QueryableThenByDescendingMethodInfo
		{
			get
			{
				MethodInfo methodInfo = ExpressionUtils.queryableThenByDescendingMethodInfo;
				MethodInfo methodInfo1 = methodInfo;
				if (methodInfo == null)
				{
					Expression[] expressionArray = new Expression[2];
					Expression[] expressionArray1 = new Expression[2];
					Expression[] expressionArray2 = new Expression[1];
					expressionArray2[0] = Expression.NewArrayInit(typeof(object), new Expression[0]);
					expressionArray1[0] = Expression.Call(null, asQueryableMethod, expressionArray2);
					ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "o");
					ParameterExpression[] parameterExpressionArray = new ParameterExpression[1];
					parameterExpressionArray[0] = parameterExpression;
					expressionArray1[1] = Expression.Quote(Expression.Lambda<Func<object, int>>(Expression.Call(parameterExpression, getHashCodeMethod, new Expression[0]), parameterExpressionArray));
					expressionArray[0] = Expression.Call(null, orderByMethod, expressionArray1);
					ParameterExpression parameterExpression1 = Expression.Parameter(typeof(object), "o");
					ParameterExpression[] parameterExpressionArray1 = new ParameterExpression[1];
					parameterExpressionArray1[0] = parameterExpression1;
					expressionArray[1] = Expression.Quote(Expression.Lambda<Func<object, string>>(Expression.Call(parameterExpression1, toStringMethod, new Expression[0]), parameterExpressionArray1));
					MethodInfo methodInfoFromLambdaBody = ExpressionUtils.GetMethodInfoFromLambdaBody<IOrderedQueryable<object>>(Expression.Lambda<Func<IOrderedQueryable<object>>>(Expression.Call(null, thenByDescMethod, expressionArray), new ParameterExpression[0]));
					methodInfo1 = methodInfoFromLambdaBody;
					ExpressionUtils.queryableThenByDescendingMethodInfo = methodInfoFromLambdaBody;
				}
				return methodInfo1;
			}
		}
		
		private static MethodInfo QueryableThenByMethodInfo
		{
			get
			{
				MethodInfo methodInfo = ExpressionUtils.queryableThenByMethodInfo;
				MethodInfo methodInfo1 = methodInfo;
				if (methodInfo == null)
				{
					Expression[] expressionArray = new Expression[2];
					Expression[] expressionArray1 = new Expression[2];
					Expression[] expressionArray2 = new Expression[1];
					expressionArray2[0] = Expression.NewArrayInit(typeof(object), new Expression[0]);
					expressionArray1[0] = Expression.Call(null, asQueryableMethod, expressionArray2);
					ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "o");
					ParameterExpression[] parameterExpressionArray = new ParameterExpression[1];
					parameterExpressionArray[0] = parameterExpression;
					expressionArray1[1] = Expression.Quote(Expression.Lambda<Func<object, int>>(Expression.Call(parameterExpression, getHashCodeMethod, new Expression[0]), parameterExpressionArray));
					expressionArray[0] = Expression.Call(null, orderByMethod, expressionArray1);
					ParameterExpression parameterExpression1 = Expression.Parameter(typeof(object), "o");
					ParameterExpression[] parameterExpressionArray1 = new ParameterExpression[1];
					parameterExpressionArray1[0] = parameterExpression1;
					expressionArray[1] = Expression.Quote(Expression.Lambda<Func<object, string>>(Expression.Call(parameterExpression1, toStringMethod, new Expression[0]), parameterExpressionArray1));
					MethodInfo methodInfoFromLambdaBody = ExpressionUtils.GetMethodInfoFromLambdaBody<IOrderedQueryable<object>>(Expression.Lambda<Func<IOrderedQueryable<object>>>(Expression.Call(null, thenByMethod, expressionArray), new ParameterExpression[0]));
					methodInfo1 = methodInfoFromLambdaBody;
					ExpressionUtils.queryableThenByMethodInfo = methodInfoFromLambdaBody;
				}
				return methodInfo1;
			}
		}
		
		private static MethodInfo QueryableWhereMethodInfo
		{
			get
			{
				MethodInfo methodInfo = ExpressionUtils.queryableWhereMethodInfo;
				MethodInfo methodInfo1 = methodInfo;
				if (methodInfo == null)
				{
					Expression[] expressionArray = new Expression[2];
					Expression[] expressionArray1 = new Expression[1];
					expressionArray1[0] = Expression.NewArrayInit(typeof(object), new Expression[0]);
					expressionArray[0] = Expression.Call(null, asQueryableMethod, expressionArray1);
					ParameterExpression parameterExpression = Expression.Parameter(typeof(object), "o");
					ParameterExpression[] parameterExpressionArray = new ParameterExpression[1];
					parameterExpressionArray[0] = parameterExpression;
					expressionArray[1] = Expression.Quote(Expression.Lambda<Func<object, bool>>(Expression.Constant(true, typeof(bool)), parameterExpressionArray));
					MethodInfo methodInfoFromLambdaBody = ExpressionUtils.GetMethodInfoFromLambdaBody<IQueryable<object>>(Expression.Lambda<Func<IQueryable<object>>>(Expression.Call(null, whereMethod, expressionArray), new ParameterExpression[0]));
					methodInfo1 = methodInfoFromLambdaBody;
					ExpressionUtils.queryableWhereMethodInfo = methodInfoFromLambdaBody;
				}
				return methodInfo1;
			}
		}
		
		static ExpressionUtils()
		{
			ExpressionUtils.NullLiteral = Expression.Constant(null);
		}
		
		private static Expression CallMethodWithCount(MethodInfo genericMethodInfo, Expression source, int count)
		{
			Type type = source.ElementType();
			Type[] typeArray = new Type[1];
			typeArray[0] = type;
			MethodInfo methodInfo = genericMethodInfo.MakeGenericMethod(typeArray);
			return Expression.Call(null, methodInfo, source, Expression.Constant(count));
		}
		
		private static Expression CallMethodWithNoParam(MethodInfo genericMethodInfo, Expression source)
		{
			Type type = source.ElementType();
			Type[] typeArray = new Type[1];
			typeArray[0] = type;
			MethodInfo methodInfo = genericMethodInfo.MakeGenericMethod(typeArray);
			Expression[] expressionArray = new Expression[1];
			expressionArray[0] = source;
			return Expression.Call(null, methodInfo, expressionArray);
		}
		
		private static Expression CallMethodWithPredicate(MethodInfo genericMethodInfo, Expression source, LambdaExpression predicate)
		{
			Type type = source.ElementType();
			Type[] typeArray = new Type[1];
			typeArray[0] = type;
			MethodInfo methodInfo = genericMethodInfo.MakeGenericMethod(typeArray);
			return Expression.Call(null, methodInfo, source, predicate);
		}
		
		private static Expression CallMethodWithSelector(MethodInfo genericMethodInfo, Expression source, LambdaExpression selector)
		{
			Type type = source.ElementType();
			Type[] typeArray = new Type[2];
			typeArray[0] = type;
			typeArray[1] = selector.Body.Type;
			MethodInfo methodInfo = genericMethodInfo.MakeGenericMethod(typeArray);
			return Expression.Call(null, methodInfo, source, selector);
		}
		
		private static Expression CallMethodWithTypeParam(MethodInfo genericMethodInfo, Expression source, Type targetType)
		{
			Type[] typeArray = new Type[1];
			typeArray[0] = targetType;
			MethodInfo methodInfo = genericMethodInfo.MakeGenericMethod(typeArray);
			Expression[] expressionArray = new Expression[1];
			expressionArray[0] = source;
			return Expression.Call(null, methodInfo, expressionArray);
		}
		
		internal static IQueryable CreateQuery(this Expression source, IQueryProvider provider)
		{
			Type[] typeArray = new Type[1];
			typeArray[0] = source.ElementType();
			object[] objArray = new object[1];
			objArray[0] = source;
			return (IQueryable)ExpressionUtils.CreateQueryMethodInfo.MakeGenericMethod(typeArray).Invoke(provider, objArray);
		}
		
		internal static Type ElementType(this Expression source)
		{
			Type enumerableElement = BaseServiceProvider.GetIEnumerableElement(source.Type);
			Type type = enumerableElement;
			if (enumerableElement == null)
			{
				type = source.Type;
			}
			return type;
		}
		
		internal static Expression EnumerableAll(this Expression source, LambdaExpression predicate)
		{
			return ExpressionUtils.CallMethodWithPredicate(ExpressionUtils.EnumerableAllMethodInfo, source, predicate);
		}
		
		internal static Expression EnumerableAny(this Expression source)
		{
			return ExpressionUtils.CallMethodWithNoParam(ExpressionUtils.EnumerableAnyWithNoPredicateMethodInfo, source);
		}
		
		internal static Expression EnumerableAny(this Expression source, LambdaExpression predicate)
		{
			return ExpressionUtils.CallMethodWithPredicate(ExpressionUtils.EnumerableAnyWithPredicateMethodInfo, source, predicate);
		}
		
		internal static Expression EnumerableCast(this Expression source, Type targetType)
		{
			return ExpressionUtils.CallMethodWithTypeParam(ExpressionUtils.EnumerableCastMethodInfo, source, targetType);
		}
		
		internal static Expression EnumerableEmpty(Type targetType)
		{
			Type[] typeArray = new Type[1];
			typeArray[0] = targetType;
			MethodInfo methodInfo = ExpressionUtils.EnumerableEmptyMethodInfo.MakeGenericMethod(typeArray);
			return Expression.Call(null, methodInfo);
		}
		
		internal static Expression EnumerableOfType(this Expression source, Type targetType)
		{
			return ExpressionUtils.CallMethodWithTypeParam(ExpressionUtils.EnumerableOfTypeMethodInfo, source, targetType);
		}
		
		internal static Expression EnumerableOrderBy(this Expression source, LambdaExpression keySelector)
		{
			return ExpressionUtils.CallMethodWithSelector(ExpressionUtils.EnumerableOrderByMethodInfo, source, keySelector);
		}
		
		internal static Expression EnumerableOrderByDescending(this Expression source, LambdaExpression keySelector)
		{
			return ExpressionUtils.CallMethodWithSelector(ExpressionUtils.EnumerableOrderByDescendingMethodInfo, source, keySelector);
		}
		
		internal static Expression EnumerableSelect(this Expression source, LambdaExpression selector)
		{
			return ExpressionUtils.CallMethodWithSelector(ExpressionUtils.EnumerableSelectMethodInfo, source, selector);
		}
		
		internal static Expression EnumerableSelectMany(this Expression source, LambdaExpression selector)
		{
			return ExpressionUtils.SelectMany(ExpressionUtils.EnumerableSelectManyMethodInfo, source, selector);
		}
		
		internal static Expression EnumerableSkip(this Expression source, int count)
		{
			return ExpressionUtils.CallMethodWithCount(ExpressionUtils.EnumerableSkipMethodInfo, source, count);
		}
		
		internal static Expression EnumerableTake(this Expression source, int count)
		{
			return ExpressionUtils.CallMethodWithCount(ExpressionUtils.EnumerableTakeMethodInfo, source, count);
		}
		
		internal static Expression EnumerableThenBy(this Expression source, LambdaExpression keySelector)
		{
			return ExpressionUtils.CallMethodWithSelector(ExpressionUtils.EnumerableThenByMethodInfo, source, keySelector);
		}
		
		internal static Expression EnumerableThenByDescending(this Expression source, LambdaExpression keySelector)
		{
			return ExpressionUtils.CallMethodWithSelector(ExpressionUtils.EnumerableThenByDescendingMethodInfo, source, keySelector);
		}
		
		internal static Expression EnumerableWhere(this Expression source, LambdaExpression predicate)
		{
			return ExpressionUtils.Where(ExpressionUtils.EnumerableWhereMethodInfo, source, predicate);
		}
		
		internal static Expression GenerateTypeAsExpression(Expression instance, ResourceType resourceType)
		{
			Expression expression;
			if (!resourceType.CanReflectOnInstanceType)
			{
				Type[] instanceType = new Type[1];
				instanceType[0] = resourceType.InstanceType;
				expression = Expression.Call(null, DataServiceProviderMethods.TypeAsMethodInfo.MakeGenericMethod(instanceType), instance, Expression.Constant(resourceType));
			}
			else
			{
				expression = Expression.TypeAs(instance, resourceType.InstanceType);
			}
			return expression;
		}
		
		private static MethodInfo GetMethodInfoFromLambdaBody<TResult>(Expression<Func<TResult>> lambda)
		{
			return ((MethodCallExpression)lambda.Body).Method.GetGenericMethodDefinition();
		}
		
		internal static bool IsNullConstant(Expression expression)
		{
			if (expression == ExpressionUtils.NullLiteral)
			{
				return true;
			}
			else
			{
				if (expression.NodeType != ExpressionType.Constant)
				{
					return false;
				}
				else
				{
					return ((ConstantExpression)expression).Value == null;
				}
			}
		}
		
		internal static Expression QueryableLongCount(this Expression source)
		{
			return ExpressionUtils.CallMethodWithNoParam(ExpressionUtils.QueryableLongCountMethodInfo, source);
		}
		
		internal static Expression QueryableOfType(this Expression source, Type targetType)
		{
			return ExpressionUtils.CallMethodWithTypeParam(ExpressionUtils.QueryableOfTypeMethodInfo, source, targetType);
		}
		
		internal static Expression QueryableOrderBy(this Expression source, LambdaExpression keySelector)
		{
			return ExpressionUtils.CallMethodWithSelector(ExpressionUtils.QueryableOrderByMethodInfo, source, keySelector);
		}
		
		internal static Expression QueryableOrderByDescending(this Expression source, LambdaExpression keySelector)
		{
			return ExpressionUtils.CallMethodWithSelector(ExpressionUtils.QueryableOrderByDescendingMethodInfo, source, keySelector);
		}
		
		internal static Expression QueryableSelect(this Expression source, LambdaExpression selector)
		{
			return ExpressionUtils.CallMethodWithSelector(ExpressionUtils.QueryableSelectMethodInfo, source, selector);
		}
		
		internal static Expression QueryableSelectMany(this Expression source, LambdaExpression selector)
		{
			return ExpressionUtils.SelectMany(ExpressionUtils.QueryableSelectManyMethodInfo, source, selector);
		}
		
		internal static Expression QueryableSkip(this Expression source, int count)
		{
			return ExpressionUtils.CallMethodWithCount(ExpressionUtils.QueryableSkipMethodInfo, source, count);
		}
		
		internal static Expression QueryableTake(this Expression source, int count)
		{
			return ExpressionUtils.CallMethodWithCount(ExpressionUtils.QueryableTakeMethodInfo, source, count);
		}
		
		internal static Expression QueryableThenBy(this Expression source, LambdaExpression keySelector)
		{
			return ExpressionUtils.CallMethodWithSelector(ExpressionUtils.QueryableThenByMethodInfo, source, keySelector);
		}
		
		internal static Expression QueryableThenByDescending(this Expression source, LambdaExpression keySelector)
		{
			return ExpressionUtils.CallMethodWithSelector(ExpressionUtils.QueryableThenByDescendingMethodInfo, source, keySelector);
		}
		
		internal static Expression QueryableWhere(this Expression source, LambdaExpression predicate)
		{
			return ExpressionUtils.Where(ExpressionUtils.QueryableWhereMethodInfo, source, predicate);
		}
		
		private static LambdaExpression ReplaceParameterTypeForLambda(LambdaExpression input, Type targetType)
		{
			ParameterExpression parameterExpression = Expression.Parameter(targetType, input.Parameters[0].Name);
			ParameterExpression[] parameterExpressionArray = new ParameterExpression[1];
			parameterExpressionArray[0] = parameterExpression;
			return Expression.Lambda(ParameterReplacerVisitor.Replace(input.Body, input.Parameters[0], parameterExpression), parameterExpressionArray);
		}
		
		private static Expression SelectMany(MethodInfo genericMethodInfo, Expression source, LambdaExpression selector)
		{
			Type type = source.ElementType();
			Type enumerableElement = BaseServiceProvider.GetIEnumerableElement(selector.Body.Type);
			Type[] typeArray = new Type[2];
			typeArray[0] = type;
			typeArray[1] = enumerableElement;
			MethodInfo methodInfo = genericMethodInfo.MakeGenericMethod(typeArray);
			return Expression.Call(null, methodInfo, source, selector);
		}
		
		private static Expression Where(MethodInfo genericMethodInfo, Expression source, LambdaExpression predicate)
		{
			Type type = source.ElementType();
			Type[] typeArray = new Type[1];
			typeArray[0] = type;
			MethodInfo methodInfo = genericMethodInfo.MakeGenericMethod(typeArray);
			if (predicate.Parameters[0].Type != type && predicate.Parameters[0].Type.IsAssignableFrom(type))
			{
				predicate = ExpressionUtils.ReplaceParameterTypeForLambda(predicate, type);
			}
			return Expression.Call(null, methodInfo, source, predicate);
		}
	}
}