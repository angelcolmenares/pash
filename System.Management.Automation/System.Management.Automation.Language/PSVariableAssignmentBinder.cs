namespace System.Management.Automation.Language
{
    using System;
    using System.Collections.Concurrent;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    internal class PSVariableAssignmentBinder : DynamicMetaObjectBinder
    {
        private static readonly PSVariableAssignmentBinder _binder = new PSVariableAssignmentBinder();
        private static int _mutableValueWithInstanceMemberVersion = 0;
        private static ConcurrentDictionary<Type, bool> MutableValueTypesWithInstanceMembers = new ConcurrentDictionary<Type, bool>();

        private PSVariableAssignmentBinder()
        {
        }

        public override DynamicMetaObject Bind(DynamicMetaObject target, DynamicMetaObject[] args)
        {
            if (!target.HasValue)
            {
                return base.Defer(target, new DynamicMetaObject[0]);
            }
            object obj2 = target.Value;
            if (obj2 == null)
            {
                return new DynamicMetaObject(ExpressionCache.NullConstant, target.PSGetTypeRestriction()).WriteToDebugLog(this);
            }
            PSObject obj3 = obj2 as PSObject;
            if (obj3 != null)
            {
                BindingRestrictions typeRestriction = BindingRestrictions.GetTypeRestriction(target.Expression, obj3.GetType());
                Expression expr = target.Expression;
                object baseObject = obj3.BaseObject;
                MemberExpression expression = Expression.Property(expr.Cast(typeof(PSObject)), CachedReflectionInfo.PSObject_BaseObject);
                if (baseObject != null)
                {
                    Type type = baseObject.GetType();
                    typeRestriction = typeRestriction.Merge(BindingRestrictions.GetTypeRestriction(expression, type));
                    if (type.IsValueType)
                    {
                        expr = GetExprForValueType(type, Expression.Convert(expression, type), expr, ref typeRestriction);
                    }
                }
                else
                {
                    typeRestriction = typeRestriction.Merge(BindingRestrictions.GetExpressionRestriction(Expression.Equal(expression, ExpressionCache.NullConstant)));
                }
                return new DynamicMetaObject(expr, typeRestriction).WriteToDebugLog(this);
            }
            Type type2 = obj2.GetType();
            if (type2.IsValueType)
            {
                Expression originalExpr = target.Expression;
                BindingRestrictions restrictions = target.PSGetTypeRestriction();
                return new DynamicMetaObject(GetExprForValueType(type2, Expression.Convert(originalExpr, type2), originalExpr, ref restrictions), restrictions).WriteToDebugLog(this);
            }
            return new DynamicMetaObject(target.Expression, BindingRestrictions.GetExpressionRestriction(Expression.AndAlso(Expression.Not(Expression.TypeIs(target.Expression, typeof(ValueType))), Expression.Not(Expression.TypeIs(target.Expression, typeof(PSObject)))))).WriteToDebugLog(this);
        }

        public override T BindDelegate<T>(CallSite<T> site, object[] args)
        {
            object obj2 = args[0];
            if (((obj2 != null) && obj2.GetType().IsClass) && (!(obj2 is PSObject) && (typeof(T) == typeof(Func<CallSite, object, object>))))
            {
                T target = (T) Convert.ChangeType(new Func<CallSite, object, object>(PSVariableAssignmentBinder.ObjectRule), typeof(T));
                base.CacheTarget<T>(target);
                return target;
            }
            return base.BindDelegate<T>(site, args);
        }

        internal static object CopyInstanceMembersOfValueType<T>(T t, object boxedT) where T: struct
        {
            PSMemberInfoInternalCollection<PSMemberInfo> internals;
            ConsolidatedString str;
            if (!PSObject.HasInstanceMembers(boxedT, out internals) && !PSObject.HasInstanceTypeName(boxedT, out str))
            {
                return t;
            }
            return PSObject.Base(PSObject.AsPSObject(boxedT).Copy());
        }

        internal static PSVariableAssignmentBinder Get()
        {
            return _binder;
        }

        private static Expression GetExprForValueType(Type type, Expression convertedExpr, Expression originalExpr, ref BindingRestrictions restrictions)
        {
            Expression expression;
            bool flag = true;
            if (MutableValueTypesWithInstanceMembers.ContainsKey(type))
            {
                expression = Expression.Call(CachedReflectionInfo.PSVariableAssignmentBinder_CopyInstanceMembersOfValueType.MakeGenericMethod(new Type[] { type }), convertedExpr, originalExpr);
                flag = false;
            }
            else if (IsValueTypeMutable(type))
            {
                ParameterExpression left = Expression.Variable(type);
                expression = Expression.Block(new ParameterExpression[] { left }, new Expression[] { Expression.Assign(left, convertedExpr), left.Cast(typeof(object)) });
            }
            else
            {
                expression = originalExpr;
            }
            if (flag)
            {
                restrictions = restrictions.Merge(GetVersionCheck(_mutableValueWithInstanceMemberVersion));
            }
            return expression;
        }

        internal static BindingRestrictions GetVersionCheck(int expectedVersionNumber)
        {
            return BindingRestrictions.GetExpressionRestriction(Expression.Equal(Expression.Field(null, CachedReflectionInfo.PSVariableAssignmentBinder__mutableValueWithInstanceMemberVersion), ExpressionCache.Constant(expectedVersionNumber)));
        }

        internal static bool IsValueTypeMutable(Type type)
        {
            if (type.IsPrimitive || type.IsEnum)
            {
                return false;
            }
            if (type.GetFields(BindingFlags.Public | BindingFlags.Instance).Any<FieldInfo>())
            {
                return true;
            }
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo info in properties)
            {
                if (info.CanWrite)
                {
                    return true;
                }
            }
            return (type.GetMethods(BindingFlags.Public | BindingFlags.Instance).Length != properties.Length);
        }

        internal static void NoteTypeHasInstanceMemberOrTypeName(Type type)
        {
            if ((type.IsValueType && IsValueTypeMutable(type)) && MutableValueTypesWithInstanceMembers.TryAdd(type, true))
            {
                _mutableValueWithInstanceMemberVersion++;
            }
        }

        private static object ObjectRule(CallSite site, object obj)
        {
            if ((obj is ValueType) || (obj is PSObject))
            {
                return ((CallSite<Func<CallSite, object, object>>) site).Update(site, obj);
            }
            return obj;
        }
    }
}

