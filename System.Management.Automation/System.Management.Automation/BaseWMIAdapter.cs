namespace System.Management.Automation
{
    using Microsoft.PowerShell;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Management;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;

    internal abstract class BaseWMIAdapter : Adapter
    {
        private static HybridDictionary instanceMethodCacheTable = new HybridDictionary();

        protected BaseWMIAdapter()
        {
        }

        protected abstract void AddAllMethods<T>(ManagementBaseObject wmiObject, PSMemberInfoInternalCollection<T> members) where T: PSMemberInfo;
        protected abstract void AddAllProperties<T>(ManagementBaseObject wmiObject, PSMemberInfoInternalCollection<T> members) where T: PSMemberInfo;
        private object AuxillaryInvokeMethod(ManagementObject obj, WMIMethodCacheEntry mdata, object[] arguments)
        {
            object[] objArray;
            MethodInformation[] methods = new MethodInformation[] { mdata.MethodInfoStructure };
            Adapter.GetBestMethodAndArguments(mdata.Name, methods, arguments, out objArray);
            ParameterInformation[] parameters = mdata.MethodInfoStructure.parameters;
            Adapter.tracer.WriteLine("Parameters found {0}. Arguments supplied {0}", new object[] { parameters.Length, objArray.Length });
            ManagementBaseObject methodParameters = CreateClassFrmObject(obj).GetMethodParameters(mdata.Name);
            for (int i = 0; i < parameters.Length; i++)
            {
                WMIParameterInformation information = (WMIParameterInformation) parameters[i];
                if ((i < arguments.Length) && (arguments[i] == null))
                {
                    objArray[i] = null;
                }
                methodParameters[information.Name] = objArray[i];
            }
            return this.InvokeManagementMethod(obj, mdata.Name, methodParameters);
        }

        private static ManagementClass CreateClassFrmObject(ManagementBaseObject mgmtBaseObject)
        {
            ManagementClass class2 = mgmtBaseObject as ManagementClass;
            if (class2 == null)
            {
                class2 = new ManagementClass(mgmtBaseObject.ClassPath);
                ManagementObject obj2 = mgmtBaseObject as ManagementObject;
                if (obj2 != null)
                {
                    class2.Scope = obj2.Scope;
                    class2.Options = obj2.Options;
                }
            }
            return class2;
        }

        protected abstract PSProperty DoGetProperty(ManagementBaseObject wmiObject, string propertyName);
        protected static Type GetDotNetType(PropertyData pData)
        {
            string fullName;
            Adapter.tracer.WriteLine("Getting DotNet Type for CimType : {0}", new object[] { pData.Type });
            switch (pData.Type)
            {
                case CimType.SInt16:
                    fullName = typeof(short).FullName;
                    break;

                case CimType.SInt32:
                    fullName = typeof(int).FullName;
                    break;

                case CimType.Real32:
                    fullName = typeof(float).FullName;
                    break;

                case CimType.Real64:
                    fullName = typeof(double).FullName;
                    break;

                case CimType.String:
                    fullName = typeof(string).FullName;
                    break;

                case CimType.Boolean:
                    fullName = typeof(bool).FullName;
                    break;

                case CimType.SInt8:
                    fullName = typeof(sbyte).FullName;
                    break;

                case CimType.UInt8:
                    fullName = typeof(byte).FullName;
                    break;

                case CimType.UInt16:
                    fullName = typeof(ushort).FullName;
                    break;

                case CimType.UInt32:
                    fullName = typeof(int).FullName;
                    break;

                case CimType.SInt64:
                    fullName = typeof(long).FullName;
                    break;

                case CimType.UInt64:
                    fullName = typeof(ulong).FullName;
                    break;

                case CimType.DateTime:
                    fullName = typeof(string).FullName;
                    break;

                case CimType.Reference:
                    fullName = typeof(string).FullName;
                    break;

                case CimType.Char16:
                    fullName = typeof(char).FullName;
                    break;

                default:
                    fullName = typeof(object).FullName;
                    break;
            }
            if (pData.IsArray)
            {
                fullName = fullName + "[]";
            }
            return Type.GetType(fullName);
        }

        protected static string GetEmbeddedObjectTypeName(PropertyData pData)
        {
            string fullName = typeof(object).FullName;
            if (pData != null)
            {
                try
                {
                    string str2 = (string) pData.Qualifiers["cimtype"].Value;
                    fullName = string.Format(CultureInfo.InvariantCulture, "{0}#{1}", new object[] { typeof(ManagementObject).FullName, str2.Replace("object:", "") });
                }
                catch (ManagementException)
                {
                }
                catch (COMException)
                {
                }
            }
            return fullName;
        }

        protected static CacheTable GetInstanceMethodTable(ManagementBaseObject wmiObject, bool staticBinding)
        {
            lock (instanceMethodCacheTable)
            {
                CacheTable methodTable = null;
                ManagementPath classPath = wmiObject.ClassPath;
                string str = string.Format(CultureInfo.InvariantCulture, "{0}#{1}", new object[] { classPath.Path, staticBinding.ToString() });
                methodTable = (CacheTable) instanceMethodCacheTable[str];
                if (methodTable != null)
                {
                    Adapter.tracer.WriteLine("Returning method information from internal cache", new object[0]);
                    return methodTable;
                }
                Adapter.tracer.WriteLine("Method information not found in internal cache. Constructing one", new object[0]);
                try
                {
                    methodTable = new CacheTable();
                    ManagementClass mgmtClass = wmiObject as ManagementClass;
                    if (mgmtClass == null)
                    {
                        mgmtClass = CreateClassFrmObject(wmiObject);
                    }
                    PopulateMethodTable(mgmtClass, methodTable, staticBinding);
                    instanceMethodCacheTable[str] = methodTable;
                }
                catch (ManagementException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
                catch (COMException)
                {
                }
                return methodTable;
            }
        }

        protected abstract T GetManagementObjectMethod<T>(ManagementBaseObject wmiObject, string methodName) where T: PSMemberInfo;
        protected override T GetMember<T>(object obj, string memberName)
        {
            Adapter.tracer.WriteLine("Getting member with name {0}", new object[] { memberName });
            ManagementBaseObject wmiObject = obj as ManagementBaseObject;
            if (wmiObject != null)
            {
                PSProperty property = this.DoGetProperty(wmiObject, memberName);
                if (typeof(T).IsAssignableFrom(typeof(PSProperty)) && (property != null))
                {
                    return (property as T);
                }
                if (typeof(T).IsAssignableFrom(typeof(PSMethod)))
                {
                    T managementObjectMethod = this.GetManagementObjectMethod<T>(wmiObject, memberName);
                    if ((managementObjectMethod != null) && (property == null))
                    {
                        return managementObjectMethod;
                    }
                }
            }
            return default(T);
        }

        protected override PSMemberInfoInternalCollection<T> GetMembers<T>(object obj)
        {
            ManagementBaseObject wmiObject = (ManagementBaseObject) obj;
            PSMemberInfoInternalCollection<T> members = new PSMemberInfoInternalCollection<T>();
            this.AddAllProperties<T>(wmiObject, members);
            this.AddAllMethods<T>(wmiObject, members);
            return members;
        }

        internal static string GetMethodDefinition(MethodData mData)
        {
            SortedList parametersList = new SortedList();
            UpdateParameters(mData.InParameters, parametersList);
            StringBuilder builder = new StringBuilder();
            if (parametersList.Count > 0)
            {
                foreach (WMIParameterInformation information in parametersList.Values)
                {
                    string embeddedObjectTypeName = information.parameterType.ToString();
                    PropertyData pData = mData.InParameters.Properties[information.Name];
                    if (pData.Type == CimType.Object)
                    {
                        embeddedObjectTypeName = GetEmbeddedObjectTypeName(pData);
                        if (pData.IsArray)
                        {
                            embeddedObjectTypeName = embeddedObjectTypeName + "[]";
                        }
                    }
                    builder.Append(embeddedObjectTypeName);
                    builder.Append(" ");
                    builder.Append(information.Name);
                    builder.Append(", ");
                }
            }
            if (builder.Length > 2)
            {
                builder.Remove(builder.Length - 2, 2);
            }
            Adapter.tracer.WriteLine("Constructing method definition for method {0}", new object[] { mData.Name });
            StringBuilder builder2 = new StringBuilder();
            builder2.Append("System.Management.ManagementBaseObject ");
            builder2.Append(mData.Name);
            builder2.Append("(");
            builder2.Append(builder.ToString());
            builder2.Append(")");
            string str2 = builder2.ToString();
            Adapter.tracer.WriteLine("Definition constructed: {0}", new object[] { str2 });
            return str2;
        }

        internal static MethodInformation GetMethodInformation(MethodData mData)
        {
            SortedList parametersList = new SortedList();
            UpdateParameters(mData.InParameters, parametersList);
            WMIParameterInformation[] array = new WMIParameterInformation[parametersList.Count];
            if (parametersList.Count > 0)
            {
                parametersList.Values.CopyTo(array, 0);
            }
            return new MethodInformation(false, true, array);
        }

        protected override IEnumerable<string> GetTypeNameHierarchy (object obj)
		{
			ManagementBaseObject managementObj = obj as ManagementBaseObject;
			if (!managementObj.ObjectExits) {
				Console.WriteLine ("GetTypeNameHierarchy Object is NULLL!!!");
			}
            bool iteratorVariable1 = false;
            foreach (string iteratorVariable2 in Adapter.GetDotNetTypeNameHierarchy(obj))
            {
                if (!iteratorVariable1)
                {
                    iteratorVariable1 = true;
                    foreach (string iteratorVariable3 in this.GetTypeNameHierarchyFromDerivation(managementObj, iteratorVariable2, true))
                    {
                        yield return iteratorVariable3;
                    }
                    foreach (string iteratorVariable4 in this.GetTypeNameHierarchyFromDerivation(managementObj, iteratorVariable2, false))
                    {
                        yield return iteratorVariable4;
                    }
                }
                yield return iteratorVariable2;
            }
        }

        private IEnumerable<string> GetTypeNameHierarchyFromDerivation(ManagementBaseObject managementObj, string dotnetBaseType, bool shouldIncludeNamespace)
        {
            StringBuilder iteratorVariable0 = new StringBuilder(200);
            iteratorVariable0.Append(dotnetBaseType);
            iteratorVariable0.Append("#");
            if (shouldIncludeNamespace)
            {
                iteratorVariable0.Append(managementObj.SystemProperties["__NAMESPACE"].Value);
                iteratorVariable0.Append(@"\");
            }
            iteratorVariable0.Append(managementObj.SystemProperties["__CLASS"].Value);
            yield return iteratorVariable0.ToString();
            PropertyData iteratorVariable1 = managementObj.SystemProperties["__Derivation"];
            if (iteratorVariable1 != null)
            {
                string[] iteratorVariable2 = Adapter.PropertySetAndMethodArgumentConvertTo(iteratorVariable1.Value, typeof(string[]), CultureInfo.InvariantCulture) as string[];
                if (iteratorVariable2 != null)
                {
                    foreach (string iteratorVariable3 in iteratorVariable2)
                    {
                        iteratorVariable0.Clear();
                        iteratorVariable0.Append(dotnetBaseType);
                        iteratorVariable0.Append("#");
                        if (shouldIncludeNamespace)
                        {
                            iteratorVariable0.Append(managementObj.SystemProperties["__NAMESPACE"].Value);
                            iteratorVariable0.Append(@"\");
                        }
                        iteratorVariable0.Append(iteratorVariable3);
                        yield return iteratorVariable0.ToString();
                    }
                }
            }
        }

        protected abstract object InvokeManagementMethod(ManagementObject wmiObject, string methodName, ManagementBaseObject inParams);
        protected static bool IsStaticMethod(MethodData mdata)
        {
            bool flag2 = false;
            try
            {
                QualifierData data = mdata.Qualifiers["static"];
                if (data == null)
                {
                    return false;
                }
                bool result = false;
                LanguagePrimitives.TryConvertTo<bool>(data.Value, out result);
                flag2 = result;
            }
            catch (ManagementException)
            {
            }
            catch (COMException)
            {
            }
            return flag2;
        }

        protected override Collection<string> MethodDefinitions(PSMethod method)
        {
            WMIMethodCacheEntry adapterData = (WMIMethodCacheEntry) method.adapterData;
            return new Collection<string> { adapterData.MethodDefinition };
        }

        protected override object MethodInvoke(PSMethod method, object[] arguments)
        {
            ManagementObject baseObject = method.baseObject as ManagementObject;
            WMIMethodCacheEntry adapterData = (WMIMethodCacheEntry) method.adapterData;
            return this.AuxillaryInvokeMethod(baseObject, adapterData, arguments);
        }

        private static void PopulateMethodTable(ManagementClass mgmtClass, CacheTable methodTable, bool staticBinding)
        {
            MethodDataCollection methods = mgmtClass.Methods;
            if (methods != null)
            {
                ManagementPath classPath = mgmtClass.ClassPath;
                foreach (MethodData data in methods)
                {
                    if (IsStaticMethod(data) == staticBinding)
                    {
                        string name = data.Name;
                        WMIMethodCacheEntry member = new WMIMethodCacheEntry(name, classPath.Path, data);
                        methodTable.Add(name, member);
                    }
                }
            }
        }

        protected override AttributeCollection PropertyAttributes(PSProperty property)
        {
            return null;
        }

        protected override object PropertyGet(PSProperty property)
        {
            PropertyData adapterData = property.adapterData as PropertyData;
            return adapterData.Value;
        }

        protected override bool PropertyIsGettable(PSProperty property)
        {
            return true;
        }

        protected override bool PropertyIsSettable(PSProperty property)
        {
            ManagementBaseObject baseObject = property.baseObject as ManagementBaseObject;
            try
            {
                return (bool) CreateClassFrmObject(baseObject).GetPropertyQualifierValue(property.Name, "Write");
            }
            catch (ManagementException)
            {
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return true;
            }
            catch (COMException)
            {
                return true;
            }
        }

        protected override void PropertySet(PSProperty property, object setValue, bool convertIfPossible)
        {
            if (!(property.baseObject is ManagementBaseObject))
            {
                throw new SetValueInvocationException("CannotSetNonManagementObjectMsg", null, ExtendedTypeSystem.CannotSetNonManagementObject, new object[] { property.Name, property.baseObject.GetType().FullName, typeof(ManagementBaseObject).FullName });
            }
            if (!this.PropertyIsSettable(property))
            {
                throw new SetValueException("ReadOnlyWMIProperty", null, ExtendedTypeSystem.ReadOnlyProperty, new object[] { property.Name });
            }
            PropertyData adapterData = property.adapterData as PropertyData;
            if (convertIfPossible && (setValue != null))
            {
                Type dotNetType = GetDotNetType(adapterData);
                setValue = Adapter.PropertySetAndMethodArgumentConvertTo(setValue, dotNetType, CultureInfo.InvariantCulture);
            }
            adapterData.Value = setValue;
        }

        protected override string PropertyToString(PSProperty property)
        {
            StringBuilder builder = new StringBuilder();
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
            PropertyData adapterData = property.adapterData as PropertyData;
            Type dotNetType = GetDotNetType(adapterData);
            if (adapterData.Type == CimType.Object)
            {
                string embeddedObjectTypeName = GetEmbeddedObjectTypeName(adapterData);
                if (adapterData.IsArray)
                {
                    embeddedObjectTypeName = embeddedObjectTypeName + "[]";
                }
                return embeddedObjectTypeName;
            }
            return (forDisplay ? ToStringCodeMethods.Type(dotNetType, false) : dotNetType.ToString());
        }

        internal static void UpdateParameters(ManagementBaseObject parameters, SortedList parametersList)
        {
            if (parameters != null)
            {
                foreach (PropertyData data in parameters.Properties)
                {
                    int count = -1;
                    WMIParameterInformation information = new WMIParameterInformation(data.Name, GetDotNetType(data));
                    try
                    {
                        count = (int) data.Qualifiers["ID"].Value;
                    }
                    catch (ManagementException)
                    {
                    }
                    catch (COMException)
                    {
                    }
                    if (count < 0)
                    {
                        count = parametersList.Count;
                    }
                    parametersList[count] = information;
                }
            }
        }

        

        internal class WMIMethodCacheEntry
        {
            private string classPath;
            private string methodDefinition;
            private MethodInformation methodInfoStructure;
            private string name;

            internal WMIMethodCacheEntry(string n, string cPath, MethodData mData)
            {
                this.name = n;
                this.classPath = cPath;
                this.methodInfoStructure = BaseWMIAdapter.GetMethodInformation(mData);
                this.methodDefinition = BaseWMIAdapter.GetMethodDefinition(mData);
            }

            public string ClassPath
            {
                get
                {
                    return this.classPath;
                }
            }

            public string MethodDefinition
            {
                get
                {
                    return this.methodDefinition;
                }
            }

            public MethodInformation MethodInfoStructure
            {
                get
                {
                    return this.methodInfoStructure;
                }
            }

            public string Name
            {
                get
                {
                    return this.name;
                }
            }
        }

        internal class WMIParameterInformation : ParameterInformation
        {
            private string name;

            public WMIParameterInformation(string name, Type ty) : base(ty, true, null, false)
            {
                this.name = name;
            }

            public string Name
            {
                get
                {
                    return this.name;
                }
            }
        }
    }
}

