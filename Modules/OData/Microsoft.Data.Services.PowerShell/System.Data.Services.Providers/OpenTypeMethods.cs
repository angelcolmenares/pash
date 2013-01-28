namespace System.Data.Services.Providers
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;

    internal static class OpenTypeMethods
    {
        internal static readonly MethodInfo AddMethodInfo = typeof(OpenTypeMethods).GetMethod("Add", BindingFlags.Public | BindingFlags.Static);
        internal static readonly MethodInfo AndAlsoMethodInfo = typeof(OpenTypeMethods).GetMethod("AndAlso", BindingFlags.Public | BindingFlags.Static);
        internal static readonly MethodInfo ConvertMethodInfo = typeof(OpenTypeMethods).GetMethod("Convert", BindingFlags.Public | BindingFlags.Static);
        internal static readonly MethodInfo DivideMethodInfo = typeof(OpenTypeMethods).GetMethod("Divide", BindingFlags.Public | BindingFlags.Static);
        internal static readonly MethodInfo EqualMethodInfo = typeof(OpenTypeMethods).GetMethod("Equal", BindingFlags.Public | BindingFlags.Static);
        internal static readonly MethodInfo GetValueOpenPropertyMethodInfo = typeof(OpenTypeMethods).GetMethod("GetValue", BindingFlags.Public | BindingFlags.Static, null, new Type[] { typeof(object), typeof(string) }, null);
        internal static readonly MethodInfo GreaterThanMethodInfo = typeof(OpenTypeMethods).GetMethod("GreaterThan", BindingFlags.Public | BindingFlags.Static);
        internal static readonly MethodInfo GreaterThanOrEqualMethodInfo = typeof(OpenTypeMethods).GetMethod("GreaterThanOrEqual", BindingFlags.Public | BindingFlags.Static);
        internal static readonly MethodInfo LessThanMethodInfo = typeof(OpenTypeMethods).GetMethod("LessThan", BindingFlags.Public | BindingFlags.Static);
        internal static readonly MethodInfo LessThanOrEqualMethodInfo = typeof(OpenTypeMethods).GetMethod("LessThanOrEqual", BindingFlags.Public | BindingFlags.Static);
        internal static readonly MethodInfo ModuloMethodInfo = typeof(OpenTypeMethods).GetMethod("Modulo", BindingFlags.Public | BindingFlags.Static);
        internal static readonly MethodInfo MultiplyMethodInfo = typeof(OpenTypeMethods).GetMethod("Multiply", BindingFlags.Public | BindingFlags.Static);
        internal static readonly MethodInfo NegateMethodInfo = typeof(OpenTypeMethods).GetMethod("Negate", BindingFlags.Public | BindingFlags.Static);
        internal static readonly MethodInfo NotEqualMethodInfo = typeof(OpenTypeMethods).GetMethod("NotEqual", BindingFlags.Public | BindingFlags.Static);
        internal static readonly MethodInfo NotMethodInfo = typeof(OpenTypeMethods).GetMethod("Not", BindingFlags.Public | BindingFlags.Static);
        internal static readonly MethodInfo OrElseMethodInfo = typeof(OpenTypeMethods).GetMethod("OrElse", BindingFlags.Public | BindingFlags.Static);
        internal static readonly MethodInfo SubtractMethodInfo = typeof(OpenTypeMethods).GetMethod("Subtract", BindingFlags.Public | BindingFlags.Static);
        internal static readonly MethodInfo TypeIsMethodInfo = typeof(OpenTypeMethods).GetMethod("TypeIs", BindingFlags.Public | BindingFlags.Static);

        public static object Add(object left, object right)
        {
            throw new NotImplementedException();
        }

        internal static Expression AddExpression(Expression left, Expression right)
        {
            return Expression.Add(ExpressionAsObject(left), ExpressionAsObject(right), AddMethodInfo);
        }

        public static object AndAlso(object left, object right)
        {
            throw new NotImplementedException();
        }

        internal static Expression AndAlsoExpression(Expression left, Expression right)
        {
            return Expression.Call(AndAlsoMethodInfo, ExpressionAsObject(left), ExpressionAsObject(right));
        }

        public static object Ceiling(object value)
        {
            throw new NotImplementedException();
        }

        public static object Concat(object first, object second)
        {
            throw new NotImplementedException();
        }

        public static object Convert(object value, ResourceType type)
        {
            throw new NotImplementedException();
        }

        public static object Day(object dateTime)
        {
            throw new NotImplementedException();
        }

        public static object Distance(object left, object right)
        {
            throw new NotImplementedException();
        }

        public static object Divide(object left, object right)
        {
            throw new NotImplementedException();
        }

        internal static Expression DivideExpression(Expression left, Expression right)
        {
            return Expression.Divide(ExpressionAsObject(left), ExpressionAsObject(right), DivideMethodInfo);
        }

        public static object EndsWith(object targetString, object substring)
        {
            throw new NotImplementedException();
        }

        public static object Equal(object left, object right)
        {
            throw new NotImplementedException();
        }

        internal static Expression EqualExpression(Expression left, Expression right)
        {
            return Expression.Equal(ExpressionAsObject(left), ExpressionAsObject(right), false, EqualMethodInfo);
        }

        private static Expression ExpressionAsObject(Expression expression)
        {
            if (!expression.Type.IsValueType)
            {
                return expression;
            }
            return Expression.Convert(expression, typeof(object));
        }

        public static object Floor(object value)
        {
            throw new NotImplementedException();
        }

        public static object GetValue(object value, string propertyName)
        {
            throw new NotImplementedException();
        }

        public static object GreaterThan(object left, object right)
        {
            throw new NotImplementedException();
        }

        internal static Expression GreaterThanExpression(Expression left, Expression right)
        {
            return Expression.GreaterThan(ExpressionAsObject(left), ExpressionAsObject(right), false, GreaterThanMethodInfo);
        }

        public static object GreaterThanOrEqual(object left, object right)
        {
            throw new NotImplementedException();
        }

        internal static Expression GreaterThanOrEqualExpression(Expression left, Expression right)
        {
            return Expression.GreaterThanOrEqual(ExpressionAsObject(left), ExpressionAsObject(right), false, GreaterThanOrEqualMethodInfo);
        }

        public static object Hour(object dateTime)
        {
            throw new NotImplementedException();
        }

        public static object IndexOf(object targetString, object substring)
        {
            throw new NotImplementedException();
        }

        public static object Length(object value)
        {
            throw new NotImplementedException();
        }

        public static object LessThan(object left, object right)
        {
            throw new NotImplementedException();
        }

        internal static Expression LessThanExpression(Expression left, Expression right)
        {
            return Expression.LessThan(ExpressionAsObject(left), ExpressionAsObject(right), false, LessThanMethodInfo);
        }

        public static object LessThanOrEqual(object left, object right)
        {
            throw new NotImplementedException();
        }

        internal static Expression LessThanOrEqualExpression(Expression left, Expression right)
        {
            return Expression.LessThanOrEqual(ExpressionAsObject(left), ExpressionAsObject(right), false, LessThanOrEqualMethodInfo);
        }

        public static object Minute(object dateTime)
        {
            throw new NotImplementedException();
        }

        public static object Modulo(object left, object right)
        {
            throw new NotImplementedException();
        }

        internal static Expression ModuloExpression(Expression left, Expression right)
        {
            return Expression.Modulo(ExpressionAsObject(left), ExpressionAsObject(right), ModuloMethodInfo);
        }

        public static object Month(object dateTime)
        {
            throw new NotImplementedException();
        }

        public static object Multiply(object left, object right)
        {
            throw new NotImplementedException();
        }

        internal static Expression MultiplyExpression(Expression left, Expression right)
        {
            return Expression.Multiply(ExpressionAsObject(left), ExpressionAsObject(right), MultiplyMethodInfo);
        }

        public static object Negate(object value)
        {
            throw new NotImplementedException();
        }

        internal static Expression NegateExpression(Expression expression)
        {
            return Expression.Negate(ExpressionAsObject(expression), NegateMethodInfo);
        }

        public static object Not(object value)
        {
            throw new NotImplementedException();
        }

        public static object NotEqual(object left, object right)
        {
            throw new NotImplementedException();
        }

        internal static Expression NotEqualExpression(Expression left, Expression right)
        {
            return Expression.NotEqual(ExpressionAsObject(left), ExpressionAsObject(right), false, NotEqualMethodInfo);
        }

        internal static Expression NotExpression(Expression expression)
        {
            return Expression.Not(ExpressionAsObject(expression), NotMethodInfo);
        }

        public static object OrElse(object left, object right)
        {
            throw new NotImplementedException();
        }

        internal static Expression OrElseExpression(Expression left, Expression right)
        {
            return Expression.Call(OrElseMethodInfo, ExpressionAsObject(left), ExpressionAsObject(right));
        }

        public static object Replace(object targetString, object substring, object newString)
        {
            throw new NotImplementedException();
        }

        public static object Round(object value)
        {
            throw new NotImplementedException();
        }

        public static object Second(object dateTime)
        {
            throw new NotImplementedException();
        }

        public static object StartsWith(object targetString, object substring)
        {
            throw new NotImplementedException();
        }

        public static object Substring(object targetString, object startIndex)
        {
            throw new NotImplementedException();
        }

        public static object Substring(object targetString, object startIndex, object length)
        {
            throw new NotImplementedException();
        }

        public static object SubstringOf(object substring, object targetString)
        {
            throw new NotImplementedException();
        }

        public static object Subtract(object left, object right)
        {
            throw new NotImplementedException();
        }

        internal static Expression SubtractExpression(Expression left, Expression right)
        {
            return Expression.Subtract(ExpressionAsObject(left), ExpressionAsObject(right), SubtractMethodInfo);
        }

        public static object ToLower(object targetString)
        {
            throw new NotImplementedException();
        }

        public static object ToUpper(object targetString)
        {
            throw new NotImplementedException();
        }

        public static object Trim(object targetString)
        {
            throw new NotImplementedException();
        }

        public static object TypeIs(object value, ResourceType type)
        {
            throw new NotImplementedException();
        }

        public static object Year(object dateTime)
        {
            throw new NotImplementedException();
        }
    }
}

