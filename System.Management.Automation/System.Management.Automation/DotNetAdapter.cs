namespace System.Management.Automation
{
    using Microsoft.PowerShell;
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Dynamic;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Runspaces;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;

    internal class DotNetAdapter : Adapter
    {
        private const BindingFlags instanceBindingFlags = (BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        private static HybridDictionary instanceEventCacheTable = new HybridDictionary();
        private static HybridDictionary instanceMethodCacheTable = new HybridDictionary();
        private static HybridDictionary instancePropertyCacheTable = new HybridDictionary();
        private bool isStatic;
        private const BindingFlags staticBindingFlags = (BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);
        private static HybridDictionary staticEventCacheTable = new HybridDictionary();
        private static HybridDictionary staticMethodCacheTable = new HybridDictionary();
        private static HybridDictionary staticPropertyCacheTable = new HybridDictionary();
        private static ConcurrentDictionary<Type, ConsolidatedString> typeToTypeNameDictionary = new ConcurrentDictionary<Type, ConsolidatedString>();

        internal DotNetAdapter()
        {
        }

        internal DotNetAdapter(bool isStatic)
        {
            this.isStatic = isStatic;
        }

        internal void AddAllDynamicMembers<T>(object obj, PSMemberInfoInternalCollection<T> members, bool ignoreDuplicates) where T: PSMemberInfo
        {
            IDynamicMetaObjectProvider provider = obj as IDynamicMetaObjectProvider;
            if (((provider != null) && !(obj is PSObject)) && typeof(T).IsAssignableFrom(typeof(PSDynamicMember)))
            {
                foreach (string str in provider.GetMetaObject(Expression.Variable(provider.GetType())).GetDynamicMemberNames())
                {
                    members.Add(new PSDynamicMember(str) as T);
                }
            }
        }

        internal void AddAllEvents<T>(object obj, PSMemberInfoInternalCollection<T> members, bool ignoreDuplicates) where T: PSMemberInfo
        {
            if (typeof(T).IsAssignableFrom(typeof(PSEvent)))
            {
                CacheTable staticEventReflectionTable;
                if (this.isStatic)
                {
                    staticEventReflectionTable = GetStaticEventReflectionTable(obj);
                }
                else
                {
                    staticEventReflectionTable = GetInstanceEventReflectionTable(obj);
                }
                foreach (EventCacheEntry entry in staticEventReflectionTable.memberCollection)
                {
                    if (!ignoreDuplicates || (members[entry.events[0].Name] == null))
                    {
                        members.Add(new PSEvent(entry.events[0]) as T);
                    }
                }
            }
        }

        internal void AddAllMethods<T>(object obj, PSMemberInfoInternalCollection<T> members, bool ignoreDuplicates) where T: PSMemberInfo
        {
            if (typeof(T).IsAssignableFrom(typeof(PSMethod)))
            {
                CacheTable staticMethodReflectionTable;
                if (this.isStatic)
                {
                    staticMethodReflectionTable = GetStaticMethodReflectionTable(obj as Type);
                }
                else
                {
                    staticMethodReflectionTable = GetInstanceMethodReflectionTable(obj.GetType());
                }
                foreach (MethodCacheEntry entry in staticMethodReflectionTable.memberCollection)
                {
                    string name = entry[0].method.Name;
                    if (!ignoreDuplicates || (members[name] == null))
                    {
                        bool isSpecialName = entry[0].method.IsSpecialName;
                        members.Add(new PSMethod(name, this, obj, entry, isSpecialName) as T);
                    }
                }
            }
        }

        internal void AddAllProperties<T>(object obj, PSMemberInfoInternalCollection<T> members, bool ignoreDuplicates) where T: PSMemberInfo
        {
            bool flag = typeof(T).IsAssignableFrom(typeof(PSProperty));
            bool flag2 = IsTypeParameterizedProperty(typeof(T));
            if (flag || flag2)
            {
                CacheTable staticPropertyReflectionTable;
                if (this.isStatic)
                {
                    staticPropertyReflectionTable = GetStaticPropertyReflectionTable(obj as Type);
                }
                else
                {
                    staticPropertyReflectionTable = GetInstancePropertyReflectionTable(obj.GetType());
                }
                foreach (object obj2 in staticPropertyReflectionTable.memberCollection)
                {
                    PropertyCacheEntry adapterData = obj2 as PropertyCacheEntry;
                    if (adapterData != null)
                    {
                        if (flag && (!ignoreDuplicates || (members[adapterData.member.Name] == null)))
                        {
                            members.Add(new PSProperty(adapterData.member.Name, this, obj, adapterData) as T);
                        }
                    }
                    else if (flag2)
                    {
                        ParameterizedPropertyCacheEntry entry2 = (ParameterizedPropertyCacheEntry) obj2;
                        if (!ignoreDuplicates || (members[entry2.propertyName] == null))
                        {
                            members.Add(new PSParameterizedProperty(entry2.propertyName, this, obj, entry2) as T);
                        }
                    }
                }
            }
        }

        private static void AddOverload(ArrayList previousMethodEntry, MethodInfo method)
        {
            bool flag = true;
            foreach (object obj2 in previousMethodEntry)
            {
                if (SameSignature((MethodInfo) obj2, method))
                {
                    flag = false;
                    break;
                }
            }
            if (flag)
            {
                previousMethodEntry.Add(method);
            }
        }

        internal static object AuxiliaryConstructorInvoke(MethodInformation methodInformation, object[] arguments, object[] originalArguments)
        {
            object obj2;
            try
            {
                obj2 = ((ConstructorInfo) methodInformation.method).Invoke(arguments);
            }
            catch (TargetInvocationException exception)
            {
                Exception innerException = (exception.InnerException == null) ? exception : exception.InnerException;
                throw new MethodInvocationException("DotNetconstructorTargetInvocation", innerException, ExtendedTypeSystem.MethodInvocationException, new object[] { ".ctor", arguments.Length, innerException.Message });
            }
            catch (Exception exception3)
            {
                CommandProcessorBase.CheckForSevereException(exception3);
                throw new MethodInvocationException("DotNetconstructorException", exception3, ExtendedTypeSystem.MethodInvocationException, new object[] { ".ctor", arguments.Length, exception3.Message });
            }
            Adapter.SetReferences(arguments, methodInformation, originalArguments);
            return obj2;
        }

        internal static object AuxiliaryMethodInvoke(object target, object[] arguments, MethodInformation methodInformation, object[] originalArguments)
        {
            object obj2;
            try
            {
                obj2 = methodInformation.Invoke(target, arguments);
            }
            catch (TargetInvocationException exception)
            {
                if ((exception.InnerException is FlowControlException) || (exception.InnerException is ScriptCallDepthException))
                {
                    throw exception.InnerException;
                }
                if (exception.InnerException is ParameterBindingException)
                {
                    throw exception.InnerException;
                }
                Exception innerException = (exception.InnerException == null) ? exception : exception.InnerException;
                throw new MethodInvocationException("DotNetMethodTargetInvocation", innerException, ExtendedTypeSystem.MethodInvocationException, new object[] { methodInformation.method.Name, arguments.Length, innerException.Message });
            }
            catch (ParameterBindingException)
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
            catch (PipelineStoppedException)
            {
                throw;
            }
            catch (Exception exception3)
            {
                CommandProcessorBase.CheckForSevereException(exception3);
                if ((methodInformation.method.DeclaringType == typeof(SteppablePipeline)) && ((methodInformation.method.Name.Equals("Begin") || methodInformation.method.Name.Equals("Process")) || methodInformation.method.Name.Equals("End")))
                {
                    throw;
                }
                throw new MethodInvocationException("DotNetMethodException", exception3, ExtendedTypeSystem.MethodInvocationException, new object[] { methodInformation.method.Name, arguments.Length, exception3.Message });
            }
            Adapter.SetReferences(arguments, methodInformation, originalArguments);
            MethodInfo method = methodInformation.method as MethodInfo;
            if ((method != null) && (method.ReturnType != typeof(void)))
            {
                return obj2;
            }
            return AutomationNull.Value;
        }

        internal static object ConstructorInvokeDotNet(Type type, ConstructorInfo[] constructors, object[] arguments)
        {
            MethodInformation[] methodInformationArray;
            object[] objArray;
            MethodInformation information = Adapter.FindCachedMethod(type, ".ctor", null, arguments, CallsiteCacheEntryFlags.Constructor);
            if (information != null)
            {
                methodInformationArray = new MethodInformation[] { information };
            }
            else
            {
                methodInformationArray = GetMethodInformationArray(constructors);
            }
            MethodInformation mi = Adapter.GetBestMethodAndArguments(type.Name, methodInformationArray, arguments, out objArray);
            if ((PSObject.memberResolution.Options & PSTraceSourceOptions.WriteLine) != PSTraceSourceOptions.None)
            {
                PSObject.memberResolution.WriteLine("Calling Constructor: {0}", new object[] { GetMethodInfoOverloadDefinition(null, mi.method, 0) });
            }
            if (information == null)
            {
                Adapter.CacheMethod(mi, type, ".ctor", null, arguments, CallsiteCacheEntryFlags.Constructor);
            }
            return AuxiliaryConstructorInvoke(mi, objArray, arguments);
        }

        internal T GetDotNetEvent<T>(object obj, string eventName) where T: PSMemberInfo
        {
            CacheTable staticEventReflectionTable;
            if (!typeof(T).IsAssignableFrom(typeof(PSEvent)))
            {
                return default(T);
            }
            if (this.isStatic)
            {
                staticEventReflectionTable = GetStaticEventReflectionTable(obj);
            }
            else
            {
                staticEventReflectionTable = GetInstanceEventReflectionTable(obj);
            }
            EventCacheEntry entry = (EventCacheEntry) staticEventReflectionTable[eventName];
            if (entry == null)
            {
                return default(T);
            }
            return (new PSEvent(entry.events[0]) as T);
        }

        internal T GetDotNetMethod<T>(object obj, string methodName) where T: PSMemberInfo
        {
            MethodCacheEntry entry;
            if (!typeof(T).IsAssignableFrom(typeof(PSMethod)))
            {
                return default(T);
            }
            if (this.isStatic)
            {
                entry = (MethodCacheEntry) GetStaticMethodReflectionTable(obj as Type)[methodName];
            }
            else
            {
                entry = (MethodCacheEntry) GetInstanceMethodReflectionTable(obj.GetType())[methodName];
            }
            if (entry == null)
            {
                return default(T);
            }
            return (new PSMethod(entry[0].method.Name, this, obj, entry) as T);
        }

        internal T GetDotNetProperty<T>(object obj, string propertyName) where T: PSMemberInfo
        {
            bool flag = typeof(T).IsAssignableFrom(typeof(PSProperty));
            bool flag2 = IsTypeParameterizedProperty(typeof(T));
            if (flag || flag2)
            {
                object obj2;
                if (this.isStatic)
                {
                    obj2 = GetStaticPropertyReflectionTable(obj as Type)[propertyName];
                }
                else
                {
                    obj2 = GetInstancePropertyReflectionTable(obj.GetType())[propertyName];
                }
                if (obj2 != null)
                {
                    PropertyCacheEntry adapterData = obj2 as PropertyCacheEntry;
                    if ((adapterData != null) && flag)
                    {
                        return (new PSProperty(adapterData.member.Name, this, obj, adapterData) as T);
                    }
                    ParameterizedPropertyCacheEntry entry2 = obj2 as ParameterizedPropertyCacheEntry;
                    if ((entry2 != null) && flag2)
                    {
                        return (new PSParameterizedProperty(entry2.propertyName, this, obj, entry2) as T);
                    }
                }
            }
            return default(T);
        }

        private static CacheTable GetInstanceEventReflectionTable(object obj)
        {
            lock (instanceEventCacheTable)
            {
                Type type = obj.GetType();
                CacheTable typeEvents = (CacheTable) instanceEventCacheTable[type];
                if (typeEvents == null)
                {
                    typeEvents = new CacheTable();
                    PopulateEventReflectionTable(type, typeEvents, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    instanceEventCacheTable[type] = typeEvents;
                }
                return typeEvents;
            }
        }

        private static CacheTable GetInstanceMethodReflectionTable(Type type)
        {
            lock (instanceMethodCacheTable)
            {
                CacheTable typeMethods = (CacheTable) instanceMethodCacheTable[type];
                if (typeMethods == null)
                {
                    typeMethods = new CacheTable();
                    PopulateMethodReflectionTable(type, typeMethods, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    instanceMethodCacheTable[type] = typeMethods;
                }
                return typeMethods;
            }
        }

        private static CacheTable GetInstancePropertyReflectionTable(Type type)
        {
            lock (instancePropertyCacheTable)
            {
                CacheTable typeProperties = (CacheTable) instancePropertyCacheTable[type];
                if (typeProperties == null)
                {
                    typeProperties = new CacheTable();
                    PopulatePropertyReflectionTable(type, typeProperties, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                    instancePropertyCacheTable[type] = typeProperties;
                }
                return typeProperties;
            }
        }

        protected override ConsolidatedString GetInternedTypeNameHierarchy(object obj)
        {
            return GetInternedTypeNameHierarchy(obj.GetType());
        }

        internal static ConsolidatedString GetInternedTypeNameHierarchy(Type type)
        {
            return typeToTypeNameDictionary.GetOrAdd(type, t => new ConsolidatedString(Adapter.GetDotNetTypeNameHierarchy(t), true));
        }

        protected override T GetMember<T>(object obj, string memberName)
        {
            T dotNetProperty = this.GetDotNetProperty<T>(obj, memberName);
            if (dotNetProperty != null)
            {
                return dotNetProperty;
            }
            return this.GetDotNetMethod<T>(obj, memberName);
        }

        protected override PSMemberInfoInternalCollection<T> GetMembers<T>(object obj)
        {
            PSMemberInfoInternalCollection<T> members = new PSMemberInfoInternalCollection<T>();
            this.AddAllProperties<T>(obj, members, false);
            this.AddAllMethods<T>(obj, members, false);
            this.AddAllEvents<T>(obj, members, false);
            this.AddAllDynamicMembers<T>(obj, members, false);
            return members;
        }

        internal static string GetMethodInfoOverloadDefinition(string memberName, MethodBase methodEntry, int parametersToIgnore)
        {
            StringBuilder builder = new StringBuilder();
            if (methodEntry.IsStatic)
            {
                builder.Append("static ");
            }
            MethodInfo info = methodEntry as MethodInfo;
            if (info != null)
            {
                builder.Append(ToStringCodeMethods.Type(info.ReturnType, false));
                builder.Append(" ");
            }
            if (methodEntry.DeclaringType.IsInterface)
            {
                builder.Append(ToStringCodeMethods.Type(methodEntry.DeclaringType, true));
                builder.Append(".");
            }
            builder.Append((memberName == null) ? methodEntry.Name : memberName);
            if (methodEntry.IsGenericMethodDefinition)
            {
                builder.Append("[");
                bool flag = true;
                foreach (Type type in methodEntry.GetGenericArguments())
                {
                    if (!flag)
                    {
                        builder.Append(", ");
                    }
                    builder.Append(ToStringCodeMethods.Type(type, false));
                    flag = false;
                }
                builder.Append("]");
            }
            builder.Append("(");
            ParameterInfo[] parameters = methodEntry.GetParameters();
            int num = parameters.Length - parametersToIgnore;
            if (num > 0)
            {
                for (int i = 0; i < num; i++)
                {
                    ParameterInfo info2 = parameters[i];
                    Type parameterType = info2.ParameterType;
                    if (parameterType.IsByRef)
                    {
                        builder.Append("[ref] ");
                        parameterType = parameterType.GetElementType();
                    }
                    if ((parameterType.IsArray && (i == (num - 1))) && (info2.GetCustomAttributes(typeof(ParamArrayAttribute), false).Length != 0))
                    {
                        builder.Append("Params ");
                    }
                    builder.Append(ToStringCodeMethods.Type(parameterType, false));
                    builder.Append(" ");
                    builder.Append(info2.Name);
                    builder.Append(", ");
                }
                builder.Remove(builder.Length - 2, 2);
            }
            builder.Append(")");
            return builder.ToString();
        }

        internal static MethodInformation[] GetMethodInformationArray(MethodBase[] methods)
        {
            int length = methods.Length;
            MethodInformation[] informationArray = new MethodInformation[length];
            for (int i = 0; i < methods.Length; i++)
            {
                informationArray[i] = new MethodInformation(methods[i], 0);
            }
            return informationArray;
        }

        internal IEnumerable<object> GetPropertiesAndMethods(Type type, bool @static)
        {
            CacheTable iteratorVariable0 = @static ? GetStaticPropertyReflectionTable(type) : GetInstancePropertyReflectionTable(type);
            foreach (object iteratorVariable1 in iteratorVariable0.memberCollection)
            {
                ArrayList source = iteratorVariable1 as ArrayList;
                if (source == null)
                {
                    ArrayList list = new ArrayList(1);
                    list.Add(iteratorVariable1);
                    source = list;
                }
                foreach (PropertyCacheEntry iteratorVariable3 in source.OfType<PropertyCacheEntry>())
                {
                    yield return iteratorVariable3.member;
                }
            }
            CacheTable iteratorVariable4 = @static ? GetStaticMethodReflectionTable(type) : GetInstanceMethodReflectionTable(type);
            foreach (object iteratorVariable5 in iteratorVariable4.memberCollection)
            {
                MethodCacheEntry iteratorVariable6 = iteratorVariable5 as MethodCacheEntry;
                if ((iteratorVariable6 != null) && !iteratorVariable6[0].method.IsSpecialName)
                {
                    yield return iteratorVariable6;
                }
            }
        }

        private static CacheTable GetStaticEventReflectionTable(object obj)
        {
            lock (staticEventCacheTable)
            {
                CacheTable typeEvents = (CacheTable) staticEventCacheTable[obj];
                if (typeEvents == null)
                {
                    typeEvents = new CacheTable();
                    PopulateEventReflectionTable((Type) obj, typeEvents, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);
                    staticEventCacheTable[obj] = typeEvents;
                }
                return typeEvents;
            }
        }

        private static CacheTable GetStaticMethodReflectionTable(Type type)
        {
            lock (staticMethodCacheTable)
            {
                CacheTable typeMethods = (CacheTable) staticMethodCacheTable[type];
                if (typeMethods == null)
                {
                    typeMethods = new CacheTable();
                    PopulateMethodReflectionTable(type, typeMethods, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);
                    staticMethodCacheTable[type] = typeMethods;
                }
                return typeMethods;
            }
        }

        private static CacheTable GetStaticPropertyReflectionTable(Type type)
        {
            lock (staticPropertyCacheTable)
            {
                CacheTable typeProperties = (CacheTable) staticPropertyCacheTable[type];
                if (typeProperties == null)
                {
                    typeProperties = new CacheTable();
                    PopulatePropertyReflectionTable(type, typeProperties, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase);
                    staticPropertyCacheTable[type] = typeProperties;
                }
                return typeProperties;
            }
        }

        internal static bool IsTypeParameterizedProperty(Type t)
        {
            if (!t.Equals(typeof(PSMemberInfo)))
            {
                return t.Equals(typeof(PSParameterizedProperty));
            }
            return true;
        }

        protected override Collection<string> MethodDefinitions(PSMethod method)
        {
            MethodCacheEntry adapterData = (MethodCacheEntry) method.adapterData;
            return new Collection<string>((from m in adapterData.methodInformationStructures select m.methodDefinition).Distinct<string>(StringComparer.Ordinal).ToList<string>());
        }

        protected override object MethodInvoke(PSMethod method, object[] arguments)
        {
            return this.MethodInvoke(method, null, arguments);
        }

        protected override object MethodInvoke(PSMethod method, PSMethodInvocationConstraints invocationConstraints, object[] arguments)
        {
            MethodCacheEntry adapterData = (MethodCacheEntry) method.adapterData;
            return MethodInvokeDotNet(method.Name, method.baseObject, adapterData.methodInformationStructures, invocationConstraints, arguments);
        }

        internal static object MethodInvokeDotNet(string methodName, object target, MethodInformation[] methodInformation, PSMethodInvocationConstraints invocationConstraints, object[] arguments)
        {
            object[] objArray;
            MethodInformation mi = Adapter.GetBestMethodAndArguments(methodName, methodInformation, invocationConstraints, arguments, out objArray);
            string methodDefinition = mi.methodDefinition;
            ScriptTrace.Trace(1, "TraceMethodCall", ParserStrings.TraceMethodCall, new object[] { methodDefinition });
            PSObject.memberResolution.WriteLine("Calling Method: {0}", new object[] { methodDefinition });
            CallsiteCacheEntryFlags none = CallsiteCacheEntryFlags.None;
            if (mi.method.IsStatic)
            {
                none = CallsiteCacheEntryFlags.Static;
            }
            Adapter.CacheMethod(mi, target, methodName, invocationConstraints, arguments, none);
            return AuxiliaryMethodInvoke(target, objArray, mi, arguments);
        }

        protected override Collection<string> ParameterizedPropertyDefinitions(PSParameterizedProperty property)
        {
            ParameterizedPropertyCacheEntry adapterData = (ParameterizedPropertyCacheEntry) property.adapterData;
            Collection<string> collection = new Collection<string>();
            foreach (string str in adapterData.propertyDefinition)
            {
                collection.Add(str);
            }
            return collection;
        }

        protected override object ParameterizedPropertyGet(PSParameterizedProperty property, object[] arguments)
        {
            ParameterizedPropertyCacheEntry adapterData = (ParameterizedPropertyCacheEntry) property.adapterData;
            return MethodInvokeDotNet(property.Name, property.baseObject, adapterData.getterInformation, null, arguments);
        }

        internal static void ParameterizedPropertyInvokeSet(string propertyName, object target, object valuetoSet, MethodInformation[] methodInformation, object[] arguments, bool addToCache)
        {
            object[] objArray;
            object obj2;
            MethodInformation mi = Adapter.GetBestMethodAndArguments(propertyName, methodInformation, arguments, out objArray);
            PSObject.memberResolution.WriteLine("Calling Set Method: {0}", new object[] { mi.methodDefinition });
            ParameterInfo[] parameters = mi.method.GetParameters();
            Type parameterType = parameters[parameters.Length - 1].ParameterType;
            try
            {
                obj2 = Adapter.PropertySetAndMethodArgumentConvertTo(valuetoSet, parameterType, CultureInfo.InvariantCulture);
            }
            catch (InvalidCastException exception)
            {
                throw new MethodException("PropertySetterConversionInvalidCastArgument", exception, ExtendedTypeSystem.MethodArgumentConversionException, new object[] { arguments.Length - 1, valuetoSet, propertyName, parameterType, exception.Message });
            }
            object[] objArray2 = new object[objArray.Length + 1];
            for (int i = 0; i < objArray.Length; i++)
            {
                objArray2[i] = objArray[i];
            }
            objArray2[objArray.Length] = obj2;
            if (addToCache)
            {
                CallsiteCacheEntryFlags parameterizedSetter = CallsiteCacheEntryFlags.ParameterizedSetter;
                if (mi.method.IsStatic)
                {
                    parameterizedSetter |= CallsiteCacheEntryFlags.Static;
                }
                Adapter.CacheMethod(mi, target, propertyName, null, arguments, parameterizedSetter);
            }
            AuxiliaryMethodInvoke(target, objArray2, mi, arguments);
        }

        protected override bool ParameterizedPropertyIsGettable(PSParameterizedProperty property)
        {
            return !((ParameterizedPropertyCacheEntry) property.adapterData).writeOnly;
        }

        protected override bool ParameterizedPropertyIsSettable(PSParameterizedProperty property)
        {
            return !((ParameterizedPropertyCacheEntry) property.adapterData).readOnly;
        }

        protected override void ParameterizedPropertySet(PSParameterizedProperty property, object setValue, object[] arguments)
        {
            ParameterizedPropertyCacheEntry adapterData = (ParameterizedPropertyCacheEntry) property.adapterData;
            ParameterizedPropertyInvokeSet(adapterData.propertyName, property.baseObject, setValue, adapterData.setterInformation, arguments, true);
        }

        protected override string ParameterizedPropertyToString(PSParameterizedProperty property)
        {
            StringBuilder builder = new StringBuilder();
            foreach (string str in this.ParameterizedPropertyDefinitions(property))
            {
                builder.Append(str);
                builder.Append(", ");
            }
            builder.Remove(builder.Length - 2, 2);
            return builder.ToString();
        }

        protected override string ParameterizedPropertyType(PSParameterizedProperty property)
        {
            ParameterizedPropertyCacheEntry adapterData = (ParameterizedPropertyCacheEntry) property.adapterData;
            return adapterData.propertyType.FullName;
        }

        private static void PopulateEventReflectionTable(Type type, CacheTable typeEvents, BindingFlags bindingFlags)
        {
            foreach (EventInfo info in type.GetEvents(bindingFlags))
            {
                string name = info.Name;
                ArrayList list = (ArrayList) typeEvents[name];
                if (list == null)
                {
                    ArrayList member = new ArrayList();
                    member.Add(info);
                    typeEvents.Add(name, member);
                }
                else
                {
                    list.Add(info);
                }
            }
            for (int i = 0; i < typeEvents.memberCollection.Count; i++)
            {
                typeEvents.memberCollection[i] = new EventCacheEntry((EventInfo[]) ((ArrayList) typeEvents.memberCollection[i]).ToArray(typeof(EventInfo)));
            }
        }

        private static void PopulateMethodReflectionTable(Type type, IEnumerable<MethodInfo> methods, CacheTable typeMethods)
        {
            foreach (MethodInfo info in methods)
            {
                if (info.DeclaringType.Equals(type))
                {
                    string name = info.Name;
                    ArrayList previousMethodEntry = (ArrayList) typeMethods[name];
                    if (previousMethodEntry == null)
                    {
                        ArrayList member = new ArrayList();
                        member.Add(info);
                        typeMethods.Add(name, member);
                    }
                    else
                    {
                        AddOverload(previousMethodEntry, info);
                    }
                }
            }
            if (type.BaseType != null)
            {
                PopulateMethodReflectionTable(type.BaseType, methods, typeMethods);
            }
        }

        private static void PopulateMethodReflectionTable(Type type, CacheTable typeMethods, BindingFlags bindingFlags)
        {
            IEnumerable<MethodInfo> methods = type.GetMethods(bindingFlags);
            PopulateMethodReflectionTable(type, methods, typeMethods);
            if (!type.IsInterface)
            {
                foreach (Type type2 in type.GetInterfaces())
                {
                    if (type2.IsPublic && (!type2.IsGenericType || !type.IsArray))
                    {
                        InterfaceMapping interfaceMap = type.GetInterfaceMap(type2);
                        for (int j = 0; j < interfaceMap.InterfaceMethods.Length; j++)
                        {
                            MethodInfo info = interfaceMap.InterfaceMethods[j];
                            if (info.IsPublic && (info.IsStatic == ((BindingFlags.Static & bindingFlags) != BindingFlags.Default)))
                            {
                                ArrayList list = (ArrayList) typeMethods[info.Name];
                                if (list == null)
                                {
                                    ArrayList member = new ArrayList();
                                    member.Add(info);
                                    typeMethods.Add(info.Name, member);
                                }
                                else if (!list.Contains(info))
                                {
                                    list.Add(info);
                                }
                            }
                        }
                    }
                }
            }
            for (int i = 0; i < typeMethods.memberCollection.Count; i++)
            {
                typeMethods.memberCollection[i] = new MethodCacheEntry((MethodInfo[]) ((ArrayList) typeMethods.memberCollection[i]).ToArray(typeof(MethodInfo)));
            }
        }

        private static void PopulatePropertyReflectionTable(Type type, CacheTable typeProperties, BindingFlags bindingFlags)
        {
            foreach (PropertyInfo info in type.GetProperties(bindingFlags))
            {
                PopulateSingleProperty(type, info, typeProperties, info.Name);
            }
            foreach (Type type2 in type.GetInterfaces())
            {
                if (type2.IsPublic)
                {
                    foreach (PropertyInfo info2 in type2.GetProperties(bindingFlags))
                    {
                        PopulateSingleProperty(type, info2, typeProperties, info2.Name);
                    }
                }
            }
            for (int i = 0; i < typeProperties.memberCollection.Count; i++)
            {
                ArrayList list = (ArrayList) typeProperties.memberCollection[i];
                PropertyInfo property = (PropertyInfo) list[0];
                if ((list.Count > 1) || (property.GetIndexParameters().Length != 0))
                {
                    typeProperties.memberCollection[i] = new ParameterizedPropertyCacheEntry((ArrayList) typeProperties.memberCollection[i]);
                }
                else
                {
                    typeProperties.memberCollection[i] = new PropertyCacheEntry(property);
                }
            }
            foreach (FieldInfo info4 in type.GetFields(bindingFlags))
            {
                string name = info4.Name;
                PropertyCacheEntry entry = (PropertyCacheEntry) typeProperties[name];
                if (entry == null)
                {
                    typeProperties.Add(name, new PropertyCacheEntry(info4));
                }
                else if (!string.Equals(entry.member.Name, name))
                {
                    throw new ExtendedTypeSystemException("NotACLSComplaintField", null, ExtendedTypeSystem.NotAClsCompliantFieldProperty, new object[] { name, type.FullName, entry.member.Name });
                }
            }
        }

        private static void PopulateSingleProperty(Type type, PropertyInfo property, CacheTable typeProperties, string propertyName)
        {
            ArrayList previousProperties = (ArrayList) typeProperties[propertyName];
            if (previousProperties == null)
            {
                ArrayList member = new ArrayList();
                member.Add(property);
                typeProperties.Add(propertyName, member);
            }
            else
            {
                PropertyInfo info = (PropertyInfo) previousProperties[0];
                if (!string.Equals(property.Name, info.Name, StringComparison.Ordinal))
                {
                    throw new ExtendedTypeSystemException("NotACLSComplaintProperty", null, ExtendedTypeSystem.NotAClsCompliantFieldProperty, new object[] { property.Name, type.FullName, info.Name });
                }
                if (!PropertyAlreadyPresent(previousProperties, property))
                {
                    previousProperties.Add(property);
                }
            }
        }

        private static bool PropertyAlreadyPresent(ArrayList previousProperties, PropertyInfo property)
        {
            ParameterInfo[] indexParameters = property.GetIndexParameters();
            int length = indexParameters.Length;
            foreach (PropertyInfo info in previousProperties)
            {
                ParameterInfo[] infoArray2 = info.GetIndexParameters();
                if (infoArray2.Length == length)
                {
                    bool flag2 = true;
                    for (int i = 0; i < infoArray2.Length; i++)
                    {
                        ParameterInfo info2 = infoArray2[i];
                        ParameterInfo info3 = indexParameters[i];
                        if (!info2.ParameterType.Equals(info3.ParameterType))
                        {
                            flag2 = false;
                            break;
                        }
                    }
                    if (flag2)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        protected override AttributeCollection PropertyAttributes(PSProperty property)
        {
            PropertyCacheEntry adapterData = (PropertyCacheEntry) property.adapterData;
            return adapterData.Attributes;
        }

        protected override object PropertyGet(PSProperty property)
        {
            PropertyCacheEntry adapterData = (PropertyCacheEntry) property.adapterData;
            PropertyInfo member = adapterData.member as PropertyInfo;
            if (member != null)
            {
                if (adapterData.writeOnly)
                {
                    throw new GetValueException("WriteOnlyProperty", null, ExtendedTypeSystem.WriteOnlyProperty, new object[] { member.Name });
                }

				/*  Begin Getter Hacks */

				if (member.Name == "ProcessName" && property.baseObject.GetType () == typeof(System.Diagnostics.Process))
				{
					return SafeGetProcessName ((System.Diagnostics.Process)property.baseObject);
				}
				if (member.Name == "Id" && property.baseObject.GetType () == typeof(System.Diagnostics.Process))
				{
					return SafeGetProcessId ((System.Diagnostics.Process)property.baseObject);
				}

				/*  End Getter Hacks */


                if (adapterData.useReflection)
                {
                    return member.GetValue(property.baseObject, null);
                }
                return adapterData.getterDelegate(property.baseObject);
            }
            FieldInfo info2 = adapterData.member as FieldInfo;
            if (adapterData.useReflection)
            {
                return info2.GetValue(property.baseObject);
            }
            return adapterData.getterDelegate(property.baseObject);
        }


		internal static int SafeGetProcessId(Process process)
		{
			int id;
			try
			{
				id = process.Id;
			}
			catch (Win32Exception win32Exception)
			{
				id = -2147483648;
			}
			catch (InvalidOperationException invalidOperationException)
			{
				id = -2147483648;
			}
			return id;
		}
		
		internal static string SafeGetProcessName(Process process)
		{
			string processName = "";
			try
			{
				if (OSHelper.IsMacOSX)
				{
					int id = SafeGetProcessId(process);
					if (id != -2147483648)
					{
						var startInfo = new ProcessStartInfo("ps", "-p " + id.ToString () + " -xco command") { UseShellExecute = false, CreateNoWindow = false, RedirectStandardOutput = true };
						bool headerPassed = false;
						Process p = Process.Start (startInfo);
						p.OutputDataReceived += (object sender, DataReceivedEventArgs e) => {
							if (headerPassed && !string.IsNullOrEmpty (e.Data)) processName = e.Data;
							headerPassed = true;
						};
						p.BeginOutputReadLine ();
						p.WaitForExit ();
					}
				}
				else {
					processName = process.ProcessName;
				}
			}
			catch (Win32Exception win32Exception)
			{
				processName = "";
			}
			catch (InvalidOperationException invalidOperationException)
			{
				processName = "";
			}
			return processName;
		}


        protected override bool PropertyIsGettable(PSProperty property)
        {
            return !((PropertyCacheEntry) property.adapterData).writeOnly;
        }

        protected override bool PropertyIsSettable(PSProperty property)
        {
            return !((PropertyCacheEntry) property.adapterData).readOnly;
        }

        private static bool PropertyIsStatic(PSProperty property)
        {
            PropertyCacheEntry adapterData = property.adapterData as PropertyCacheEntry;
            if (adapterData == null)
            {
                return false;
            }
            return adapterData.isStatic;
        }

        protected override void PropertySet(PSProperty property, object setValue, bool convertIfPossible)
        {
            PropertyCacheEntry adapterData = (PropertyCacheEntry) property.adapterData;
            if (adapterData.readOnly)
            {
                throw new SetValueException("ReadOnlyProperty", null, ExtendedTypeSystem.ReadOnlyProperty, new object[] { adapterData.member.Name });
            }
            PropertyInfo member = adapterData.member as PropertyInfo;
            if (member != null)
            {
                if (convertIfPossible)
                {
                    setValue = Adapter.PropertySetAndMethodArgumentConvertTo(setValue, member.PropertyType, CultureInfo.InvariantCulture);
                }
                if (adapterData.useReflection)
                {
                    member.SetValue(property.baseObject, setValue, null);
                }
                else
                {
                    adapterData.setterDelegate(property.baseObject, setValue);
                }
            }
            else
            {
                FieldInfo info2 = adapterData.member as FieldInfo;
                if (convertIfPossible)
                {
                    setValue = Adapter.PropertySetAndMethodArgumentConvertTo(setValue, info2.FieldType, CultureInfo.InvariantCulture);
                }
                if (adapterData.useReflection)
                {
                    info2.SetValue(property.baseObject, setValue);
                }
                else
                {
                    adapterData.setterDelegate(property.baseObject, setValue);
                }
            }
        }

        protected override string PropertyToString(PSProperty property)
        {
            StringBuilder builder = new StringBuilder();
            if (PropertyIsStatic(property))
            {
                builder.Append("static ");
            }
            builder.Append(this.PropertyType(property, true));
            builder.Append(" ");
            builder.Append(property.Name);
            builder.Append(" {");
            if (this.PropertyIsGettable(property))
            {
                builder.Append("get;");
            }
            if (this.PropertyIsSettable(property))
            {
                builder.Append("set;");
            }
            builder.Append("}");
            return builder.ToString();
        }

        protected override string PropertyType(PSProperty property, bool forDisplay)
        {
            Type propertyType = ((PropertyCacheEntry) property.adapterData).propertyType;
            if (!forDisplay)
            {
                return propertyType.FullName;
            }
            return ToStringCodeMethods.Type(propertyType, false);
        }

        private static bool SameSignature(MethodInfo method1, MethodInfo method2)
        {
            if (method1.GetGenericArguments().Length != method2.GetGenericArguments().Length)
            {
                return false;
            }
            ParameterInfo[] parameters = method1.GetParameters();
            ParameterInfo[] infoArray2 = method2.GetParameters();
            if (parameters.Length != infoArray2.Length)
            {
                return false;
            }
            for (int i = 0; i < parameters.Length; i++)
            {
                if ((!parameters[i].ParameterType.Equals(infoArray2[i].ParameterType) || (parameters[i].IsOut != infoArray2[i].IsOut)) || (parameters[i].IsOptional != infoArray2[i].IsOptional))
                {
                    return false;
                }
            }
            return true;
        }

        internal override bool SiteBinderCanOptimize
        {
            get
            {
                return true;
            }
        }

        

        internal class EventCacheEntry
        {
            internal EventInfo[] events;

            internal EventCacheEntry(EventInfo[] events)
            {
                this.events = events;
            }
        }

        internal class MethodCacheEntry
        {
            internal MethodInformation[] methodInformationStructures;

            internal MethodCacheEntry(MethodInfo[] methods)
            {
                this.methodInformationStructures = DotNetAdapter.GetMethodInformationArray(methods);
            }

            internal MethodInformation this[int i]
            {
                get
                {
                    return this.methodInformationStructures[i];
                }
            }
        }

        internal class ParameterizedPropertyCacheEntry
        {
            internal MethodInformation[] getterInformation;
            internal string[] propertyDefinition;
            internal string propertyName;
            internal Type propertyType;
            internal bool readOnly;
            internal MethodInformation[] setterInformation;
            internal bool writeOnly;

            internal ParameterizedPropertyCacheEntry(ArrayList properties)
            {
                PropertyInfo info = (PropertyInfo) properties[0];
                this.propertyName = info.Name;
                this.propertyType = info.PropertyType;
                List<MethodInfo> list = new List<MethodInfo>();
                List<MethodInfo> list2 = new List<MethodInfo>();
                List<string> list3 = new List<string>();
                foreach (PropertyInfo info2 in properties)
                {
                    if (!info2.PropertyType.Equals(this.propertyType))
                    {
                        this.propertyType = typeof(object);
                    }
                    MethodInfo getMethod = info2.GetGetMethod();
                    StringBuilder builder = new StringBuilder();
                    StringBuilder builder2 = new StringBuilder();
                    if (getMethod != null)
                    {
                        builder2.Append("get;");
                        builder.Append(DotNetAdapter.GetMethodInfoOverloadDefinition(this.propertyName, getMethod, 0));
                        list.Add(getMethod);
                    }
                    MethodInfo setMethod = info2.GetSetMethod();
                    if (setMethod != null)
                    {
                        builder2.Append("set;");
                        if (builder.Length == 0)
                        {
                            builder.Append(DotNetAdapter.GetMethodInfoOverloadDefinition(this.propertyName, setMethod, 1));
                        }
                        list2.Add(setMethod);
                    }
                    builder.Append(" {");
                    builder.Append(builder2);
                    builder.Append("}");
                    list3.Add(builder.ToString());
                }
                this.propertyDefinition = list3.ToArray();
                this.writeOnly = list.Count == 0;
                this.readOnly = list2.Count == 0;
                this.getterInformation = new MethodInformation[list.Count];
                for (int i = 0; i < list.Count; i++)
                {
                    this.getterInformation[i] = new MethodInformation(list[i], 0);
                }
                this.setterInformation = new MethodInformation[list2.Count];
                for (int j = 0; j < list2.Count; j++)
                {
                    this.setterInformation[j] = new MethodInformation(list2[j], 1);
                }
            }
        }

        internal class PropertyCacheEntry
        {
            private AttributeCollection attributes;
            internal GetterDelegate getterDelegate;
            internal bool isStatic;
            internal MemberInfo member;
            internal Type propertyType;
            internal bool readOnly;
            internal SetterDelegate setterDelegate;
            internal bool useReflection;
            internal bool writeOnly;

            internal PropertyCacheEntry(FieldInfo field)
            {
                this.member = field;
                this.isStatic = field.IsStatic;
                this.propertyType = field.FieldType;
                if (field.IsLiteral || field.IsInitOnly)
                {
                    this.readOnly = true;
                }
                if ((field.IsLiteral || field.DeclaringType.IsValueType) || (field.FieldType.IsGenericType || field.DeclaringType.IsGenericType))
                {
                    this.useReflection = true;
                }
                else
                {
                    this.getterDelegate = GetFieldGetter(field);
                    this.setterDelegate = GetFieldSetter(field);
                }
            }

            internal PropertyCacheEntry(PropertyInfo property)
            {
                this.member = property;
                this.propertyType = property.PropertyType;
                if ((property.DeclaringType.IsValueType || property.PropertyType.IsGenericType) || ((property.DeclaringType.IsGenericType || property.DeclaringType.IsCOMObject) || property.PropertyType.IsCOMObject))
                {
                    this.readOnly = property.GetSetMethod() == null;
                    this.writeOnly = property.GetGetMethod() == null;
                    this.useReflection = true;
                }
                else
                {
                    MethodInfo getMethod = property.GetGetMethod();
                    if (getMethod != null)
                    {
                        this.isStatic = getMethod.IsStatic;
                        this.getterDelegate = GetPropertyGetter(property, getMethod);
                    }
                    else
                    {
                        this.writeOnly = true;
                    }
                    MethodInfo setMethod = property.GetSetMethod();
                    if (setMethod != null)
                    {
                        this.isStatic = setMethod.IsStatic;
                        this.setterDelegate = GetPropertySetter(property, setMethod);
                    }
                    else
                    {
                        this.readOnly = true;
                    }
                }
            }

            internal static GetterDelegate GetFieldGetter(FieldInfo field)
            {
                DynamicMethod method = new DynamicMethod("getter", typeof(object), new Type[] { typeof(object) }, typeof(Adapter).Module, true);
                ILGenerator iLGenerator = method.GetILGenerator();
                if (!field.IsStatic)
                {
                    iLGenerator.Emit(OpCodes.Ldarg_0);
                    iLGenerator.Emit(OpCodes.Ldfld, field);
                }
                else
                {
                    iLGenerator.Emit(OpCodes.Ldsfld, field);
                }
                Adapter.DoBoxingIfNecessary(iLGenerator, field.FieldType);
                iLGenerator.Emit(OpCodes.Ret);
                return (GetterDelegate) method.CreateDelegate(typeof(GetterDelegate));
            }

            internal static SetterDelegate GetFieldSetter(FieldInfo field)
            {
                DynamicMethod method = new DynamicMethod("setter", typeof(void), new Type[] { typeof(object), typeof(object) }, typeof(Adapter).Module, true);
                ILGenerator iLGenerator = method.GetILGenerator();
                if (!field.IsStatic)
                {
                    iLGenerator.Emit(OpCodes.Ldarg_0);
                }
                iLGenerator.Emit(OpCodes.Ldarg_1);
                if (field.FieldType.IsValueType)
                {
                    iLGenerator.Emit(OpCodes.Unbox, field.FieldType);
                    iLGenerator.Emit(OpCodes.Ldobj, field.FieldType);
                }
                if (field.IsStatic)
                {
                    iLGenerator.Emit(OpCodes.Stsfld, field);
                }
                else
                {
                    iLGenerator.Emit(OpCodes.Stfld, field);
                }
                iLGenerator.Emit(OpCodes.Ret);
                return (SetterDelegate) method.CreateDelegate(typeof(SetterDelegate));
            }

            internal static GetterDelegate GetPropertyGetter(PropertyInfo property, MethodInfo getterMethodInfo)
            {
                DynamicMethod method = new DynamicMethod("getter", typeof(object), new Type[] { typeof(object) }, typeof(Adapter).Module, true);
                ILGenerator iLGenerator = method.GetILGenerator();
                if (!getterMethodInfo.IsStatic)
                {
                    iLGenerator.Emit(OpCodes.Ldarg_0);
                }
                iLGenerator.Emit(OpCodes.Call, getterMethodInfo);
                Adapter.DoBoxingIfNecessary(iLGenerator, property.PropertyType);
                iLGenerator.Emit(OpCodes.Ret);
                return (GetterDelegate) method.CreateDelegate(typeof(GetterDelegate));
            }

            internal static SetterDelegate GetPropertySetter(PropertyInfo property, MethodInfo setterMethodInfo)
            {
                DynamicMethod method = new DynamicMethod("setter", typeof(void), new Type[] { typeof(object), typeof(object) }, typeof(Adapter).Module, true);
                ILGenerator iLGenerator = method.GetILGenerator();
                if (!setterMethodInfo.IsStatic)
                {
                    iLGenerator.Emit(OpCodes.Ldarg_0);
                }
                iLGenerator.Emit(OpCodes.Ldarg_1);
                if (property.PropertyType.IsValueType)
                {
                    iLGenerator.Emit(OpCodes.Unbox, property.PropertyType);
                    iLGenerator.Emit(OpCodes.Ldobj, property.PropertyType);
                }
                iLGenerator.Emit(OpCodes.Call, setterMethodInfo);
                iLGenerator.Emit(OpCodes.Ret);
                return (SetterDelegate) method.CreateDelegate(typeof(SetterDelegate));
            }

            internal AttributeCollection Attributes
            {
                get
                {
                    if (this.attributes == null)
                    {
                        object[] customAttributes = this.member.GetCustomAttributes(true);
                        Attribute[] attributes = new Attribute[customAttributes.Length];
                        for (int i = 0; i < customAttributes.Length; i++)
                        {
                            attributes[i] = (Attribute) customAttributes[i];
                        }
                        this.attributes = new AttributeCollection(attributes);
                    }
                    return this.attributes;
                }
            }

            internal delegate object GetterDelegate(object instance);

            internal delegate void SetterDelegate(object instance, object setValue);
        }
    }
}

