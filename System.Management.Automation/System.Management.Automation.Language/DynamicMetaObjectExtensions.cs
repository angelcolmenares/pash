namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Linq.Expressions;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal static class DynamicMetaObjectExtensions
    {
        internal static readonly DynamicMetaObject FakeError = new DynamicMetaObject(ExpressionCache.NullConstant, BindingRestrictions.Empty);

        internal static Expression CastOrConvert(this DynamicMetaObject target, Type type)
        {
            bool flag;
            if (target.LimitType.Equals(type))
            {
                return target.Expression.Cast(type);
            }
            return PSConvertBinder.InvokeConverter(LanguagePrimitives.FigureConversion(target.Value, type, out flag), target.Expression, type, flag, ExpressionCache.InvariantCulture);
        }

        internal static Expression CastOrConvertMethodArgument(this DynamicMetaObject target, Type parameterType, string parameterName, string methodName, List<ParameterExpression> temps, List<Expression> initTemps)
        {
            bool flag;
            if (target.Value == AutomationNull.Value)
            {
                return Expression.Constant(null, parameterType);
            }
            Type limitType = target.LimitType;
            if (parameterType.Equals(typeof(object)) && limitType.Equals(typeof(PSObject)))
            {
                return Expression.Call(CachedReflectionInfo.PSObject_Base, target.Expression.Cast(typeof(PSObject)));
            }
            if (parameterType.IsAssignableFrom(limitType))
            {
                return target.Expression.Cast(parameterType);
            }
            ParameterExpression variable = Expression.Variable(typeof(Exception));
            ParameterExpression expression2 = Expression.Variable(target.Expression.Type);
            Expression expression3 = PSConvertBinder.InvokeConverter(LanguagePrimitives.FigureConversion(target.Value, parameterType, out flag), expression2, parameterType, flag, ExpressionCache.InvariantCulture);
            BlockExpression right = Expression.Block(new ParameterExpression[] { expression2 }, new Expression[] { Expression.TryCatch(Expression.Block(Expression.Assign(expression2, target.Expression), expression3), new CatchBlock[] { Expression.Catch(variable, Expression.Block(Expression.Call(CachedReflectionInfo.ExceptionHandlingOps_ConvertToArgumentConversionException, variable, Expression.Constant(parameterName), expression2.Cast(typeof(object)), Expression.Constant(methodName), Expression.Constant(parameterType)), Expression.Default(expression3.Type))) }) });
            ParameterExpression item = Expression.Variable(right.Type);
            temps.Add(item);
            initTemps.Add(Expression.Assign(item, right));
            return item;
        }

        internal static BindingRestrictions CombineRestrictions(this DynamicMetaObject target, params DynamicMetaObject[] args)
        {
            BindingRestrictions restrictions = (target.Restrictions == BindingRestrictions.Empty) ? target.PSGetTypeRestriction() : target.Restrictions;
            foreach (DynamicMetaObject obj2 in args)
            {
                restrictions = restrictions.Merge((obj2.Restrictions == BindingRestrictions.Empty) ? obj2.PSGetTypeRestriction() : obj2.Restrictions);
            }
            return restrictions;
        }

        internal static BindingRestrictions GetSimpleTypeRestriction(this DynamicMetaObject obj)
        {
            if (obj.Value == null)
            {
                return BindingRestrictions.GetInstanceRestriction(obj.Expression, null);
            }
            return BindingRestrictions.GetTypeRestriction(obj.Expression, obj.Value.GetType());
        }

        internal static BindingRestrictions PSGetMethodArgumentRestriction(this DynamicMetaObject obj)
        {
            BindingRestrictions typeRestriction;
            Expression expression;
            object obj2 = PSObject.Base(obj.Value);
            if ((obj2 == null) || !obj2.GetType().Equals(typeof(object[])))
            {
                return obj.PSGetTypeRestriction();
            }
            Type type = Adapter.EffectiveArgumentType(obj.Value);
            MethodInfo method = !type.Equals(typeof(object[])) ? CachedReflectionInfo.PSInvokeMemberBinder_IsHomogenousArray.MakeGenericMethod(new Type[] { type.GetElementType() }) : CachedReflectionInfo.PSInvokeMemberBinder_IsHeterogeneousArray;
            if (obj.Value != obj2)
            {
                typeRestriction = BindingRestrictions.GetTypeRestriction(obj.Expression, typeof(PSObject));
                ParameterExpression left = Expression.Variable(typeof(object[]));
                expression = Expression.Block(new ParameterExpression[] { left }, new Expression[] { Expression.Assign(left, Expression.TypeAs(Expression.Call(CachedReflectionInfo.PSObject_Base, obj.Expression), typeof(object[]))), Expression.AndAlso(Expression.NotEqual(left, ExpressionCache.NullObjectArray), Expression.Call(method, left)) });
            }
            else
            {
                typeRestriction = BindingRestrictions.GetTypeRestriction(obj.Expression, typeof(object[]));
                Expression expression3 = obj.Expression.Cast(typeof(object[]));
                expression = Expression.Call(method, expression3);
            }
            return typeRestriction.Merge(BindingRestrictions.GetExpressionRestriction(expression));
        }

        internal static BindingRestrictions PSGetTypeRestriction(this DynamicMetaObject obj)
        {
            if (obj.Restrictions != BindingRestrictions.Empty)
            {
                return obj.Restrictions;
            }
            if (obj.Value == null)
            {
                return BindingRestrictions.GetInstanceRestriction(obj.Expression, null);
            }
            object obj2 = PSObject.Base(obj.Value);
            if (obj2 == null)
            {
                return BindingRestrictions.GetExpressionRestriction(Expression.Equal(obj.Expression, Expression.Constant(AutomationNull.Value)));
            }
            BindingRestrictions typeRestriction = BindingRestrictions.GetTypeRestriction(obj.Expression, obj.LimitType);
            if (obj.Value != obj2)
            {
                return typeRestriction.Merge(BindingRestrictions.GetTypeRestriction(Expression.Call(CachedReflectionInfo.PSObject_Base, obj.Expression), obj2.GetType()));
            }
            if (obj2 is PSObject)
            {
                typeRestriction = typeRestriction.Merge(BindingRestrictions.GetExpressionRestriction(Expression.Equal(Expression.Call(CachedReflectionInfo.PSObject_Base, obj.Expression), obj.Expression)));
            }
            return typeRestriction;
        }

        internal static DynamicMetaObject ThrowRuntimeError(this DynamicMetaObject target, DynamicMetaObject[] args, BindingRestrictions moreTests, string errorID, string resourceString, params Expression[] exceptionArgs)
        {
            return new DynamicMetaObject(Compiler.ThrowRuntimeError(errorID, resourceString, exceptionArgs), target.CombineRestrictions(args).Merge(moreTests));
        }

        internal static DynamicMetaObject WriteToDebugLog(this DynamicMetaObject obj, DynamicMetaObjectBinder binder)
        {
            return obj;
        }
    }
}

