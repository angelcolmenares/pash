namespace System.Management.Automation
{
    using System;
    using System.Management;

    internal class ManagementClassApdapter : BaseWMIAdapter
    {
        protected override void AddAllMethods<T>(ManagementBaseObject wmiObject, PSMemberInfoInternalCollection<T> members)
        {
            if (typeof(T).IsAssignableFrom(typeof(PSMethod)))
            {
                foreach (BaseWMIAdapter.WMIMethodCacheEntry entry in BaseWMIAdapter.GetInstanceMethodTable(wmiObject, true).memberCollection)
                {
                    if (members[entry.Name] == null)
                    {
                        Adapter.tracer.WriteLine("Adding method {0}", new object[] { entry.Name });
                        members.Add(new PSMethod(entry.Name, this, wmiObject, entry) as T);
                    }
                }
            }
        }

        protected override void AddAllProperties<T>(ManagementBaseObject wmiObject, PSMemberInfoInternalCollection<T> members)
        {
            if (wmiObject.SystemProperties != null)
            {
                foreach (PropertyData data in wmiObject.SystemProperties)
                {
                    members.Add(new PSProperty(data.Name, this, wmiObject, data) as T);
                }
            }
        }

        protected override PSProperty DoGetProperty(ManagementBaseObject wmiObject, string propertyName)
        {
            if (wmiObject.SystemProperties != null)
            {
                foreach (PropertyData data in wmiObject.SystemProperties)
                {
                    if (propertyName.Equals(data.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        return new PSProperty(data.Name, this, wmiObject, data);
                    }
                }
            }
            return null;
        }

        protected override T GetManagementObjectMethod<T>(ManagementBaseObject wmiObject, string methodName)
        {
            if (!typeof(T).IsAssignableFrom(typeof(PSMethod)))
            {
                return default(T);
            }
            BaseWMIAdapter.WMIMethodCacheEntry adapterData = (BaseWMIAdapter.WMIMethodCacheEntry) BaseWMIAdapter.GetInstanceMethodTable(wmiObject, true)[methodName];
            if (adapterData == null)
            {
                return default(T);
            }
            return (new PSMethod(adapterData.Name, this, wmiObject, adapterData) as T);
        }

        protected override object InvokeManagementMethod(ManagementObject wmiObject, string methodName, ManagementBaseObject inParams)
        {
            object obj2;
            Adapter.tracer.WriteLine("Invoking class method: {0}", new object[] { methodName });
            ManagementClass class2 = wmiObject as ManagementClass;
            try
            {
                obj2 = class2.InvokeMethod(methodName, inParams, null);
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw new MethodInvocationException("WMIMethodException", exception, ExtendedTypeSystem.WMIMethodInvocationException, new object[] { methodName, exception.Message });
            }
            return obj2;
        }
    }
}

