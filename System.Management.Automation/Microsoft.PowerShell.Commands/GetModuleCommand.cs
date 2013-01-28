namespace Microsoft.PowerShell.Commands
{
    using Microsoft.Management.Infrastructure;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;
    using System.Runtime.CompilerServices;
    using System.Threading;

    [OutputType(new Type[] { typeof(PSModuleInfo) }), Cmdlet("Get", "Module", DefaultParameterSetName="Loaded", HelpUri="http://go.microsoft.com/fwlink/?LinkID=141552")]
    public sealed class GetModuleCommand : ModuleCmdletBase, IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private bool _disposed;
        private const string ParameterSet_AvailableInCimSession = "CimSession";
        private const string ParameterSet_AvailableInPsrpSession = "PsSession";
        private const string ParameterSet_AvailableLocally = "Available";
        private const string ParameterSet_Loaded = "Loaded";

        private void AssertListAvailableMode()
        {
            if (!this.ListAvailable.IsPresent)
            {
                ArgumentException exception = new ArgumentException(Modules.RemoteDiscoveryWorksOnlyInListAvailableMode);
                ErrorRecord errorRecord = new ErrorRecord(exception, "RemoteDiscoveryWorksOnlyInListAvailableMode", ErrorCategory.InvalidArgument, null);
                base.ThrowTerminatingError(errorRecord);
            }
        }

        private PSModuleInfo ConvertCimModuleInfoToPSModuleInfo(RemoteDiscoveryHelper.CimModule cimModule, string computerName)
        {
            try
            {
                bool containedErrors = false;
                if (cimModule.MainManifest == null)
                {
                    return this.GetModuleInfoForRemoteModuleWithoutManifest(cimModule);
                }
                string temporaryModuleManifestPath = Path.Combine(RemoteDiscoveryHelper.GetModulePath(cimModule.ModuleName, null, computerName, base.Context.CurrentRunspace), Path.GetFileName(cimModule.ModuleName));
                Hashtable originalManifest = null;
                if (!containedErrors)
                {
                    originalManifest = RemoteDiscoveryHelper.ConvertCimModuleFileToManifestHashtable(cimModule.MainManifest, temporaryModuleManifestPath, this, ref containedErrors);
                    if (originalManifest == null)
                    {
                        return this.GetModuleInfoForRemoteModuleWithoutManifest(cimModule);
                    }
                }
                if (!containedErrors)
                {
                    originalManifest = RemoteDiscoveryHelper.RewriteManifest(originalManifest);
                }
                Hashtable localizedData = originalManifest;
                PSModuleInfo moduleInfoForRemoteModuleWithoutManifest = null;
                if (!containedErrors)
                {
                    ModuleCmdletBase.ImportModuleOptions options = new ModuleCmdletBase.ImportModuleOptions();
                    moduleInfoForRemoteModuleWithoutManifest = base.LoadModuleManifest(temporaryModuleManifestPath, null, originalManifest, localizedData, 0, base.BaseMinimumVersion, base.BaseRequiredVersion, ref options, ref containedErrors);
                }
                if ((moduleInfoForRemoteModuleWithoutManifest == null) || containedErrors)
                {
                    moduleInfoForRemoteModuleWithoutManifest = this.GetModuleInfoForRemoteModuleWithoutManifest(cimModule);
                }
                return moduleInfoForRemoteModuleWithoutManifest;
            }
            catch (Exception exception)
            {
                ErrorRecord errorRecordForProcessingOfCimModule = RemoteDiscoveryHelper.GetErrorRecordForProcessingOfCimModule(exception, cimModule.ModuleName);
                base.WriteError(errorRecordForProcessingOfCimModule);
                return null;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    this._cancellationTokenSource.Dispose();
                }
                this._disposed = true;
            }
        }

        private void GetAvailableLocallyModules(string[] name, bool all)
        {
            bool isPresent = this.Refresh.IsPresent;
            foreach (PSModuleInfo info in base.GetModule(name, all, isPresent))
            {
                PSObject sendToPipeline = new PSObject(info);
                sendToPipeline.TypeNames.Insert(0, "ModuleInfoGrouping");
                base.WriteObject(sendToPipeline);
            }
        }

        private void GetAvailableViaCimSession(string[] moduleNames, Microsoft.Management.Infrastructure.CimSession cimSession, Uri resourceUri, string cimNamespace)
        {
            foreach (PSModuleInfo info in this.GetAvailableViaCimSessionCore(moduleNames, cimSession, resourceUri, cimNamespace))
            {
                RemoteDiscoveryHelper.AssociatePSModuleInfoWithSession(info, cimSession, resourceUri, cimNamespace);
                base.WriteObject(info);
            }
        }

        private IEnumerable<PSModuleInfo> GetAvailableViaCimSessionCore(IEnumerable<string> moduleNames, Microsoft.Management.Infrastructure.CimSession cimSession, Uri resourceUri, string cimNamespace)
        {
            return (from cimModule in RemoteDiscoveryHelper.GetCimModules(cimSession, resourceUri, cimNamespace, moduleNames, true, this, this.CancellationToken)
                select this.ConvertCimModuleInfoToPSModuleInfo(cimModule, cimSession.ComputerName) into moduleInfo
                where moduleInfo != null
                select moduleInfo);
        }

        private void GetAvailableViaPsrpSession(string[] moduleNames, System.Management.Automation.Runspaces.PSSession session)
        {
            foreach (PSModuleInfo info in this.GetAvailableViaPsrpSessionCore(moduleNames, session.Runspace))
            {
                RemoteDiscoveryHelper.AssociatePSModuleInfoWithSession(info, session);
                base.WriteObject(info);
            }
        }

        private IEnumerable<PSModuleInfo> GetAvailableViaPsrpSessionCore(string[] moduleNames, Runspace remoteRunspace)
        {
            using (PowerShell iteratorVariable0 = PowerShell.Create())
            {
                iteratorVariable0.Runspace = remoteRunspace;
                iteratorVariable0.AddCommand("Get-Module");
                iteratorVariable0.AddParameter("ListAvailable", true);
                if (this.Refresh.IsPresent)
                {
                    iteratorVariable0.AddParameter("Refresh", true);
                }
                if (moduleNames != null)
                {
                    iteratorVariable0.AddParameter("Name", moduleNames);
                }
                string errorMessageTemplate = string.Format(CultureInfo.InvariantCulture, Modules.RemoteDiscoveryRemotePsrpCommandFailed, new object[] { "Get-Module" });
                foreach (PSObject iteratorVariable2 in RemoteDiscoveryHelper.InvokePowerShell(iteratorVariable0, this.CancellationToken, this, errorMessageTemplate))
                {
                    PSModuleInfo iteratorVariable3 = RemoteDiscoveryHelper.RehydratePSModuleInfo(iteratorVariable2);
                    yield return iteratorVariable3;
                }
            }
        }

        private void GetLoadedModules(string[] name, bool all)
        {
            foreach (PSModuleInfo info in base.Context.Modules.GetModules(name, all))
            {
                base.WriteObject(info);
            }
        }

        private PSModuleInfo GetModuleInfoForRemoteModuleWithoutManifest(RemoteDiscoveryHelper.CimModule cimModule)
        {
            return new PSModuleInfo(cimModule.ModuleName, null, null);
        }

        private static string GetModuleName(object moduleObject)
        {
            if (moduleObject == null)
            {
                return null;
            }
            PSPropertyInfo info = PSObject.AsPSObject(moduleObject).Properties["Name"];
            if (info == null)
            {
                return null;
            }
            return (info.Value as string);
        }

        protected override void ProcessRecord()
        {
            if (base.ParameterSetName.Equals("Loaded", StringComparison.OrdinalIgnoreCase))
            {
                this.GetLoadedModules(this.Name, (bool) this.All);
            }
            else if (base.ParameterSetName.Equals("Available", StringComparison.OrdinalIgnoreCase))
            {
                if (this.ListAvailable.IsPresent)
                {
                    this.GetAvailableLocallyModules(this.Name, (bool) this.All);
                }
                else
                {
                    this.GetLoadedModules(this.Name, (bool) this.All);
                }
            }
            else if (base.ParameterSetName.Equals("PsSession", StringComparison.OrdinalIgnoreCase))
            {
                this.AssertListAvailableMode();
                this.GetAvailableViaPsrpSession(this.Name, this.PSSession);
            }
            else if (base.ParameterSetName.Equals("CimSession", StringComparison.OrdinalIgnoreCase))
            {
                this.AssertListAvailableMode();
                this.GetAvailableViaCimSession(this.Name, this.CimSession, this.CimResourceUri, this.CimNamespace);
            }
        }

        protected override void StopProcessing()
        {
            this._cancellationTokenSource.Cancel();
        }

        [Parameter(ParameterSetName="Available"), Parameter(ParameterSetName="Loaded")]
        public SwitchParameter All { get; set; }

        private System.Threading.CancellationToken CancellationToken
        {
            get
            {
                return this._cancellationTokenSource.Token;
            }
        }

        [ValidateNotNullOrEmpty, Parameter(ParameterSetName="CimSession", Mandatory=false)]
        public string CimNamespace { get; set; }

        [Parameter(ParameterSetName="CimSession", Mandatory=false), ValidateNotNull]
        public Uri CimResourceUri { get; set; }

        [Parameter(ParameterSetName="CimSession", Mandatory=true), ValidateNotNull]
        public Microsoft.Management.Infrastructure.CimSession CimSession { get; set; }

        [Parameter(ParameterSetName="CimSession"), Parameter(ParameterSetName="PsSession"), Parameter(ParameterSetName="Available", Mandatory=true)]
        public SwitchParameter ListAvailable { get; set; }

        [Parameter(ParameterSetName="CimSession", ValueFromPipeline=true, Position=0), Parameter(ParameterSetName="Available", ValueFromPipeline=true, Position=0), Parameter(ParameterSetName="PsSession", ValueFromPipeline=true, Position=0), Parameter(ParameterSetName="Loaded", ValueFromPipeline=true, Position=0)]
        public string[] Name { get; set; }

        [ValidateNotNull, Parameter(ParameterSetName="PsSession", Mandatory=true)]
        public System.Management.Automation.Runspaces.PSSession PSSession { get; set; }

        [Parameter(ParameterSetName="PsSession"), Parameter(ParameterSetName="Available"), Parameter(ParameterSetName="CimSession")]
        public SwitchParameter Refresh { get; set; }

        
    }
}

