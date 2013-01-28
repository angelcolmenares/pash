namespace System.Management.Automation.Remoting
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Reflection;

    internal class HostDefaultData
    {
        private Dictionary<HostDefaultDataId, object> data = new Dictionary<HostDefaultDataId, object>();

        private HostDefaultData()
        {
        }

        internal static HostDefaultData Create(PSHostRawUserInterface hostRawUI)
        {
            if (hostRawUI == null)
            {
                return null;
            }
            HostDefaultData data = new HostDefaultData();
            try
            {
                data.SetValue(HostDefaultDataId.ForegroundColor, hostRawUI.ForegroundColor);
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
            }
            try
            {
                data.SetValue(HostDefaultDataId.BackgroundColor, hostRawUI.BackgroundColor);
            }
            catch (Exception exception2)
            {
                CommandProcessorBase.CheckForSevereException(exception2);
            }
            try
            {
                data.SetValue(HostDefaultDataId.CursorPosition, hostRawUI.CursorPosition);
            }
            catch (Exception exception3)
            {
                CommandProcessorBase.CheckForSevereException(exception3);
            }
            try
            {
                data.SetValue(HostDefaultDataId.WindowPosition, hostRawUI.WindowPosition);
            }
            catch (Exception exception4)
            {
                CommandProcessorBase.CheckForSevereException(exception4);
            }
            try
            {
                data.SetValue(HostDefaultDataId.CursorSize, hostRawUI.CursorSize);
            }
            catch (Exception exception5)
            {
                CommandProcessorBase.CheckForSevereException(exception5);
            }
            try
            {
                data.SetValue(HostDefaultDataId.BufferSize, hostRawUI.BufferSize);
            }
            catch (Exception exception6)
            {
                CommandProcessorBase.CheckForSevereException(exception6);
            }
            try
            {
                data.SetValue(HostDefaultDataId.WindowSize, hostRawUI.WindowSize);
            }
            catch (Exception exception7)
            {
                CommandProcessorBase.CheckForSevereException(exception7);
            }
            try
            {
                data.SetValue(HostDefaultDataId.MaxWindowSize, hostRawUI.MaxWindowSize);
            }
            catch (Exception exception8)
            {
                CommandProcessorBase.CheckForSevereException(exception8);
            }
            try
            {
                data.SetValue(HostDefaultDataId.MaxPhysicalWindowSize, hostRawUI.MaxPhysicalWindowSize);
            }
            catch (Exception exception9)
            {
                CommandProcessorBase.CheckForSevereException(exception9);
            }
            try
            {
                data.SetValue(HostDefaultDataId.WindowTitle, hostRawUI.WindowTitle);
            }
            catch (Exception exception10)
            {
                CommandProcessorBase.CheckForSevereException(exception10);
            }
            return data;
        }

        internal object GetValue(HostDefaultDataId id)
        {
            if (this.data.ContainsKey(id))
            {
                return this.data[id];
            }
            return null;
        }

        internal bool HasValue(HostDefaultDataId id)
        {
            return this.data.ContainsKey(id);
        }

        internal void SetValue(HostDefaultDataId id, object dataValue)
        {
            this.data[id] = dataValue;
        }

        internal object this[HostDefaultDataId id]
        {
            get
            {
                return this.GetValue(id);
            }
        }
    }
}

