namespace System.Management.Automation.Language
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Linq.Expressions;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Runspaces;
    using System.Reflection;

    internal static class ExpressionCache
    {
        private static readonly Expression[] _intConstants = new Expression[0x66];
        internal static readonly Expression AutomationNullConstant = Expression.Constant(AutomationNull.Value, typeof(object));
        internal static readonly Expression BoxedFalse = Expression.Field(null, typeof(Boxed).GetField("False", BindingFlags.NonPublic | BindingFlags.Static));
        internal static readonly Expression BoxedTrue = Expression.Field(null, typeof(Boxed).GetField("True", BindingFlags.NonPublic | BindingFlags.Static));
        internal static readonly Expression CatchAllType = Expression.Constant(typeof(ExceptionHandlingOps.CatchAll));
        internal static readonly Expression ConstEmptyString = Expression.Constant("");
        internal static readonly Expression CurrentCultureIgnoreCaseComparer = Expression.Constant(StringComparer.CurrentCultureIgnoreCase);
        internal static readonly Expression Empty = Expression.Empty();
        internal static readonly Expression FalseConstant = Expression.Constant(false);
        internal static Expression GetExecutionContextFromTLS = Expression.Call(CachedReflectionInfo.LocalPipeline_GetExecutionContextFromTLS, new Expression[0]);
        internal static readonly Expression InvariantCulture = Expression.Constant(CultureInfo.InvariantCulture);
        internal static readonly Expression NullCommandRedirections = Expression.Constant(null, typeof(CommandRedirection[][]));
        internal static readonly Expression NullConstant = Expression.Constant(null);
        internal static readonly Expression NullDelegateArray = Expression.Constant(null, typeof(Action<FunctionContext>[]));
        internal static readonly Expression NullEnumerator = Expression.Constant(null, typeof(IEnumerator));
        internal static readonly Expression NullExecutionContext = Expression.Constant(null, typeof(ExecutionContext));
        internal static readonly Expression NullExtent = Expression.Constant(null, typeof(IScriptExtent));
        internal static readonly Expression NullFormatProvider = Expression.Constant(null, typeof(IFormatProvider));
        internal static readonly Expression NullObjectArray = Expression.Constant(null, typeof(object[]));
        internal static readonly Expression NullPSObject = Expression.Constant(null, typeof(PSObject));
        internal static readonly Expression NullType = Expression.Constant(null, typeof(Type));
        internal static readonly Expression NullTypeArray = Expression.Constant(null, typeof(Type[]));
        internal static readonly Expression NullTypeTable = Expression.Constant(null, typeof(TypeTable));
        internal static readonly Expression Ordinal = Expression.Constant(StringComparison.Ordinal);
        internal static readonly Expression StringComparisonInvariantCulture = Expression.Constant(StringComparison.InvariantCulture);
        internal static readonly Expression StringComparisonInvariantCultureIgnoreCase = Expression.Constant(StringComparison.InvariantCultureIgnoreCase);
        internal static readonly Expression TrueConstant = Expression.Constant(true);

        internal static Expression Constant(bool b)
        {
            if (!b)
            {
                return FalseConstant;
            }
            return TrueConstant;
        }

        internal static Expression Constant(int i)
        {
            if ((i < -1) || (i > 100))
            {
                return Expression.Constant(i);
            }
            Expression expression = _intConstants[i + 1];
            if (expression == null)
            {
                expression = Expression.Constant(i);
                _intConstants[i + 1] = expression;
            }
            return expression;
        }
    }
}

