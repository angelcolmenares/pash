namespace System.Management.Automation.Interpreter
{
    using System;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal static class Utils
    {
        private static readonly DefaultExpression VoidInstance = Expression.Empty();

        public static Expression Box(Expression expression)
        {
            MethodInfo booleanToObjectMethod;
            if (expression.Type == typeof(int))
            {
                booleanToObjectMethod = ScriptingRuntimeHelpers.Int32ToObjectMethod;
            }
            else if (expression.Type == typeof(bool))
            {
                booleanToObjectMethod = ScriptingRuntimeHelpers.BooleanToObjectMethod;
            }
            else
            {
                booleanToObjectMethod = null;
            }
            return Expression.Convert(expression, typeof(object), booleanToObjectMethod);
        }

        internal static Expression Constant(object value)
        {
            return Expression.Constant(value);
        }

        public static Expression Convert(Expression expression, Type type)
        {
            if (expression.Type == type)
            {
                return expression;
            }
            if (expression.Type == typeof(void))
            {
                return Expression.Block(expression, Default(type));
            }
            if (type == typeof(void))
            {
                return Void(expression);
            }
            if (type == typeof(object))
            {
                return Box(expression);
            }
            return Expression.Convert(expression, type);
        }

        public static DefaultExpression Default(Type type)
        {
            if (type == typeof(void))
            {
                return Empty();
            }
            return Expression.Default(type);
        }

        public static DefaultExpression Empty()
        {
            return VoidInstance;
        }

        public static bool IsReadWriteAssignment(this ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.AddAssign:
                case ExpressionType.AndAssign:
                case ExpressionType.DivideAssign:
                case ExpressionType.ExclusiveOrAssign:
                case ExpressionType.LeftShiftAssign:
                case ExpressionType.ModuloAssign:
                case ExpressionType.MultiplyAssign:
                case ExpressionType.OrAssign:
                case ExpressionType.PowerAssign:
                case ExpressionType.RightShiftAssign:
                case ExpressionType.SubtractAssign:
                case ExpressionType.AddAssignChecked:
                case ExpressionType.MultiplyAssignChecked:
                case ExpressionType.SubtractAssignChecked:
                case ExpressionType.PreIncrementAssign:
                case ExpressionType.PreDecrementAssign:
                case ExpressionType.PostIncrementAssign:
                case ExpressionType.PostDecrementAssign:
                    return true;
            }
            return false;
        }

        public static Expression Void(Expression expression)
        {
            if (expression.Type == typeof(void))
            {
                return expression;
            }
            return Expression.Block(expression, Empty());
        }
    }
}

