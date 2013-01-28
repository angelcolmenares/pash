namespace System.Management.Automation
{
    using System;
    using System.Globalization;
    using System.Management;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Tracing;

    internal class ManagementObjectAdapter : ManagementClassApdapter
    {
        protected override void AddAllMethods<T>(ManagementBaseObject wmiObject, PSMemberInfoInternalCollection<T> members)
        {
            if (typeof(T).IsAssignableFrom(typeof(PSMethod)))
            {
                foreach (BaseWMIAdapter.WMIMethodCacheEntry entry in BaseWMIAdapter.GetInstanceMethodTable(wmiObject, false).memberCollection)
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
            base.AddAllProperties<T>(wmiObject, members);
            if (wmiObject.Properties != null)
            {
                foreach (PropertyData data in wmiObject.Properties)
                {
                    members.Add(new PSProperty(data.Name, this, wmiObject, data) as T);
                }
            }
        }

        protected override PSProperty DoGetProperty(ManagementBaseObject wmiObject, string propertyName)
        {
            PropertyData adapterData = null;
            PSProperty property = base.DoGetProperty(wmiObject, propertyName);
            if (property != null)
            {
                return property;
            }
            try
            {
                adapterData = wmiObject.Properties[propertyName];
                return new PSProperty(adapterData.Name, this, wmiObject, adapterData);
            }
            catch (ManagementException)
            {
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                new PSEtwLogProvider().WriteEvent(PSEventId.Engine_Health, PSChannel.Analytic, PSOpcode.Exception, PSLevel.Informational, PSTask.None, PSKeyword.UseAlwaysOperational, new object[] { string.Format(CultureInfo.InvariantCulture, "ManagementBaseObjectAdapter::DoGetProperty::PropertyName:{0}, Exception:{1}, StackTrace:{2}", new object[] { propertyName, exception.Message, exception.StackTrace }), string.Empty, string.Empty });
            }
            return null;
        }

        protected override T GetManagementObjectMethod<T>(ManagementBaseObject wmiObject, string methodName)
        {
            if (!typeof(T).IsAssignableFrom(typeof(PSMethod)))
            {
                return default(T);
            }
            BaseWMIAdapter.WMIMethodCacheEntry adapterData = (BaseWMIAdapter.WMIMethodCacheEntry) BaseWMIAdapter.GetInstanceMethodTable(wmiObject, false)[methodName];
            if (adapterData == null)
            {
                return default(T);
            }
            return (new PSMethod(adapterData.Name, this, wmiObject, adapterData) as T);
        }

        protected override object InvokeManagementMethod(ManagementObject obj, string methodName, ManagementBaseObject inParams)
        {
            object obj3;
            Adapter.tracer.WriteLine("Invoking class method: {0}", new object[] { methodName });
            try
            {
                obj3 = obj.InvokeMethod(methodName, inParams, null);
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                throw new MethodInvocationException("WMIMethodException", exception, ExtendedTypeSystem.WMIMethodInvocationException, new object[] { methodName, exception.Message });
            }
            return obj3;
        }
    }
}

