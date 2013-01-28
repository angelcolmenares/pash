namespace System.Management.Automation
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq.Expressions;
    using System.Reflection;

    internal class ReflectionParameterBinder : ParameterBinderBase
    {
        private static readonly ConcurrentDictionary<Tuple<Type, string>, Func<object, object>> _getterMethods = new ConcurrentDictionary<Tuple<Type, string>, Func<object, object>>();
        private static readonly ConcurrentDictionary<Tuple<Type, string>, Action<object, object>> _setterMethods = new ConcurrentDictionary<Tuple<Type, string>, Action<object, object>>();

        internal ReflectionParameterBinder(object target, Cmdlet command) : base(target, command.MyInvocation, command.Context, command)
        {
        }

        internal ReflectionParameterBinder(object target, Cmdlet command, CommandLineParameters commandLineParameters) : base(target, command.MyInvocation, command.Context, command)
        {
            base.CommandLineParameters = commandLineParameters;
        }

        internal override void BindParameter(string name, object value)
        {
            try
            {
                GetSetter(base.Target.GetType(), name)(base.Target, value);
            }
            catch (TargetInvocationException exception)
            {
                Exception innerException = exception.InnerException ?? exception;
                throw new SetValueInvocationException("CatchFromBaseAdapterSetValueTI", innerException, ExtendedTypeSystem.ExceptionWhenSetting, new object[] { name, innerException.Message });
            }
            catch (SetValueException)
            {
                throw;
            }
            catch (Exception exception3)
            {
                CommandProcessorBase.CheckForSevereException(exception3);
                throw new SetValueInvocationException("CatchFromBaseAdapterSetValue", exception3, ExtendedTypeSystem.ExceptionWhenSetting, new object[] { name, exception3.Message });
            }
        }

        internal override object GetDefaultParameterValue(string name)
        {
            object obj2;
            try
            {
                obj2 = GetGetter(base.Target.GetType(), name)(base.Target);
            }
            catch (TargetInvocationException exception)
            {
                Exception innerException = exception.InnerException ?? exception;
                throw new GetValueInvocationException("CatchFromBaseAdapterGetValueTI", innerException, ExtendedTypeSystem.ExceptionWhenGetting, new object[] { name, innerException.Message });
            }
            catch (GetValueException)
            {
                throw;
            }
            catch (Exception exception3)
            {
                CommandProcessorBase.CheckForSevereException(exception3);
                throw new GetValueInvocationException("CatchFromBaseAdapterGetValue", exception3, ExtendedTypeSystem.ExceptionWhenGetting, new object[] { name, exception3.Message });
            }
            return obj2;
        }

        private static Func<object, object> GetGetter(Type type, string property)
        {
            return _getterMethods.GetOrAdd(Tuple.Create<Type, string>(type, property), delegate (Tuple<Type, string> _) {
                ParameterExpression expression = Expression.Parameter(typeof(object));
                return Expression.Lambda<Func<object, object>>(Expression.Convert(GetPropertyOrFieldExpr(type, property, Expression.Convert(expression, type)), typeof(object)), new ParameterExpression[] { expression }).Compile();
            });
        }

        private static Expression GetPropertyOrFieldExpr(Type type, string name, Expression target)
        {
            try
            {
                PropertyInfo property = type.GetProperty(name, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance);
                if (property != null)
                {
                    return Expression.Property(target, property);
                }
            }
            catch (AmbiguousMatchException)
            {
                foreach (PropertyInfo info2 in type.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance))
                {
                    if (info2.Name.Equals(name, StringComparison.Ordinal))
                    {
                        return Expression.Property(target, info2);
                    }
                }
            }
            try
            {
                FieldInfo field = type.GetField(name, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance);
                if (field != null)
                {
                    return Expression.Field(target, field);
                }
            }
            catch (AmbiguousMatchException)
            {
                foreach (FieldInfo info4 in type.GetFields(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance))
                {
                    if (info4.Name.Equals(name, StringComparison.Ordinal))
                    {
                        return Expression.Field(target, info4);
                    }
                }
            }
            throw PSTraceSource.NewInvalidOperationException();
        }

        private static Action<object, object> GetSetter(Type type, string property)
        {
            return _setterMethods.GetOrAdd(Tuple.Create<Type, string>(type, property), delegate (Tuple<Type, string> _) {
                ParameterExpression expression = Expression.Parameter(typeof(object));
                ParameterExpression expression2 = Expression.Parameter(typeof(object));
                Expression left = GetPropertyOrFieldExpr(type, property, Expression.Convert(expression, type));
                return Expression.Lambda<Action<object, object>>(Expression.Block(new Expression[] { Expression.Assign(left, Expression.Convert(expression2, left.Type)) }), new ParameterExpression[] { expression, expression2 }).Compile();
            });
        }
    }
}

