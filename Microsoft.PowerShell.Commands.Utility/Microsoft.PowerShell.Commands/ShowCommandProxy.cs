namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Threading;

    internal class ShowCommandProxy
    {
        private ShowCommandCommand cmdlet;
        private GraphicalHostReflectionWrapper graphicalHostReflectionWrapper;
        private const string ShowCommandHelperName = "Microsoft.PowerShell.Commands.ShowCommandInternal.ShowCommandHelper";

        internal ShowCommandProxy(ShowCommandCommand cmdlet)
        {
            this.cmdlet = cmdlet;
            this.graphicalHostReflectionWrapper = GraphicalHostReflectionWrapper.GetGraphicalHostReflectionWrapper(cmdlet, "Microsoft.PowerShell.Commands.ShowCommandInternal.ShowCommandHelper");
        }

        internal void ActivateWindow()
        {
            this.graphicalHostReflectionWrapper.CallMethod("ActivateWindow", new object[0]);
        }

        internal void CloseWindow()
        {
            this.graphicalHostReflectionWrapper.CallMethod("CloseWindow", new object[0]);
        }

        internal void DisplayHelp(Collection<PSObject> helpResults)
        {
            this.graphicalHostReflectionWrapper.CallMethod("DisplayHelp", new object[] { helpResults });
        }

        internal List<CommandInfo> GetCommandList(object[] commandObjects)
        {
            return (List<CommandInfo>) this.graphicalHostReflectionWrapper.CallStaticMethod("GetCommandList", new object[] { commandObjects });
        }

        internal object GetCommandViewModel(CommandInfo command, bool noCommonParameter, Dictionary<string, PSModuleInfo> importedModules, bool moduleQualify)
        {
            return this.graphicalHostReflectionWrapper.CallStaticMethod("GetCommandViewModel", new object[] { command, noCommonParameter, importedModules, moduleQualify });
        }

        internal string GetHelpCommand(string command)
        {
            return (string) this.graphicalHostReflectionWrapper.CallStaticMethod("GetHelpCommand", new object[] { command });
        }

        internal Dictionary<string, PSModuleInfo> GetImportedModulesDictionary(object[] moduleObjects)
        {
            return (Dictionary<string, PSModuleInfo>) this.graphicalHostReflectionWrapper.CallStaticMethod("GetImportedModulesDictionary", new object[] { moduleObjects });
        }

        internal string GetImportModuleCommand(string module)
        {
            return (string) this.graphicalHostReflectionWrapper.CallStaticMethod("GetImportModuleCommand", new object[] { module });
        }

        internal string GetScript()
        {
            return (string) this.graphicalHostReflectionWrapper.CallMethod("GetScript", new object[0]);
        }

        internal string GetShowAllModulesCommand()
        {
            return (string) this.graphicalHostReflectionWrapper.CallStaticMethod("GetShowAllModulesCommand", new object[0]);
        }

        internal string GetShowCommandCommand(string commandName, bool includeAliasAndModules)
        {
            return (string) this.graphicalHostReflectionWrapper.CallStaticMethod("GetShowCommandCommand", new object[] { commandName, includeAliasAndModules });
        }

        internal void ImportModuleDone(Dictionary<string, PSModuleInfo> importedModules, IEnumerable<CommandInfo> commands)
        {
            this.graphicalHostReflectionWrapper.CallMethod("ImportModuleDone", new object[] { importedModules, commands });
        }

        internal void ImportModuleFailed(Exception reason)
        {
            this.graphicalHostReflectionWrapper.CallMethod("ImportModuleFailed", new object[] { reason });
        }

        internal bool SetPendingISECommand(string command)
        {
            return (bool) this.graphicalHostReflectionWrapper.CallMethod("SetPendingISECommand", new object[] { command });
        }

        internal void ShowAllModulesWindow(Dictionary<string, PSModuleInfo> importedModules, IEnumerable<CommandInfo> commands, bool noCommonParameter, bool passThrough)
        {
            this.graphicalHostReflectionWrapper.CallMethod("ShowAllModulesWindow", new object[] { this.cmdlet, importedModules, commands, noCommonParameter, this.cmdlet.Width, this.cmdlet.Height, passThrough });
        }

        internal void ShowCommandWindow(object commandViewModelObj, bool passThrough)
        {
            this.graphicalHostReflectionWrapper.CallMethod("ShowCommandWindow", new object[] { this.cmdlet, commandViewModelObj, this.cmdlet.Width, this.cmdlet.Height, passThrough });
        }

        internal void ShowErrorString(string error)
        {
            this.graphicalHostReflectionWrapper.CallMethod("ShowErrorString", new object[] { error });
        }

        internal string CommandNeedingHelp
        {
            get
            {
                return (string) this.graphicalHostReflectionWrapper.GetPropertyValue("CommandNeedingHelp");
            }
        }

        internal bool HasHostWindow
        {
            get
            {
                return (bool) this.graphicalHostReflectionWrapper.GetPropertyValue("HasHostWindow");
            }
        }

        internal AutoResetEvent HelpNeeded
        {
            get
            {
                return (AutoResetEvent) this.graphicalHostReflectionWrapper.GetPropertyValue("HelpNeeded");
            }
        }

        internal AutoResetEvent ImportModuleNeeded
        {
            get
            {
                return (AutoResetEvent) this.graphicalHostReflectionWrapper.GetPropertyValue("ImportModuleNeeded");
            }
        }

        internal string ParentModuleNeedingImportModule
        {
            get
            {
                return (string) this.graphicalHostReflectionWrapper.GetPropertyValue("ParentModuleNeedingImportModule");
            }
        }

        internal double ScreenHeight
        {
            get
            {
                return (double) this.graphicalHostReflectionWrapper.GetStaticPropertyValue("ScreenHeight");
            }
        }

        internal double ScreenWidth
        {
            get
            {
                return (double) this.graphicalHostReflectionWrapper.GetStaticPropertyValue("ScreenWidth");
            }
        }

        internal AutoResetEvent WindowClosed
        {
            get
            {
                return (AutoResetEvent) this.graphicalHostReflectionWrapper.GetPropertyValue("WindowClosed");
            }
        }

        internal AutoResetEvent WindowLoaded
        {
            get
            {
                return (AutoResetEvent) this.graphicalHostReflectionWrapper.GetPropertyValue("WindowLoaded");
            }
        }
    }
}

