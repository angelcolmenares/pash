using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Management.Automation;
using System.Management.Automation.Internal;
using System.Management.Automation.Runspaces;
using System.Reflection;

namespace System.Management.Automation.Language
{
    internal class PSInvokeMemberBinder : InvokeMemberBinder
    {
        private readonly static Dictionary<Tuple<string, CallInfo, bool, bool, PSMethodInvocationConstraints>, PSInvokeMemberBinder> _binderCache;

        private readonly static Dictionary<Tuple<string, CallInfo, bool, bool, PSMethodInvocationConstraints>, PSInvokeMemberBinder> _staticBinderCache;

        private readonly PSMethodInvocationConstraints _invocationConstraints;

        private readonly PSGetMemberBinder _getMemberBinder;

        private readonly bool _static;

        private readonly bool _propertySetter;

        private readonly bool _nonEnumerating;

        static PSInvokeMemberBinder()
        {
            PSInvokeMemberBinder._binderCache = new Dictionary<Tuple<string, CallInfo, bool, bool, PSMethodInvocationConstraints>, PSInvokeMemberBinder>(new PSInvokeMemberBinder.KeyComparer());
            PSInvokeMemberBinder._staticBinderCache = new Dictionary<Tuple<string, CallInfo, bool, bool, PSMethodInvocationConstraints>, PSInvokeMemberBinder>(new PSInvokeMemberBinder.KeyComparer());
        }

        private PSInvokeMemberBinder(string name, bool ignoreCase, bool @static, bool propertySetter, bool nonEnumerating, CallInfo callInfo, PSMethodInvocationConstraints invocationConstraints)
            : base(name, ignoreCase, callInfo)
        {
            this._static = @static;
            this._propertySetter = propertySetter;
            this._nonEnumerating = nonEnumerating;
            this._invocationConstraints = invocationConstraints;
            this._getMemberBinder = PSGetMemberBinder.Get(name, @static);
        }

        public override DynamicMetaObject FallbackInvoke(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
        {
            DynamicMetaObject dynamicMetaObject = errorSuggestion;
            DynamicMetaObject dynamicMetaObject1 = dynamicMetaObject;
            if (dynamicMetaObject == null)
            {
                PSInvokeBinder pSInvokeBinder = new PSInvokeBinder(base.CallInfo);
                Type type = typeof(object);
                IEnumerable<DynamicMetaObject> dynamicMetaObjects = args.Prepend<DynamicMetaObject>(target);
                dynamicMetaObject1 = new DynamicMetaObject(Expression.Dynamic(pSInvokeBinder, type, dynamicMetaObjects.Select<DynamicMetaObject, Expression>((DynamicMetaObject dmo) => dmo.Expression)), target.Restrictions.Merge(BindingRestrictions.Combine(args)));
            }
            return dynamicMetaObject1;
        }

        public override DynamicMetaObject FallbackInvokeMember(DynamicMetaObject target, DynamicMetaObject[] args, DynamicMetaObject errorSuggestion)
        {
            PSMemberInfo pSMemberInfo = null;
            BindingRestrictions bindingRestriction = null;
            bool flag = false;
            Type type = null;
            Expression expression;
            Type value;
            MethodInformation[] methodInformationArray;
            Type type1;
            if (target.HasValue)
            {
                DynamicMetaObject[] dynamicMetaObjectArray = args;
                if (!dynamicMetaObjectArray.Where<DynamicMetaObject>((DynamicMetaObject arg) => !arg.HasValue).Any<DynamicMetaObject>())
                {
                    object obj = PSObject.Base(target.Value);
                    if (obj != null)
                    {
                        if (!this._getMemberBinder.HasInstanceMember || !PSGetMemberBinder.TryGetInstanceMember(target.Value, base.Name, out pSMemberInfo))
                        {
                            PSMethodInfo pSMethodInfo = this._getMemberBinder.GetPSMemberInfo(target, out bindingRestriction, out flag, out type, null, null) as PSMethodInfo;
                            DynamicMetaObject[] dynamicMetaObjectArray1 = args;
                            BindingRestrictions bindingRestriction1 = bindingRestriction;
                            bindingRestriction = dynamicMetaObjectArray1.Aggregate<DynamicMetaObject, BindingRestrictions>(bindingRestriction1, (BindingRestrictions current, DynamicMetaObject arg) => current.Merge(arg.PSGetMethodArgumentRestriction()));
                            if (ExecutionContext.HasEverUsedConstrainedLanguage)
                            {
                                bindingRestriction = bindingRestriction.Merge(BinderUtils.GetLanguageModeCheckIfHasEverUsedConstrainedLanguage());
                                ExecutionContext executionContextFromTLS = LocalPipeline.GetExecutionContextFromTLS();
                                DynamicMetaObject dynamicMetaObject = PSGetMemberBinder.EnsureAllowedInLanguageMode(executionContextFromTLS.LanguageMode, target, obj, base.Name, this._static, args, bindingRestriction, "MethodInvocationNotSupportedInConstrainedLanguage", ParserStrings.InvokeMethodConstrainedLanguage);
                                if (dynamicMetaObject != null)
                                {
                                    return dynamicMetaObject.WriteToDebugLog(this);
                                }
                            }
                            if (flag)
                            {
                                PSObject pSObject = target.Value as PSObject;
                                if (pSObject != null && (obj.GetType().Equals(typeof(Hashtable)) || obj.GetType().Equals(typeof(ArrayList))))
                                {
                                    if (!pSObject.isDeserialized)
                                    {
                                        bindingRestriction = bindingRestriction.Merge(BindingRestrictions.GetExpressionRestriction(Expression.Not(Expression.Field(target.Expression.Cast(typeof(PSObject)), CachedReflectionInfo.PSObject_isDeserialized))));
                                    }
                                    else
                                    {
                                        pSMethodInfo = null;
                                        bindingRestriction = bindingRestriction.Merge(BindingRestrictions.GetExpressionRestriction(Expression.Field(target.Expression.Cast(typeof(PSObject)), CachedReflectionInfo.PSObject_isDeserialized)));
                                    }
                                }
                                PSMethod pSMethod = pSMethodInfo as PSMethod;
                                if (pSMethod == null)
                                {
                                    PSScriptMethod pSScriptMethod = pSMethodInfo as PSScriptMethod;
                                    if (pSScriptMethod == null)
                                    {
                                        PSCodeMethod pSCodeMethod = pSMethodInfo as PSCodeMethod;
                                        if (pSCodeMethod == null)
                                        {
                                            PSParameterizedProperty pSParameterizedProperty = pSMethodInfo as PSParameterizedProperty;
                                            if (pSParameterizedProperty == null)
                                            {
                                                if (errorSuggestion == null)
                                                {
                                                    if (this._static)
                                                    {
                                                        value = (Type)target.Value;
                                                    }
                                                    else
                                                    {
                                                        value = PSObject.Base(target.Value).GetType();
                                                    }
                                                    Type type2 = value;
                                                    if (!this._static && !this._nonEnumerating && target.Value != AutomationNull.Value)
                                                    {
                                                        DynamicMetaObject dynamicMetaObject1 = PSEnumerableBinder.IsEnumerable(target);
                                                        if (dynamicMetaObject1 != null)
                                                        {
                                                            return this.InvokeMemberOnCollection(dynamicMetaObject1, args, type2, bindingRestriction);
                                                        }
                                                    }
                                                    Expression[] expressionArray = new Expression[2];
                                                    expressionArray[0] = Expression.Constant(type2.FullName);
                                                    expressionArray[1] = Expression.Constant(base.Name);
                                                    return new DynamicMetaObject(Compiler.ThrowRuntimeError("MethodNotFound", ParserStrings.MethodNotFound, expressionArray), bindingRestriction).WriteToDebugLog(this);
                                                }
                                                else
                                                {
                                                    return errorSuggestion.WriteToDebugLog(this);
                                                }
                                            }
                                            else
                                            {
                                                DotNetAdapter.ParameterizedPropertyCacheEntry parameterizedPropertyCacheEntry = (DotNetAdapter.ParameterizedPropertyCacheEntry)pSParameterizedProperty.adapterData;
                                                PSInvokeMemberBinder pSInvokeMemberBinder = this;
                                                DynamicMetaObject dynamicMetaObject2 = target;
                                                DynamicMetaObject[] dynamicMetaObjectArray2 = args;
                                                BindingRestrictions bindingRestriction2 = bindingRestriction;
                                                if (this._propertySetter)
                                                {
                                                    methodInformationArray = parameterizedPropertyCacheEntry.setterInformation;
                                                }
                                                else
                                                {
                                                    methodInformationArray = parameterizedPropertyCacheEntry.getterInformation;
                                                }
                                                if (this._propertySetter)
                                                {
                                                    type1 = typeof(SetValueInvocationException);
                                                }
                                                else
                                                {
                                                    type1 = typeof(GetValueInvocationException);
                                                }
                                                return pSInvokeMemberBinder.InvokeDotNetMethod(dynamicMetaObject2, dynamicMetaObjectArray2, bindingRestriction2, methodInformationArray, type1).WriteToDebugLog(this);
                                            }
                                        }
                                        else
                                        {
                                            return new DynamicMetaObject(PSInvokeMemberBinder.InvokeMethod(pSCodeMethod.CodeReference, null, args.Prepend<DynamicMetaObject>(target).ToArray<DynamicMetaObject>(), false).Cast(typeof(object)), bindingRestriction).WriteToDebugLog(this);
                                        }
                                    }
                                    else
                                    {
                                        MethodInfo pSScriptMethodInvokeScript = CachedReflectionInfo.PSScriptMethod_InvokeScript;
                                        ConstantExpression constantExpression = Expression.Constant(base.Name);
                                        ConstantExpression constantExpression1 = Expression.Constant(pSScriptMethod.Script);
                                        Expression expression1 = target.Expression.Cast(typeof(object));
                                        Type type3 = typeof(object);
                                        DynamicMetaObject[] dynamicMetaObjectArray3 = args;
                                        return new DynamicMetaObject(Expression.Call(pSScriptMethodInvokeScript, constantExpression, constantExpression1, expression1, Expression.NewArrayInit(type3, dynamicMetaObjectArray3.Select<DynamicMetaObject, Expression>((DynamicMetaObject e) => e.Expression.Cast(typeof(object))))), bindingRestriction).WriteToDebugLog(this);
                                    }
                                }
                                else
                                {
                                    DotNetAdapter.MethodCacheEntry methodCacheEntry = (DotNetAdapter.MethodCacheEntry)pSMethod.adapterData;
                                    return this.InvokeDotNetMethod(target, args, bindingRestriction, methodCacheEntry.methodInformationStructures, typeof(MethodException)).WriteToDebugLog(this);
                                }
                            }
                            else
                            {
                                if (!this._propertySetter)
                                {
                                    MethodInfo pSInvokeMemberBinderInvokeAdaptedMember = CachedReflectionInfo.PSInvokeMemberBinder_InvokeAdaptedMember;
                                    Expression expression2 = PSGetMemberBinder.GetTargetExpr(target).Cast(typeof(object));
                                    ConstantExpression constantExpression2 = Expression.Constant(base.Name);
                                    Type type4 = typeof(object);
                                    DynamicMetaObject[] dynamicMetaObjectArray4 = args;
                                    expression = Expression.Call(pSInvokeMemberBinderInvokeAdaptedMember, expression2, constantExpression2, Expression.NewArrayInit(type4, dynamicMetaObjectArray4.Select<DynamicMetaObject, Expression>((DynamicMetaObject arg) => arg.Expression.Cast(typeof(object)))));
                                }
                                else
                                {
                                    MethodInfo pSInvokeMemberBinderInvokeAdaptedSetMember = CachedReflectionInfo.PSInvokeMemberBinder_InvokeAdaptedSetMember;
                                    Expression expression3 = PSGetMemberBinder.GetTargetExpr(target).Cast(typeof(object));
                                    ConstantExpression constantExpression3 = Expression.Constant(base.Name);
                                    Type type5 = typeof(object);
                                    IEnumerable<DynamicMetaObject> dynamicMetaObjects = args.Take<DynamicMetaObject>((int)args.Length - 1);
                                    expression = Expression.Call(pSInvokeMemberBinderInvokeAdaptedSetMember, expression3, constantExpression3, Expression.NewArrayInit(type5, dynamicMetaObjects.Select<DynamicMetaObject, Expression>((DynamicMetaObject arg) => arg.Expression.Cast(typeof(object)))), args.Last<DynamicMetaObject>().Expression.Cast(typeof(object)));
                                }
                                return new DynamicMetaObject(expression, bindingRestriction).WriteToDebugLog(this);
                            }
                        }
                        else
                        {
                            ParameterExpression parameterExpression = Expression.Variable(typeof(PSMethodInfo));
                            MethodCallExpression methodCallExpression = Expression.Call(CachedReflectionInfo.PSInvokeMemberBinder_TryGetInstanceMethod, target.Expression.Cast(typeof(object)), Expression.Constant(base.Name), parameterExpression);
                            ParameterExpression parameterExpression1 = parameterExpression;
                            MethodInfo pSMethodInfoInvoke = CachedReflectionInfo.PSMethodInfo_Invoke;
                            Expression[] expressionArray1 = new Expression[1];
                            Expression[] expressionArray2 = expressionArray1;
                            int num = 0;
                            Type type6 = typeof(object);
                            DynamicMetaObject[] dynamicMetaObjectArray5 = args;
                            expressionArray2[num] = Expression.NewArrayInit(type6, dynamicMetaObjectArray5.Select<DynamicMetaObject, Expression>((DynamicMetaObject dmo) => dmo.Expression.Cast(typeof(object))));
                            ConditionalExpression conditionalExpression = Expression.Condition(methodCallExpression, Expression.Call(parameterExpression1, pSMethodInfoInvoke, expressionArray1), base.GetUpdateExpression(typeof(object)));
                            ParameterExpression[] parameterExpressionArray = new ParameterExpression[1];
                            parameterExpressionArray[0] = parameterExpression;
                            Expression[] expressionArray3 = new Expression[1];
                            expressionArray3[0] = conditionalExpression;
                            return new DynamicMetaObject(Expression.Block(parameterExpressionArray, expressionArray3), BinderUtils.GetVersionCheck(this._getMemberBinder, this._getMemberBinder._version)).WriteToDebugLog(this);
                        }
                    }
                    else
                    {
                        return target.ThrowRuntimeError(args, BindingRestrictions.Empty, "InvokeMethodOnNull", ParserStrings.InvokeMethodOnNull, new Expression[0]).WriteToDebugLog(this);
                    }
                }
            }
            return base.Defer(args.Prepend<DynamicMetaObject>(target).ToArray<DynamicMetaObject>());
        }

        internal static MethodInfo FindBestMethod(DynamicMetaObject target, IEnumerable<DynamicMetaObject> args, string methodName, bool @static, PSMethodInvocationConstraints invocationConstraints)
        {
            bool flag = false;
            MethodInfo methodInfo = null;
            PSMethod dotNetMethod = PSObject.dotNetInstanceAdapter.GetDotNetMethod<PSMethod>(PSObject.Base(target.Value), methodName);
            if (dotNetMethod != null)
            {
                DotNetAdapter.MethodCacheEntry methodCacheEntry = (DotNetAdapter.MethodCacheEntry)dotNetMethod.adapterData;
                string str = null;
                string str1 = null;
                MethodInformation[] methodInformationArray = methodCacheEntry.methodInformationStructures;
                PSMethodInvocationConstraints pSMethodInvocationConstraint = invocationConstraints;
                IEnumerable<DynamicMetaObject> dynamicMetaObjects = args;
                MethodInformation methodInformation = Adapter.FindBestMethod(methodInformationArray, pSMethodInvocationConstraint, dynamicMetaObjects.Select<DynamicMetaObject, object>((DynamicMetaObject arg) =>
                {
                    if (arg.Value == AutomationNull.Value)
                    {
                        return null;
                    }
                    else
                    {
                        return arg.Value;
                    }
                }
                ).ToArray<object>(), ref str, ref str1, out flag);
                if (methodInformation != null)
                {
                    methodInfo = (MethodInfo)methodInformation.method;
                }
            }
            return methodInfo;
        }

        public static PSInvokeMemberBinder Get(string memberName, CallInfo callInfo, bool @static, bool propertySetter, PSMethodInvocationConstraints constraints)
        {
            return PSInvokeMemberBinder.Get(memberName, callInfo, @static, propertySetter, false, constraints);
        }

        private static PSInvokeMemberBinder Get(string memberName, CallInfo callInfo, bool @static, bool propertySetter, bool nonEnumerating, PSMethodInvocationConstraints constraints)
        {
            PSInvokeMemberBinder pSInvokeMemberBinder = null;
            Dictionary<Tuple<string, CallInfo, bool, bool, PSMethodInvocationConstraints>, PSInvokeMemberBinder> tuples;
            if (@static)
            {
                tuples = PSInvokeMemberBinder._staticBinderCache;
            }
            else
            {
                tuples = PSInvokeMemberBinder._binderCache;
            }
            Dictionary<Tuple<string, CallInfo, bool, bool, PSMethodInvocationConstraints>, PSInvokeMemberBinder> tuples1 = tuples;
            lock (tuples1)
            {
                Tuple<string, CallInfo, bool, bool, PSMethodInvocationConstraints> tuple = Tuple.Create<string, CallInfo, bool, bool, PSMethodInvocationConstraints>(memberName, callInfo, propertySetter, nonEnumerating, constraints);
                if (!tuples1.TryGetValue(tuple, out pSInvokeMemberBinder))
                {
                    pSInvokeMemberBinder = new PSInvokeMemberBinder(memberName, true, @static, propertySetter, nonEnumerating, callInfo, constraints);
                    tuples1.Add(tuple, pSInvokeMemberBinder);
                }
            }
            return pSInvokeMemberBinder;
        }

        private PSInvokeMemberBinder GetNonEnumeratingBinder()
        {
            return PSInvokeMemberBinder.Get(base.Name, base.CallInfo, false, this._propertySetter, true, this._invocationConstraints);
        }

        internal static void InvalidateCache()
        {
            lock (PSInvokeMemberBinder._binderCache)
            {
                foreach (PSInvokeMemberBinder value in PSInvokeMemberBinder._binderCache.Values)
                {
                    PSGetMemberBinder pSGetMemberBinder = value._getMemberBinder;
                    pSGetMemberBinder._version = pSGetMemberBinder._version + 1;
                }
            }
            lock (PSInvokeMemberBinder._staticBinderCache)
            {
                foreach (PSInvokeMemberBinder pSInvokeMemberBinder in PSInvokeMemberBinder._staticBinderCache.Values)
                {
                    PSGetMemberBinder pSGetMemberBinder1 = pSInvokeMemberBinder._getMemberBinder;
                    pSGetMemberBinder1._version = pSGetMemberBinder1._version + 1;
                }
            }
        }

        internal static object InvokeAdaptedMember(object obj, string methodName, object[] args)
        {
            TypeTable typeTable;
            ExecutionContext executionContextFromTLS = LocalPipeline.GetExecutionContextFromTLS();
            object obj1 = obj;
            if (executionContextFromTLS != null)
            {
                typeTable = executionContextFromTLS.TypeTable;
            }
            else
            {
                typeTable = null;
            }
            PSObject.AdapterSet mappedAdapter = PSObject.GetMappedAdapter(obj1, typeTable);
            PSMethodInfo pSMethodInfo = mappedAdapter.OriginalAdapter.BaseGetMember<PSMemberInfo>(obj, methodName) as PSMethodInfo;
            if (pSMethodInfo == null && mappedAdapter.DotNetAdapter != null)
            {
                pSMethodInfo = mappedAdapter.DotNetAdapter.BaseGetMember<PSMemberInfo>(obj, methodName) as PSMethodInfo;
            }
            if (pSMethodInfo == null)
            {
                object[] typeFullName = new object[2];
                typeFullName[0] = ParserOps.GetTypeFullName(obj);
                typeFullName[1] = methodName;
                throw InterpreterError.NewInterpreterException(methodName, typeof(RuntimeException), null, "MethodNotFound", ParserStrings.MethodNotFound, typeFullName);
            }
            else
            {
                return pSMethodInfo.Invoke(args);
            }
        }

        internal static object InvokeAdaptedSetMember(object obj, string methodName, object[] args, object valueToSet)
        {
            TypeTable typeTable;
            ExecutionContext executionContextFromTLS = LocalPipeline.GetExecutionContextFromTLS();
            object obj1 = obj;
            if (executionContextFromTLS != null)
            {
                typeTable = executionContextFromTLS.TypeTable;
            }
            else
            {
                typeTable = null;
            }
            PSObject.AdapterSet mappedAdapter = PSObject.GetMappedAdapter(obj1, typeTable);
            PSParameterizedProperty pSParameterizedProperty = mappedAdapter.OriginalAdapter.BaseGetMember<PSParameterizedProperty>(obj, methodName);
            if (pSParameterizedProperty == null && mappedAdapter.DotNetAdapter != null)
            {
                pSParameterizedProperty = mappedAdapter.DotNetAdapter.BaseGetMember<PSParameterizedProperty>(obj, methodName);
            }
            if (pSParameterizedProperty == null)
            {
                object[] typeFullName = new object[2];
                typeFullName[0] = ParserOps.GetTypeFullName(obj);
                typeFullName[1] = methodName;
                throw InterpreterError.NewInterpreterException(methodName, typeof(RuntimeException), null, "MethodNotFound", ParserStrings.MethodNotFound, typeFullName);
            }
            else
            {
                pSParameterizedProperty.InvokeSet(valueToSet, args);
                return valueToSet;
            }
        }

        private DynamicMetaObject InvokeDotNetMethod(DynamicMetaObject target, DynamicMetaObject[] args, BindingRestrictions restrictions, MethodInformation[] mi, Type errorExceptionType)
        {
            bool flag = false;
            object obj;
            string str = null;
            string str1 = null;
            int length = (int)args.Length;
            if (this._propertySetter)
            {
                length--;
            }
            object[] objArray = new object[length];
            for (int i = 0; i < length; i++)
            {
                object value = args[i].Value;
                object[] objArray1 = objArray;
                int num = i;
                if (value == AutomationNull.Value)
                {
                    obj = null;
                }
                else
                {
                    obj = value;
                }
                objArray1[num] = obj;
            }
            MethodInformation methodInformation = Adapter.FindBestMethod(mi, this._invocationConstraints, objArray, ref str, ref str1, out flag);
            if (methodInformation == null)
            {
                Type[] typeArray = new Type[4];
                typeArray[0] = typeof(string);
                typeArray[1] = typeof(Exception);
                typeArray[2] = typeof(string);
                typeArray[3] = typeof(object[]);
                object[] objArray2 = new object[4];
                objArray2[0] = str;
                objArray2[2] = str1;
                object[] name = new object[2];
                name[0] = base.Name;
                name[1] = base.CallInfo.ArgumentCount;
                objArray2[3] = name;
                return new DynamicMetaObject(Compiler.CreateThrow(typeof(object), errorExceptionType, typeArray, objArray2), restrictions);
            }
            else
            {
                MethodInfo methodInfo = (MethodInfo)methodInformation.method;
                Expression expression = PSInvokeMemberBinder.InvokeMethod(methodInfo, target, args, flag);
                if (expression.Type.Equals(typeof(void)))
                {
                    expression = Expression.Block(expression, ExpressionCache.AutomationNullConstant);
                }
                if ((!(methodInfo.DeclaringType == typeof(SteppablePipeline)) || !methodInfo.Name.Equals("Begin", StringComparison.Ordinal)) && !methodInfo.Name.Equals("Process", StringComparison.Ordinal) && !methodInfo.Name.Equals("End", StringComparison.Ordinal))
                {
                    ParameterExpression parameterExpression = Expression.Variable(typeof(Exception));
                    CatchBlock[] catchBlockArray = new CatchBlock[1];
                    catchBlockArray[0] = Expression.Catch(parameterExpression, Expression.Block(Expression.Call(CachedReflectionInfo.ExceptionHandlingOps_ConvertToMethodInvocationException, parameterExpression, Expression.Constant(errorExceptionType), Expression.Constant(methodInfo.Name), ExpressionCache.Constant((int)args.Length), Expression.Constant(methodInfo)), Expression.Rethrow(typeof(object))));
                    expression = Expression.TryCatch(expression.Cast(typeof(object)), catchBlockArray);
                    return new DynamicMetaObject(expression, restrictions);
                }
                else
                {
                    return new DynamicMetaObject(expression, restrictions);
                }
            }
        }

        private DynamicMetaObject InvokeMemberOnCollection(DynamicMetaObject targetEnumerator, DynamicMetaObject[] args, Type typeForMessage, BindingRestrictions restrictions)
        {
            PSInvokeMemberBinder pSInvokeMemberBinder = this;
            Type returnType = this.ReturnType;
            DynamicMetaObject[] dynamicMetaObjectArray = args;
            DynamicExpression dynamicExpression = Expression.Dynamic(pSInvokeMemberBinder, returnType, dynamicMetaObjectArray.Select<DynamicMetaObject, Expression>((DynamicMetaObject a) => a.Expression).Prepend<Expression>(ExpressionCache.NullConstant));
            MethodInfo enumerableOpsMethodInvoker = CachedReflectionInfo.EnumerableOps_MethodInvoker;
            ConstantExpression constantExpression = Expression.Constant(this.GetNonEnumeratingBinder());
            ConstantExpression constantExpression1 = Expression.Constant(dynamicExpression.DelegateType);
            Expression expression = targetEnumerator.Expression;
            Type type = typeof(object);
            DynamicMetaObject[] dynamicMetaObjectArray1 = args;
            return new DynamicMetaObject(Expression.Call(enumerableOpsMethodInvoker, constantExpression, constantExpression1, expression, Expression.NewArrayInit(type, dynamicMetaObjectArray1.Select<DynamicMetaObject, Expression>((DynamicMetaObject a) => a.Expression.Cast(typeof(object)))), Expression.Constant(typeForMessage)), targetEnumerator.Restrictions.Merge(restrictions));
        }

        internal static Expression InvokeMethod(MethodInfo mi, DynamicMetaObject target, DynamicMetaObject[] args, bool expandParameters)
		{
			Expression expression;
			List<ParameterExpression> parameterExpressions = new List<ParameterExpression>();
			List<Expression> expressions = new List<Expression>();
			var temps = new List<ParameterExpression>();
			var initTemps = new List<Expression>();
			List<Expression> expressions1 = new List<Expression>();
			ParameterInfo[] parameters = mi.GetParameters();
			Expression[] expressionArray = new Expression[(int)parameters.Length];
			for (int i = 0; i < (int)parameters.Length; i++)
			{
				string name = parameters[i].Name;
				var paramName = parameters[i].Name;
				if (string.IsNullOrWhiteSpace(parameters[i].Name))
				{
					paramName = i.ToString(CultureInfo.InvariantCulture);
				}
				if ((int)parameters[i].GetCustomAttributes(typeof(ParamArrayAttribute), false).Length == 0)
				{
					if (i < (int)args.Length)
					{
						if (!parameters[i].ParameterType.IsByRef)
						{
							expressionArray[i] = args[i].CastOrConvertMethodArgument(parameters[i].ParameterType, paramName, mi.Name, temps, initTemps);
						}
						else
						{
							if (args[i].Value as PSReference != null)
							{
								ParameterExpression parameterExpression = Expression.Variable(parameters[i].ParameterType.GetElementType());
								temps.Add(parameterExpression);
								MemberExpression memberExpression = Expression.Property(args[i].Expression.Cast(typeof(PSReference)), CachedReflectionInfo.PSReference_Value);
								initTemps.Add(Expression.Assign(parameterExpression, memberExpression.Convert(parameterExpression.Type)));
								expressions1.Add(Expression.Assign(memberExpression, parameterExpression.Cast(typeof(object))));
								expressionArray[i] = parameterExpression;
							}
							else
							{
								Type[] typeArray = new Type[4];
								typeArray[0] = typeof(string);
								typeArray[1] = typeof(Exception);
								typeArray[2] = typeof(string);
								typeArray[3] = typeof(object[]);
								object[] nonRefArgumentToRefParameter = new object[4];
								nonRefArgumentToRefParameter[0] = "NonRefArgumentToRefParameterMsg";
								nonRefArgumentToRefParameter[2] = ExtendedTypeSystem.NonRefArgumentToRefParameter;
								object[] fullName = new object[3];
								fullName[0] = i + 1;
								fullName[1] = typeof(PSReference).FullName;
								fullName[2] = "[ref]";
								nonRefArgumentToRefParameter[3] = fullName;
								return Compiler.CreateThrow(typeof(object), typeof(MethodException), typeArray, nonRefArgumentToRefParameter);
							}
						}
					}
					else
					{
						object defaultValue = parameters[i].DefaultValue;
						if (defaultValue != null)
						{
							expressionArray[i] = Expression.Constant(defaultValue);
						}
						else
						{
							expressionArray[i] = Expression.Constant(null, parameters[i].ParameterType);
						}
					}
				}
				else
				{
					Func<DynamicMetaObject, Expression> func = null;
					Type elementType = parameters[i].ParameterType.GetElementType();
					if (!expandParameters)
					{
						Expression expression1 = args[i].CastOrConvertMethodArgument(parameters[i].ParameterType, paramName, mi.Name, temps, initTemps);
						expressionArray[i] = expression1;
					}
					else
					{
						Expression[] expressionArray1 = expressionArray;
						int num = i;
						Type type = elementType;
						IEnumerable<DynamicMetaObject> dynamicMetaObjects = args.Skip<DynamicMetaObject>(i);
						if (func == null)
						{
							func = (DynamicMetaObject a) => a.CastOrConvertMethodArgument(elementType, name, mi.Name, parameterExpressions, expressions);
						}
						expressionArray1[num] = Expression.NewArrayInit(type, dynamicMetaObjects.Select<DynamicMetaObject, Expression>(func));
					}
				}
			}
			if (mi.IsStatic)
			{
				expression = Expression.Call(mi, expressionArray);
			}
			else
			{
				expression = Expression.Call(PSGetMemberBinder.GetTargetExpr(target).Cast(mi.DeclaringType), mi, expressionArray);
			}
			Expression expression2 = expression;
			if (temps.Any<ParameterExpression>())
			{
				if (!mi.ReturnType.Equals(typeof(void)) && expressions1.Any<Expression>())
				{
					ParameterExpression parameterExpression1 = Expression.Variable(mi.ReturnType);
					temps.Add(parameterExpression1);
					expression2 = Expression.Assign(parameterExpression1, expression2);
					expressions1.Add(parameterExpression1);
				}
				expression2 = Expression.Block(mi.ReturnType, temps, initTemps.Append<Expression>(expression2).Concat<Expression>(expressions1));
			}
			return expression2;
		}

        internal static bool IsHeterogeneousArray(object[] args)
        {
            if ((int)args.Length != 0)
            {
                object obj1 = PSObject.Base(args[0]);
                if (obj1 != null)
                {
                    Type type = obj1.GetType();
                    if (!type.Equals(typeof(object)))
                    {
                        return args.Skip<object>(1).Any<object>((object element) =>
                        {
                            object obj = PSObject.Base(element);
                            if (obj == null)
                            {
                                return true;
                            }
                            else
                            {
                                return !type.Equals(obj.GetType());
                            }
                        }
                        );
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }

        internal static bool IsHomogenousArray<T>(object[] args)
		{
			if ((int)args.Length != 0)
			{
                return args.All<object>(new Func<object, bool>(x => true)); //TODO: PSInvokeMemberBinder.<IsHomogenousArray>b__1d<T>
			}
			else
			{
				return false;
			}
		}

        public override string ToString()
        {
            object obj;
            object obj1;
            object str;
            CultureInfo invariantCulture = CultureInfo.InvariantCulture;
            string str1 = "PSInvokeMember: {0}{1}{2} ver:{3} args:{4} constraints:<{5}>";
            object[] name = new object[6];
            object[] objArray = name;
            int num = 0;
            if (this._static)
            {
                obj = "static ";
            }
            else
            {
                obj = "";
            }
            objArray[num] = obj;
            object[] objArray1 = name;
            int num1 = 1;
            if (this._propertySetter)
            {
                obj1 = "propset ";
            }
            else
            {
                obj1 = "";
            }
            objArray1[num1] = obj1;
            name[2] = base.Name;
            name[3] = this._getMemberBinder._version;
            name[4] = base.CallInfo.ArgumentCount;
            object[] objArray2 = name;
            int num2 = 5;
            if (this._invocationConstraints != null)
            {
                str = this._invocationConstraints.ToString();
            }
            else
            {
                str = "";
            }
            objArray2[num2] = str;
            return string.Format(invariantCulture, str1, name);
        }

        internal static bool TryGetInstanceMethod(object value, string memberName, out PSMethodInfo methodInfo)
        {
            PSMemberInfoInternalCollection<PSMemberInfo> pSMemberInfos = null;
            PSMemberInfo item;
            if (PSObject.HasInstanceMembers(value, out pSMemberInfos))
            {
                item = pSMemberInfos[memberName];
            }
            else
            {
                item = null;
            }
            PSMemberInfo pSMemberInfo = item;
            methodInfo = pSMemberInfo as PSMethodInfo;
            if (pSMemberInfo != null)
            {
                if (methodInfo != null)
                {
                    return true;
                }
                else
                {
                    object[] typeFullName = new object[2];
                    typeFullName[0] = ParserOps.GetTypeFullName(value);
                    typeFullName[1] = memberName;
                    throw InterpreterError.NewInterpreterException(memberName, typeof(RuntimeException), null, "MethodNotFound", ParserStrings.MethodNotFound, typeFullName);
                }
            }
            else
            {
                return false;
            }
        }

        private class KeyComparer : IEqualityComparer<Tuple<string, CallInfo, bool, bool, PSMethodInvocationConstraints>>
        {
            public KeyComparer()
            {
            }

            public bool Equals(Tuple<string, CallInfo, bool, bool, PSMethodInvocationConstraints> x, Tuple<string, CallInfo, bool, bool, PSMethodInvocationConstraints> y)
            {
                if (x.Item1.Equals(y.Item1, StringComparison.OrdinalIgnoreCase) && x.Item2.Equals(y.Item2))
                {
                    bool item3 = x.Item3;
                    if (item3.Equals(y.Item3))
                    {
                        bool item4 = x.Item4;
                        if (item4.Equals(y.Item4))
                        {
                            if (x.Item5 == null)
                            {
                                return y.Item5 == null;
                            }
                            else
                            {
                                return x.Item5.Equals(y.Item5);
                            }
                        }
                    }
                }
                return false;
            }

            public int GetHashCode(Tuple<string, CallInfo, bool, bool, PSMethodInvocationConstraints> obj)
            {
                return obj.GetHashCode();
            }
        }
    }
}