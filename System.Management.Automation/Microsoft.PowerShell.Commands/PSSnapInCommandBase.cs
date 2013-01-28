namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Management.Automation.Runspaces;

    public abstract class PSSnapInCommandBase : PSCmdlet, IDisposable
    {
        private bool _disposed;
        private bool _shouldGetAll;
        internal const string resBaseName = "MshSnapInCmdletResources";
        private RegistryStringResourceIndirect resourceReader;

        protected PSSnapInCommandBase()
        {
        }

        public void Dispose()
        {
            if (!this._disposed)
            {
                if (this.resourceReader != null)
                {
                    this.resourceReader.Dispose();
                    this.resourceReader = null;
                }
                GC.SuppressFinalize(this);
            }
            this._disposed = true;
        }

        protected override void EndProcessing()
        {
            if (this.resourceReader != null)
            {
                this.resourceReader.Dispose();
                this.resourceReader = null;
            }
        }

        protected internal Collection<PSSnapInInfo> GetSnapIns(string pattern)
        {
            if (this.Runspace != null)
            {
                if (pattern != null)
                {
                    return this.Runspace.ConsoleInfo.GetPSSnapIn(pattern, this._shouldGetAll);
                }
                return this.Runspace.ConsoleInfo.PSSnapIns;
            }
            WildcardPattern pattern2 = null;
            if (!string.IsNullOrEmpty(pattern))
            {
                if (!WildcardPattern.ContainsWildcardCharacters(pattern))
                {
                    PSSnapInInfo.VerifyPSSnapInFormatThrowIfError(pattern);
                }
                pattern2 = new WildcardPattern(pattern, WildcardOptions.IgnoreCase);
            }
            Collection<PSSnapInInfo> collection = new Collection<PSSnapInInfo>();
            if (this._shouldGetAll)
            {
                foreach (PSSnapInInfo info in PSSnapInReader.ReadAll())
                {
                    if ((pattern2 == null) || pattern2.IsMatch(info.Name))
                    {
                        collection.Add(info);
                    }
                }
                return collection;
            }
            List<CmdletInfo> cmdlets = base.InvokeCommand.GetCmdlets();
            Dictionary<PSSnapInInfo, bool> dictionary = new Dictionary<PSSnapInInfo, bool>();
            foreach (CmdletInfo info2 in cmdlets)
            {
                PSSnapInInfo pSSnapIn = info2.PSSnapIn;
                if ((pSSnapIn != null) && !dictionary.ContainsKey(pSSnapIn))
                {
                    dictionary.Add(pSSnapIn, true);
                }
            }
            foreach (PSSnapInInfo info4 in dictionary.Keys)
            {
                if ((pattern2 == null) || pattern2.IsMatch(info4.Name))
                {
                    collection.Add(info4);
                }
            }
            return collection;
        }

        internal static PSSnapInInfo IsSnapInLoaded(Collection<PSSnapInInfo> loadedSnapins, PSSnapInInfo psSnapInInfo)
        {
            if (loadedSnapins != null)
            {
                foreach (PSSnapInInfo info in loadedSnapins)
                {
                    string assemblyName = info.AssemblyName;
                    if ((string.Equals(info.Name, psSnapInInfo.Name, StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(assemblyName)) && string.Equals(assemblyName, psSnapInInfo.AssemblyName, StringComparison.OrdinalIgnoreCase))
                    {
                        return info;
                    }
                }
            }
            return null;
        }

        internal Collection<string> SearchListForPattern(Collection<PSSnapInInfo> searchList, string pattern)
        {
            Collection<string> collection = new Collection<string>();
            if (searchList != null)
            {
                WildcardPattern pattern2 = new WildcardPattern(pattern, WildcardOptions.IgnoreCase);
                foreach (PSSnapInInfo info in searchList)
                {
                    if (pattern2.IsMatch(info.Name))
                    {
                        collection.Add(info.Name);
                    }
                }
            }
            return collection;
        }

        internal void WriteNonTerminatingError(object targetObject, string errorId, Exception innerException, ErrorCategory category)
        {
            base.WriteError(new ErrorRecord(innerException, errorId, category, targetObject));
        }

        internal RegistryStringResourceIndirect ResourceReader
        {
            get
            {
                if (this.resourceReader == null)
                {
                    this.resourceReader = RegistryStringResourceIndirect.GetResourceIndirectReader();
                }
                return this.resourceReader;
            }
        }

        internal RunspaceConfigForSingleShell Runspace
        {
            get
            {
                RunspaceConfigForSingleShell runspaceConfiguration = base.Context.RunspaceConfiguration as RunspaceConfigForSingleShell;
                if (runspaceConfiguration == null)
                {
                    return null;
                }
                return runspaceConfiguration;
            }
        }

        protected internal bool ShouldGetAll
        {
            get
            {
                return this._shouldGetAll;
            }
            set
            {
                this._shouldGetAll = value;
            }
        }
    }
}

