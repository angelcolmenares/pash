namespace System.Management.Automation.Runspaces
{
    using Microsoft.PowerShell;
    using Microsoft.PowerShell.Commands.Internal.Format;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;

    public abstract class RunspaceConfiguration
    {
        private RunspaceConfigurationEntryCollection<AssemblyConfigurationEntry> _assemblies;
        private System.Management.Automation.AuthorizationManager _authorizationManager;
        private RunspaceConfigurationEntryCollection<CmdletConfigurationEntry> _cmdlets;
        private RunspaceConfigurationEntryCollection<FormatConfigurationEntry> _formats;
        private PSHost _host;
        private RunspaceConfigurationEntryCollection<ScriptConfigurationEntry> _initializationScripts;
        private bool _initialized;
        private RunspaceConfigurationEntryCollection<ProviderConfigurationEntry> _providers;
        private RunspaceConfigurationEntryCollection<ScriptConfigurationEntry> _scripts;
        private object _syncObject = new object();
        private RunspaceConfigurationEntryCollection<TypeConfigurationEntry> _types;
        private TypeInfoDataBaseManager formatDBManger = new TypeInfoDataBaseManager();
        [TraceSource("RunspaceInit", "Initialization code for Runspace")]
        private static PSTraceSource runspaceInitTracer = PSTraceSource.GetTracer("RunspaceInit", "Initialization code for Runspace", false);
        private System.Management.Automation.Runspaces.TypeTable typeTable;
		private bool _importSystemModules;

        protected RunspaceConfiguration()
        {
        }

        public PSSnapInInfo AddPSSnapIn(string name, out PSSnapInException warning)
        {
            return this.DoAddPSSnapIn(name, out warning);
        }

		public bool ImportSystemModules {
			get { return _importSystemModules; }
			set { _importSystemModules = value; }
		}

        internal void Bind(ExecutionContext executionContext)
        {
            this._host = executionContext.EngineHostInterface;
            this.Initialize(executionContext);
            this.Assemblies.OnUpdate += new RunspaceConfigurationEntryUpdateEventHandler(executionContext.UpdateAssemblyCache);
            runspaceInitTracer.WriteLine("initializing assembly list", new object[0]);
            try
            {
                this.Assemblies.Update(true);
            }
            catch (RuntimeException exception)
            {
                runspaceInitTracer.WriteLine("assembly list initialization failed", new object[0]);
                MshLog.LogEngineHealthEvent(executionContext, 0x67, exception, Severity.Error);
                executionContext.ReportEngineStartupError(exception.Message);
                throw;
            }
            if (executionContext.CommandDiscovery != null)
            {
                this.Cmdlets.OnUpdate += new RunspaceConfigurationEntryUpdateEventHandler(executionContext.CommandDiscovery.UpdateCmdletCache);
                runspaceInitTracer.WriteLine("initializing cmdlet list", new object[0]);
                try
                {
                    this.Cmdlets.Update(true);
                }
                catch (PSNotSupportedException exception2)
                {
                    runspaceInitTracer.WriteLine("cmdlet list initialization failed", new object[0]);
                    MshLog.LogEngineHealthEvent(executionContext, 0x67, exception2, Severity.Error);
                    executionContext.ReportEngineStartupError(exception2.Message);
                    throw;
                }
            }
            if (executionContext.EngineSessionState != null)
            {
                this.Providers.OnUpdate += new RunspaceConfigurationEntryUpdateEventHandler(executionContext.EngineSessionState.UpdateProviders);
                runspaceInitTracer.WriteLine("initializing provider list", new object[0]);
                try
                {
                    this.Providers.Update(true);
                }
                catch (PSNotSupportedException exception3)
                {
                    runspaceInitTracer.WriteLine("provider list initialization failed", new object[0]);
                    MshLog.LogEngineHealthEvent(executionContext, 0x67, exception3, Severity.Error);
                    executionContext.ReportEngineStartupError(exception3.Message);
                    throw;
                }
            }
        }

        public static RunspaceConfiguration Create(bool importSystemModules = false)
        {
            return RunspaceConfigForSingleShell.CreateDefaultConfiguration();
        }

        private static RunspaceConfiguration Create(Assembly assembly)
        {
            RunspaceConfiguration configuration;
            if (assembly == null)
            {
                throw PSTraceSource.NewArgumentNullException("assembly");
            }
            object[] customAttributes = assembly.GetCustomAttributes(typeof(RunspaceConfigurationTypeAttribute), false);
            if ((customAttributes == null) || (customAttributes.Length == 0))
            {
                throw new RunspaceConfigurationAttributeException("RunspaceConfigurationAttributeNotExist", assembly.FullName);
            }
            if (customAttributes.Length > 1)
            {
                throw new RunspaceConfigurationAttributeException("RunspaceConfigurationAttributeDuplicate", assembly.FullName);
            }
            RunspaceConfigurationTypeAttribute attribute = (RunspaceConfigurationTypeAttribute) customAttributes[0];
            try
            {
                configuration = Create(assembly.GetType(attribute.RunspaceConfigurationType, true));
            }
            catch (SecurityException)
            {
                throw new RunspaceConfigurationTypeException(assembly.FullName, attribute.RunspaceConfigurationType);
            }
            return configuration;
        }

        public static RunspaceConfiguration Create(string assemblyName)
        {
            if (string.IsNullOrEmpty(assemblyName))
            {
                throw PSTraceSource.NewArgumentNullException("assemblyName");
            }
            Assembly assembly = null;
            foreach (Assembly assembly2 in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (string.Equals(assembly2.GetName().Name, assemblyName, StringComparison.OrdinalIgnoreCase))
                {
                    assembly = assembly2;
                    break;
                }
            }
            if (assembly == null)
            {
                assembly = Assembly.Load(assemblyName);
            }
            return Create(assembly);
        }

        private static RunspaceConfiguration Create(Type runspaceConfigType)
        {
            MethodInfo method = runspaceConfigType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);
            if (method == null)
            {
                return null;
            }
            return (RunspaceConfiguration) method.Invoke(null, null);
        }

        public static RunspaceConfiguration Create(string consoleFilePath, out PSConsoleLoadException warnings)
        {
            return RunspaceConfigForSingleShell.Create(consoleFilePath, out warnings);
        }

        internal virtual PSSnapInInfo DoAddPSSnapIn(string name, out PSSnapInException warning)
        {
            throw PSTraceSource.NewNotSupportedException();
        }

        internal virtual PSSnapInInfo DoRemovePSSnapIn(string name, out PSSnapInException warning)
        {
            throw PSTraceSource.NewNotSupportedException();
        }

        internal void Initialize(ExecutionContext executionContext)
        {
            lock (this._syncObject)
            {
                if (!this._initialized)
                {
                    this._initialized = true;
                    this.Types.OnUpdate += new RunspaceConfigurationEntryUpdateEventHandler(this.UpdateTypes);
                    this.Formats.OnUpdate += new RunspaceConfigurationEntryUpdateEventHandler(this.UpdateFormats);
                    runspaceInitTracer.WriteLine("initializing types information", new object[0]);
                    try
                    {
                        this.UpdateTypes(true);
                    }
                    catch (RuntimeException exception)
                    {
                        runspaceInitTracer.WriteLine("type information initialization failed", new object[0]);
                        MshLog.LogEngineHealthEvent(executionContext, 0x67, exception, Severity.Warning);
                        executionContext.ReportEngineStartupError(exception.Message);
                    }
                    runspaceInitTracer.WriteLine("initializing format information", new object[0]);
                    try
                    {
                        this.UpdateFormats(true);
                    }
                    catch (RuntimeException exception2)
                    {
                        runspaceInitTracer.WriteLine("format information initialization failed", new object[0]);
                        MshLog.LogEngineHealthEvent(executionContext, 0x67, exception2, Severity.Warning);
                        executionContext.ReportEngineStartupError(exception2.Message);
                    }
                }
            }
        }

        private void RemoveNeedlessEntries(RunspaceConfigurationCategory category, IList<int> entryIndicesToRemove)
        {
            for (int i = entryIndicesToRemove.Count - 1; i >= 0; i--)
            {
                if (category == RunspaceConfigurationCategory.Types)
                {
                    this.Types.RemoveItem(entryIndicesToRemove[i]);
                }
                else if (category == RunspaceConfigurationCategory.Formats)
                {
                    this.Formats.RemoveItem(entryIndicesToRemove[i]);
                }
            }
        }

        public PSSnapInInfo RemovePSSnapIn(string name, out PSSnapInException warning)
        {
            return this.DoRemovePSSnapIn(name, out warning);
        }

        internal void Unbind(ExecutionContext executionContext)
        {
            if (executionContext != null)
            {
                if (executionContext.CommandDiscovery != null)
                {
                    this.Cmdlets.OnUpdate -= new RunspaceConfigurationEntryUpdateEventHandler(executionContext.CommandDiscovery.UpdateCmdletCache);
                }
                if (executionContext.EngineSessionState != null)
                {
                    this.Providers.OnUpdate -= new RunspaceConfigurationEntryUpdateEventHandler(executionContext.EngineSessionState.UpdateProviders);
                }
                this.Assemblies.OnUpdate -= new RunspaceConfigurationEntryUpdateEventHandler(executionContext.UpdateAssemblyCache);
            }
        }

        internal void UpdateFormats()
        {
            this.UpdateFormats(false);
        }

        private void UpdateFormats(bool preValidated)
        {
            Collection<string> independentErrors = new Collection<string>();
            Collection<int> entryIndicesToRemove = new Collection<int>();
            Collection<PSSnapInTypeAndFormatErrors> mshsnapins = FormatAndTypeDataHelper.GetFormatAndTypesErrors(this, this._host, this.Formats, RunspaceConfigurationCategory.Formats, independentErrors, entryIndicesToRemove);
            if (entryIndicesToRemove.Count > 0)
            {
                this.RemoveNeedlessEntries(RunspaceConfigurationCategory.Formats, entryIndicesToRemove);
            }
            this.FormatDBManager.UpdateDataBase(mshsnapins, this.AuthorizationManager, this._host, preValidated);
            FormatAndTypeDataHelper.ThrowExceptionOnError("ErrorsUpdatingFormats", independentErrors, mshsnapins, RunspaceConfigurationCategory.Formats);
        }

        internal void UpdateTypes()
        {
            this.UpdateTypes(false);
        }

        internal void UpdateTypes(bool preValidated)
        {
            Collection<string> independentErrors = new Collection<string>();
            Collection<int> entryIndicesToRemove = new Collection<int>();
            Collection<PSSnapInTypeAndFormatErrors> psSnapinTypes = FormatAndTypeDataHelper.GetFormatAndTypesErrors(this, this._host, this.Types, RunspaceConfigurationCategory.Types, independentErrors, entryIndicesToRemove);
            if (entryIndicesToRemove.Count > 0)
            {
                this.RemoveNeedlessEntries(RunspaceConfigurationCategory.Types, entryIndicesToRemove);
            }
            this.TypeTable.Update(psSnapinTypes, this._authorizationManager, this._host, preValidated);
            FormatAndTypeDataHelper.ThrowExceptionOnError("ErrorsUpdatingTypes", independentErrors, psSnapinTypes, RunspaceConfigurationCategory.Types);
        }

        public virtual RunspaceConfigurationEntryCollection<AssemblyConfigurationEntry> Assemblies
        {
            get
            {
                if (this._assemblies == null)
                {
                    this._assemblies = new RunspaceConfigurationEntryCollection<AssemblyConfigurationEntry>();
                }
                return this._assemblies;
            }
        }

        public virtual System.Management.Automation.AuthorizationManager AuthorizationManager
        {
            get
            {
                if (this._authorizationManager == null)
                {
                    this._authorizationManager = new PSAuthorizationManager(this.ShellId);
                }
                return this._authorizationManager;
            }
        }

        public virtual RunspaceConfigurationEntryCollection<CmdletConfigurationEntry> Cmdlets
        {
            get
            {
                if (this._cmdlets == null)
                {
                    this._cmdlets = new RunspaceConfigurationEntryCollection<CmdletConfigurationEntry>();
                }
                return this._cmdlets;
            }
        }

        internal TypeInfoDataBaseManager FormatDBManager
        {
            get
            {
                return this.formatDBManger;
            }
        }

        public virtual RunspaceConfigurationEntryCollection<FormatConfigurationEntry> Formats
        {
            get
            {
                if (this._formats == null)
                {
                    this._formats = new RunspaceConfigurationEntryCollection<FormatConfigurationEntry>();
                }
                return this._formats;
            }
        }

        public virtual RunspaceConfigurationEntryCollection<ScriptConfigurationEntry> InitializationScripts
        {
            get
            {
                if (this._initializationScripts == null)
                {
                    this._initializationScripts = new RunspaceConfigurationEntryCollection<ScriptConfigurationEntry>();
                }
                return this._initializationScripts;
            }
        }

        public virtual RunspaceConfigurationEntryCollection<ProviderConfigurationEntry> Providers
        {
            get
            {
                if (this._providers == null)
                {
                    this._providers = new RunspaceConfigurationEntryCollection<ProviderConfigurationEntry>();
                }
                return this._providers;
            }
        }

        public virtual RunspaceConfigurationEntryCollection<ScriptConfigurationEntry> Scripts
        {
            get
            {
                if (this._scripts == null)
                {
                    this._scripts = new RunspaceConfigurationEntryCollection<ScriptConfigurationEntry>();
                }
                return this._scripts;
            }
        }

        public abstract string ShellId { get; }

        public virtual RunspaceConfigurationEntryCollection<TypeConfigurationEntry> Types
        {
            get
            {
                if (this._types == null)
                {
                    this._types = new RunspaceConfigurationEntryCollection<TypeConfigurationEntry>();
                }
                return this._types;
            }
        }

        internal System.Management.Automation.Runspaces.TypeTable TypeTable
        {
            get
            {
                if (this.typeTable == null)
                {
                    this.typeTable = new System.Management.Automation.Runspaces.TypeTable();
                }
                return this.typeTable;
            }
        }
    }
}

