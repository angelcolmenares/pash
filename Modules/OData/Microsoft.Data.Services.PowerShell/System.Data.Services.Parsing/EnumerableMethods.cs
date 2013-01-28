using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace System.Data.Services.Parsing
{
	internal static class EnumerableMethods
	{
		public static MethodInfo GetMethod(Type type, string method)
		{
			return GetMethod (type, method, typeof(object));
		}

		public static MethodInfo GetMethod(Type type, string method, Type dynamicMethod)
		{
			return type.GetMethod (method, BindingFlags.Public | BindingFlags.Static).MakeGenericMethod (new Type[] { dynamicMethod });    
		}
		public static IEnumerable<TResult> Select<TSource, TResult>(this IEnumerable<TSource> obj, Expression<Func<TSource, TResult>> func)
		{
			return obj.Select((Func<TSource, TResult>)func.Compile ());
		}

	}
}

