namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Management.Automation.Runspaces;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;

    internal abstract class Adapter
    {
        private static Dictionary<CallsiteCacheEntry, MethodInformation> callsiteCache = new Dictionary<CallsiteCacheEntry, MethodInformation>(0x400);
        [TraceSource("ETS", "Extended Type System")]
        protected static PSTraceSource tracer = PSTraceSource.GetTracer("ETS", "Extended Type System");

        protected Adapter()
        {
        }

        internal T BaseGetMember<T>(object obj, string memberName) where T: PSMemberInfo
        {
            T member;
            try
            {
                member = this.GetMember<T>(obj, memberName);
            }
            catch (ExtendedTypeSystemException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw NewException(exception, "CatchFromBaseGetMember", "CatchFromBaseGetMemberTI", ExtendedTypeSystem.ExceptionGettingMember, new object[] { memberName });
            }
            return member;
        }

        internal PSMemberInfoInternalCollection<T> BaseGetMembers<T>(object obj) where T: PSMemberInfo
        {
            PSMemberInfoInternalCollection<T> members;
            try
            {
                members = this.GetMembers<T>(obj);
            }
            catch (ExtendedTypeSystemException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw NewException(exception, "CatchFromBaseGetMembers", "CatchFromBaseGetMembersTI", ExtendedTypeSystem.ExceptionGettingMembers, new object[0]);
            }
            return members;
        }

        internal ConsolidatedString BaseGetTypeNameHierarchy(object obj)
        {
            ConsolidatedString internedTypeNameHierarchy;
            try
            {
                internedTypeNameHierarchy = this.GetInternedTypeNameHierarchy(obj);
            }
            catch (ExtendedTypeSystemException ex)
            {
				var msg = ex.Message;
                throw;
            }
            catch (Exception exception)
            {
				Console.WriteLine (exception.Message);
				Console.WriteLine();
				Console.WriteLine (exception.StackTrace);
                CommandProcessorBase.CheckForSevereException(exception);
                throw NewException(exception, "CatchFromBaseGetTypeNameHierarchy", "CatchFromBaseGetTypeNameHierarchyTI", ExtendedTypeSystem.ExceptionRetrievingTypeNameHierarchy, new object[0]);
            }
            return internedTypeNameHierarchy;
        }

        internal Collection<string> BaseMethodDefinitions(PSMethod method)
        {
            Collection<string> collection;
            try
            {
                collection = this.MethodDefinitions(method);
            }
            catch (ExtendedTypeSystemException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw NewException(exception, "CatchFromBaseMethodDefinitions", "CatchFromBaseMethodDefinitionsTI", ExtendedTypeSystem.ExceptionRetrievingMethodDefinitions, new object[] { method.Name });
            }
            return collection;
        }

        internal object BaseMethodInvoke(PSMethod method, PSMethodInvocationConstraints invocationConstraints, params object[] arguments)
        {
            object obj2;
            try
            {
                obj2 = this.MethodInvoke(method, invocationConstraints, arguments);
            }
            catch (TargetInvocationException exception)
            {
                Exception innerException = (exception.InnerException == null) ? exception : exception.InnerException;
                throw new MethodInvocationException("CatchFromBaseAdapterMethodInvokeTI", innerException, ExtendedTypeSystem.MethodInvocationException, new object[] { method.Name, arguments.Length, innerException.Message });
            }
            catch (FlowControlException)
            {
                throw;
            }
            catch (ScriptCallDepthException)
            {
                throw;
            }
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (MethodException)
            {
                throw;
            }
            catch (Exception exception3)
            {
                CommandProcessorBase.CheckForSevereException(exception3);
                if ((method.baseObject is SteppablePipeline) && ((method.Name.Equals("Begin", StringComparison.OrdinalIgnoreCase) || method.Name.Equals("Process", StringComparison.OrdinalIgnoreCase)) || method.Name.Equals("End", StringComparison.OrdinalIgnoreCase)))
                {
                    throw;
                }
                throw new MethodInvocationException("CatchFromBaseAdapterMethodInvoke", exception3, ExtendedTypeSystem.MethodInvocationException, new object[] { method.Name, arguments.Length, exception3.Message });
            }
            return obj2;
        }

        internal string BaseMethodToString(PSMethod method)
        {
            string str;
            try
            {
                str = this.MethodToString(method);
            }
            catch (ExtendedTypeSystemException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw NewException(exception, "CatchFromBaseMethodToString", "CatchFromBaseMethodToStringTI", ExtendedTypeSystem.ExceptionRetrievingMethodString, new object[] { method.Name });
            }
            return str;
        }

        internal Collection<string> BaseParameterizedPropertyDefinitions(PSParameterizedProperty property)
        {
            Collection<string> collection;
            try
            {
                collection = this.ParameterizedPropertyDefinitions(property);
            }
            catch (ExtendedTypeSystemException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw NewException(exception, "CatchFromBaseParameterizedPropertyDefinitions", "CatchFromBaseParameterizedPropertyDefinitionsTI", ExtendedTypeSystem.ExceptionRetrievingParameterizedPropertyDefinitions, new object[] { property.Name });
            }
            return collection;
        }

        internal object BaseParameterizedPropertyGet(PSParameterizedProperty property, params object[] arguments)
        {
            object obj2;
            try
            {
                obj2 = this.ParameterizedPropertyGet(property, arguments);
            }
            catch (TargetInvocationException exception)
            {
                Exception innerException = (exception.InnerException == null) ? exception : exception.InnerException;
                throw new GetValueInvocationException("CatchFromBaseAdapterParameterizedPropertyGetValueTI", innerException, ExtendedTypeSystem.ExceptionWhenGetting, new object[] { property.Name, innerException.Message });
            }
            catch (GetValueException)
            {
                throw;
            }
            catch (Exception exception3)
            {
                CommandProcessorBase.CheckForSevereException(exception3);
                throw new GetValueInvocationException("CatchFromBaseParameterizedPropertyAdapterGetValue", exception3, ExtendedTypeSystem.ExceptionWhenGetting, new object[] { property.Name, exception3.Message });
            }
            return obj2;
        }

        internal bool BaseParameterizedPropertyIsGettable(PSParameterizedProperty property)
        {
            bool flag;
            try
            {
                flag = this.ParameterizedPropertyIsGettable(property);
            }
            catch (ExtendedTypeSystemException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw NewException(exception, "CatchFromBaseParameterizedPropertyIsGettable", "CatchFromBaseParameterizedPropertyIsGettableTI", ExtendedTypeSystem.ExceptionRetrievingParameterizedPropertyReadState, new object[] { property.Name });
            }
            return flag;
        }

        internal bool BaseParameterizedPropertyIsSettable(PSParameterizedProperty property)
        {
            bool flag;
            try
            {
                flag = this.ParameterizedPropertyIsSettable(property);
            }
            catch (ExtendedTypeSystemException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw NewException(exception, "CatchFromBaseParameterizedPropertyIsSettable", "CatchFromBaseParameterizedPropertyIsSettableTI", ExtendedTypeSystem.ExceptionRetrievingParameterizedPropertyWriteState, new object[] { property.Name });
            }
            return flag;
        }

        internal void BaseParameterizedPropertySet(PSParameterizedProperty property, object setValue, params object[] arguments)
        {
            try
            {
                this.ParameterizedPropertySet(property, setValue, arguments);
            }
            catch (TargetInvocationException exception)
            {
                Exception innerException = (exception.InnerException == null) ? exception : exception.InnerException;
                throw new SetValueInvocationException("CatchFromBaseAdapterParameterizedPropertySetValueTI", innerException, ExtendedTypeSystem.ExceptionWhenSetting, new object[] { property.Name, innerException.Message });
            }
            catch (SetValueException)
            {
                throw;
            }
            catch (Exception exception3)
            {
                CommandProcessorBase.CheckForSevereException(exception3);
                throw new SetValueInvocationException("CatchFromBaseAdapterParameterizedPropertySetValue", exception3, ExtendedTypeSystem.ExceptionWhenSetting, new object[] { property.Name, exception3.Message });
            }
        }

        internal string BaseParameterizedPropertyToString(PSParameterizedProperty property)
        {
            string str;
            try
            {
                str = this.ParameterizedPropertyToString(property);
            }
            catch (ExtendedTypeSystemException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw NewException(exception, "CatchFromBaseParameterizedPropertyToString", "CatchFromBaseParameterizedPropertyToStringTI", ExtendedTypeSystem.ExceptionRetrievingParameterizedPropertyString, new object[] { property.Name });
            }
            return str;
        }

        internal string BaseParameterizedPropertyType(PSParameterizedProperty property)
        {
            string str;
            try
            {
                str = this.ParameterizedPropertyType(property);
            }
            catch (ExtendedTypeSystemException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw NewException(exception, "CatchFromBaseParameterizedPropertyType", "CatchFromBaseParameterizedPropertyTypeTI", ExtendedTypeSystem.ExceptionRetrievingParameterizedPropertytype, new object[] { property.Name });
            }
            return str;
        }

        internal AttributeCollection BasePropertyAttributes(PSProperty property)
        {
            AttributeCollection attributes;
            try
            {
                attributes = this.PropertyAttributes(property);
            }
            catch (ExtendedTypeSystemException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw NewException(exception, "CatchFromBasePropertyAttributes", "CatchFromBasePropertyAttributesTI", ExtendedTypeSystem.ExceptionRetrievingPropertyAttributes, new object[] { property.Name });
            }
            return attributes;
        }

        internal object BasePropertyGet(PSProperty property)
        {
            object obj2;
            try
            {
                obj2 = this.PropertyGet(property);
            }
            catch (TargetInvocationException exception)
            {
                Exception innerException = (exception.InnerException == null) ? exception : exception.InnerException;
                throw new GetValueInvocationException("CatchFromBaseAdapterGetValueTI", innerException, ExtendedTypeSystem.ExceptionWhenGetting, new object[] { property.Name, innerException.Message });
            }
            catch (GetValueException)
            {
                throw;
            }
            catch (Exception exception3)
            {
                CommandProcessorBase.CheckForSevereException(exception3);
                throw new GetValueInvocationException("CatchFromBaseAdapterGetValue", exception3, ExtendedTypeSystem.ExceptionWhenGetting, new object[] { property.Name, exception3.Message });
            }
            return obj2;
        }

        internal bool BasePropertyIsGettable(PSProperty property)
        {
            bool flag;
            try
            {
                flag = this.PropertyIsGettable(property);
            }
            catch (ExtendedTypeSystemException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw NewException(exception, "CatchFromBasePropertyIsGettable", "CatchFromBasePropertyIsGettableTI", ExtendedTypeSystem.ExceptionRetrievingPropertyReadState, new object[] { property.Name });
            }
            return flag;
        }

        internal bool BasePropertyIsSettable(PSProperty property)
        {
            bool flag;
            try
            {
                flag = this.PropertyIsSettable(property);
            }
            catch (ExtendedTypeSystemException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw NewException(exception, "CatchFromBasePropertyIsSettable", "CatchFromBasePropertyIsSettableTI", ExtendedTypeSystem.ExceptionRetrievingPropertyWriteState, new object[] { property.Name });
            }
            return flag;
        }

        internal void BasePropertySet(PSProperty property, object setValue, bool convert)
        {
            try
            {
                this.PropertySet(property, setValue, convert);
            }
            catch (TargetInvocationException exception)
            {
                Exception innerException = (exception.InnerException == null) ? exception : exception.InnerException;
                throw new SetValueInvocationException("CatchFromBaseAdapterSetValueTI", innerException, ExtendedTypeSystem.ExceptionWhenSetting, new object[] { property.Name, innerException.Message });
            }
            catch (SetValueException)
            {
                throw;
            }
            catch (Exception exception3)
            {
                CommandProcessorBase.CheckForSevereException(exception3);
                throw new SetValueInvocationException("CatchFromBaseAdapterSetValue", exception3, ExtendedTypeSystem.ExceptionWhenSetting, new object[] { property.Name, exception3.Message });
            }
        }

        internal string BasePropertyToString(PSProperty property)
        {
            string str;
            try
            {
                str = this.PropertyToString(property);
            }
            catch (ExtendedTypeSystemException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw NewException(exception, "CatchFromBasePropertyToString", "CatchFromBasePropertyToStringTI", ExtendedTypeSystem.ExceptionRetrievingPropertyString, new object[] { property.Name });
            }
            return str;
        }

        internal string BasePropertyType(PSProperty property)
        {
            string str;
            try
            {
                str = this.PropertyType(property, false);
            }
            catch (ExtendedTypeSystemException)
            {
                throw;
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw NewException(exception, "CatchFromBasePropertyType", "CatchFromBasePropertyTypeTI", ExtendedTypeSystem.ExceptionRetrievingPropertyType, new object[] { property.Name });
            }
            return str;
        }

        internal static void CacheMethod(MethodInformation mi, object target, string methodName, PSMethodInvocationConstraints invocationConstraints, object[] arguments, CallsiteCacheEntryFlags flags)
        {
            Type type;
            if ((flags & (CallsiteCacheEntryFlags.Constructor | CallsiteCacheEntryFlags.Static)) != CallsiteCacheEntryFlags.None)
            {
                type = (Type) target;
            }
            else
            {
                type = target.GetType();
            }
            if ((type != typeof(PSObject)) && (type != typeof(PSCustomObject)))
            {
                CallsiteSignature signature = new CallsiteSignature(type, invocationConstraints, arguments, flags);
                CallsiteCacheEntry key = new CallsiteCacheEntry(methodName, signature);
                lock (callsiteCache)
                {
                    if (!callsiteCache.ContainsKey(key))
                    {
                        if (callsiteCache.Count > 0x800)
                        {
                            callsiteCache.Clear();
                        }
                        callsiteCache[key] = mi;
                    }
                }
            }
        }

        private static int CompareOverloadCandidates(OverloadCandidate candidate1, OverloadCandidate candidate2, object[] arguments)
        {
            ParameterInformation[] informationArray = candidate1.expandedParameters ?? candidate1.parameters;
            ParameterInformation[] informationArray2 = candidate2.expandedParameters ?? candidate2.parameters;
            int num = 0;
            int length = informationArray.Length;
            int index = 0;
            while (index < informationArray.Length)
            {
                if (candidate1.conversionRanks[index] < candidate2.conversionRanks[index])
                {
                    num -= length;
                }
                else if (candidate1.conversionRanks[index] > candidate2.conversionRanks[index])
                {
                    num += length;
                }
                else if (candidate1.conversionRanks[index] == ConversionRank.UnrelatedArrays)
                {
                    Type elementType = EffectiveArgumentType(arguments[index]).GetElementType();
                    ConversionRank conversionRank = LanguagePrimitives.GetConversionRank(elementType, informationArray[index].parameterType.GetElementType());
                    ConversionRank rank2 = LanguagePrimitives.GetConversionRank(elementType, informationArray2[index].parameterType.GetElementType());
                    if (conversionRank < rank2)
                    {
                        num -= length;
                    }
                    else if (conversionRank > rank2)
                    {
                        num += length;
                    }
                }
                index++;
                length--;
            }
            if (num == 0)
            {
                length = informationArray.Length;
                int num4 = 0;
                while (num4 < informationArray.Length)
                {
                    ConversionRank rank3 = candidate1.conversionRanks[num4];
                    ConversionRank rank4 = candidate2.conversionRanks[num4];
                    if (((rank3 >= ConversionRank.NullToValue) && (rank4 >= ConversionRank.NullToValue)) && ((rank3 >= ConversionRank.NumericImplicit) == (rank4 >= ConversionRank.NumericImplicit)))
                    {
                        if (rank3 >= ConversionRank.NumericImplicit)
                        {
                            length = -length;
                        }
                        rank3 = LanguagePrimitives.GetConversionRank(informationArray[num4].parameterType, informationArray2[num4].parameterType);
                        rank4 = LanguagePrimitives.GetConversionRank(informationArray2[num4].parameterType, informationArray[num4].parameterType);
                        if (rank3 < rank4)
                        {
                            num += length;
                        }
                        else if (rank3 > rank4)
                        {
                            num -= length;
                        }
                    }
                    num4++;
                    length = Math.Abs(length) - 1;
                }
            }
            if (num == 0)
            {
                for (int i = 0; i < informationArray.Length; i++)
                {
                    if (!informationArray[i].parameterType.Equals(informationArray2[i].parameterType))
                    {
                        return 0;
                    }
                }
            }
            if (num == 0)
            {
                if ((candidate1.expandedParameters != null) && (candidate2.expandedParameters != null))
                {
                    if (candidate1.parameters.Length <= candidate2.parameters.Length)
                    {
                        return -1;
                    }
                    return 1;
                }
                if (candidate1.expandedParameters != null)
                {
                    return -1;
                }
                if (candidate2.expandedParameters != null)
                {
                    return 1;
                }
            }
            if (num == 0)
            {
                num = CompareTypeSpecificity(candidate1, candidate2);
            }
            return num;
        }

        private static int CompareTypeSpecificity(Type[] params1, Type[] params2)
        {
            bool flag = false;
            bool flag2 = false;
            for (int i = 0; i < params1.Length; i++)
            {
                int num2 = CompareTypeSpecificity(params1[i], params2[i]);
                if (num2 > 0)
                {
                    flag = true;
                }
                else if (num2 < 0)
                {
                    flag2 = true;
                }
                if (flag && flag2)
                {
                    break;
                }
            }
            if (flag && !flag2)
            {
                return 1;
            }
            if (flag2 && !flag)
            {
                return -1;
            }
            return 0;
        }

        private static int CompareTypeSpecificity(OverloadCandidate candidate1, OverloadCandidate candidate2)
        {
            if (!candidate1.method.isGeneric && !candidate2.method.isGeneric)
            {
                return 0;
            }
            Type[] typeArray = (from p in GetGenericMethodDefinitionIfPossible(candidate1.method.method).GetParameters() select p.ParameterType).ToArray<Type>();
            Type[] typeArray2 = (from p in GetGenericMethodDefinitionIfPossible(candidate2.method.method).GetParameters() select p.ParameterType).ToArray<Type>();
            return CompareTypeSpecificity(typeArray, typeArray2);
        }

        private static int CompareTypeSpecificity(Type type1, Type type2)
        {
            if (type1.IsGenericParameter || type2.IsGenericParameter)
            {
                int num = 0;
                if (type1.IsGenericParameter)
                {
                    num--;
                }
                if (type2.IsGenericParameter)
                {
                    num++;
                }
                return num;
            }
            if (type1.IsArray)
            {
                return CompareTypeSpecificity(type1.GetElementType(), type2.GetElementType());
            }
            if (type1.IsGenericType)
            {
                return CompareTypeSpecificity(type1.GetGenericArguments(), type2.GetGenericArguments());
            }
            return 0;
        }

        internal static void DoBoxingIfNecessary(ILGenerator generator, Type type)
        {
            if (type.IsByRef)
            {
                type = type.GetElementType();
                if (type.IsPrimitive)
                {
                    if (type.Equals(typeof(byte)))
                    {
                        generator.Emit(OpCodes.Ldind_U1);
                    }
                    else if (type.Equals(typeof(ushort)))
                    {
                        generator.Emit(OpCodes.Ldind_U2);
                    }
                    else if (type.Equals(typeof(int)))
                    {
                        generator.Emit(OpCodes.Ldind_U4);
                    }
                    else if (type.Equals(typeof(sbyte)))
                    {
                        generator.Emit(OpCodes.Ldind_I8);
                    }
                    else if (type.Equals(typeof(short)))
                    {
                        generator.Emit(OpCodes.Ldind_I2);
                    }
                    else if (type.Equals(typeof(int)))
                    {
                        generator.Emit(OpCodes.Ldind_I4);
                    }
                    else if (type.Equals(typeof(long)))
                    {
                        generator.Emit(OpCodes.Ldind_I8);
                    }
                    else if (type.Equals(typeof(float)))
                    {
                        generator.Emit(OpCodes.Ldind_R4);
                    }
                    else if (type.Equals(typeof(double)))
                    {
                        generator.Emit(OpCodes.Ldind_R8);
                    }
                }
                else if (type.IsValueType)
                {
                    generator.Emit(OpCodes.Ldobj, type);
                }
                else
                {
                    generator.Emit(OpCodes.Ldind_Ref);
                }
            }
            else if (type.IsPointer)
            {
                MethodInfo method = typeof(Pointer).GetMethod("Box");
                MethodInfo meth = typeof(Type).GetMethod("GetTypeFromHandle");
                generator.Emit(OpCodes.Ldtoken, type);
                generator.Emit(OpCodes.Call, meth);
                generator.Emit(OpCodes.Call, method);
            }
            if (type.IsValueType)
            {
                generator.Emit(OpCodes.Box, type);
            }
        }

        internal static Type EffectiveArgumentType(object arg)
        {
            if (arg == null)
            {
                return typeof(LanguagePrimitives.Null);
            }
            arg = PSObject.Base(arg);
            object[] objArray = arg as object[];
            if (((objArray != null) && (objArray.Length > 0)) && (PSObject.Base(objArray[0]) != null))
            {
                Type type = PSObject.Base(objArray[0]).GetType();
                bool flag = true;
                for (int i = 1; i < objArray.Length; i++)
                {
                    if ((objArray[i] == null) || !type.Equals(PSObject.Base(objArray[i]).GetType()))
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag)
                {
                    return type.MakeArrayType();
                }
            }
            return arg.GetType();
        }

        private static ParameterInformation[] ExpandParameters(int argCount, ParameterInformation[] parameters, Type elementType)
        {
            ParameterInformation[] destinationArray = new ParameterInformation[argCount];
            ParameterInformation information1 = parameters[parameters.Length - 1];
            Array.Copy(parameters, destinationArray, (int) (parameters.Length - 1));
            for (int i = parameters.Length - 1; i < argCount; i++)
            {
                destinationArray[i] = new ParameterInformation(elementType, false, null, false);
            }
            return destinationArray;
        }

        private static OverloadCandidate FindBestCandidate(IEnumerable<OverloadCandidate> candidates, object[] arguments)
        {
            OverloadCandidate candidate = null;
            bool flag = false;
            foreach (OverloadCandidate candidate2 in candidates)
            {
                if (candidate == null)
                {
                    candidate = candidate2;
                }
                else
                {
                    int num = CompareOverloadCandidates(candidate, candidate2, arguments);
                    if (num == 0)
                    {
                        flag = true;
                    }
                    else if (num < 0)
                    {
                        candidate = candidate2;
                        flag = false;
                    }
                }
            }
            if (!flag)
            {
                return candidate;
            }
            return null;
        }

        private static OverloadCandidate FindBestCandidate(IEnumerable<OverloadCandidate> candidates, object[] arguments, PSMethodInvocationConstraints invocationConstraints)
        {
            List<OverloadCandidate> list = (from candidate in candidates
                where IsInvocationConstraintSatisfied(candidate, invocationConstraints)
                select candidate).ToList<OverloadCandidate>();
            if (list.Count > 0)
            {
                candidates = list;
            }
            return FindBestCandidate(candidates, arguments);
        }

        internal static MethodInformation FindBestMethod(MethodInformation[] methods, PSMethodInvocationConstraints invocationConstraints, object[] arguments, ref string errorId, ref string errorMsg, out bool expandParamsOnBest)
        {
            OverloadCandidate candidate2;
            expandParamsOnBest = false;
            if (((((methods.Length == 1) && !methods[0].hasVarArgs) && !methods[0].isGeneric) && ((methods[0].method == null) || !methods[0].method.DeclaringType.IsGenericTypeDefinition)) && (methods[0].parameters.Length == arguments.Length))
            {
                return methods[0];
            }
            Type[] sourceArray = arguments.Select<object, Type>(new Func<object, Type>(Adapter.EffectiveArgumentType)).ToArray<Type>();
            List<OverloadCandidate> candidates = new List<OverloadCandidate>();
            for (int i = 0; i < methods.Length; i++)
            {
                OverloadCandidate candidate;
                MethodInformation genericMethod = methods[i];
                if ((genericMethod.method != null) && genericMethod.method.DeclaringType.IsGenericTypeDefinition)
                {
                    continue;
                }
                if (genericMethod.isGeneric)
                {
                    Type[] destinationArray = new Type[sourceArray.Length];
                    Array.Copy(sourceArray, destinationArray, sourceArray.Length);
                    if ((invocationConstraints != null) && (invocationConstraints.ParameterTypes != null))
                    {
                        int index = 0;
                        foreach (Type type in invocationConstraints.ParameterTypes)
                        {
                            if (type != null)
                            {
                                destinationArray[index] = type;
                            }
                            index++;
                        }
                    }
                    genericMethod = TypeInference.Infer(genericMethod, destinationArray);
                    if (genericMethod == null)
                    {
                        continue;
                    }
                }
                if (!IsInvocationTargetConstraintSatisfied(genericMethod, invocationConstraints))
                {
                    continue;
                }
                ParameterInformation[] parameters = genericMethod.parameters;
                if (arguments.Length != parameters.Length)
                {
                    if (arguments.Length > parameters.Length)
                    {
                        if (genericMethod.hasVarArgs)
                        {
                            goto Label_01B4;
                        }
                        continue;
                    }
                    if (!genericMethod.hasOptional && (!genericMethod.hasVarArgs || ((arguments.Length + 1) != parameters.Length)))
                    {
                        continue;
                    }
                    if (genericMethod.hasOptional)
                    {
                        int num3 = 0;
                        for (int k = 0; k < parameters.Length; k++)
                        {
                            if (parameters[k].isOptional)
                            {
                                num3++;
                            }
                        }
                        if ((arguments.Length + num3) < parameters.Length)
                        {
                            continue;
                        }
                    }
                }
            Label_01B4:
                candidate = new OverloadCandidate(genericMethod, arguments.Length);
                for (int j = 0; (candidate != null) && (j < parameters.Length); j++)
                {
                    ParameterInformation information2 = parameters[j];
                    if (information2.isOptional && (arguments.Length <= j))
                    {
                        break;
                    }
                    if (information2.isParamArray)
                    {
                        Type elementType = information2.parameterType.GetElementType();
                        if (parameters.Length == arguments.Length)
                        {
                            ConversionRank argumentConversionRank = GetArgumentConversionRank(arguments[j], information2.parameterType);
                            ConversionRank rank2 = GetArgumentConversionRank(arguments[j], elementType);
                            if (rank2 > argumentConversionRank)
                            {
                                candidate.expandedParameters = ExpandParameters(arguments.Length, parameters, elementType);
                                candidate.conversionRanks[j] = rank2;
                            }
                            else
                            {
                                candidate.conversionRanks[j] = argumentConversionRank;
                            }
                            if (candidate.conversionRanks[j] == ConversionRank.None)
                            {
                                candidate = null;
                            }
                            continue;
                        }
                        for (int n = j; n < arguments.Length; n++)
                        {
                            candidate.conversionRanks[n] = GetArgumentConversionRank(arguments[n], elementType);
                            if (candidate.conversionRanks[n] == ConversionRank.None)
                            {
                                candidate = null;
                                break;
                            }
                        }
                        if (candidate != null)
                        {
                            candidate.expandedParameters = ExpandParameters(arguments.Length, parameters, elementType);
                        }
                        continue;
                    }
                    candidate.conversionRanks[j] = GetArgumentConversionRank(arguments[j], information2.parameterType);
                    if (candidate.conversionRanks[j] == ConversionRank.None)
                    {
                        candidate = null;
                    }
                }
                if (candidate != null)
                {
                    candidates.Add(candidate);
                }
            }
            if (candidates.Count == 0)
            {
                if ((methods.Length > 0) && methods.All<MethodInformation>(m => (((m.method != null) && m.method.DeclaringType.IsGenericTypeDefinition) && m.method.IsStatic)))
                {
                    errorId = "CannotInvokeStaticMethodOnUninstantiatedGenericType";
                    errorMsg = string.Format(CultureInfo.InvariantCulture, ExtendedTypeSystem.CannotInvokeStaticMethodOnUninstantiatedGenericType, new object[] { methods[0].method.DeclaringType.FullName });
                    return null;
                }
                errorId = "MethodCountCouldNotFindBest";
                errorMsg = ExtendedTypeSystem.MethodArgumentCountException;
                return null;
            }
            if (candidates.Count == 1)
            {
                candidate2 = candidates[0];
            }
            else
            {
                candidate2 = FindBestCandidate(candidates, arguments, invocationConstraints);
            }
            if (candidate2 != null)
            {
                expandParamsOnBest = candidate2.expandedParameters != null;
                return candidate2.method;
            }
            errorId = "MethodCountCouldNotFindBest";
            errorMsg = ExtendedTypeSystem.MethodAmbiguousException;
            return null;
        }

        internal static MethodInformation FindCachedMethod(Type targetType, string methodName, PSMethodInvocationConstraints invocationConstraints, object[] arguments, CallsiteCacheEntryFlags flags)
        {
            if ((targetType == typeof(PSObject)) || (targetType == typeof(PSCustomObject)))
            {
                return null;
            }
            CallsiteSignature signature = new CallsiteSignature(targetType, invocationConstraints, arguments, flags);
            CallsiteCacheEntry key = new CallsiteCacheEntry(methodName, signature);
            MethodInformation information = null;
            lock (callsiteCache)
            {
                callsiteCache.TryGetValue(key, out information);
            }
            return information;
        }

        internal static ConversionRank GetArgumentConversionRank(object argument, Type parameterType)
        {
            ConversionRank conversionRank = LanguagePrimitives.GetConversionRank(GetArgumentType(argument), parameterType);
            if (conversionRank == ConversionRank.None)
            {
                conversionRank = LanguagePrimitives.GetConversionRank(GetArgumentType(PSObject.Base(argument)), parameterType);
            }
            return conversionRank;
        }

        private static Type GetArgumentType(object argument)
        {
            if (argument == null)
            {
                return typeof(LanguagePrimitives.Null);
            }
            PSReference reference = argument as PSReference;
            if (reference != null)
            {
                return GetArgumentType(PSObject.Base(reference.Value));
            }
            return argument.GetType();
        }

        internal static MethodInformation GetBestMethodAndArguments(string methodName, MethodInformation[] methods, object[] arguments, out object[] newArguments)
        {
            return GetBestMethodAndArguments(methodName, methods, null, arguments, out newArguments);
        }

        internal static MethodInformation GetBestMethodAndArguments(string methodName, MethodInformation[] methods, PSMethodInvocationConstraints invocationConstraints, object[] arguments, out object[] newArguments)
        {
            bool flag;
            string errorId = null;
            string errorMsg = null;
            MethodInformation information = FindBestMethod(methods, invocationConstraints, arguments, ref errorId, ref errorMsg, out flag);
            if (information == null)
            {
                throw new MethodException(errorId, null, errorMsg, new object[] { methodName, arguments.Length });
            }
            newArguments = GetMethodArgumentsBase(methodName, information.parameters, arguments, flag);
            return information;
        }

        protected static IEnumerable<string> GetDotNetTypeNameHierarchy(object obj)
        {
            return GetDotNetTypeNameHierarchy(obj.GetType());
        }

        protected static IEnumerable<string> GetDotNetTypeNameHierarchy(Type type)
        {
            while (true)
            {
                if (type == null)
                {
                    yield break;
                }
                yield return type.FullName;
                type = type.BaseType;
            }
        }

        private static MethodBase GetGenericMethodDefinitionIfPossible(MethodBase method)
        {
            if (method.IsGenericMethod && !method.IsGenericMethodDefinition)
            {
                MethodInfo info = method as MethodInfo;
                if (info != null)
                {
                    return info.GetGenericMethodDefinition();
                }
            }
            return method;
        }

        protected virtual ConsolidatedString GetInternedTypeNameHierarchy(object obj)
        {
            return new ConsolidatedString(this.GetTypeNameHierarchy(obj));
        }

        protected abstract T GetMember<T>(object obj, string memberName) where T: PSMemberInfo;
        protected abstract PSMemberInfoInternalCollection<T> GetMembers<T>(object obj) where T: PSMemberInfo;
        internal static object[] GetMethodArgumentsBase(string methodName, ParameterInformation[] parameters, object[] arguments, bool expandParamsOnBest)
        {
            int length = parameters.Length;
            if (length == 0)
            {
                return new object[0];
            }
            object[] newArguments = new object[length];
            for (int i = 0; i < (length - 1); i++)
            {
                ParameterInformation information = parameters[i];
                SetNewArgument(methodName, arguments, newArguments, information, i);
            }
            ParameterInformation parameter = parameters[length - 1];
            if (!expandParamsOnBest)
            {
                SetNewArgument(methodName, arguments, newArguments, parameter, length - 1);
                return newArguments;
            }
            if (arguments.Length < length)
            {
                newArguments[length - 1] = new ArrayList().ToArray(parameter.parameterType.GetElementType());
                return newArguments;
            }
            int num3 = (arguments.Length - length) + 1;
            if ((num3 == 1) && (arguments[arguments.Length - 1] == null))
            {
                newArguments[length - 1] = null;
                return newArguments;
            }
            object[] valueToConvert = new object[num3];
            Type elementType = parameter.parameterType.GetElementType();
            for (int j = 0; j < num3; j++)
            {
                int parameterIndex = (j + length) - 1;
                try
                {
                    valueToConvert[j] = MethodArgumentConvertTo(arguments[parameterIndex], false, parameterIndex, elementType, CultureInfo.InvariantCulture);
                }
                catch (InvalidCastException exception)
                {
                    throw new MethodException("MethodArgumentConversionInvalidCastArgument", exception, ExtendedTypeSystem.MethodArgumentConversionException, new object[] { parameterIndex, arguments[parameterIndex], methodName, elementType, exception.Message });
                }
            }
            try
            {
                newArguments[length - 1] = MethodArgumentConvertTo(valueToConvert, parameter.isByRef, length - 1, parameter.parameterType, CultureInfo.InvariantCulture);
            }
            catch (InvalidCastException exception2)
            {
                throw new MethodException("MethodArgumentConversionParamsConversion", exception2, ExtendedTypeSystem.MethodArgumentConversionException, new object[] { length - 1, valueToConvert, methodName, parameter.parameterType, exception2.Message });
            }
            return newArguments;
        }

        protected virtual IEnumerable<string> GetTypeNameHierarchy(object obj)
        {
            return GetDotNetTypeNameHierarchy(obj);
        }

        private static bool IsInvocationConstraintSatisfied(OverloadCandidate overloadCandidate, PSMethodInvocationConstraints invocationConstraints)
        {
            if ((invocationConstraints != null) && (invocationConstraints.ParameterTypes != null))
            {
                int index = 0;
                foreach (Type type in invocationConstraints.ParameterTypes)
                {
                    if (type != null)
                    {
                        if (index >= overloadCandidate.parameters.Length)
                        {
                            return false;
                        }
                        if (!overloadCandidate.parameters[index].parameterType.Equals(type))
                        {
                            return false;
                        }
                    }
                    index++;
                }
            }
            return true;
        }

        private static bool IsInvocationTargetConstraintSatisfied(MethodInformation method, PSMethodInvocationConstraints invocationConstraints)
        {
            if (method.method == null)
            {
                return true;
            }
            Type declaringType = method.method.DeclaringType;
            if ((invocationConstraints == null) || (invocationConstraints.MethodTargetType == null))
            {
                return !declaringType.IsInterface;
            }
            Type methodTargetType = invocationConstraints.MethodTargetType;
            if (methodTargetType.IsInterface)
            {
                return declaringType.Equals(methodTargetType);
            }
            if (declaringType.IsInterface)
            {
                return false;
            }
            return declaringType.IsAssignableFrom(methodTargetType);
        }

        internal static object MethodArgumentConvertTo(object valueToConvert, bool isParameterByRef, int parameterIndex, Type resultType, IFormatProvider formatProvider)
        {
            using (PSObject.memberResolution.TraceScope("Method argument conversion.", new object[0]))
            {
                bool flag;
                if (resultType == null)
                {
                    throw PSTraceSource.NewArgumentNullException("resultType");
                }
                valueToConvert = UnReference(valueToConvert, out flag);
                if (isParameterByRef && !flag)
                {
                    throw new MethodException("NonRefArgumentToRefParameterMsg", null, ExtendedTypeSystem.NonRefArgumentToRefParameter, new object[] { parameterIndex + 1, typeof(PSReference).FullName, "[ref]" });
                }
                if (flag && !isParameterByRef)
                {
                    throw new MethodException("RefArgumentToNonRefParameterMsg", null, ExtendedTypeSystem.RefArgumentToNonRefParameter, new object[] { parameterIndex + 1, typeof(PSReference).FullName, "[ref]" });
                }
                return PropertySetAndMethodArgumentConvertTo(valueToConvert, resultType, formatProvider);
            }
        }

        protected abstract Collection<string> MethodDefinitions(PSMethod method);
        protected abstract object MethodInvoke(PSMethod method, object[] arguments);
        protected virtual object MethodInvoke(PSMethod method, PSMethodInvocationConstraints invocationConstraints, object[] arguments)
        {
            return this.MethodInvoke(method, arguments);
        }

        protected virtual string MethodToString(PSMethod method)
        {
            StringBuilder builder = new StringBuilder();
            foreach (string str in this.MethodDefinitions(method))
            {
                builder.Append(str);
                builder.Append(", ");
            }
            builder.Remove(builder.Length - 2, 2);
            return builder.ToString();
        }

        private static Exception NewException(Exception e, string errorId, string targetErrorId, string resourceString, params object[] parameters)
        {
            object[] arguments = new object[parameters.Length + 1];
            for (int i = 0; i < parameters.Length; i++)
            {
                arguments[i + 1] = parameters[i];
            }
            Exception exception = e as TargetInvocationException;
            if (exception != null)
            {
                Exception innerException = (exception.InnerException == null) ? exception : exception.InnerException;
                arguments[0] = innerException.Message;
                return new ExtendedTypeSystemException(targetErrorId, innerException, resourceString, arguments);
            }
            arguments[0] = e.Message;
            return new ExtendedTypeSystemException(errorId, e, resourceString, arguments);
        }

        protected virtual Collection<string> ParameterizedPropertyDefinitions(PSParameterizedProperty property)
        {
            throw PSTraceSource.NewNotSupportedException();
        }

        protected virtual object ParameterizedPropertyGet(PSParameterizedProperty property, object[] arguments)
        {
            throw PSTraceSource.NewNotSupportedException();
        }

        protected virtual bool ParameterizedPropertyIsGettable(PSParameterizedProperty property)
        {
            throw PSTraceSource.NewNotSupportedException();
        }

        protected virtual bool ParameterizedPropertyIsSettable(PSParameterizedProperty property)
        {
            throw PSTraceSource.NewNotSupportedException();
        }

        protected virtual void ParameterizedPropertySet(PSParameterizedProperty property, object setValue, object[] arguments)
        {
            throw PSTraceSource.NewNotSupportedException();
        }

        protected virtual string ParameterizedPropertyToString(PSParameterizedProperty property)
        {
            throw PSTraceSource.NewNotSupportedException();
        }

        protected virtual string ParameterizedPropertyType(PSParameterizedProperty property)
        {
            throw PSTraceSource.NewNotSupportedException();
        }

        protected abstract AttributeCollection PropertyAttributes(PSProperty property);
        protected abstract object PropertyGet(PSProperty property);
        protected abstract bool PropertyIsGettable(PSProperty property);
        protected abstract bool PropertyIsSettable(PSProperty property);
        protected abstract void PropertySet(PSProperty property, object setValue, bool convertIfPossible);
        internal static object PropertySetAndMethodArgumentConvertTo(object valueToConvert, Type resultType, IFormatProvider formatProvider)
        {
            using (PSObject.memberResolution.TraceScope("Converting parameter \"{0}\" to \"{1}\".", new object[] { valueToConvert, resultType }))
            {
                if (resultType == null)
                {
                    throw PSTraceSource.NewArgumentNullException("resultType");
                }
                PSObject obj2 = valueToConvert as PSObject;
                if ((obj2 != null) && resultType.Equals(typeof(object)))
                {
                    PSObject.memberResolution.WriteLine("Parameter was an PSObject and will be converted to System.Object.", new object[0]);
                    return PSObject.Base(obj2);
                }
                return LanguagePrimitives.ConvertTo(valueToConvert, resultType, formatProvider);
            }
        }

        protected abstract string PropertyToString(PSProperty property);
        protected abstract string PropertyType(PSProperty property, bool forDisplay);
        internal static void ResetCaches()
        {
            lock (callsiteCache)
            {
                callsiteCache.Clear();
            }
        }

        internal static void SetNewArgument(string methodName, object[] arguments, object[] newArguments, ParameterInformation parameter, int index)
        {
            if (arguments.Length > index)
            {
                try
                {
                    newArguments[index] = MethodArgumentConvertTo(arguments[index], parameter.isByRef, index, parameter.parameterType, CultureInfo.InvariantCulture);
                    return;
                }
                catch (InvalidCastException exception)
                {
                    throw new MethodException("MethodArgumentConversionInvalidCastArgument", exception, ExtendedTypeSystem.MethodArgumentConversionException, new object[] { index, arguments[index], methodName, parameter.parameterType, exception.Message });
                }
            }
            newArguments[index] = parameter.defaultValue;
        }

        internal static void SetReferences(object[] arguments, MethodInformation methodInformation, object[] originalArguments)
        {
            using (PSObject.memberResolution.TraceScope("Checking for possible references.", new object[0]))
            {
                ParameterInformation[] parameters = methodInformation.parameters;
                for (int i = 0; ((i < originalArguments.Length) && (i < parameters.Length)) && (i < arguments.Length); i++)
                {
                    object obj2 = originalArguments[i];
                    PSReference baseObject = obj2 as PSReference;
                    if (baseObject == null)
                    {
                        PSObject obj3 = obj2 as PSObject;
                        if (obj3 == null)
                        {
                            continue;
                        }
                        baseObject = obj3.BaseObject as PSReference;
                        if (baseObject == null)
                        {
                            continue;
                        }
                    }
                    ParameterInformation information = parameters[i];
                    if (information.isByRef)
                    {
                        object obj4 = arguments[i];
                        PSObject.memberResolution.WriteLine("Argument '{0}' was a reference so it will be set to \"{1}\".", new object[] { i + 1, obj4 });
                        baseObject.Value = obj4;
                    }
                }
            }
        }

        internal static object UnReference(object obj, out bool isArgumentByRef)
        {
            isArgumentByRef = false;
            PSReference baseObject = obj as PSReference;
            if (baseObject != null)
            {
                PSObject.memberResolution.WriteLine("Parameter was a reference.", new object[0]);
                isArgumentByRef = true;
                return baseObject.Value;
            }
            PSObject obj2 = obj as PSObject;
            if (obj2 != null)
            {
                baseObject = obj2.BaseObject as PSReference;
            }
            if (baseObject != null)
            {
                PSObject.memberResolution.WriteLine("Parameter was an PSObject containing a reference.", new object[0]);
                isArgumentByRef = true;
                return baseObject.Value;
            }
            return obj;
        }

        internal virtual bool SiteBinderCanOptimize
        {
            get
            {
                return false;
            }
        }

        

        [DebuggerDisplay("OverloadCandidate: {method.methodDefinition}")]
        private class OverloadCandidate
        {
            internal ConversionRank[] conversionRanks;
            internal ParameterInformation[] expandedParameters;
            internal MethodInformation method;
            internal ParameterInformation[] parameters;

            internal OverloadCandidate(MethodInformation method, int argCount)
            {
                this.method = method;
                this.parameters = method.parameters;
                this.conversionRanks = new ConversionRank[argCount];
            }
        }
    }
}

