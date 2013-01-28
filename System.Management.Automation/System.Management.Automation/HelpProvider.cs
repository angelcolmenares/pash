namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Management.Automation.Runspaces;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal abstract class HelpProvider
    {
        private System.Management.Automation.HelpSystem _helpSystem;

        internal HelpProvider(System.Management.Automation.HelpSystem helpSystem)
        {
            this._helpSystem = helpSystem;
        }

        internal bool AreSnapInsSupported()
        {
            return (this._helpSystem.ExecutionContext.RunspaceConfiguration is RunspaceConfigForSingleShell);
        }

        internal abstract IEnumerable<HelpInfo> ExactMatchHelp(HelpRequest helpRequest);
        internal string GetDefaultShellSearchPath()
        {
            string shellPathFromRegistry = CommandDiscovery.GetShellPathFromRegistry(this.HelpSystem.ExecutionContext.ShellID);
            if (shellPathFromRegistry == null)
            {
                return Path.GetDirectoryName(PsUtils.GetMainModule(Process.GetCurrentProcess()).FileName);
            }
            if (OSHelper.IsWindows) shellPathFromRegistry = Path.GetDirectoryName(shellPathFromRegistry);
            if (!Directory.Exists(shellPathFromRegistry))
            {
                shellPathFromRegistry = Path.GetDirectoryName(PsUtils.GetMainModule(Process.GetCurrentProcess()).FileName);
            }
            return shellPathFromRegistry;
        }

        internal Collection<string> GetSearchPaths()
        {
            Collection<string> searchPaths = this.HelpSystem.GetSearchPaths();
            searchPaths.Add(this.GetDefaultShellSearchPath());
            return searchPaths;
        }

        internal virtual IEnumerable<HelpInfo> ProcessForwardedHelp(HelpInfo helpInfo, HelpRequest helpRequest)
        {
            helpInfo.ForwardHelpCategory ^= this.HelpCategory;
            yield return helpInfo;
        }

        internal void ReportHelpFileError(Exception exception, string target, string helpFile)
        {
            ErrorRecord item = new ErrorRecord(exception, "LoadHelpFileForTargetFailed", ErrorCategory.OpenError, null) {
                ErrorDetails = new ErrorDetails(Assembly.GetExecutingAssembly(), "HelpErrors", "LoadHelpFileForTargetFailed", new object[] { target, helpFile, exception.Message })
            };
            this.HelpSystem.LastErrors.Add(item);
        }

        internal virtual void Reset()
        {
        }

        internal abstract IEnumerable<HelpInfo> SearchHelp(HelpRequest helpRequest, bool searchOnlyContent);

        internal abstract System.Management.Automation.HelpCategory HelpCategory { get; }

        internal System.Management.Automation.HelpSystem HelpSystem
        {
            get
            {
                return this._helpSystem;
            }
        }

        internal abstract string Name { get; }

        
    }
}

