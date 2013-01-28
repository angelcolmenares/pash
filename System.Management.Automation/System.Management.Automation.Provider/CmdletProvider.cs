namespace System.Management.Automation.Provider
{
    using System;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Runspaces;
    using System.Resources;
    using System.Runtime.InteropServices;
    using System.Security.AccessControl;
    using System.Threading;

    public abstract class CmdletProvider : IResourceSupplier
    {
        private CmdletProviderContext contextBase;
        [TraceSource("CmdletProviderClasses", "The namespace provider base classes tracer")]
        internal static PSTraceSource providerBaseTracer = PSTraceSource.GetTracer("CmdletProviderClasses", "The namespace provider base classes tracer");
        private System.Management.Automation.ProviderInfo providerInformation;

        protected CmdletProvider()
        {
        }

        private static void CheckIfSecurityDescriptorInterfaceIsSupported(ISecurityDescriptorCmdletProvider permissionProvider)
        {
            if (permissionProvider == null)
            {
                throw PSTraceSource.NewNotSupportedException("ProviderBaseSecurity", "ISecurityDescriptorCmdletProvider_NotSupported", new object[0]);
            }
        }

        internal void ClearContent(string path, CmdletProviderContext cmdletProviderContext)
        {
            this.Context = cmdletProviderContext;
            IContentCmdletProvider provider = this as IContentCmdletProvider;
            if (provider == null)
            {
                throw PSTraceSource.NewNotSupportedException("SessionStateStrings", "IContentCmdletProvider_NotSupported", new object[0]);
            }
            provider.ClearContent(path);
        }

        internal object ClearContentDynamicParameters(string path, CmdletProviderContext cmdletProviderContext)
        {
            this.Context = cmdletProviderContext;
            IContentCmdletProvider provider = this as IContentCmdletProvider;
            if (provider == null)
            {
                return null;
            }
            return provider.ClearContentDynamicParameters(path);
        }

        internal void ClearProperty(string path, Collection<string> propertyName, CmdletProviderContext cmdletProviderContext)
        {
            this.Context = cmdletProviderContext;
            IPropertyCmdletProvider provider = this as IPropertyCmdletProvider;
            if (provider == null)
            {
                throw PSTraceSource.NewNotSupportedException("SessionStateStrings", "IPropertyCmdletProvider_NotSupported", new object[0]);
            }
            provider.ClearProperty(path, propertyName);
        }

        internal object ClearPropertyDynamicParameters(string path, Collection<string> providerSpecificPickList, CmdletProviderContext cmdletProviderContext)
        {
            this.Context = cmdletProviderContext;
            IPropertyCmdletProvider provider = this as IPropertyCmdletProvider;
            if (provider == null)
            {
                return null;
            }
            return provider.ClearPropertyDynamicParameters(path, providerSpecificPickList);
        }

        internal void CopyProperty(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty, CmdletProviderContext cmdletProviderContext)
        {
            this.Context = cmdletProviderContext;
            IDynamicPropertyCmdletProvider provider = this as IDynamicPropertyCmdletProvider;
            if (provider == null)
            {
                throw PSTraceSource.NewNotSupportedException("SessionStateStrings", "IDynamicPropertyCmdletProvider_NotSupported", new object[0]);
            }
            provider.CopyProperty(sourcePath, sourceProperty, destinationPath, destinationProperty);
        }

        internal object CopyPropertyDynamicParameters(string path, string sourceProperty, string destinationPath, string destinationProperty, CmdletProviderContext cmdletProviderContext)
        {
            this.Context = cmdletProviderContext;
            IDynamicPropertyCmdletProvider provider = this as IDynamicPropertyCmdletProvider;
            if (provider == null)
            {
                return null;
            }
            return provider.CopyPropertyDynamicParameters(path, sourceProperty, destinationPath, destinationProperty);
        }

        internal IContentReader GetContentReader(string path, CmdletProviderContext cmdletProviderContext)
        {
            this.Context = cmdletProviderContext;
            IContentCmdletProvider provider = this as IContentCmdletProvider;
            if (provider == null)
            {
                throw PSTraceSource.NewNotSupportedException("SessionStateStrings", "IContentCmdletProvider_NotSupported", new object[0]);
            }
            return provider.GetContentReader(path);
        }

        internal object GetContentReaderDynamicParameters(string path, CmdletProviderContext cmdletProviderContext)
        {
            this.Context = cmdletProviderContext;
            IContentCmdletProvider provider = this as IContentCmdletProvider;
            if (provider == null)
            {
                return null;
            }
            return provider.GetContentReaderDynamicParameters(path);
        }

        internal IContentWriter GetContentWriter(string path, CmdletProviderContext cmdletProviderContext)
        {
            this.Context = cmdletProviderContext;
            IContentCmdletProvider provider = this as IContentCmdletProvider;
            if (provider == null)
            {
                throw PSTraceSource.NewNotSupportedException("SessionStateStrings", "IContentCmdletProvider_NotSupported", new object[0]);
            }
            return provider.GetContentWriter(path);
        }

        internal object GetContentWriterDynamicParameters(string path, CmdletProviderContext cmdletProviderContext)
        {
            this.Context = cmdletProviderContext;
            IContentCmdletProvider provider = this as IContentCmdletProvider;
            if (provider == null)
            {
                return null;
            }
            return provider.GetContentWriterDynamicParameters(path);
        }

        internal void GetProperty(string path, Collection<string> providerSpecificPickList, CmdletProviderContext cmdletProviderContext)
        {
            this.Context = cmdletProviderContext;
            IPropertyCmdletProvider provider = this as IPropertyCmdletProvider;
            if (provider == null)
            {
                throw PSTraceSource.NewNotSupportedException("SessionStateStrings", "IPropertyCmdletProvider_NotSupported", new object[0]);
            }
            provider.GetProperty(path, providerSpecificPickList);
        }

        internal object GetPropertyDynamicParameters(string path, Collection<string> providerSpecificPickList, CmdletProviderContext cmdletProviderContext)
        {
            this.Context = cmdletProviderContext;
            IPropertyCmdletProvider provider = this as IPropertyCmdletProvider;
            if (provider == null)
            {
                return null;
            }
            return provider.GetPropertyDynamicParameters(path, providerSpecificPickList);
        }

        public virtual string GetResourceString(string baseName, string resourceId)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                if (string.IsNullOrEmpty(baseName))
                {
                    throw PSTraceSource.NewArgumentException("baseName");
                }
                if (string.IsNullOrEmpty(resourceId))
                {
                    throw PSTraceSource.NewArgumentException("resourceId");
                }
                ResourceManager resourceManager = ResourceManagerCache.GetResourceManager(base.GetType().Assembly, baseName);
                string str = null;
                try
                {
                    str = resourceManager.GetString(resourceId, Thread.CurrentThread.CurrentUICulture);
                }
                catch (MissingManifestResourceException)
                {
                    throw PSTraceSource.NewArgumentException("baseName", "GetErrorText", "ResourceBaseNameFailure", new object[] { baseName });
                }
                if (str == null)
                {
                    throw PSTraceSource.NewArgumentException("resourceId", "GetErrorText", "ResourceIdFailure", new object[] { resourceId });
                }
                return str;
            }
        }

        internal void GetSecurityDescriptor(string path, AccessControlSections sections, CmdletProviderContext context)
        {
            this.Context = context;
            ISecurityDescriptorCmdletProvider permissionProvider = this as ISecurityDescriptorCmdletProvider;
            CheckIfSecurityDescriptorInterfaceIsSupported(permissionProvider);
            permissionProvider.GetSecurityDescriptor(path, sections);
        }

        internal virtual bool IsFilterSet()
        {
            return !string.IsNullOrEmpty(this.Filter);
        }

        internal void MoveProperty(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty, CmdletProviderContext cmdletProviderContext)
        {
            this.Context = cmdletProviderContext;
            IDynamicPropertyCmdletProvider provider = this as IDynamicPropertyCmdletProvider;
            if (provider == null)
            {
                throw PSTraceSource.NewNotSupportedException("SessionStateStrings", "IDynamicPropertyCmdletProvider_NotSupported", new object[0]);
            }
            provider.MoveProperty(sourcePath, sourceProperty, destinationPath, destinationProperty);
        }

        internal object MovePropertyDynamicParameters(string path, string sourceProperty, string destinationPath, string destinationProperty, CmdletProviderContext cmdletProviderContext)
        {
            this.Context = cmdletProviderContext;
            IDynamicPropertyCmdletProvider provider = this as IDynamicPropertyCmdletProvider;
            if (provider == null)
            {
                return null;
            }
            return provider.MovePropertyDynamicParameters(path, sourceProperty, destinationPath, destinationProperty);
        }

        internal void NewProperty(string path, string propertyName, string propertyTypeName, object value, CmdletProviderContext cmdletProviderContext)
        {
            this.Context = cmdletProviderContext;
            IDynamicPropertyCmdletProvider provider = this as IDynamicPropertyCmdletProvider;
            if (provider == null)
            {
                throw PSTraceSource.NewNotSupportedException("SessionStateStrings", "IDynamicPropertyCmdletProvider_NotSupported", new object[0]);
            }
            provider.NewProperty(path, propertyName, propertyTypeName, value);
        }

        internal object NewPropertyDynamicParameters(string path, string propertyName, string propertyTypeName, object value, CmdletProviderContext cmdletProviderContext)
        {
            this.Context = cmdletProviderContext;
            IDynamicPropertyCmdletProvider provider = this as IDynamicPropertyCmdletProvider;
            if (provider == null)
            {
                return null;
            }
            return provider.NewPropertyDynamicParameters(path, propertyName, propertyTypeName, value);
        }

        internal void RemoveProperty(string path, string propertyName, CmdletProviderContext cmdletProviderContext)
        {
            this.Context = cmdletProviderContext;
            IDynamicPropertyCmdletProvider provider = this as IDynamicPropertyCmdletProvider;
            if (provider == null)
            {
                throw PSTraceSource.NewNotSupportedException("SessionStateStrings", "IDynamicPropertyCmdletProvider_NotSupported", new object[0]);
            }
            provider.RemoveProperty(path, propertyName);
        }

        internal object RemovePropertyDynamicParameters(string path, string propertyName, CmdletProviderContext cmdletProviderContext)
        {
            this.Context = cmdletProviderContext;
            IDynamicPropertyCmdletProvider provider = this as IDynamicPropertyCmdletProvider;
            if (provider == null)
            {
                return null;
            }
            return provider.RemovePropertyDynamicParameters(path, propertyName);
        }

        internal void RenameProperty(string path, string propertyName, string newPropertyName, CmdletProviderContext cmdletProviderContext)
        {
            this.Context = cmdletProviderContext;
            IDynamicPropertyCmdletProvider provider = this as IDynamicPropertyCmdletProvider;
            if (provider == null)
            {
                throw PSTraceSource.NewNotSupportedException("SessionStateStrings", "IDynamicPropertyCmdletProvider_NotSupported", new object[0]);
            }
            provider.RenameProperty(path, propertyName, newPropertyName);
        }

        internal object RenamePropertyDynamicParameters(string path, string sourceProperty, string destinationProperty, CmdletProviderContext cmdletProviderContext)
        {
            this.Context = cmdletProviderContext;
            IDynamicPropertyCmdletProvider provider = this as IDynamicPropertyCmdletProvider;
            if (provider == null)
            {
                return null;
            }
            return provider.RenamePropertyDynamicParameters(path, sourceProperty, destinationProperty);
        }

        internal void SetProperty(string path, PSObject propertyValue, CmdletProviderContext cmdletProviderContext)
        {
            this.Context = cmdletProviderContext;
            IPropertyCmdletProvider provider = this as IPropertyCmdletProvider;
            if (provider == null)
            {
                throw PSTraceSource.NewNotSupportedException("SessionStateStrings", "IPropertyCmdletProvider_NotSupported", new object[0]);
            }
            provider.SetProperty(path, propertyValue);
        }

        internal object SetPropertyDynamicParameters(string path, PSObject propertyValue, CmdletProviderContext cmdletProviderContext)
        {
            this.Context = cmdletProviderContext;
            IPropertyCmdletProvider provider = this as IPropertyCmdletProvider;
            if (provider == null)
            {
                return null;
            }
            return provider.SetPropertyDynamicParameters(path, propertyValue);
        }

        internal void SetProviderInformation(System.Management.Automation.ProviderInfo providerInfoToSet)
        {
            if (providerInfoToSet == null)
            {
                throw PSTraceSource.NewArgumentNullException("providerInfoToSet");
            }
            this.providerInformation = providerInfoToSet;
        }

        internal void SetSecurityDescriptor(string path, ObjectSecurity securityDescriptor, CmdletProviderContext context)
        {
            this.Context = context;
            ISecurityDescriptorCmdletProvider permissionProvider = this as ISecurityDescriptorCmdletProvider;
            CheckIfSecurityDescriptorInterfaceIsSupported(permissionProvider);
            permissionProvider.SetSecurityDescriptor(path, securityDescriptor);
        }

        public bool ShouldContinue(string query, string caption)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                return this.Context.ShouldContinue(query, caption);
            }
        }

        public bool ShouldContinue(string query, string caption, ref bool yesToAll, ref bool noToAll)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                return this.Context.ShouldContinue(query, caption, ref yesToAll, ref noToAll);
            }
        }

        public bool ShouldProcess(string target)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                return this.Context.ShouldProcess(target);
            }
        }

        public bool ShouldProcess(string target, string action)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                return this.Context.ShouldProcess(target, action);
            }
        }

        public bool ShouldProcess(string verboseDescription, string verboseWarning, string caption)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                return this.Context.ShouldProcess(verboseDescription, verboseWarning, caption);
            }
        }

        public bool ShouldProcess(string verboseDescription, string verboseWarning, string caption, out ShouldProcessReason shouldProcessReason)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                return this.Context.ShouldProcess(verboseDescription, verboseWarning, caption, out shouldProcessReason);
            }
        }

        protected virtual System.Management.Automation.ProviderInfo Start(System.Management.Automation.ProviderInfo providerInfo)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                return providerInfo;
            }
        }

        internal System.Management.Automation.ProviderInfo Start(System.Management.Automation.ProviderInfo providerInfo, CmdletProviderContext cmdletProviderContext)
        {
            this.Context = cmdletProviderContext;
            return this.Start(providerInfo);
        }

        protected virtual object StartDynamicParameters()
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                return null;
            }
        }

        internal object StartDynamicParameters(CmdletProviderContext cmdletProviderContext)
        {
            this.Context = cmdletProviderContext;
            return this.StartDynamicParameters();
        }

        protected virtual void Stop()
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
            }
        }

        internal void Stop(CmdletProviderContext cmdletProviderContext)
        {
            this.Context = cmdletProviderContext;
            this.Stop();
        }

        protected internal virtual void StopProcessing()
        {
        }

        public void ThrowTerminatingError(ErrorRecord errorRecord)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                if (errorRecord == null)
                {
                    throw PSTraceSource.NewArgumentNullException("errorRecord");
                }
                if ((errorRecord.ErrorDetails != null) && (errorRecord.ErrorDetails.TextLookupError != null))
                {
                    Exception textLookupError = errorRecord.ErrorDetails.TextLookupError;
                    errorRecord.ErrorDetails.TextLookupError = null;
                    MshLog.LogProviderHealthEvent(this.Context.ExecutionContext, this.ProviderInfo.Name, textLookupError, Severity.Warning);
                }
                ProviderInvocationException exception2 = new ProviderInvocationException(this.ProviderInfo, errorRecord);
                MshLog.LogProviderHealthEvent(this.Context.ExecutionContext, this.ProviderInfo.Name, exception2, Severity.Warning);
                throw exception2;
            }
        }

        public bool TransactionAvailable()
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                if (this.Context == null)
                {
                    return false;
                }
                return this.Context.TransactionAvailable();
            }
        }

        private PSObject WrapOutputInPSObject(object item, string path)
        {
            if (item == null)
            {
                throw PSTraceSource.NewArgumentNullException("item");
            }
            PSObject obj2 = new PSObject(item);
            PSObject obj3 = item as PSObject;
            if (obj3 != null)
            {
                obj2.InternalTypeNames = new ConsolidatedString(obj3.InternalTypeNames);
            }
            string providerQualifiedPath = LocationGlobber.GetProviderQualifiedPath(path, this.ProviderInfo);
            obj2.AddOrSetProperty("PSPath", providerQualifiedPath);
            providerBaseTracer.WriteLine("Attaching {0} = {1}", new object[] { "PSPath", providerQualifiedPath });
            NavigationCmdletProvider provider = this as NavigationCmdletProvider;
            if ((provider != null) && (path != null))
            {
                string str2 = null;
                if (this.PSDriveInfo != null)
                {
                    str2 = provider.GetParentPath(path, this.PSDriveInfo.Root, this.Context);
                }
                else
                {
                    str2 = provider.GetParentPath(path, string.Empty, this.Context);
                }
                string str3 = string.Empty;
                if (!string.IsNullOrEmpty(str2))
                {
                    str3 = LocationGlobber.GetProviderQualifiedPath(str2, this.ProviderInfo);
                }
                obj2.AddOrSetProperty("PSParentPath", str3);
                providerBaseTracer.WriteLine("Attaching {0} = {1}", new object[] { "PSParentPath", str3 });
                string childName = provider.GetChildName(path, this.Context);
                obj2.AddOrSetProperty("PSChildName", childName);
                providerBaseTracer.WriteLine("Attaching {0} = {1}", new object[] { "PSChildName", childName });
            }
            if (this.PSDriveInfo != null)
            {
                obj2.AddOrSetProperty(this.PSDriveInfo.GetNotePropertyForProviderCmdlets("PSDrive"));
                providerBaseTracer.WriteLine("Attaching {0} = {1}", new object[] { "PSDrive", this.PSDriveInfo });
            }
            obj2.AddOrSetProperty(this.ProviderInfo.GetNotePropertyForProviderCmdlets("PSProvider"));
            providerBaseTracer.WriteLine("Attaching {0} = {1}", new object[] { "PSProvider", this.ProviderInfo });
            return obj2;
        }

        public void WriteDebug(string text)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                this.Context.WriteDebug(text);
            }
        }

        public void WriteError(ErrorRecord errorRecord)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                if (errorRecord == null)
                {
                    throw PSTraceSource.NewArgumentNullException("errorRecord");
                }
                if ((errorRecord.ErrorDetails != null) && (errorRecord.ErrorDetails.TextLookupError != null))
                {
                    MshLog.LogProviderHealthEvent(this.Context.ExecutionContext, this.ProviderInfo.Name, errorRecord.ErrorDetails.TextLookupError, Severity.Warning);
                }
                this.Context.WriteError(errorRecord);
            }
        }

        public void WriteItemObject(object item, string path, bool isContainer)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                this.WriteObject(item, path, isContainer);
            }
        }

        private void WriteObject(object item, string path)
        {
            PSObject obj2 = this.WrapOutputInPSObject(item, path);
            this.Context.WriteObject(obj2);
        }

        private void WriteObject(object item, string path, bool isContainer)
        {
            PSObject obj2 = this.WrapOutputInPSObject(item, path);
            obj2.AddOrSetProperty("PSIsContainer", isContainer ? Boxed.True : Boxed.False);
            providerBaseTracer.WriteLine("Attaching {0} = {1}", new object[] { "PSIsContainer", isContainer });
            this.Context.WriteObject(obj2);
        }

        public void WriteProgress(ProgressRecord progressRecord)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                if (progressRecord == null)
                {
                    throw PSTraceSource.NewArgumentNullException("progressRecord");
                }
                this.Context.WriteProgress(progressRecord);
            }
        }

        public void WritePropertyObject(object propertyValue, string path)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                this.WriteObject(propertyValue, path);
            }
        }

        public void WriteSecurityDescriptorObject(ObjectSecurity securityDescriptor, string path)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                this.WriteObject(securityDescriptor, path);
            }
        }

        public void WriteVerbose(string text)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                this.Context.WriteVerbose(text);
            }
        }

        public void WriteWarning(string text)
        {
            using (PSTransactionManager.GetEngineProtectionScope())
            {
                this.Context.WriteWarning(text);
            }
        }

        internal CmdletProviderContext Context
        {
            get
            {
                return this.contextBase;
            }
            set
            {
                if (value == null)
                {
                    throw PSTraceSource.NewArgumentNullException("value");
                }
                if (((value.Credential != null) && (value.Credential != PSCredential.Empty)) && !CmdletProviderManagementIntrinsics.CheckProviderCapabilities(ProviderCapabilities.Credentials, this.providerInformation))
                {
                    throw PSTraceSource.NewNotSupportedException("SessionStateStrings", "Credentials_NotSupported", new object[0]);
                }
                if ((((this.providerInformation != null) && !string.IsNullOrEmpty(this.providerInformation.Name)) && (this.providerInformation.Name.Equals("FileSystem") && (value.Credential != null))) && ((value.Credential != PSCredential.Empty) && !value.ExecutionContext.CurrentCommandProcessor.Command.GetType().Name.Equals("NewPSDriveCommand")))
                {
                    throw PSTraceSource.NewNotSupportedException("SessionStateStrings", "FileSystemProviderCredentials_NotSupported", new object[0]);
                }
                if (!string.IsNullOrEmpty(value.Filter) && !CmdletProviderManagementIntrinsics.CheckProviderCapabilities(ProviderCapabilities.Filter, this.providerInformation))
                {
                    throw PSTraceSource.NewNotSupportedException("SessionStateStrings", "Filter_NotSupported", new object[0]);
                }
                if (value.UseTransaction && !CmdletProviderManagementIntrinsics.CheckProviderCapabilities(ProviderCapabilities.Transactions, this.providerInformation))
                {
                    throw PSTraceSource.NewNotSupportedException("SessionStateStrings", "Transactions_NotSupported", new object[0]);
                }
                this.contextBase = value;
                this.contextBase.ProviderInstance = this;
            }
        }

        public PSCredential Credential
        {
            get
            {
                using (PSTransactionManager.GetEngineProtectionScope())
                {
                    return this.Context.Credential;
                }
            }
        }

        public PSTransactionContext CurrentPSTransaction
        {
            get
            {
                if (this.Context == null)
                {
                    return null;
                }
                return this.Context.CurrentPSTransaction;
            }
        }

        protected object DynamicParameters
        {
            get
            {
                using (PSTransactionManager.GetEngineProtectionScope())
                {
                    return this.Context.DynamicParameters;
                }
            }
        }

        public Collection<string> Exclude
        {
            get
            {
                using (PSTransactionManager.GetEngineProtectionScope())
                {
                    return this.Context.Exclude;
                }
            }
        }

        public string Filter
        {
            get
            {
                using (PSTransactionManager.GetEngineProtectionScope())
                {
                    return this.Context.Filter;
                }
            }
        }

        public SwitchParameter Force
        {
            get
            {
                using (PSTransactionManager.GetEngineProtectionScope())
                {
                    return this.Context.Force;
                }
            }
        }

        public PSHost Host
        {
            get
            {
                using (PSTransactionManager.GetEngineProtectionScope())
                {
                    return this.Context.ExecutionContext.EngineHostInterface;
                }
            }
        }

        public Collection<string> Include
        {
            get
            {
                using (PSTransactionManager.GetEngineProtectionScope())
                {
                    return this.Context.Include;
                }
            }
        }

        public CommandInvocationIntrinsics InvokeCommand
        {
            get
            {
                using (PSTransactionManager.GetEngineProtectionScope())
                {
                    return new CommandInvocationIntrinsics(this.Context.ExecutionContext);
                }
            }
        }

        public ProviderIntrinsics InvokeProvider
        {
            get
            {
                using (PSTransactionManager.GetEngineProtectionScope())
                {
                    return new ProviderIntrinsics(this.Context.ExecutionContext.EngineSessionState);
                }
            }
        }

        protected internal System.Management.Automation.ProviderInfo ProviderInfo
        {
            get
            {
                using (PSTransactionManager.GetEngineProtectionScope())
                {
                    return this.providerInformation;
                }
            }
        }

        protected System.Management.Automation.PSDriveInfo PSDriveInfo
        {
            get
            {
                using (PSTransactionManager.GetEngineProtectionScope())
                {
                    return this.Context.Drive;
                }
            }
        }

        public System.Management.Automation.SessionState SessionState
        {
            get
            {
                using (PSTransactionManager.GetEngineProtectionScope())
                {
                    return new System.Management.Automation.SessionState(this.Context.ExecutionContext.EngineSessionState);
                }
            }
        }

        public bool Stopping
        {
            get
            {
                using (PSTransactionManager.GetEngineProtectionScope())
                {
                    return this.Context.Stopping;
                }
            }
        }
    }
}

