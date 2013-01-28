namespace System.Management.Automation.Language
{
    using System;
    using System.Linq.Expressions;
    using System.Management.Automation;
    using System.Runtime.CompilerServices;

    internal static class ExpressionExtensions
    {
        internal static Expression Cast(this Expression expr, Type type)
        {
            if (expr.Type.Equals(type))
            {
                return expr;
            }
            if ((expr.Type.IsFloating() || expr.Type.Equals(typeof(decimal))) && type.IsPrimitive)
            {
                expr = Expression.Call(CachedReflectionInfo.Convert_ChangeType, Expression.Convert(expr, typeof(object)), Expression.Constant(type));
            }
            return Expression.Convert(expr, type);
        }

        internal static Expression Convert(this Expression expr, Type type)
        {
            if (expr.Type.Equals(type))
            {
                return expr;
            }
            if (expr.Type.Equals(typeof(void)))
            {
                expr = ExpressionCache.NullConstant;
            }
            if (LanguagePrimitives.GetConversionRank(expr.Type, type) == ConversionRank.Assignable)
            {
                return Expression.Convert(expr, type);
            }
            if (type.ContainsGenericParameters)
            {
                return Expression.Call(CachedReflectionInfo.LanguagePrimitives_ThrowInvalidCastException, expr.Cast(typeof(object)), Expression.Constant(type));
            }
            return Expression.Dynamic(PSConvertBinder.Get(type), type, expr);
        }
    }
}

