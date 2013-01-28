namespace System.Management.Automation.Language
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Globalization;
    using System.Linq.Expressions;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Runspaces;
    using System.Reflection;

    internal class PSSetMemberBinder : SetMemberBinder
    {
        private static readonly Dictionary<string, PSSetMemberBinder> _binderCache = new Dictionary<string, PSSetMemberBinder>(StringComparer.Ordinal);
        private static readonly Dictionary<string, List<PSSetMemberBinder>> _binderCacheIgnoringCase = new Dictionary<string, List<PSSetMemberBinder>>(StringComparer.OrdinalIgnoreCase);
        private readonly PSGetMemberBinder _getMemberBinder;
        private readonly bool _static;
        private static readonly Dictionary<string, PSSetMemberBinder> _staticBinderCache = new Dictionary<string, PSSetMemberBinder>(StringComparer.OrdinalIgnoreCase);

        public PSSetMemberBinder(string name, bool ignoreCase, bool @static) : base(name, ignoreCase)
        {
            this._static = @static;
            this._getMemberBinder = PSGetMemberBinder.Get(name, @static);
        }

        public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion)
        {
            PSMemberInfo info;
            BindingRestrictions restrictions2;
            bool flag3;
            Type type4;
            if (!target.HasValue || !value.HasValue)
            {
                return base.Defer(target, new DynamicMetaObject[] { value });
            }
            object obj2 = PSObject.Base(target.Value);
            if (obj2 == null)
            {
                return target.ThrowRuntimeError(new DynamicMetaObject[] { value }, BindingRestrictions.Empty, "PropertyNotFound", ParserStrings.PropertyNotFound, new Expression[] { Expression.Constant(base.Name) }).WriteToDebugLog(this);
            }
            if (value.Value == AutomationNull.Value)
            {
                value = new DynamicMetaObject(ExpressionCache.NullConstant, value.PSGetTypeRestriction(), null);
            }
            if (this._getMemberBinder.HasInstanceMember && PSGetMemberBinder.TryGetInstanceMember(target.Value, base.Name, out info))
            {
                ParameterExpression expression = Expression.Variable(typeof(PSMemberInfo));
                ParameterExpression expression2 = Expression.Variable(typeof(object));
                ConditionalExpression expression3 = Expression.Condition(Expression.Call(CachedReflectionInfo.PSGetMemberBinder_TryGetInstanceMember, target.Expression.Cast(typeof(object)), Expression.Constant(base.Name), expression), Expression.Assign(Expression.Property(expression, "Value"), value.Expression.Cast(typeof(object))), base.GetUpdateExpression(typeof(object)));
                BindingRestrictions restrictions = BinderUtils.GetVersionCheck(this._getMemberBinder, this._getMemberBinder._version).Merge(value.PSGetTypeRestriction());
                return new DynamicMetaObject(Expression.Block(new ParameterExpression[] { expression, expression2 }, new Expression[] { expression3 }), restrictions).WriteToDebugLog(this);
            }
            if (obj2 is IDictionary)
            {
                Type genericTypeArg = null;
                bool flag = PSGetMemberBinder.IsGenericDictionary(obj2, ref genericTypeArg);
                if (!flag || (genericTypeArg != null))
                {
                    bool flag2;
                    Type type = flag ? typeof(IDictionary<,>).MakeGenericType(new Type[] { typeof(string), genericTypeArg }) : typeof(IDictionary);
                    MethodInfo method = type.GetMethod("set_Item");
                    ParameterExpression left = Expression.Variable(genericTypeArg ?? typeof(object));
                    Type resultType = left.Type;
                    LanguagePrimitives.ConversionData conversion = LanguagePrimitives.FigureConversion(value.Value, resultType, out flag2);
                    if (conversion.Rank != ConversionRank.None)
                    {
                        Expression right = PSConvertBinder.InvokeConverter(conversion, value.Expression, resultType, flag2, ExpressionCache.InvariantCulture);
                        return new DynamicMetaObject(Expression.Block(new ParameterExpression[] { left }, new Expression[] { Expression.Assign(left, right), Expression.Call(PSGetMemberBinder.GetTargetExpr(target).Cast(type), method, Expression.Constant(base.Name), right), right.Cast(typeof(object)) }), target.CombineRestrictions(new DynamicMetaObject[] { value })).WriteToDebugLog(this);
                    }
                }
            }
            info = this._getMemberBinder.GetPSMemberInfo(target, out restrictions2, out flag3, out type4, null, null);
            restrictions2 = restrictions2.Merge(value.PSGetTypeRestriction());
            if (ExecutionContext.HasEverUsedConstrainedLanguage)
            {
                restrictions2 = restrictions2.Merge(BinderUtils.GetLanguageModeCheckIfHasEverUsedConstrainedLanguage());
                DynamicMetaObject obj3 = PSGetMemberBinder.EnsureAllowedInLanguageMode(LocalPipeline.GetExecutionContextFromTLS().LanguageMode, target, obj2, base.Name, this._static, new DynamicMetaObject[] { value }, restrictions2, "PropertySetterNotSupportedInConstrainedLanguage", ParserStrings.PropertySetConstrainedLanguage);
                if (obj3 != null)
                {
                    return obj3.WriteToDebugLog(this);
                }
            }
            if (flag3)
            {
                if (info == null)
                {
                    return (errorSuggestion ?? new DynamicMetaObject(Compiler.ThrowRuntimeError("PropertyAssignmentException", ParserStrings.PropertyNotFound, this.ReturnType, new Expression[] { Expression.Constant(base.Name) }), restrictions2)).WriteToDebugLog(this);
                }
                PSPropertyInfo info3 = info as PSPropertyInfo;
                if (info3 != null)
                {
                    if (!info3.IsSettable)
                    {
                        Expression innerException = Expression.New(CachedReflectionInfo.SetValueException_ctor, new Expression[] { Expression.Constant("PropertyAssignmentException"), Expression.Constant(null, typeof(Exception)), Expression.Constant(ParserStrings.PropertyIsReadOnly), Expression.NewArrayInit(typeof(object), new Expression[] { Expression.Constant(base.Name) }) });
                        return new DynamicMetaObject(Compiler.ThrowRuntimeErrorWithInnerException("PropertyAssignmentException", Expression.Constant(ParserStrings.PropertyIsReadOnly), innerException, this.ReturnType, new Expression[] { Expression.Constant(base.Name) }), restrictions2).WriteToDebugLog(this);
                    }
                    PSProperty property = info3 as PSProperty;
                    if (property != null)
                    {
                        DotNetAdapter.PropertyCacheEntry adapterData = property.adapterData as DotNetAdapter.PropertyCacheEntry;
                        if (adapterData != null)
                        {
                            Expression expression10;
                            Type propertyType;
                            if (adapterData.member.DeclaringType.IsGenericTypeDefinition)
                            {
                                Expression expression9 = Expression.New(CachedReflectionInfo.SetValueException_ctor, new Expression[] { Expression.Constant("PropertyAssignmentException"), Expression.Constant(null, typeof(Exception)), Expression.Constant(ExtendedTypeSystem.CannotInvokeStaticMethodOnUninstantiatedGenericType), Expression.NewArrayInit(typeof(object), new Expression[] { Expression.Constant(adapterData.member.DeclaringType.FullName) }) });
                                return new DynamicMetaObject(Compiler.ThrowRuntimeErrorWithInnerException("PropertyAssignmentException", Expression.Constant(ExtendedTypeSystem.CannotInvokeStaticMethodOnUninstantiatedGenericType), expression9, this.ReturnType, new Expression[] { Expression.Constant(adapterData.member.DeclaringType.FullName) }), restrictions2).WriteToDebugLog(this);
                            }
                            PropertyInfo member = adapterData.member as PropertyInfo;
                            if (member != null)
                            {
                                propertyType = member.PropertyType;
                                Expression expression11 = this._static ? null : PSGetMemberBinder.GetTargetExpr(target);
                                expression10 = Expression.Property(expression11, member);
                            }
                            else
                            {
                                FieldInfo field = (FieldInfo) adapterData.member;
                                propertyType = field.FieldType;
                                Expression expression12 = this._static ? null : PSGetMemberBinder.GetTargetExpr(target);
                                expression10 = Expression.Field(expression12, field);
                            }
                            Type underlyingType = Nullable.GetUnderlyingType(propertyType);
                            Type type7 = underlyingType ?? propertyType;
                            ParameterExpression expression13 = Expression.Variable(type7);
                            Expression expression14 = (underlyingType != null) ? ((value.Value == null) ? ((Expression) Expression.Constant(null, propertyType)) : ((Expression) Expression.New(propertyType.GetConstructor(new Type[] { underlyingType }), new Expression[] { expression13 }))) : ((Expression) expression13);
                            Expression expression15 = (type7.Equals(typeof(object)) && value.LimitType.Equals(typeof(PSObject))) ? Expression.Call(CachedReflectionInfo.PSObject_Base, value.Expression.Cast(typeof(PSObject))) : value.CastOrConvert(type7);
                            Expression expr = Expression.Block(new ParameterExpression[] { expression13 }, new Expression[] { Expression.Assign(expression13, expression15), Expression.Assign(expression10, expression14), expression13.Cast(typeof(object)) });
                            ParameterExpression variable = Expression.Variable(typeof(Exception));
                            return new DynamicMetaObject(Expression.TryCatch(expr.Cast(typeof(object)), new CatchBlock[] { Expression.Catch(variable, Expression.Block(Expression.Call(CachedReflectionInfo.ExceptionHandlingOps_ConvertToMethodInvocationException, variable, Expression.Constant(typeof(SetValueInvocationException)), Expression.Constant(base.Name), ExpressionCache.Constant(0), Expression.Constant(null, typeof(MemberInfo))), Expression.Rethrow(typeof(object)))) }), restrictions2).WriteToDebugLog(this);
                        }
                    }
                    PSCodeProperty property2 = info3 as PSCodeProperty;
                    if (property2 != null)
                    {
                        ParameterExpression expression17 = Expression.Variable(typeof(object));
                        return new DynamicMetaObject(Expression.Block(new ParameterExpression[] { expression17 }, new Expression[] { Expression.Assign(expression17, value.CastOrConvert(expression17.Type)), PSInvokeMemberBinder.InvokeMethod(property2.SetterCodeReference, null, new DynamicMetaObject[] { target, value }, false), expression17 }), restrictions2).WriteToDebugLog(this);
                    }
                    PSScriptProperty property3 = info3 as PSScriptProperty;
                    if (property3 != null)
                    {
                        return new DynamicMetaObject(Expression.Call(Expression.Constant(property3), CachedReflectionInfo.PSScriptProperty_InvokeSetter, PSGetMemberBinder.GetTargetExpr(target), value.Expression.Cast(typeof(object))), restrictions2).WriteToDebugLog(this);
                    }
                }
                if (errorSuggestion != null)
                {
                    return errorSuggestion.WriteToDebugLog(this);
                }
            }
            return new DynamicMetaObject(Expression.Call(CachedReflectionInfo.PSSetMemberBinder_SetAdaptedValue, PSGetMemberBinder.GetTargetExpr(target).Cast(typeof(object)), Expression.Constant(base.Name), value.Expression.Cast(typeof(object))), restrictions2).WriteToDebugLog(this);
        }

        public static PSSetMemberBinder Get(string memberName, bool @static)
        {
            PSSetMemberBinder binder;
            Dictionary<string, PSSetMemberBinder> dictionary = @static ? _staticBinderCache : _binderCache;
            lock (dictionary)
            {
                if (dictionary.TryGetValue(memberName, out binder))
                {
                    return binder;
                }
                binder = new PSSetMemberBinder(memberName, true, @static);
                if (!@static)
                {
                    List<PSSetMemberBinder> list;
                    if (!_binderCacheIgnoringCase.TryGetValue(memberName, out list))
                    {
                        list = new List<PSSetMemberBinder>();
                        _binderCacheIgnoringCase.Add(memberName, list);
                    }
                    list.Add(binder);
                }
                dictionary.Add(memberName, binder);
            }
            return binder;
        }

        internal static void InvalidateCache()
        {
            lock (_binderCache)
            {
                foreach (PSSetMemberBinder binder in _binderCache.Values)
                {
                    binder._getMemberBinder._version++;
                }
            }
            lock (_binderCacheIgnoringCase)
            {
                foreach (List<PSSetMemberBinder> list in _binderCacheIgnoringCase.Values)
                {
                    foreach (PSSetMemberBinder binder2 in list)
                    {
                        binder2._getMemberBinder._version++;
                    }
                }
            }
            lock (_staticBinderCache)
            {
                foreach (PSSetMemberBinder binder3 in _staticBinderCache.Values)
                {
                    binder3._getMemberBinder._version++;
                }
            }
        }

        internal static object SetAdaptedValue(object obj, string member, object value)
        {
            object obj2;
            try
            {
                ExecutionContext executionContextFromTLS = LocalPipeline.GetExecutionContextFromTLS();
                PSMemberInfo memberInfo = null;
                if ((executionContextFromTLS != null) && (executionContextFromTLS.TypeTable != null))
                {
                    ConsolidatedString typeNames = PSObject.GetTypeNames(obj);
                    memberInfo = executionContextFromTLS.TypeTable.GetMembers<PSMemberInfo>(typeNames)[member];
                    if (memberInfo != null)
                    {
                        memberInfo = PSGetMemberBinder.CloneMemberInfo(memberInfo, obj);
                    }
                }
                PSObject.AdapterSet mappedAdapter = PSObject.GetMappedAdapter(obj, (executionContextFromTLS != null) ? executionContextFromTLS.TypeTable : null);
                if (memberInfo == null)
                {
                    memberInfo = mappedAdapter.OriginalAdapter.BaseGetMember<PSMemberInfo>(obj, member);
                }
                if ((memberInfo == null) && (mappedAdapter.DotNetAdapter != null))
                {
                    memberInfo = mappedAdapter.DotNetAdapter.BaseGetMember<PSMemberInfo>(obj, member);
                }
                if (memberInfo == null)
                {
                    throw InterpreterError.NewInterpreterException(null, typeof(RuntimeException), null, "PropertyAssignmentException", ParserStrings.PropertyNotFound, new object[] { member });
                }
                memberInfo.Value = value;
                obj2 = value;
            }
            catch (SetValueException)
            {
                throw;
            }
            catch (Exception exception)
            {
                ExceptionHandlingOps.ConvertToMethodInvocationException(exception, typeof(SetValueInvocationException), member, 0, null);
                throw;
            }
            return obj2;
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "SetMember: {0}{1} ver:{2}", new object[] { this._static ? "static " : "", base.Name, this._getMemberBinder._version });
        }
    }
}

