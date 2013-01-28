namespace System.Management.Automation.Language
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Runspaces;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal class PSGetMemberBinder : GetMemberBinder
    {
        private static readonly Dictionary<string, PSGetMemberBinder> _binderCache = new Dictionary<string, PSGetMemberBinder>(StringComparer.Ordinal);
        private static readonly ConcurrentDictionary<string, List<PSGetMemberBinder>> _binderCacheIgnoringCase = new ConcurrentDictionary<string, List<PSGetMemberBinder>>(StringComparer.OrdinalIgnoreCase);
        private static readonly Dictionary<string, PSGetMemberBinder> _binderCacheNonEnumerable = new Dictionary<string, PSGetMemberBinder>(StringComparer.Ordinal);
        private bool _hasInstanceMember;
        private bool _hasTypeTableMember;
        private readonly bool _nonEnumerating;
        private readonly bool _static;
        private static readonly Dictionary<string, PSGetMemberBinder> _staticBinderCache = new Dictionary<string, PSGetMemberBinder>(StringComparer.OrdinalIgnoreCase);
        internal int _version;

        static PSGetMemberBinder()
        {
            _binderCache.Add("psadapted", new ReservedMemberBinder("psadapted", true, false));
            _binderCache.Add("psextended", new ReservedMemberBinder("psextended", true, false));
            _binderCache.Add("psbase", new ReservedMemberBinder("psbase", true, false));
            _binderCache.Add("psobject", new ReservedMemberBinder("psobject", true, false));
            _binderCache.Add("pstypenames", new ReservedMemberBinder("pstypenames", true, false));
        }

        private PSGetMemberBinder(string name, bool ignoreCase, bool @static, bool nonEnumerating) : base(name, ignoreCase)
        {
            this._static = @static;
            this._version = 0;
            this._nonEnumerating = nonEnumerating;
        }

        internal static PSMemberInfo CloneMemberInfo(PSMemberInfo memberInfo, object obj)
        {
            memberInfo = memberInfo.Copy();
            memberInfo.ReplicateInstance(obj);
            return memberInfo;
        }

        internal static DynamicMetaObject EnsureAllowedInLanguageMode(PSLanguageMode languageMode, DynamicMetaObject target, object targetValue, string name, bool isStatic, DynamicMetaObject[] args, BindingRestrictions moreTests, string errorID, string resourceString)
        {
            if ((languageMode == PSLanguageMode.ConstrainedLanguage) && !IsAllowedInConstrainedLanguage(targetValue, name, isStatic))
            {
                return target.ThrowRuntimeError(args, moreTests, errorID, resourceString, new Expression[0]);
            }
            return null;
        }

        public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
        {
            BindingRestrictions restrictions;
            PSMemberInfo info;
            bool flag;
            Type type;
            if (!target.HasValue)
            {
                return base.Defer(target, new DynamicMetaObject[0]);
            }
            object obj2 = PSObject.Base(target.Value);
            if (obj2 == null)
            {
                return this.PropertyDoesntExist(target, target.PSGetTypeRestriction()).WriteToDebugLog(this);
            }
            Expression expr = null;
            if (this._hasInstanceMember && TryGetInstanceMember(target.Value, base.Name, out info))
            {
                ParameterExpression expression2 = Expression.Variable(typeof(PSMemberInfo));
                expr = WrapGetMemberInTry(Expression.Condition(Expression.Call(CachedReflectionInfo.PSGetMemberBinder_TryGetInstanceMember, target.Expression.Cast(typeof(object)), Expression.Constant(base.Name), expression2), Expression.Property(expression2, "Value"), base.GetUpdateExpression(typeof(object))));
                return new DynamicMetaObject(Expression.Block(new ParameterExpression[] { expression2 }, new Expression[] { expr }), BinderUtils.GetVersionCheck(this, this._version)).WriteToDebugLog(this);
            }
            info = this.GetPSMemberInfo(target, out restrictions, out flag, out type, null, null);
            if (!flag)
            {
                return new DynamicMetaObject(WrapGetMemberInTry(Expression.Call(CachedReflectionInfo.PSGetMemberBinder_GetAdaptedValue, GetTargetExpr(target).Cast(typeof(object)), Expression.Constant(base.Name))), restrictions).WriteToDebugLog(this);
            }
            if (info != null)
            {
                PSPropertyInfo info2 = info as PSPropertyInfo;
                if (info2 != null)
                {
                    if (!info2.IsGettable)
                    {
                        return new DynamicMetaObject(Compiler.ThrowRuntimeError("WriteOnlyProperty", ExtendedTypeSystem.WriteOnlyProperty, this.ReturnType, new Expression[] { Expression.Constant(base.Name) }), restrictions).WriteToDebugLog(this);
                    }
                    PSProperty property = info2 as PSProperty;
                    if (property != null)
                    {
                        DotNetAdapter.PropertyCacheEntry adapterData = property.adapterData as DotNetAdapter.PropertyCacheEntry;
                        if (!adapterData.member.DeclaringType.IsGenericTypeDefinition)
                        {
                            Expression expression = this._static ? null : GetTargetExpr(target);
                            PropertyInfo member = adapterData.member as PropertyInfo;
                            if (member != null)
                            {
                                expr = Expression.Property(expression, member);
                            }
                            else
                            {
                                expr = Expression.Field(expression, (FieldInfo) adapterData.member);
                            }
                        }
                        else
                        {
                            expr = ExpressionCache.NullConstant;
                        }
                    }
                    PSScriptProperty property2 = info2 as PSScriptProperty;
                    if (property2 != null)
                    {
                        expr = Expression.Call(Expression.Constant(property2), CachedReflectionInfo.PSScriptProperty_InvokeGetter, new Expression[] { target.Expression.Cast(typeof(object)) });
                    }
                    PSCodeProperty property3 = info2 as PSCodeProperty;
                    if (property3 != null)
                    {
                        expr = PSInvokeMemberBinder.InvokeMethod(property3.GetterCodeReference, null, new DynamicMetaObject[] { target }, false);
                    }
                    if (info2 is PSNoteProperty)
                    {
                        expr = Expression.Property(Expression.Constant(info2), CachedReflectionInfo.PSNoteProperty_Value);
                    }
                    if (type != null)
                    {
                        expr = expr.Convert(type);
                    }
                }
                else
                {
                    expr = Expression.Call(CachedReflectionInfo.PSGetMemberBinder_CloneMemberInfo, Expression.Constant(info).Cast(typeof(PSMemberInfo)), target.Expression.Cast(typeof(object)));
                }
            }
            if (obj2 is IDictionary)
            {
                Type genericTypeArg = null;
                bool flag2 = IsGenericDictionary(obj2, ref genericTypeArg);
                if (!flag2 || (genericTypeArg != null))
                {
                    ParameterExpression ifTrue = Expression.Variable(typeof(object));
                    if (expr == null)
                    {
                        expr = (errorSuggestion ?? this.PropertyDoesntExist(target, restrictions)).Expression;
                    }
                    MethodInfo method = flag2 ? CachedReflectionInfo.PSGetMemberBinder_TryGetGenericDictionaryValue.MakeGenericMethod(new Type[] { genericTypeArg }) : CachedReflectionInfo.PSGetMemberBinder_TryGetIDictionaryValue;
                    expr = Expression.Block(new ParameterExpression[] { ifTrue }, new Expression[] { Expression.Condition(Expression.Call(method, GetTargetExpr(target).Cast(method.GetParameters()[0].ParameterType), Expression.Constant(base.Name), ifTrue), ifTrue, expr.Cast(typeof(object))) });
                }
            }
            if (expr != null)
            {
                return new DynamicMetaObject(WrapGetMemberInTry(expr), restrictions).WriteToDebugLog(this);
            }
            return (errorSuggestion ?? this.PropertyDoesntExist(target, restrictions)).WriteToDebugLog(this);
        }

        public static PSGetMemberBinder Get(string memberName, bool @static)
        {
            return Get(memberName, @static, false);
        }

        private static PSGetMemberBinder Get(string memberName, bool @static, bool nonEnumerating)
        {
            PSGetMemberBinder binder;
            Dictionary<string, PSGetMemberBinder> dictionary = @static ? _staticBinderCache : (nonEnumerating ? _binderCacheNonEnumerable : _binderCache);
            lock (dictionary)
            {
                if (dictionary.TryGetValue(memberName, out binder))
                {
                    return binder;
                }
                if (PSMemberInfoCollection<PSMemberInfo>.IsReservedName(memberName))
                {
                    binder = dictionary[memberName.ToLowerInvariant()];
                }
                else
                {
                    binder = new PSGetMemberBinder(memberName, true, @static, nonEnumerating);
                    if (!@static)
                    {
                        List<PSGetMemberBinder> orAdd = _binderCacheIgnoringCase.GetOrAdd(memberName, _ => new List<PSGetMemberBinder>());
                        lock (orAdd)
                        {
                            if (orAdd.Any<PSGetMemberBinder>())
                            {
                                binder._hasInstanceMember = orAdd[0]._hasInstanceMember;
                                binder._hasTypeTableMember = orAdd[0]._hasTypeTableMember;
                            }
                            orAdd.Add(binder);
                        }
                    }
                }
                dictionary.Add(memberName, binder);
            }
            return binder;
        }

        internal static object GetAdaptedValue(object obj, string member)
        {
            ExecutionContext executionContextFromTLS = LocalPipeline.GetExecutionContextFromTLS();
            PSMemberInfo memberInfo = null;
            if ((executionContextFromTLS != null) && (executionContextFromTLS.TypeTable != null))
            {
                ConsolidatedString typeNames = PSObject.GetTypeNames(obj);
                memberInfo = executionContextFromTLS.TypeTable.GetMembers<PSMemberInfo>(typeNames)[member];
                if (memberInfo != null)
                {
                    memberInfo = CloneMemberInfo(memberInfo, obj);
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
            if (memberInfo != null)
            {
                return memberInfo.Value;
            }
            if ((executionContextFromTLS != null) && executionContextFromTLS.IsStrictVersion(2))
            {
                throw new PropertyNotFoundException("PropertyNotFoundStrict", null, ParserStrings.PropertyNotFoundStrict, new object[] { LanguagePrimitives.ConvertTo<string>(member) });
            }
            return null;
        }

        private PSGetMemberBinder GetNonEnumeratingBinder()
        {
            return Get(base.Name, false, true);
        }

        internal PSMemberInfo GetPSMemberInfo(DynamicMetaObject target, out BindingRestrictions restrictions, out bool canOptimize, out Type aliasConversionType, HashSet<string> aliases = null, List<BindingRestrictions> aliasRestrictions = null)
        {
            lock (this)
            {
                aliasConversionType = null;
                if (this._static)
                {
                    restrictions = typeof(Type).IsAssignableFrom(target.LimitType) ? BindingRestrictions.GetInstanceRestriction(target.Expression, target.Value) : target.PSGetTypeRestriction();
                    restrictions = restrictions.Merge(BinderUtils.GetVersionCheck(this, this._version));
                    canOptimize = true;
                    return PSObject.GetStaticCLRMember(target.Value, base.Name);
                }
                canOptimize = false;
                PSMemberInfo info = null;
                ConsolidatedString types = null;
                ExecutionContext executionContextFromTLS = LocalPipeline.GetExecutionContextFromTLS();
                TypeTable typeTable = (executionContextFromTLS != null) ? executionContextFromTLS.TypeTable : null;
                if (this._hasTypeTableMember)
                {
                    types = PSObject.GetTypeNames(target.Value);
                    if (typeTable != null)
                    {
                        info = typeTable.GetMembers<PSMemberInfo>(types)[base.Name];
                        if (info != null)
                        {
                            canOptimize = true;
                        }
                    }
                }
                object obj2 = PSObject.Base(target.Value);
                PSObject.AdapterSet mappedAdapter = PSObject.GetMappedAdapter(obj2, typeTable);
                if (info == null)
                {
                    canOptimize = mappedAdapter.OriginalAdapter.SiteBinderCanOptimize;
                    if (canOptimize)
                    {
                        info = mappedAdapter.OriginalAdapter.BaseGetMember<PSMemberInfo>(obj2, base.Name);
                    }
                }
                if (((info == null) && canOptimize) && (mappedAdapter.DotNetAdapter != null))
                {
                    info = mappedAdapter.DotNetAdapter.BaseGetMember<PSMemberInfo>(obj2, base.Name);
                }
                restrictions = BinderUtils.GetVersionCheck(this, this._version);
                PSAliasProperty alias = info as PSAliasProperty;
                if (alias != null)
                {
                    aliasConversionType = alias.ConversionType;
                    if (aliasRestrictions == null)
                    {
                        aliasRestrictions = new List<BindingRestrictions>();
                    }
                    info = ResolveAlias(alias, target, aliases, aliasRestrictions);
                    if (info == null)
                    {
                        canOptimize = false;
                    }
                    else
                    {
                        foreach (BindingRestrictions restrictions2 in aliasRestrictions)
                        {
                            restrictions = restrictions.Merge(restrictions2);
                        }
                    }
                }
                if (this._hasInstanceMember)
                {
                    restrictions = restrictions.Merge(this.NotInstanceMember(target));
                }
                restrictions = restrictions.Merge(target.PSGetTypeRestriction());
                if (this._hasTypeTableMember)
                {
                    restrictions = restrictions.Merge(BindingRestrictions.GetInstanceRestriction(Expression.Call(CachedReflectionInfo.PSGetMemberBinder_GetTypeTableFromTLS, new Expression[0]), typeTable));
                    restrictions = restrictions.Merge(BindingRestrictions.GetExpressionRestriction(Expression.Call(CachedReflectionInfo.PSGetMemberBinder_IsTypeNameSame, target.Expression.Cast(typeof(object)), Expression.Constant(types.Key))));
                }
                return info;
            }
        }

        internal static Expression GetTargetExpr(DynamicMetaObject target)
        {
            Expression expression = target.Expression;
            object obj2 = target.Value;
            if ((obj2 is PSObject) && (obj2 != AutomationNull.Value))
            {
                expression = Expression.Call(CachedReflectionInfo.PSObject_Base, expression);
                obj2 = PSObject.Base(obj2);
            }
            Type o = obj2.GetType();
            if (!expression.Type.Equals(o))
            {
                expression = o.IsValueType ? ((Nullable.GetUnderlyingType(expression.Type) != null) ? ((Expression) Expression.Property(expression, "Value")) : ((Expression) Expression.Unbox(expression, o))) : expression.Cast(o);
            }
            return expression;
        }

        internal static TypeTable GetTypeTableFromTLS()
        {
            ExecutionContext executionContextFromTLS = LocalPipeline.GetExecutionContextFromTLS();
            if (executionContextFromTLS == null)
            {
                return null;
            }
            return executionContextFromTLS.TypeTable;
        }

        internal static bool IsAllowedInConstrainedLanguage(object targetValue, string name, bool isStatic)
        {
            if (!string.Equals(name, "ToString", StringComparison.OrdinalIgnoreCase))
            {
                Type inputType = targetValue as Type;
                if (!isStatic || (inputType == null))
                {
                    inputType = targetValue.GetType();
                }
                if (!CoreTypes.Contains(inputType))
                {
                    return false;
                }
            }
            return true;
        }

        internal static bool IsGenericDictionary(object value, ref Type genericTypeArg)
        {
            bool flag = false;
            foreach (Type type in value.GetType().GetInterfaces())
            {
                if (type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(IDictionary<,>)))
                {
                    flag = true;
                    Type[] genericArguments = type.GetGenericArguments();
                    if (genericArguments[0].Equals(typeof(string)))
                    {
                        genericTypeArg = genericArguments[1];
                    }
                }
            }
            return flag;
        }

        internal static bool IsTypeNameSame(object value, string typeName)
        {
            return ((value != null) && string.Equals(PSObject.GetTypeNames(value).Key, typeName, StringComparison.OrdinalIgnoreCase));
        }

        internal BindingRestrictions NotInstanceMember(DynamicMetaObject target)
        {
            ParameterExpression expression = Expression.Variable(typeof(PSMemberInfo));
            MethodCallExpression expression2 = Expression.Call(CachedReflectionInfo.PSGetMemberBinder_TryGetInstanceMember, target.Expression.Cast(typeof(object)), Expression.Constant(base.Name), expression);
            return BindingRestrictions.GetExpressionRestriction(Expression.Block(new ParameterExpression[] { expression }, new Expression[] { Expression.Not(expression2) }));
        }

        private DynamicMetaObject PropertyDoesntExist(DynamicMetaObject target, BindingRestrictions restrictions)
        {
            if (!this._nonEnumerating && (target.Value != AutomationNull.Value))
            {
                DynamicMetaObject obj2 = PSEnumerableBinder.IsEnumerable(target);
                if (obj2 != null)
                {
                    return new DynamicMetaObject(Expression.Call(CachedReflectionInfo.EnumerableOps_PropertyGetter, Expression.Constant(this.GetNonEnumeratingBinder()), obj2.Expression), restrictions);
                }
            }
            if (base.Name.Equals("Length", StringComparison.OrdinalIgnoreCase) || base.Name.Equals("Count", StringComparison.OrdinalIgnoreCase))
            {
                int i = (PSObject.Base(target.Value) == null) ? 0 : 1;
                return new DynamicMetaObject(Expression.Condition(Compiler.IsStrictMode(2, null), this.ThrowPropertyNotFoundStrict(), ExpressionCache.Constant(i).Cast(typeof(object))), restrictions);
            }
            return new DynamicMetaObject(Expression.Condition(Compiler.IsStrictMode(2, null), this.ThrowPropertyNotFoundStrict(), this._nonEnumerating ? ExpressionCache.AutomationNullConstant : ExpressionCache.NullConstant), restrictions);
        }

        internal static PSMemberInfo ResolveAlias(PSAliasProperty alias, DynamicMetaObject target, HashSet<string> aliases, List<BindingRestrictions> aliasRestrictions)
        {
            bool flag;
            Type type;
            BindingRestrictions restrictions;
            if (aliases == null)
            {
                aliases = new HashSet<string> { alias.Name };
            }
            else
            {
                if (aliases.Contains(alias.Name))
                {
                    throw new ExtendedTypeSystemException("CycleInAliasLookup", null, ExtendedTypeSystem.CycleInAlias, new object[] { alias.Name });
                }
                aliases.Add(alias.Name);
            }
            PSGetMemberBinder binder = Get(alias.ReferencedMemberName, false);
            if (binder.HasInstanceMember)
            {
                return null;
            }
            PSMemberInfo info = binder.GetPSMemberInfo(target, out restrictions, out flag, out type, aliases, aliasRestrictions);
            aliasRestrictions.Add(BinderUtils.GetVersionCheck(binder, binder._version));
            return info;
        }

        internal static void SetHasInstanceMember(string memberName)
        {
            List<PSGetMemberBinder> orAdd = _binderCacheIgnoringCase.GetOrAdd(memberName, _ => new List<PSGetMemberBinder>());
            lock (orAdd)
            {
                if (!orAdd.Any<PSGetMemberBinder>())
                {
                    Get(memberName, false);
                }
                foreach (PSGetMemberBinder binder in orAdd)
                {
                    if (!binder._hasInstanceMember)
                    {
                        lock (binder)
                        {
                            if (!binder._hasInstanceMember)
                            {
                                binder._version++;
                                binder._hasInstanceMember = true;
                            }
                        }
                    }
                }
            }
        }

        internal static void SetHasTypeTableMember(string memberName, bool value)
        {
            List<PSGetMemberBinder> orAdd = _binderCacheIgnoringCase.GetOrAdd(memberName, _ => new List<PSGetMemberBinder>());
            lock (orAdd)
            {
                if (!orAdd.Any<PSGetMemberBinder>())
                {
                    Get(memberName, false);
                }
                foreach (PSGetMemberBinder binder in orAdd)
                {
                    lock (binder)
                    {
                        binder._version++;
                        binder._hasTypeTableMember = value;
                    }
                }
            }
        }

        private Expression ThrowPropertyNotFoundStrict()
        {
            object[] exceptionArgs = new object[4];
            exceptionArgs[0] = "PropertyNotFoundStrict";
            exceptionArgs[2] = ParserStrings.PropertyNotFoundStrict;
            exceptionArgs[3] = new object[] { base.Name };
            return Compiler.CreateThrow(typeof(object), typeof(PropertyNotFoundException), new Type[] { typeof(string), typeof(Exception), typeof(string), typeof(object[]) }, exceptionArgs);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "GetMember: {0}{1}{2} ver:{3}", new object[] { base.Name, this._static ? " static" : "", this._nonEnumerating ? " nonEnumerating" : "", this._version });
        }

        internal static bool TryGetGenericDictionaryValue<T>(IDictionary<string, T> hash, string memberName, out object value)
        {
            T local;
            if (hash.TryGetValue(memberName, out local))
            {
                value = local;
                return true;
            }
            value = null;
            return false;
        }

        internal static bool TryGetIDictionaryValue(IDictionary hash, string memberName, out object value)
        {
            try
            {
                if (hash.Contains(memberName))
                {
                    value = hash[memberName];
                    return true;
                }
            }
            catch (InvalidOperationException)
            {
            }
            value = null;
            return false;
        }

        internal static bool TryGetInstanceMember(object value, string memberName, out PSMemberInfo memberInfo)
        {
            PSMemberInfoInternalCollection<PSMemberInfo> internals;
            memberInfo = PSObject.HasInstanceMembers(value, out internals) ? internals[memberName] : null;
            return (memberInfo != null);
        }

        private static Expression WrapGetMemberInTry(Expression expr)
        {
            ParameterExpression variable = Expression.Variable(typeof(Exception));
            return Expression.TryCatch(expr.Cast(typeof(object)), new CatchBlock[] { Expression.Catch(typeof(TerminateException), Expression.Rethrow(typeof(object))), Expression.Catch(typeof(MethodException), Expression.Rethrow(typeof(object))), Expression.Catch(typeof(PropertyNotFoundException), Expression.Rethrow(typeof(object))), Expression.Catch(variable, Expression.Block(Expression.Call(CachedReflectionInfo.CommandProcessorBase_CheckForSevereException, variable), ExpressionCache.NullConstant)) });
        }

        internal bool HasInstanceMember
        {
            get
            {
                return this._hasInstanceMember;
            }
        }

        private class ReservedMemberBinder : PSGetMemberBinder
        {
            internal ReservedMemberBinder(string name, bool ignoreCase, bool @static) : base(name, ignoreCase, @static, false)
            {
            }

            public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion)
            {
                MethodInfo method = null;
                Expression expression = null;
                string name = base.Name;
                if (name != null)
                {
                    if (!(name == "psadapted"))
                    {
                        if (name == "psbase")
                        {
                            method = CachedReflectionInfo.ReservedNameMembers_GeneratePSBaseMemberSet;
                            expression = target.Expression.Cast(typeof(object));
                        }
                        else if (name == "psextended")
                        {
                            method = CachedReflectionInfo.ReservedNameMembers_GeneratePSExtendedMemberSet;
                            expression = target.Expression.Cast(typeof(object));
                        }
                        else if (name == "psobject")
                        {
                            method = CachedReflectionInfo.ReservedNameMembers_GeneratePSObjectMemberSet;
                            expression = target.Expression.Cast(typeof(object));
                        }
                        else if (name == "pstypenames")
                        {
                            method = CachedReflectionInfo.ReservedNameMembers_PSTypeNames;
                            expression = target.Expression.Convert(typeof(PSObject));
                        }
                    }
                    else
                    {
                        method = CachedReflectionInfo.ReservedNameMembers_GeneratePSAdaptedMemberSet;
                        expression = target.Expression.Cast(typeof(object));
                    }
                }
                return new DynamicMetaObject(PSGetMemberBinder.WrapGetMemberInTry(Expression.Call(method, expression)), target.PSGetTypeRestriction());
            }
        }
    }
}

