namespace System.Management.Automation
{
    using Microsoft.Win32;
    using System;
    using System.IO;
    using System.Reflection;
    using System.Security;

    internal sealed class RegistryStringResourceIndirect : IDisposable
    {
        private bool _disposed;
        private AppDomain _domain;
        private ResourceRetriever _resourceRetriever;

        private void CreateAppDomain()
        {
            if (this._domain == null)
            {
                this._domain = AppDomain.CreateDomain("ResourceIndirectDomain");
                this._resourceRetriever = (ResourceRetriever) this._domain.CreateInstanceAndUnwrap(Assembly.GetExecutingAssembly().FullName, "System.Management.Automation.ResourceRetriever");
            }
        }

        public void Dispose()
        {
            if (!this._disposed && (this._domain != null))
            {
                AppDomain.Unload(this._domain);
                this._domain = null;
                this._resourceRetriever = null;
            }
            this._disposed = true;
        }

        private static string GetRegKeyValueAsString(RegistryKey key, string valueName)
        {
            string str = null;
            try
            {
                if (key.GetValueKind(valueName) == RegistryValueKind.String)
                {
                    str = key.GetValue(valueName) as string;
                }
            }
            catch (ArgumentException)
            {
            }
            catch (IOException)
            {
            }
            catch (SecurityException)
            {
            }
            return str;
        }

        internal static RegistryStringResourceIndirect GetResourceIndirectReader()
        {
            return new RegistryStringResourceIndirect();
        }

        internal string GetResourceStringIndirect(string assemblyName, string modulePath, string baseNameRIDPair)
        {
            if (this._disposed)
            {
                throw PSTraceSource.NewInvalidOperationException("PSSnapinInfo", "ResourceReaderDisposed", new object[0]);
            }
            if (string.IsNullOrEmpty(assemblyName))
            {
                throw PSTraceSource.NewArgumentException("assemblyName");
            }
            if (string.IsNullOrEmpty(modulePath))
            {
                throw PSTraceSource.NewArgumentException("modulePath");
            }
            if (string.IsNullOrEmpty(baseNameRIDPair))
            {
                throw PSTraceSource.NewArgumentException("baseNameRIDPair");
            }
            string str = null;
            if (this._resourceRetriever == null)
            {
                this.CreateAppDomain();
            }
            if (this._resourceRetriever != null)
            {
                string[] strArray = baseNameRIDPair.Split(new char[] { ',' });
                if (strArray.Length == 2)
                {
                    string baseName = strArray[0];
                    string resourceID = strArray[1];
                    str = this._resourceRetriever.GetStringResource(assemblyName, modulePath, baseName, resourceID);
                }
            }
            return str;
        }

        internal string GetResourceStringIndirect(RegistryKey key, string valueName, string assemblyName, string modulePath)
        {
            if (this._disposed)
            {
                throw PSTraceSource.NewInvalidOperationException("PSSnapinInfo", "ResourceReaderDisposed", new object[0]);
            }
            if (key == null)
            {
                throw PSTraceSource.NewArgumentNullException("key");
            }
            if (string.IsNullOrEmpty(valueName))
            {
                throw PSTraceSource.NewArgumentException("valueName");
            }
            if (string.IsNullOrEmpty(assemblyName))
            {
                throw PSTraceSource.NewArgumentException("assemblyName");
            }
            if (string.IsNullOrEmpty(modulePath))
            {
                throw PSTraceSource.NewArgumentException("modulePath");
            }
            string str = null;
            string regKeyValueAsString = GetRegKeyValueAsString(key, valueName);
            if (regKeyValueAsString != null)
            {
                str = this.GetResourceStringIndirect(assemblyName, modulePath, regKeyValueAsString);
            }
            return str;
        }
    }
}

