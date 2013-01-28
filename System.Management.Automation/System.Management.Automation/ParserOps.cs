namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Language;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;

    internal static class ParserOps
    {
        private static readonly string[] _chars = new string[0xff];
        internal static readonly object _FalseObject = false;
        private static readonly object[] _integerCache = new object[0x44c];
        private const int _MaxCache = 0x3e8;
        private const int _MinCache = -100;
        private static Dictionary<string, Regex> _regexCache = new Dictionary<string, Regex>();
        internal static readonly object _TrueObject = true;
        private const int MaxRegexCache = 0x3e8;
        internal const string MethodNotFoundErrorId = "MethodNotFound";

        static ParserOps()
        {
            for (int i = 0; i < 0x44c; i++)
            {
                _integerCache[i] = i + -100;
            }
            for (char ch = '\0'; ch < '\x00ff'; ch = (char) (ch + '\x0001'))
            {
                _chars[ch] = new string(ch, 1);
            }
        }

        internal static object BoolToObject(bool value)
        {
            if (!value)
            {
                return _FalseObject;
            }
            return _TrueObject;
        }

        internal static object CallMethod(IScriptExtent errorPosition, object target, string methodName, PSMethodInvocationConstraints invocationConstraints, object[] paramArray, bool callStatic, object valueToSet)
        {
            PSMethodInfo staticCLRMember = null;
            MethodInformation methodInformation = null;
            object obj2 = null;
            PSObject obj3 = null;
            Type type;
            object obj4;
            if (LanguagePrimitives.IsNull(target))
            {
                throw InterpreterError.NewInterpreterException(methodName, typeof(RuntimeException), errorPosition, "InvokeMethodOnNull", ParserStrings.InvokeMethodOnNull, new object[0]);
            }
            obj2 = PSObject.Base(target);
            obj3 = PSObject.AsPSObject(target);
            CallsiteCacheEntryFlags none = CallsiteCacheEntryFlags.None;
            if (callStatic)
            {
                none |= CallsiteCacheEntryFlags.Static;
                type = (Type) obj2;
            }
            else
            {
                type = obj2.GetType();
            }
            if (valueToSet != AutomationNull.Value)
            {
                none |= CallsiteCacheEntryFlags.ParameterizedSetter;
            }
            if (!obj3.isDeserialized)
            {
                methodInformation = Adapter.FindCachedMethod(type, methodName, invocationConstraints, paramArray, none);
            }
            if (methodInformation == null)
            {
                if (callStatic)
                {
                    staticCLRMember = PSObject.GetStaticCLRMember(target, methodName) as PSMethod;
                }
                else
                {
                    staticCLRMember = obj3.Members[methodName] as PSMethodInfo;
                }
                if (staticCLRMember == null)
                {
                    string fullName = null;
                    if (callStatic)
                    {
                        fullName = type.FullName;
                    }
                    else
                    {
                        fullName = GetTypeFullName(target);
                    }
                    if (valueToSet == AutomationNull.Value)
                    {
                        throw InterpreterError.NewInterpreterException(methodName, typeof(RuntimeException), errorPosition, "MethodNotFound", ParserStrings.MethodNotFound, new object[] { fullName, methodName });
                    }
                    throw InterpreterError.NewInterpreterException(methodName, typeof(RuntimeException), errorPosition, "ParameterizedPropertyAssignmentFailed", ParserStrings.ParameterizedPropertyAssignmentFailed, new object[] { fullName, methodName });
                }
            }
            try
            {
                if (methodInformation != null)
                {
                    object[] objArray;
                    PSObject.memberResolution.WriteLine("cache hit, Calling Method: {0}", new object[] { methodInformation.methodDefinition });
                    if (valueToSet != AutomationNull.Value)
                    {
                        DotNetAdapter.ParameterizedPropertyInvokeSet(methodName, obj2, valueToSet, new MethodInformation[] { methodInformation }, paramArray, false);
                        return valueToSet;
                    }
                    MethodInformation[] methods = new MethodInformation[] { methodInformation };
                    Adapter.GetBestMethodAndArguments(methodName, methods, paramArray, out objArray);
                    return DotNetAdapter.AuxiliaryMethodInvoke(obj2, objArray, methodInformation, paramArray);
                }
                if (valueToSet != AutomationNull.Value)
                {
                    PSParameterizedProperty property = staticCLRMember as PSParameterizedProperty;
                    if (property == null)
                    {
                        throw InterpreterError.NewInterpreterException(methodName, typeof(RuntimeException), errorPosition, "ParameterizedPropertyAssignmentFailed", ParserStrings.ParameterizedPropertyAssignmentFailed, new object[] { GetTypeFullName(target), methodName });
                    }
                    property.InvokeSet(valueToSet, paramArray);
                    return valueToSet;
                }
                PSMethod method = staticCLRMember as PSMethod;
                if (method != null)
                {
                    return method.Invoke(invocationConstraints, paramArray);
                }
                obj4 = staticCLRMember.Invoke(paramArray);
            }
            catch (MethodInvocationException exception)
            {
                if (exception.ErrorRecord.InvocationInfo == null)
                {
                    exception.ErrorRecord.SetInvocationInfo(new InvocationInfo(null, errorPosition));
                }
                throw;
            }
            catch (RuntimeException exception2)
            {
                if (exception2.ErrorRecord.InvocationInfo == null)
                {
                    exception2.ErrorRecord.SetInvocationInfo(new InvocationInfo(null, errorPosition));
                }
                throw;
            }
            catch (FlowControlException)
            {
                throw;
            }
            catch (ScriptCallDepthException)
            {
                throw;
            }
            catch (Exception exception3)
            {
                CommandProcessorBase.CheckForSevereException(exception3);
                throw InterpreterError.NewInterpreterExceptionByMessage(typeof(RuntimeException), errorPosition, exception3.Message, "MethodInvocationException", exception3);
            }
            return obj4;
        }

        internal static string CharToString(char ch)
        {
            if (ch < '\x00ff')
            {
                return _chars[ch];
            }
            return new string(ch, 1);
        }

        internal static object CompareOperators(System.Management.Automation.ExecutionContext context, IScriptExtent errorPosition, object left, object right, CompareDelegate compareDelegate, bool ignoreCase)
        {
            IEnumerator enumerator = LanguagePrimitives.GetEnumerator(left);
            if (enumerator == null)
            {
                return BoolToObject(compareDelegate(left, right, ignoreCase));
            }
            ArrayList list = new ArrayList();
            while (MoveNext(context, errorPosition, enumerator))
            {
                object lhs = Current(errorPosition, enumerator);
                if (compareDelegate(lhs, right, ignoreCase))
                {
                    list.Add(lhs);
                }
            }
            return list.ToArray();
        }

        internal static bool CompareScalarEq(object lhs, object rhs, bool ignoreCase)
        {
            return LanguagePrimitives.Equals(lhs, rhs, ignoreCase, CultureInfo.InvariantCulture);
        }

        internal static bool CompareScalarGe(object lhs, object rhs, bool ignoreCase)
        {
            return (LanguagePrimitives.Compare(lhs, rhs, ignoreCase, CultureInfo.InvariantCulture) >= 0);
        }

        internal static bool CompareScalarGt(object lhs, object rhs, bool ignoreCase)
        {
            return (LanguagePrimitives.Compare(lhs, rhs, ignoreCase, CultureInfo.InvariantCulture) > 0);
        }

        internal static bool CompareScalarLe(object lhs, object rhs, bool ignoreCase)
        {
            return (LanguagePrimitives.Compare(lhs, rhs, ignoreCase, CultureInfo.InvariantCulture) <= 0);
        }

        internal static bool CompareScalarLt(object lhs, object rhs, bool ignoreCase)
        {
            return (LanguagePrimitives.Compare(lhs, rhs, ignoreCase, CultureInfo.InvariantCulture) < 0);
        }

        internal static bool CompareScalarNe(object lhs, object rhs, bool ignoreCase)
        {
            return !LanguagePrimitives.Equals(lhs, rhs, ignoreCase, CultureInfo.InvariantCulture);
        }

        internal static object ContainsOperator(System.Management.Automation.ExecutionContext context, IScriptExtent errorPosition, object left, object right, bool contains, bool ignoreCase)
        {
            IEnumerator enumerator = LanguagePrimitives.GetEnumerator(left);
            if (enumerator != null)
            {
                while (MoveNext(context, errorPosition, enumerator))
                {
                    if (LanguagePrimitives.Equals(Current(errorPosition, enumerator), right, ignoreCase, CultureInfo.InvariantCulture))
                    {
                        return BoolToObject(contains);
                    }
                }
                return BoolToObject(!contains);
            }
            return BoolToObject(contains == LanguagePrimitives.Equals(left, right, ignoreCase, CultureInfo.InvariantCulture));
        }

        internal static bool ContainsOperatorCompiled(System.Management.Automation.ExecutionContext context, CallSite<Func<CallSite, object, IEnumerator>> getEnumeratorSite, CallSite<Func<CallSite, object, object, object>> comparerSite, object left, object right)
        {
            IEnumerator enumerator = getEnumeratorSite.Target(getEnumeratorSite, left);
            if ((enumerator != null) && !(enumerator is EnumerableOps.NonEnumerableObjectEnumerator))
            {
                while (EnumerableOps.MoveNext(context, enumerator))
                {
                    object obj2 = EnumerableOps.Current(enumerator);
                    if ((bool) comparerSite.Target(comparerSite, obj2, right))
                    {
                        return true;
                    }
                }
                return false;
            }
            return (bool) comparerSite.Target(comparerSite, left, right);
        }

        internal static T ConvertTo<T>(object obj, IScriptExtent errorPosition)
        {
            T local;
            try
            {
                local = (T) LanguagePrimitives.ConvertTo(obj, typeof(T), CultureInfo.InvariantCulture);
            }
            catch (PSInvalidCastException exception)
            {
                RuntimeException exception2 = new RuntimeException(exception.Message, exception);
                exception2.ErrorRecord.SetInvocationInfo(new InvocationInfo(null, errorPosition));
                throw exception2;
            }
            return local;
        }

        internal static object Current(IScriptExtent errorPosition, IEnumerator enumerator)
        {
            object current;
            try
            {
                current = enumerator.Current;
            }
            catch (RuntimeException)
            {
                throw;
            }
            catch (ScriptCallDepthException)
            {
                throw;
            }
            catch (FlowControlException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw InterpreterError.NewInterpreterExceptionWithInnerException(enumerator, typeof(RuntimeException), errorPosition, "BadEnumeration", ParserStrings.BadEnumeration, exception, new object[] { exception.Message });
            }
            return current;
        }

        private static IEnumerable<string> enumerateContent(System.Management.Automation.ExecutionContext context, IScriptExtent errorPosition, SplitImplOptions implOptions, object tuple)
        {
            IEnumerator enumerator = LanguagePrimitives.GetEnumerator(tuple);
            if (enumerator == null)
            {
                enumerator = new object[] { tuple }.GetEnumerator();
            }
            while (true)
            {
                if (!MoveNext(context, errorPosition, enumerator))
                {
                    yield break;
                }
                string iteratorVariable1 = PSObject.ToStringParser(context, enumerator.Current);
                if ((implOptions & SplitImplOptions.TrimContent) != SplitImplOptions.None)
                {
                    iteratorVariable1 = iteratorVariable1.Trim();
                }
                yield return iteratorVariable1;
            }
        }

        private static void ExtendList<T>(IList<T> list, IList<T> items)
        {
            foreach (T local in items)
            {
                list.Add(local);
            }
        }

        internal static int FixNum(object obj, IScriptExtent errorPosition)
        {
            obj = PSObject.Base(obj);
            if (obj == null)
            {
                return 0;
            }
            if (obj is int)
            {
                return (int) obj;
            }
            return ConvertTo<int>(obj, errorPosition);
        }

        internal static string GetTypeFullName(object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }
            PSObject obj2 = obj as PSObject;
            if (obj2 == null)
            {
                return obj.GetType().FullName;
            }
            if (obj2.InternalTypeNames.Count == 0)
            {
                return typeof(PSObject).FullName;
            }
            return obj2.InternalTypeNames[0];
        }

        internal static object ImplicitOp(object lval, object rval, string op, IScriptExtent errorPosition, string errorOp)
        {
            Type type;
            lval = PSObject.Base(lval);
            rval = PSObject.Base(rval);
            if ((lval == null) || lval.GetType().IsPrimitive)
            {
                type = ((rval == null) || rval.GetType().IsPrimitive) ? null : rval.GetType();
            }
            else
            {
                type = lval.GetType();
            }
            if (type == null)
            {
                throw InterpreterError.NewInterpreterException(lval, typeof(RuntimeException), errorPosition, "NotADefinedOperationForType", ParserStrings.NotADefinedOperationForType, new object[] { (lval == null) ? "$null" : lval.GetType().FullName, errorOp, (rval == null) ? "$null" : rval.GetType().FullName });
            }
            object[] paramArray = new object[] { lval, rval };
            return CallMethod(errorPosition, type, op, null, paramArray, true, AutomationNull.Value);
        }

        internal static object IntToObject(int value)
        {
            if ((value < 0x3e8) && (value >= -100))
            {
                return _integerCache[value - -100];
            }
            return value;
        }

        internal static object IsNotOperator(System.Management.Automation.ExecutionContext context, IScriptExtent errorPosition, object left, object right)
        {
            object o = PSObject.Base(left);
            object obj3 = PSObject.Base(right);
            Type type = obj3 as Type;
            if (type == null)
            {
                type = ConvertTo<Type>(obj3, errorPosition);
                if (type == null)
                {
                    throw InterpreterError.NewInterpreterException(obj3, typeof(RuntimeException), errorPosition, "IsOperatorRequiresType", ParserStrings.IsOperatorRequiresType, new object[0]);
                }
            }
            if ((type == typeof(PSCustomObject)) && (o is PSObject))
            {
                return _FalseObject;
            }
            if (type.Equals(typeof(PSObject)) && (left is PSObject))
            {
                return _FalseObject;
            }
            return BoolToObject(!type.IsInstanceOfType(o));
        }

        internal static object IsOperator(System.Management.Automation.ExecutionContext context, IScriptExtent errorPosition, object left, object right)
        {
            object o = PSObject.Base(left);
            object obj3 = PSObject.Base(right);
            Type type = obj3 as Type;
            if (type == null)
            {
                type = ConvertTo<Type>(obj3, errorPosition);
                if (type == null)
                {
                    throw InterpreterError.NewInterpreterException(obj3, typeof(RuntimeException), errorPosition, "IsOperatorRequiresType", ParserStrings.IsOperatorRequiresType, new object[0]);
                }
            }
            if ((type == typeof(PSCustomObject)) && (o is PSObject))
            {
                return _TrueObject;
            }
            if (type.Equals(typeof(PSObject)) && (left is PSObject))
            {
                return _TrueObject;
            }
            return BoolToObject(type.IsInstanceOfType(o));
        }

        internal static object JoinOperator(System.Management.Automation.ExecutionContext context, IScriptExtent errorPosition, object lval, object rval)
        {
            string separator = PSObject.ToStringParser(context, rval);
            IEnumerable enumerable = LanguagePrimitives.GetEnumerable(lval);
            if (enumerable != null)
            {
                return PSObject.ToStringEnumerable(context, enumerable, separator, null, null);
            }
            return PSObject.ToStringParser(context, lval);
        }

        internal static object LikeOperator(System.Management.Automation.ExecutionContext context, IScriptExtent errorPosition, object lval, object rval, bool notLike, bool ignoreCase)
        {
            WildcardPattern pattern = new WildcardPattern(PSObject.ToStringParser(context, rval), ignoreCase ? WildcardOptions.IgnoreCase : WildcardOptions.None);
            IEnumerator enumerator = LanguagePrimitives.GetEnumerator(lval);
            if (enumerator == null)
            {
                string input = (lval == null) ? string.Empty : PSObject.ToStringParser(context, lval);
                return BoolToObject(pattern.IsMatch(input) ^ notLike);
            }
            ArrayList list = new ArrayList();
            while (MoveNext(context, errorPosition, enumerator))
            {
                object obj2 = Current(errorPosition, enumerator);
                string str2 = (obj2 == null) ? string.Empty : PSObject.ToStringParser(context, obj2);
                if (pattern.IsMatch(str2) ^ notLike)
                {
                    list.Add(str2);
                }
            }
            return list.ToArray();
        }

        internal static object MatchOperator(System.Management.Automation.ExecutionContext context, IScriptExtent errorPosition, object lval, object rval, bool notMatch, bool ignoreCase)
        {
            object obj3;
            RegexOptions options = ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;
            Regex regex = PSObject.Base(rval) as Regex;
            if (regex == null)
            {
                regex = NewRegex(PSObject.ToStringParser(context, rval), options);
            }
            IEnumerator targetObject = LanguagePrimitives.GetEnumerator(lval);
            if (targetObject == null)
            {
                string input = (lval == null) ? string.Empty : PSObject.ToStringParser(context, lval);
                Match match = regex.Match(input);
                if (match.Success)
                {
                    GroupCollection groups = match.Groups;
                    if (groups.Count > 0)
                    {
                        Hashtable newValue = new Hashtable(StringComparer.CurrentCultureIgnoreCase);
                        foreach (string str2 in regex.GetGroupNames())
                        {
                            Group group = groups[str2];
                            if (group.Success)
                            {
                                int num;
                                if (int.TryParse(str2, out num))
                                {
                                    newValue.Add(num, group.ToString());
                                }
                                else
                                {
                                    newValue.Add(str2, group.ToString());
                                }
                            }
                        }
                        context.SetVariable(SpecialVariables.MatchesVarPath, newValue);
                    }
                }
                return BoolToObject(match.Success ^ notMatch);
            }
            ArrayList list = new ArrayList();
            int num2 = 0;
            try
            {
                while (targetObject.MoveNext())
                {
                    object current = targetObject.Current;
                    string str3 = (current == null) ? string.Empty : PSObject.ToStringParser(context, current);
                    if (regex.Match(str3).Success ^ notMatch)
                    {
                        list.Add(current);
                    }
                    if (num2++ > 0x3e8)
                    {
                        if ((context != null) && context.CurrentPipelineStopping)
                        {
                            throw new PipelineStoppedException();
                        }
                        num2 = 0;
                    }
                }
                obj3 = list.ToArray();
            }
            catch (RuntimeException)
            {
                throw;
            }
            catch (FlowControlException)
            {
                throw;
            }
            catch (ScriptCallDepthException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw InterpreterError.NewInterpreterExceptionWithInnerException(targetObject, typeof(RuntimeException), errorPosition, "BadEnumeration", ParserStrings.BadEnumeration, exception, new object[] { exception.Message });
            }
            return obj3;
        }

        internal static bool MoveNext(System.Management.Automation.ExecutionContext context, IScriptExtent errorPosition, IEnumerator enumerator)
        {
            bool flag;
            try
            {
                if ((context != null) && context.CurrentPipelineStopping)
                {
                    throw new PipelineStoppedException();
                }
                flag = enumerator.MoveNext();
            }
            catch (RuntimeException)
            {
                throw;
            }
            catch (FlowControlException)
            {
                throw;
            }
            catch (ScriptCallDepthException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw InterpreterError.NewInterpreterExceptionWithInnerException(enumerator, typeof(RuntimeException), errorPosition, "BadEnumeration", ParserStrings.BadEnumeration, exception, new object[] { exception.Message });
            }
            return flag;
        }

        internal static Regex NewRegex(string patternString, RegexOptions options)
        {
            if (options != RegexOptions.IgnoreCase)
            {
                return new Regex(patternString, options);
            }
            lock (_regexCache)
            {
                if (_regexCache.ContainsKey(patternString))
                {
                    return _regexCache[patternString];
                }
                if (_regexCache.Count > 0x3e8)
                {
                    _regexCache.Clear();
                }
                Regex regex = new Regex(patternString, RegexOptions.IgnoreCase);
                _regexCache.Add(patternString, regex);
                return regex;
            }
        }

        private static RegexOptions parseRegexOptions(SplitOptions options)
        {
            int[][] numArray = new int[][] { new int[] { 4, 0x200 }, new int[] { 8, 0x20 }, new int[] { 0x10, 2 }, new int[] { 0x20, 0x10 }, new int[] { 0x40, 1 }, new int[] { 0x80, 4 } };
            RegexOptions none = RegexOptions.None;
            foreach (int[] numArray2 in numArray)
            {
                if ((options & (SplitOptions)numArray2[0]) != 0)
                {
                    none |= (RegexOptions)numArray2[1];
                }
            }
            return none;
        }

        internal static object ReplaceOperator(System.Management.Automation.ExecutionContext context, IScriptExtent errorPosition, object lval, object rval, bool ignoreCase)
        {
            string replacement = "";
            object obj2 = "";
            rval = PSObject.Base(rval);
            IList list = rval as IList;
            if (list != null)
            {
                if (list.Count > 2)
                {
                    throw InterpreterError.NewInterpreterException(rval, typeof(RuntimeException), errorPosition, "BadReplaceArgument", ParserStrings.BadReplaceArgument, new object[] { ignoreCase ? "-ireplace" : "-replace", list.Count });
                }
                if (list.Count > 0)
                {
                    obj2 = list[0];
                    if (list.Count > 1)
                    {
                        replacement = PSObject.ToStringParser(context, list[1]);
                    }
                }
            }
            else
            {
                obj2 = rval;
            }
            RegexOptions none = RegexOptions.None;
            if (ignoreCase)
            {
                none = RegexOptions.IgnoreCase;
            }
            Regex regex = obj2 as Regex;
            if (regex == null)
            {
                try
                {
                    regex = NewRegex(PSObject.ToStringParser(context, obj2), none);
                }
                catch (ArgumentException exception)
                {
                    throw InterpreterError.NewInterpreterExceptionWithInnerException(obj2, typeof(RuntimeException), null, "InvalidRegularExpression", ParserStrings.InvalidRegularExpression, exception, new object[] { obj2 });
                }
            }
            IEnumerator enumerator = LanguagePrimitives.GetEnumerator(lval);
            if (enumerator == null)
            {
                string input = ((lval == null) ? string.Empty : lval).ToString();
                return regex.Replace(input, replacement);
            }
            ArrayList list2 = new ArrayList();
            while (MoveNext(context, errorPosition, enumerator))
            {
                string str3 = PSObject.ToStringParser(context, Current(errorPosition, enumerator));
                list2.Add(regex.Replace(str3, replacement));
            }
            return list2.ToArray();
        }

        internal static object SplitOperator(System.Management.Automation.ExecutionContext context, IScriptExtent errorPosition, object lval, object rval, bool ignoreCase)
        {
            return SplitOperatorImpl(context, errorPosition, lval, rval, SplitImplOptions.None, ignoreCase);
        }

        private static object SplitOperatorImpl(System.Management.Automation.ExecutionContext context, IScriptExtent errorPosition, object lval, object rval, SplitImplOptions implOptions, bool ignoreCase)
        {
            IEnumerable<string> content = enumerateContent(context, errorPosition, implOptions, lval);
            ScriptBlock predicate = null;
            string separatorPattern = null;
            int limit = 0;
            SplitOptions options = 0;
            object[] objArray = unfoldTuple(context, errorPosition, rval);
            if (objArray.Length < 1)
            {
                throw InterpreterError.NewInterpreterException(rval, typeof(RuntimeException), errorPosition, "BadOperatorArgument", ParserStrings.BadOperatorArgument, new object[] { "-split", rval });
            }
            object obj1 = objArray[0];
            predicate = objArray[0] as ScriptBlock;
            if (predicate == null)
            {
                separatorPattern = PSObject.ToStringParser(context, objArray[0]);
            }
            if (objArray.Length >= 2)
            {
                limit = FixNum(objArray[1], errorPosition);
            }
            if ((objArray.Length >= 3) && (objArray[2] != null))
            {
                string str2 = objArray[2] as string;
                if ((str2 == null) || !string.IsNullOrEmpty(str2))
                {
                    options = ConvertTo<SplitOptions>(objArray[2], errorPosition);
                    if (predicate != null)
                    {
                        throw InterpreterError.NewInterpreterException(null, typeof(ParseException), errorPosition, "InvalidSplitOptionWithPredicate", ParserStrings.InvalidSplitOptionWithPredicate, new object[0]);
                    }
                    if (ignoreCase && ((options & SplitOptions.IgnoreCase) == 0))
                    {
                        options |= SplitOptions.IgnoreCase;
                    }
                }
            }
            else if (ignoreCase)
            {
                options |= SplitOptions.IgnoreCase;
            }
            if (predicate != null)
            {
                return SplitWithPredicate(context, errorPosition, content, predicate, limit);
            }
            return SplitWithPattern(context, errorPosition, content, separatorPattern, limit, options);
        }

        private static object SplitWithPattern(System.Management.Automation.ExecutionContext context, IScriptExtent errorPosition, IEnumerable<string> content, string separatorPattern, int limit, SplitOptions options)
        {
            if (((options & SplitOptions.SimpleMatch) == 0) && ((options & SplitOptions.RegexMatch) == 0))
            {
                options |= SplitOptions.RegexMatch;
            }
            if (((options & SplitOptions.SimpleMatch) != 0) && ((options & ~(SplitOptions.IgnoreCase | SplitOptions.SimpleMatch)) != 0))
            {
                throw InterpreterError.NewInterpreterException(null, typeof(ParseException), errorPosition, "InvalidSplitOptionCombination", ParserStrings.InvalidSplitOptionCombination, new object[0]);
            }
            if ((options & (SplitOptions.Singleline | SplitOptions.Multiline)) == (SplitOptions.Singleline | SplitOptions.Multiline))
            {
                throw InterpreterError.NewInterpreterException(null, typeof(ParseException), errorPosition, "InvalidSplitOptionCombination", ParserStrings.InvalidSplitOptionCombination, new object[0]);
            }
            if ((options & SplitOptions.SimpleMatch) != 0)
            {
                separatorPattern = Regex.Escape(separatorPattern);
            }
            if (limit < 0)
            {
                limit = 0;
            }
            RegexOptions options2 = parseRegexOptions(options);
            Regex regex = NewRegex(separatorPattern, options2);
            List<string> list = new List<string>();
            foreach (string str in content)
            {
                string[] items = regex.Split(str, limit, 0);
                ExtendList<string>(list, items);
            }
            return list.ToArray();
        }

        private static object SplitWithPredicate(System.Management.Automation.ExecutionContext context, IScriptExtent errorPosition, IEnumerable<string> content, ScriptBlock predicate, int limit)
        {
            List<string> list = new List<string>();
            foreach (string str in content)
            {
                List<string> items = new List<string>();
                if (limit == 1)
                {
                    list.Add(str);
                    continue;
                }
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < str.Length; i++)
                {
                    if (LanguagePrimitives.IsTrue(predicate.DoInvokeReturnAsIs(true, ScriptBlock.ErrorHandlingBehavior.WriteToExternalErrorPipe, CharToString(str[i]), AutomationNull.Value, AutomationNull.Value, new object[] { str, i })))
                    {
                        items.Add(builder.ToString());
                        builder = new StringBuilder();
                        if ((limit > 0) && (items.Count >= (limit - 1)))
                        {
                            if ((i + 1) < str.Length)
                            {
                                items.Add(str.Substring(i + 1));
                            }
                            else
                            {
                                items.Add("");
                            }
                            break;
                        }
                        if (i == (str.Length - 1))
                        {
                            items.Add("");
                        }
                    }
                    else
                    {
                        builder.Append(str[i]);
                    }
                }
                if ((builder.Length > 0) && ((limit <= 0) || (items.Count < limit)))
                {
                    items.Add(builder.ToString());
                }
                ExtendList<string>(list, items);
            }
            return list.ToArray();
        }

        internal static object UnaryJoinOperator(System.Management.Automation.ExecutionContext context, IScriptExtent errorPosition, object lval)
        {
            return JoinOperator(context, errorPosition, lval, "");
        }

        internal static object UnarySplitOperator(System.Management.Automation.ExecutionContext context, IScriptExtent errorPosition, object lval)
        {
            return SplitOperatorImpl(context, errorPosition, lval, new object[] { @"\s+" }, SplitImplOptions.TrimContent, false);
        }

        private static object[] unfoldTuple(System.Management.Automation.ExecutionContext context, IScriptExtent errorPosition, object tuple)
        {
            List<object> list = new List<object>();
            IEnumerator enumerator = LanguagePrimitives.GetEnumerator(tuple);
            if (enumerator != null)
            {
                while (MoveNext(context, errorPosition, enumerator))
                {
                    object item = Current(errorPosition, enumerator);
                    list.Add(item);
                }
            }
            else
            {
                list.Add(tuple);
            }
            return list.ToArray();
        }

        internal static PSObject WrappedNumber(object data, string text)
        {
            return new PSObject(data) { TokenText = text };
        }

        

        internal delegate bool CompareDelegate(object lhs, object rhs, bool ignoreCase);

        [Flags]
        private enum SplitImplOptions
        {
            None,
            TrimContent
        }
    }
}

