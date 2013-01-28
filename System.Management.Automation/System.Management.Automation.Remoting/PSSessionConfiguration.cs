namespace System.Management.Automation.Remoting
{
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Runspaces;
    using System.Management.Automation.Tracing;
    using System.Reflection;
    using System.Security;

    public abstract class PSSessionConfiguration : IDisposable
    {
        private const string configProviderApplicationBaseKeyName = "ApplicationBase";
        private const string configProviderAssemblyNameKeyName = "AssemblyName";
        private const string configProvidersKeyName = "PSConfigurationProviders";
        private const string resBaseName = "remotingerroridstrings";
        private static Dictionary<string, ConfigurationDataFromXML> ssnStateProviders = new Dictionary<string, ConfigurationDataFromXML>(StringComparer.OrdinalIgnoreCase);
        private static object syncObject = new object();
        [TraceSource("ServerRemoteSession", "ServerRemoteSession")]
        private static readonly PSTraceSource tracer = PSTraceSource.GetTracer("ServerRemoteSession", "ServerRemoteSession");

        protected PSSessionConfiguration()
        {
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
        }

        public virtual PSPrimitiveDictionary GetApplicationPrivateData(PSSenderInfo senderInfo)
        {
            return null;
        }

        private static RegistryKey GetConfigurationProvidersRegistryKey()
        {
            try
            {
                return PSSnapInReader.GetVersionRootKey(PSSnapInReader.GetMonadRootKey(), Utils.GetCurrentMajorVersion()).OpenSubKey("PSConfigurationProviders");
            }
            catch (ArgumentException)
            {
            }
            catch (SecurityException)
            {
            }
            return null;
        }

        public abstract InitialSessionState GetInitialSessionState(PSSenderInfo senderInfo);
        public virtual InitialSessionState GetInitialSessionState(PSSessionConfigurationData sessionConfigurationData, PSSenderInfo senderInfo, string configProviderId)
        {
            throw new NotImplementedException();
        }

        public virtual int? GetMaximumReceivedDataSizePerCommand(PSSenderInfo senderInfo)
        {
            return 0x3200000;
        }

        public virtual int? GetMaximumReceivedObjectSize(PSSenderInfo senderInfo)
        {
            return 0xa00000;
        }

        private static Type LoadAndAnalyzeAssembly(string shellId, string applicationBase, string assemblyName, string typeToLoad)
        {
            if ((string.IsNullOrEmpty(assemblyName) && !string.IsNullOrEmpty(typeToLoad)) || (!string.IsNullOrEmpty(assemblyName) && string.IsNullOrEmpty(typeToLoad)))
            {
                throw PSTraceSource.NewInvalidOperationException("remotingerroridstrings", "TypeNeedsAssembly", new object[] { "assemblyname", "pssessionconfigurationtypename", "InitializationParameters" });
            }
            Assembly assembly = null;
            if (!string.IsNullOrEmpty(assemblyName))
            {
                PSEtwLog.LogAnalyticVerbose(PSEventId.LoadingPSCustomShellAssembly, PSOpcode.Connect, PSTask.None, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic, new object[] { assemblyName, shellId });
                assembly = LoadSsnStateProviderAssembly(applicationBase, assemblyName);
                if (null == assembly)
                {
                    throw PSTraceSource.NewArgumentException("assemblyName", "remotingerroridstrings", "UnableToLoadAssembly", new object[] { assemblyName, "InitializationParameters" });
                }
            }
            if (null == assembly)
            {
                return typeof(DefaultRemotePowerShellConfiguration);
            }
            try
            {
                PSEtwLog.LogAnalyticVerbose(PSEventId.LoadingPSCustomShellType, PSOpcode.Connect, PSTask.None, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic, new object[] { typeToLoad, shellId });
                Type type = assembly.GetType(typeToLoad, true, true);
                if (null == type)
                {
                    throw PSTraceSource.NewArgumentException("typeToLoad", "remotingerroridstrings", "UnableToLoadType", new object[] { typeToLoad, "InitializationParameters" });
                }
                return type;
            }
            catch (ReflectionTypeLoadException)
            {
            }
            catch (TypeLoadException)
            {
            }
            catch (ArgumentException)
            {
            }
            catch (MissingMethodException)
            {
            }
            catch (InvalidCastException)
            {
            }
            catch (TargetInvocationException)
            {
            }
            throw PSTraceSource.NewArgumentException("typeToLoad", "remotingerroridstrings", "UnableToLoadType", new object[] { typeToLoad, "InitializationParameters" });
        }

        internal static ConfigurationDataFromXML LoadEndPointConfiguration(string shellId, string initializationParameters)
        {
            ConfigurationDataFromXML mxml = null;
            if (!ssnStateProviders.ContainsKey(initializationParameters))
            {
                LoadRSConfigProvider(shellId, initializationParameters);
            }
            lock (syncObject)
            {
                if (!ssnStateProviders.TryGetValue(initializationParameters, out mxml))
                {
                    throw PSTraceSource.NewInvalidOperationException("remotingerroridstrings", "NonExistentInitialSessionStateProvider", new object[] { shellId });
                }
            }
            return mxml;
        }

        private static void LoadRSConfigProvider(string shellId, string initializationParameters)
        {
            ConfigurationDataFromXML mxml = ConfigurationDataFromXML.Create(initializationParameters);
            Type type = LoadAndAnalyzeAssembly(shellId, mxml.ApplicationBase, mxml.AssemblyName, mxml.EndPointConfigurationTypeName);
            mxml.EndPointConfigurationType = type;
            lock (syncObject)
            {
                if (!ssnStateProviders.ContainsKey(initializationParameters))
                {
                    ssnStateProviders.Add(initializationParameters, mxml);
                }
            }
        }

        private static Assembly LoadSsnStateProviderAssembly(string applicationBase, string assemblyName)
        {
            string path = string.Empty;
            if (!string.IsNullOrEmpty(applicationBase))
            {
                try
                {
                    path = Directory.GetCurrentDirectory();
                    Directory.SetCurrentDirectory(applicationBase);
                }
                catch (ArgumentException exception)
                {
                    tracer.TraceWarning("Not able to change curent working directory to {0}: {1}", new object[] { applicationBase, exception.Message });
                }
                catch (PathTooLongException exception2)
                {
                    tracer.TraceWarning("Not able to change curent working directory to {0}: {1}", new object[] { applicationBase, exception2.Message });
                }
                catch (FileNotFoundException exception3)
                {
                    tracer.TraceWarning("Not able to change curent working directory to {0}: {1}", new object[] { applicationBase, exception3.Message });
                }
                catch (IOException exception4)
                {
                    tracer.TraceWarning("Not able to change curent working directory to {0}: {1}", new object[] { applicationBase, exception4.Message });
                }
                catch (SecurityException exception5)
                {
                    tracer.TraceWarning("Not able to change curent working directory to {0}: {1}", new object[] { applicationBase, exception5.Message });
                }
                catch (UnauthorizedAccessException exception6)
                {
                    tracer.TraceWarning("Not able to change curent working directory to {0}: {1}", new object[] { applicationBase, exception6.Message });
                }
            }
            Assembly assembly = null;
            try
            {
                try
                {
                    assembly = Assembly.Load(assemblyName);
                }
                catch (FileLoadException exception7)
                {
                    tracer.TraceWarning("Not able to load assembly {0}: {1}", new object[] { assemblyName, exception7.Message });
                }
                catch (BadImageFormatException exception8)
                {
                    tracer.TraceWarning("Not able to load assembly {0}: {1}", new object[] { assemblyName, exception8.Message });
                }
                catch (FileNotFoundException exception9)
                {
                    tracer.TraceWarning("Not able to load assembly {0}: {1}", new object[] { assemblyName, exception9.Message });
                }
                if (null != assembly)
                {
                    return assembly;
                }
                tracer.WriteLine("Loading assembly from path {0}", new object[] { applicationBase });
                try
                {
                    assembly = Assembly.LoadFrom(assemblyName);
                }
                catch (FileLoadException exception10)
                {
                    tracer.TraceWarning("Not able to load assembly {0}: {1}", new object[] { assemblyName, exception10.Message });
                }
                catch (BadImageFormatException exception11)
                {
                    tracer.TraceWarning("Not able to load assembly {0}: {1}", new object[] { assemblyName, exception11.Message });
                }
                catch (FileNotFoundException exception12)
                {
                    tracer.TraceWarning("Not able to load assembly {0}: {1}", new object[] { assemblyName, exception12.Message });
                }
            }
            finally
            {
                if (!string.IsNullOrEmpty(applicationBase))
                {
                    Directory.SetCurrentDirectory(path);
                }
            }
            return assembly;
        }

        private static string ReadStringValue(RegistryKey registryKey, string name, bool mandatory)
        {
            object obj2 = registryKey.GetValue(name);
            if ((obj2 == null) && mandatory)
            {
                tracer.TraceError("Mandatory property {0} not specified for registry key {1}", new object[] { name, registryKey.Name });
                throw PSTraceSource.NewArgumentException("name", "remotingerroridstrings", "MandatoryValueNotPresent", new object[] { name, registryKey.Name });
            }
            string str = obj2 as string;
            if (string.IsNullOrEmpty(str) && mandatory)
            {
                tracer.TraceError("Value is null or empty for mandatory property {0} in {1}", new object[] { name, registryKey.Name });
                throw PSTraceSource.NewArgumentException("name", "remotingerroridstrings", "MandatoryValueNotInCorrectFormat", new object[] { name, registryKey.Name });
            }
            return str;
        }
    }
}

