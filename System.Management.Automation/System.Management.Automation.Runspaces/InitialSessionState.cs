namespace System.Management.Automation.Runspaces
{
    using Microsoft.PowerShell;
    using Microsoft.PowerShell.Commands;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using System.Management.Automation.Host;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Remoting;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading;

    public class InitialSessionState
    {
        internal const ConfirmImpact defaultConfirmPreference = ConfirmImpact.High;
        internal const ActionPreference defaultDebugPreference = ActionPreference.SilentlyContinue;
        internal const ActionPreference defaultErrorActionPreference = ActionPreference.Continue;
        internal const int DefaultFormatEnumerationLimit = 4;
        internal const ActionPreference defaultProgressPreference = ActionPreference.Continue;
        internal const string DefaultPromptComments = "# .Link\n# http://go.microsoft.com/fwlink/?LinkID=225750\n# .ExternalHelp System.Management.Automation.dll-help.xml\n";
        internal const string DefaultPromptContent = "PS $($executionContext.SessionState.Path.CurrentLocation)$('>' * ($nestedPromptLevel + 1)) ";
        internal static string DefaultPromptString = "\"PS $($executionContext.SessionState.Path.CurrentLocation)$('>' * ($nestedPromptLevel + 1)) \"\n# .Link\n# http://go.microsoft.com/fwlink/?LinkID=225750\n# .ExternalHelp System.Management.Automation.dll-help.xml\n";
        private InitialSessionStateEntryCollection<SessionStateAssemblyEntry> _assemblies;
        private System.Management.Automation.AuthorizationManager _authorizationManager = new PSAuthorizationManager(Utils.DefaultPowerShellShellID);
        private InitialSessionStateEntryCollection<SessionStateCommandEntry> _commands;
        private HashSet<string> _coreModulesToImport = new HashSet<string>();
        private InitialSessionStateEntryCollection<SessionStateFormatEntry> _formats;
        private Dictionary<string, PSSnapInInfo> _importedSnapins = new Dictionary<string, PSSnapInInfo>(StringComparer.OrdinalIgnoreCase);
        private PSLanguageMode _languageMode = PSLanguageMode.NoLanguage;
        private Collection<ModuleSpecification> _moduleSpecificationsToImport = new Collection<ModuleSpecification>();
        private InitialSessionStateEntryCollection<SessionStateProviderEntry> _providers;
        private static PSTraceSource _PSSnapInTracer = PSTraceSource.GetTracer("PSSnapInLoadUnload", "Loading and unloading mshsnapins", false);
        private object _syncObject = new object();
        private InitialSessionStateEntryCollection<SessionStateTypeEntry> _types;
        private bool _useFullLanguageModeInDebugger;
        private InitialSessionStateEntryCollection<SessionStateVariableEntry> _variables;
        private static List<string> allowedAliases = new List<string> { 
            "compare", "diff", "%", "foreach", "exsn", "fc", "fl", "ft", "fw", "gcm", "gjb", "gmo", "gv", "group", "ipmo", "measure", 
            "rv", "rcjb", "rjb", "rmo", "rujb", "select", "set", "sv", "sort", "spjb", "sujb", "wjb", "?", "where"
         };
        private System.Threading.ApartmentState apartmentState = System.Threading.ApartmentState.Unknown;
        private static readonly string[] AutoDiscoveryCmdlets = new string[] { "Get-Module" };

		static SessionStateFunctionEntry[] GetBuiltInFunctions ()
		{
			if (OSHelper.IsUnix) {
				return new SessionStateFunctionEntry[] { 
					new SessionStateFunctionEntry("prompt", DefaultPromptString), 
					new SessionStateFunctionEntry("quit", "Quit-Shell"),
					new SessionStateFunctionEntry("OSX:", "Set-Location /::/"),
					new SessionStateFunctionEntry("TabExpansion2", TabExpansionFunctionText), 
					new SessionStateFunctionEntry("Clear-Host", "$Host.UI.RawUI.Clear(0)\n$space = New-Object System.Management.Automation.Host.BufferCell\n$space.Character = ' '\n$space.ForegroundColor = $host.ui.rawui.ForegroundColor\n$space.BackgroundColor = $host.ui.rawui.BackgroundColor\n$rect = New-Object System.Management.Automation.Host.Rectangle\n$rect.Top = $rect.Bottom = $rect.Right = $rect.Left = -1\n$origin = New-Object System.Management.Automation.Host.Coordinates\n$Host.UI.RawUI.CursorPosition = $origin\n$Host.UI.RawUI.SetBufferContents($rect, $space)\n# .Link\n# http://go.microsoft.com/fwlink/?LinkID=225747\n# .ExternalHelp System.Management.Automation.dll-help.xml\n"), new SessionStateFunctionEntry("more", "param([string[]]$paths)\n\n$OutputEncoding = [System.Console]::OutputEncoding\n\nif($paths)\n{\n    foreach ($file in $paths)\n    {\n        Get-Content $file | more.com\n    }\n}\nelse\n{\n    $input | more.com\n}\n"), new SessionStateFunctionEntry("help", GetHelpPagingFunctionText()), new SessionStateFunctionEntry("mkdir", GetMkdirFunctionText()), new SessionStateFunctionEntry("Get-Verb", GetGetVerbText()), new SessionStateFunctionEntry("oss", GetOSTFunctionText()), new SessionStateFunctionEntry("cd..", "Set-Location .."), new SessionStateFunctionEntry(@"cd\", @"Set-Location \"), 
					new SessionStateFunctionEntry("ImportSystemModules", ImportSystemModulesText), new SessionStateFunctionEntry("Pause", string.Format(CultureInfo.InvariantCulture, "Read-Host '{0}' | Out-Null", new object[] { CommandMetadata.EscapeSingleQuotedString(RunspaceInit.PauseDefinitionString) }))
				};
			}

			return new SessionStateFunctionEntry[] { 
				new SessionStateFunctionEntry("prompt", DefaultPromptString), new SessionStateFunctionEntry("TabExpansion2", TabExpansionFunctionText), new SessionStateFunctionEntry("Clear-Host", "$space = New-Object System.Management.Automation.Host.BufferCell\n$space.Character = ' '\n$space.ForegroundColor = $host.ui.rawui.ForegroundColor\n$space.BackgroundColor = $host.ui.rawui.BackgroundColor\n$rect = New-Object System.Management.Automation.Host.Rectangle\n$rect.Top = $rect.Bottom = $rect.Right = $rect.Left = -1\n$origin = New-Object System.Management.Automation.Host.Coordinates\n$Host.UI.RawUI.CursorPosition = $origin\n$Host.UI.RawUI.Clear()\n$Host.UI.RawUI.SetBufferContents($rect, $space)\n# .Link\n# http://go.microsoft.com/fwlink/?LinkID=225747\n# .ExternalHelp System.Management.Automation.dll-help.xml\n"), new SessionStateFunctionEntry("more", "param([string[]]$paths)\n\n$OutputEncoding = [System.Console]::OutputEncoding\n\nif($paths)\n{\n    foreach ($file in $paths)\n    {\n        Get-Content $file | more.com\n    }\n}\nelse\n{\n    $input | more.com\n}\n"), new SessionStateFunctionEntry("help", GetHelpPagingFunctionText()), new SessionStateFunctionEntry("mkdir", GetMkdirFunctionText()), new SessionStateFunctionEntry("Get-Verb", GetGetVerbText()), new SessionStateFunctionEntry("oss", GetOSTFunctionText()), new SessionStateFunctionEntry("A:", "Set-Location A:"), new SessionStateFunctionEntry("B:", "Set-Location B:"), new SessionStateFunctionEntry("C:", "Set-Location C:"), new SessionStateFunctionEntry("D:", "Set-Location D:"), new SessionStateFunctionEntry("E:", "Set-Location E:"), new SessionStateFunctionEntry("F:", "Set-Location F:"), new SessionStateFunctionEntry("G:", "Set-Location G:"), new SessionStateFunctionEntry("H:", "Set-Location H:"), 
				new SessionStateFunctionEntry("I:", "Set-Location I:"), new SessionStateFunctionEntry("J:", "Set-Location J:"), new SessionStateFunctionEntry("K:", "Set-Location K:"), new SessionStateFunctionEntry("L:", "Set-Location L:"), new SessionStateFunctionEntry("M:", "Set-Location M:"), new SessionStateFunctionEntry("N:", "Set-Location N:"), new SessionStateFunctionEntry("O:", "Set-Location O:"), new SessionStateFunctionEntry("P:", "Set-Location P:"), new SessionStateFunctionEntry("Q:", "Set-Location Q:"), new SessionStateFunctionEntry("R:", "Set-Location R:"), new SessionStateFunctionEntry("S:", "Set-Location S:"), new SessionStateFunctionEntry("T:", "Set-Location T:"), new SessionStateFunctionEntry("U:", "Set-Location U:"), new SessionStateFunctionEntry("V:", "Set-Location V:"), new SessionStateFunctionEntry("W:", "Set-Location W:"), new SessionStateFunctionEntry("X:", "Set-Location X:"), 
				new SessionStateFunctionEntry("Y:", "Set-Location Y:"), new SessionStateFunctionEntry("Z:", "Set-Location Z:"), new SessionStateFunctionEntry("cd..", "Set-Location .."), new SessionStateFunctionEntry(@"cd\", @"Set-Location \"), new SessionStateFunctionEntry("ImportSystemModules", ImportSystemModulesText), new SessionStateFunctionEntry("Pause", string.Format(CultureInfo.InvariantCulture, "Read-Host '{0}' | Out-Null", new object[] { CommandMetadata.EscapeSingleQuotedString(RunspaceInit.PauseDefinitionString) }))
			};
		}

		internal static SessionStateFunctionEntry[] BuiltInFunctions = GetBuiltInFunctions();
        internal static SessionStateVariableEntry[] BuiltInVariables = new SessionStateVariableEntry[] { 
            new SessionStateVariableEntry("$", null, string.Empty), new SessionStateVariableEntry("^", null, string.Empty), new SessionStateVariableEntry("StackTrace", null, string.Empty), new SessionStateVariableEntry("OutputEncoding", Encoding.ASCII, RunspaceInit.OutputEncodingDescription, ScopedItemOptions.None, new ArgumentTypeConverterAttribute(new Type[] { typeof(Encoding) })), new SessionStateVariableEntry("ConfirmPreference", ConfirmImpact.High, RunspaceInit.ConfirmPreferenceDescription, ScopedItemOptions.None, new ArgumentTypeConverterAttribute(new Type[] { typeof(ConfirmImpact) })), new SessionStateVariableEntry("DebugPreference", ActionPreference.SilentlyContinue, RunspaceInit.DebugPreferenceDescription, ScopedItemOptions.None, new ArgumentTypeConverterAttribute(new Type[] { typeof(ActionPreference) })), new SessionStateVariableEntry("ErrorActionPreference", ActionPreference.Continue, RunspaceInit.ErrorActionPreferenceDescription, ScopedItemOptions.None, new ArgumentTypeConverterAttribute(new Type[] { typeof(ActionPreference) })), new SessionStateVariableEntry("ProgressPreference", ActionPreference.Continue, RunspaceInit.ProgressPreferenceDescription, ScopedItemOptions.None, new ArgumentTypeConverterAttribute(new Type[] { typeof(ActionPreference) })), new SessionStateVariableEntry("VerbosePreference", ActionPreference.SilentlyContinue, RunspaceInit.VerbosePreferenceDescription, ScopedItemOptions.None, new ArgumentTypeConverterAttribute(new Type[] { typeof(ActionPreference) })), new SessionStateVariableEntry("WarningPreference", ActionPreference.Continue, RunspaceInit.WarningPreferenceDescription, ScopedItemOptions.None, new ArgumentTypeConverterAttribute(new Type[] { typeof(ActionPreference) })), new SessionStateVariableEntry("ErrorView", "NormalView", RunspaceInit.ErrorViewDescription), new SessionStateVariableEntry("NestedPromptLevel", 0, RunspaceInit.NestedPromptLevelDescription), new SessionStateVariableEntry("WhatIfPreference", false, RunspaceInit.WhatIfPreferenceDescription), new SessionStateVariableEntry("FormatEnumerationLimit", 4, RunspaceInit.FormatEnunmerationLimitDescription), new SessionStateVariableEntry("PSEmailServer", string.Empty, RunspaceInit.PSEmailServerDescription), new SessionStateVariableEntry("PSSessionOption", new PSSessionOption(), PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.PSDefaultSessionOptionDescription, new object[0]), ScopedItemOptions.None), 
            new SessionStateVariableEntry("PSSessionConfigurationName", "http://schemas.microsoft.com/powershell/Microsoft.PowerShell", PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.PSSessionConfigurationName, new object[0]), ScopedItemOptions.None), new SessionStateVariableEntry("PSSessionApplicationName", "wsman", PSRemotingErrorInvariants.FormatResourceString(RemotingErrorIdStrings.PSSessionAppName, new object[0]), ScopedItemOptions.None)
         };
        internal const string ConsoleInfoResourceBaseName = "ConsoleInfoErrorStrings";
        internal static HashSet<string> ConstantEngineModules;
        internal static HashSet<string> ConstantEngineNestedModules;
        internal static string CoreModule = "Microsoft.PowerShell.Core";
        internal static string CoreSnapin = "Microsoft.PowerShell.Core";
        private static string TabExpansionFunctionText = "\r\n<# Options include:\r\n     RelativeFilePaths - [bool]\r\n         Always resolve file paths using Resolve-Path -Relative.\r\n         The default is to use some heuristics to guess if relative or absolute is better.\r\n\r\n   To customize your own custom options, pass a hashtable to CompleteInput, e.g.\r\n         return [System.Management.Automation.CommandCompletion]::CompleteInput($inputScript, $cursorColumn,\r\n             @{ RelativeFilePaths=$false }\r\n#>\r\n\r\n[CmdletBinding(DefaultParameterSetName = 'ScriptInputSet')]\r\nParam(\r\n    [Parameter(ParameterSetName = 'ScriptInputSet', Mandatory = $true, Position = 0)]\r\n    [string] $inputScript,\r\n    \r\n    [Parameter(ParameterSetName = 'ScriptInputSet', Mandatory = $true, Position = 1)]\r\n    [int] $cursorColumn,\r\n\r\n    [Parameter(ParameterSetName = 'AstInputSet', Mandatory = $true, Position = 0)]\r\n    [System.Management.Automation.Language.Ast] $ast,\r\n\r\n    [Parameter(ParameterSetName = 'AstInputSet', Mandatory = $true, Position = 1)]\r\n    [System.Management.Automation.Language.Token[]] $tokens,\r\n\r\n    [Parameter(ParameterSetName = 'AstInputSet', Mandatory = $true, Position = 2)]\r\n    [System.Management.Automation.Language.IScriptPosition] $positionOfCursor,\r\n    \r\n    [Parameter(ParameterSetName = 'ScriptInputSet', Position = 2)]\r\n    [Parameter(ParameterSetName = 'AstInputSet', Position = 3)]\r\n    [Hashtable] $options = $null\r\n)\r\n\r\nEnd\r\n{\r\n    if ($psCmdlet.ParameterSetName -eq 'ScriptInputSet')\r\n    {\r\n        return [System.Management.Automation.CommandCompletion]::CompleteInput(\r\n            <#inputScript#>  $inputScript,\r\n            <#cursorColumn#> $cursorColumn,\r\n            <#options#>      $options)\r\n    }\r\n    else\r\n    {\r\n        return [System.Management.Automation.CommandCompletion]::CompleteInput(\r\n            <#ast#>              $ast,\r\n            <#tokens#>           $tokens,\r\n            <#positionOfCursor#> $positionOfCursor,\r\n            <#options#>          $options)\r\n    }\r\n}\r\n        ";
        private PSThreadOptions createThreadOptions;
        internal Collection<PSSnapInInfo> defaultSnapins = new Collection<PSSnapInInfo>();
        private static readonly string[] DefaultTypeFiles = new string[] { "types.ps1xml", "typesv3.ps1xml" };
        internal const ActionPreference defaultVerbosePreference = ActionPreference.SilentlyContinue;
        internal const ActionPreference defaultWarningPreference = ActionPreference.Continue;
        internal const bool defaultWhatIfPreference = false;
        internal static Dictionary<string, string> EngineModuleNestedModuleMapping;
		internal static HashSet<string> EngineModules = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Microsoft.PowerShell.Utility", "Microsoft.PowerShell.Management", "Microsoft.PowerShell.Diagnostics", "Microsoft.PowerShell.Host", "Microsoft.PowerShell.Security", "Microsoft.WSMan.Management" };
        internal const string FormatEnumerationLimit = "FormatEnumerationLimit";
        internal PSHost Host;
        private static readonly string[] ImplicitRemotingCmdlets = new string[] { "Get-Command", "Select-Object", "Measure-Object", "Get-Help", "Get-FormatData", "Exit-PSSession", "Out-Default" };
		private static string ImportSystemModulesText = "Import-Module OData";
        private static readonly string[] JobCmdlets = new string[] { "Get-Job", "Stop-Job", "Wait-Job", "Suspend-Job", "Resume-Job", "Remove-Job", "Receive-Job" };
        private static readonly string[] LanguageHelperCmdlets = new string[] { 
            "Compare-Object", "ForEach-Object", "Group-Object", "Sort-Object", "Where-Object", "Out-File", "Out-Null", "Out-String", "Format-Custom", "Format-List", "Format-Table", "Format-Wide", "Remove-Module", "Get-Variable", "Set-Variable", "Remove-Variable", 
            "Get-Credential", "Set-StrictMode"
         };
        private static readonly string[] MiscCmdlets = new string[] { "Join-Path", "Import-Module" };
        private static readonly string[] MiscCommands = new string[] { "TabExpansion2" };
        internal static HashSet<string> NestedEngineModules = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Microsoft.PowerShell.Commands.Utility", "Microsoft.PowerShell.Commands.Management", "Microsoft.PowerShell.Commands.Diagnostics", "Microsoft.PowerShell.ConsoleHost" };
        internal static Dictionary<string, string> NestedModuleEngineModuleMapping;
        internal bool RefreshTypeAndFormatSetting;
        
        private bool throwOnRunspaceOpenError;

        static InitialSessionState()
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            dictionary.Add("Microsoft.PowerShell.Utility", "Microsoft.PowerShell.Commands.Utility");
            dictionary.Add("Microsoft.PowerShell.Management", "Microsoft.PowerShell.Commands.Management");
            dictionary.Add("Microsoft.PowerShell.Diagnostics", "Microsoft.PowerShell.Commands.Diagnostics");
            dictionary.Add("Microsoft.PowerShell.Host", "Microsoft.PowerShell.ConsoleHost");
            EngineModuleNestedModuleMapping = dictionary;
            Dictionary<string, string> dictionary2 = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            dictionary2.Add("Microsoft.PowerShell.Commands.Utility", "Microsoft.PowerShell.Utility");
            dictionary2.Add("Microsoft.PowerShell.Commands.Management", "Microsoft.PowerShell.Management");
            dictionary2.Add("Microsoft.PowerShell.Commands.Diagnostics", "Microsoft.PowerShell.Diagnostics");
            dictionary2.Add("Microsoft.PowerShell.ConsoleHost", "Microsoft.PowerShell.Host");
            dictionary2.Add("Microsoft.PowerShell.Security", "Microsoft.PowerShell.Security");
            dictionary2.Add("Microsoft.WSMan.Management", "Microsoft.WSMan.Management");
            NestedModuleEngineModuleMapping = dictionary2;
            ConstantEngineModules = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { CoreModule };
            ConstantEngineNestedModules = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "System.Management.Automation" };
        }

        protected InitialSessionState()
        {

        }

        internal void Bind(System.Management.Automation.ExecutionContext context, bool updateOnly)
        {
            this.Bind(context, updateOnly, null, false, false);
        }

        internal void Bind(System.Management.Automation.ExecutionContext context, bool updateOnly, PSModuleInfo module, bool noClobber, bool local)
        {
            this.Host = context.EngineHostInterface;
            lock (this._syncObject)
            {
                SessionStateInternal engineSessionState = context.EngineSessionState;
                if (!updateOnly)
                {
                    engineSessionState.Applications.Clear();
                    engineSessionState.Scripts.Clear();
                }
                foreach (SessionStateAssemblyEntry entry in this.Assemblies)
                {
                    Exception error = null;
                    if ((context.AddAssembly(entry.Name, entry.FileName, out error) == null) || (error != null))
                    {
                        if (error == null)
                        {
                            error = new DllNotFoundException(StringUtil.Format(System.Management.Automation.Modules.ModuleAssemblyFound, entry.Name));
                        }
                        if ((!string.IsNullOrEmpty(context.ModuleBeingProcessed) && Path.GetExtension(context.ModuleBeingProcessed).Equals(".psd1", StringComparison.OrdinalIgnoreCase)) || this.throwOnRunspaceOpenError)
                        {
                            throw error;
                        }
                        context.ReportEngineStartupError(error.Message);
                    }
                }
                InitialSessionState initialSessionState = null;
                foreach (SessionStateCommandEntry entry2 in this.Commands)
                {
                    SessionStateCmdletEntry entry3 = entry2 as SessionStateCmdletEntry;
                    if (entry3 != null)
                    {
                        if (noClobber && ModuleCmdletBase.CommandFound(entry3.Name, engineSessionState))
                        {
                            entry3._isImported = false;
                        }
                        else
                        {
                            engineSessionState.AddSessionStateEntry(entry3, local);
                            entry2.SetModule(module);
                        }
                    }
                    else
                    {
                        entry2.SetModule(module);
                        SessionStateFunctionEntry entry4 = entry2 as SessionStateFunctionEntry;
                        if (entry4 != null)
                        {
                            engineSessionState.AddSessionStateEntry(entry4);
                        }
                        else
                        {
                            SessionStateAliasEntry entry5 = entry2 as SessionStateAliasEntry;
                            if (entry5 != null)
                            {
                                engineSessionState.AddSessionStateEntry(entry5);
                            }
                            else
                            {
                                SessionStateApplicationEntry entry6 = entry2 as SessionStateApplicationEntry;
                                if (entry6 != null)
                                {
                                    engineSessionState.AddSessionStateEntry(entry6);
                                }
                                else
                                {
                                    SessionStateScriptEntry entry7 = entry2 as SessionStateScriptEntry;
                                    if (entry7 != null)
                                    {
                                        engineSessionState.AddSessionStateEntry(entry7);
                                    }
                                    else
                                    {
                                        SessionStateWorkflowEntry entry8 = entry2 as SessionStateWorkflowEntry;
                                        if (entry8 != null)
                                        {
                                            if (initialSessionState == null)
                                            {
                                                initialSessionState = this.Clone();
                                                List<SessionStateCommandEntry> list = (from e in initialSessionState.Commands
                                                    where !(e is SessionStateWorkflowEntry)
                                                    select e).ToList<SessionStateCommandEntry>();
                                                initialSessionState.Commands.Clear();
                                                foreach (SessionStateCommandEntry entry9 in list)
                                                {
                                                    initialSessionState.Commands.Add(entry9);
                                                }
                                            }
                                            engineSessionState.AddSessionStateEntry(initialSessionState, entry8);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                foreach (SessionStateProviderEntry entry10 in this.Providers)
                {
                    engineSessionState.AddSessionStateEntry(entry10);
                }
                foreach (SessionStateVariableEntry entry11 in this.Variables)
                {
                    engineSessionState.AddSessionStateEntry(entry11);
                }
                try
                {
                    this.UpdateTypes(context, updateOnly, true);
                }
                catch (RuntimeException exception2)
                {
                    MshLog.LogEngineHealthEvent(context, 0x67, exception2, Severity.Warning);
                    if (this.ThrowOnRunspaceOpenError)
                    {
                        throw;
                    }
                    context.ReportEngineStartupError(exception2.Message);
                }
                try
                {
                    this.UpdateFormats(context, updateOnly);
                }
                catch (RuntimeException exception3)
                {
                    MshLog.LogEngineHealthEvent(context, 0x67, exception3, Severity.Warning);
                    if (this.ThrowOnRunspaceOpenError)
                    {
                        throw;
                    }
                    context.ReportEngineStartupError(exception3.Message);
                }
                if (!updateOnly)
                {
                    engineSessionState.LanguageMode = this.LanguageMode;
                }
            }
            SetSessionStateDrive(context, false);
        }

        public InitialSessionState Clone()
        {
            InitialSessionState state = new InitialSessionState();
            state.Variables.Add(this.Variables.Clone());
            state.Commands.Add(this.Commands.Clone());
            state.Assemblies.Add(this.Assemblies.Clone());
            state.Types.Add(this.Types.Clone());
            state.Formats.Add(this.Formats.Clone());
            state.Providers.Add(this.Providers.Clone());
            state.AuthorizationManager = this.AuthorizationManager;
            state.LanguageMode = this.LanguageMode;
            state.UseFullLanguageModeInDebugger = this.UseFullLanguageModeInDebugger;
            state.ApartmentState = this.ApartmentState;
            state.ThreadOptions = this.ThreadOptions;
            state.ThrowOnRunspaceOpenError = this.ThrowOnRunspaceOpenError;
            foreach (ModuleSpecification specification in this.ModuleSpecificationsToImport)
            {
                state.ModuleSpecificationsToImport.Add(specification);
            }
            foreach (string str in this.CoreModulesToImport)
            {
                state.CoreModulesToImport.Add(str);
            }
            state.DisableFormatUpdates = this.DisableFormatUpdates;
            foreach (PSSnapInInfo info in this.defaultSnapins)
            {
                state.defaultSnapins.Add(info);
            }
            if (this.WarmUpTabCompletionOnIdle)
            {
                state.WarmUpTabCompletionOnIdle = true;
            }
            foreach (KeyValuePair<string, PSSnapInInfo> pair in this._importedSnapins)
            {
                state.ImportedSnapins.Add(pair.Key, pair.Value);
            }
            return state;
        }

        public static InitialSessionState Create()
        {
            return new InitialSessionState();
        }

        public static InitialSessionState Create(string snapInName)
        {
            return new InitialSessionState();
        }

        public static InitialSessionState Create(string[] snapInNameCollection, out PSConsoleLoadException warning)
        {
            warning = null;
            return new InitialSessionState();
        }

        public static InitialSessionState CreateDefault()
        {
            InitialSessionState state = new InitialSessionState();
            state.Variables.Add(BuiltInVariables);
            state.Commands.Add(new SessionStateApplicationEntry("*"));
            state.Commands.Add(new SessionStateScriptEntry("*"));
            state.Commands.Add(BuiltInFunctions);
            state.Commands.Add(BuiltInAliases);
            foreach (PSSnapInInfo info in PSSnapInReader.ReadEnginePSSnapIns())
            {
                try
                {
                    PSSnapInException exception;
                    state.ImportPSSnapIn(info, out exception);
                }
                catch (PSSnapInException exception2)
                {
                    throw exception2;
                }
            }
            state.LanguageMode = PSLanguageMode.FullLanguage;
            state.AuthorizationManager = new PSAuthorizationManager(Utils.DefaultPowerShellShellID);
            return state.Clone();
        }

        public static InitialSessionState CreateDefault2()
        {
            InitialSessionState state = new InitialSessionState();
            state.Variables.Add(BuiltInVariables);
            state.Commands.Add(new SessionStateApplicationEntry("*"));
            state.Commands.Add(new SessionStateScriptEntry("*"));
            state.Commands.Add(BuiltInFunctions);
            state.Commands.Add(BuiltInAliases);
            state.ImportCorePSSnapIn();
            state.LanguageMode = PSLanguageMode.FullLanguage;
            state.AuthorizationManager = new PSAuthorizationManager(Utils.DefaultPowerShellShellID);
            return state.Clone();
        }

        public static InitialSessionState CreateFrom(string snapInPath, out PSConsoleLoadException warnings)
        {
            warnings = null;
            return new InitialSessionState();
        }

        public static InitialSessionState CreateFrom(string[] snapInPathCollection, out PSConsoleLoadException warnings)
        {
            warnings = null;
            return new InitialSessionState();
        }

        internal static void CreateQuestionVariable(System.Management.Automation.ExecutionContext context)
        {
            QuestionMarkVariable variable = new QuestionMarkVariable(context);
            context.EngineSessionState.SetVariableAtScope(variable, "global", true, CommandOrigin.Internal);
        }

        public static InitialSessionState CreateRestricted(SessionCapabilities sessionCapabilities)
        {
            if (SessionCapabilities.RemoteServer == sessionCapabilities)
            {
                return CreateRestrictedForRemoteServer();
            }
            if (SessionCapabilities.WorkflowServer == sessionCapabilities)
            {
                return CreateRestrictedForWorkflowServerMinimum();
            }
            if (sessionCapabilities == (SessionCapabilities.WorkflowServer | SessionCapabilities.RemoteServer))
            {
                return CreateRestrictedForWorkflowServer();
            }
            if (sessionCapabilities == (SessionCapabilities.Language | SessionCapabilities.WorkflowServer | SessionCapabilities.RemoteServer))
            {
                return CreateRestrictedForWorkflowServerWithFullLanguage();
            }
            return Create();
        }

        private static InitialSessionState CreateRestrictedForRemoteServer()
        {
            InitialSessionState state = Create();
            state.LanguageMode = PSLanguageMode.NoLanguage;
            state.ThrowOnRunspaceOpenError = true;
            state.UseFullLanguageModeInDebugger = false;
            List<string> list = new List<string> { "Microsoft.PowerShell.Core", "Microsoft.PowerShell.Utility", "Microsoft.PowerShell.Security" };
            using (IEnumerator<PSSnapInInfo> enumerator = PSSnapInReader.ReadEnginePSSnapIns().GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Predicate<string> match = null;
                    PSSnapInInfo si = enumerator.Current;
                    if (match == null)
                    {
                        match = allowed => allowed.Equals(si.Name, StringComparison.OrdinalIgnoreCase);
                    }
                    if (list.Exists(match))
                    {
                        PSSnapInException exception;
                        state.ImportPSSnapIn(si, out exception);
                    }
                }
            }
            List<string> allowedNames = new List<string> { "Get-Command", "Get-FormatData", "Select-Object", "Get-Help", "Measure-Object", "Out-Default", "Exit-PSSession" };
            MakeDisallowedEntriesPrivate<SessionStateCommandEntry>(state.Commands, allowedNames, commandEntry => commandEntry.Name);
            List<string> list3 = new List<string> { "Certificate.Format.ps1xml", "DotNetTypes.Format.ps1xml", "FileSystem.Format.ps1xml", "Help.Format.ps1xml", "HelpV3.format.ps1xml", "PowerShellCore.format.ps1xml", "PowerShellTrace.format.ps1xml", "Registry.format.ps1xml" };
            RemoveDisallowedEntries<SessionStateFormatEntry>(state.Formats, list3, formatEntry => Path.GetFileName(formatEntry.FileName));
            List<string> list4 = new List<string> { "types.ps1xml" };
            RemoveDisallowedEntries<SessionStateTypeEntry>(state.Types, list4, typeEntry => Path.GetFileName(typeEntry.FileName));
            state.Providers.Clear();
            state.Variables.Clear();
            foreach (KeyValuePair<string, CommandMetadata> pair in CommandMetadata.GetRestrictedCommands(SessionCapabilities.RemoteServer))
            {
                string key = pair.Key;
                Collection<SessionStateCommandEntry> collection2 = state.Commands[key];
                collection2[0].Visibility = SessionStateEntryVisibility.Private;
                string definition = ProxyCommand.Create(pair.Value);
                state.Commands.Add(new SessionStateFunctionEntry(key, definition));
            }
            return state;
        }

        private static InitialSessionState CreateRestrictedForWorkflowServer()
        {
            InitialSessionState state = CreateDefault();
            state.LanguageMode = PSLanguageMode.NoLanguage;
            state.ThrowOnRunspaceOpenError = true;
            state.UseFullLanguageModeInDebugger = false;
            foreach (SessionStateCommandEntry entry in state.Commands)
            {
                if (entry.GetType() == typeof(SessionStateApplicationEntry))
                {
                    state.Commands.Remove(entry.Name, entry);
                    break;
                }
            }
            List<string> allowedNames = new List<string>();
            allowedNames.AddRange(JobCmdlets);
            allowedNames.AddRange(ImplicitRemotingCmdlets);
            allowedNames.AddRange(MiscCmdlets);
            allowedNames.AddRange(AutoDiscoveryCmdlets);
            MakeDisallowedEntriesPrivate<SessionStateCommandEntry>(state.Commands, allowedNames, commandEntry => commandEntry.Name);
            foreach (SessionStateCommandEntry entry2 in state.Commands)
            {
                if (entry2.GetType() == typeof(SessionStateAliasEntry))
                {
                    if (allowedAliases.Contains<string>(entry2.Name, StringComparer.OrdinalIgnoreCase))
                    {
                        entry2.Visibility = SessionStateEntryVisibility.Public;
                    }
                    else
                    {
                        entry2.Visibility = SessionStateEntryVisibility.Private;
                    }
                }
            }
            List<string> list2 = new List<string> { "Certificate.Format.ps1xml", "Event.format.ps1xml", "Diagnostics.format.ps1xml", "DotNetTypes.Format.ps1xml", "FileSystem.Format.ps1xml", "Help.Format.ps1xml", "HelpV3.format.ps1xml", "PowerShellCore.format.ps1xml", "PowerShellTrace.format.ps1xml", "Registry.format.ps1xml", "WSMan.format.ps1xml" };
            RemoveDisallowedEntries<SessionStateFormatEntry>(state.Formats, list2, formatEntry => Path.GetFileName(formatEntry.FileName));
            List<string> list3 = new List<string>();
            list3.AddRange(DefaultTypeFiles);
            RemoveDisallowedEntries<SessionStateTypeEntry>(state.Types, list3, typeEntry => Path.GetFileName(typeEntry.FileName));
            state.Variables.Clear();
            return state;
        }

        private static InitialSessionState CreateRestrictedForWorkflowServerMinimum()
        {
            InitialSessionState state = CreateDefault();
            state.LanguageMode = PSLanguageMode.NoLanguage;
            state.ThrowOnRunspaceOpenError = true;
            state.UseFullLanguageModeInDebugger = false;
            foreach (SessionStateCommandEntry entry in state.Commands)
            {
                if (entry.GetType() == typeof(SessionStateApplicationEntry))
                {
                    state.Commands.Remove(entry.Name, entry);
                    break;
                }
            }
            List<string> allowedNames = new List<string> { "Get-Command" };
            allowedNames.AddRange(JobCmdlets);
            allowedNames.AddRange(MiscCmdlets);
            MakeDisallowedEntriesPrivate<SessionStateCommandEntry>(state.Commands, allowedNames, commandEntry => commandEntry.Name);
            state.Formats.Clear();
            List<string> list2 = new List<string>();
            list2.AddRange(DefaultTypeFiles);
            RemoveDisallowedEntries<SessionStateTypeEntry>(state.Types, list2, typeEntry => Path.GetFileName(typeEntry.FileName));
            state.Variables.Clear();
            SessionStateVariableEntry item = new SessionStateVariableEntry("PSDisableModuleAutoDiscovery", true, "True if we disable module autodiscovery", ScopedItemOptions.Constant);
            state.Variables.Add(item);
            return state;
        }

        private static InitialSessionState CreateRestrictedForWorkflowServerWithFullLanguage()
        {
            InitialSessionState state = CreateDefault();
            state.LanguageMode = PSLanguageMode.FullLanguage;
            state.ThrowOnRunspaceOpenError = true;
            state.UseFullLanguageModeInDebugger = false;
            foreach (SessionStateCommandEntry entry in state.Commands)
            {
                if (entry.GetType() == typeof(SessionStateApplicationEntry))
                {
                    state.Commands.Remove(entry.Name, entry);
                    break;
                }
            }
            List<string> allowedNames = new List<string>();
            allowedNames.AddRange(JobCmdlets);
            allowedNames.AddRange(ImplicitRemotingCmdlets);
            allowedNames.AddRange(MiscCmdlets);
            allowedNames.AddRange(MiscCommands);
            allowedNames.AddRange(AutoDiscoveryCmdlets);
            allowedNames.AddRange(LanguageHelperCmdlets);
            MakeDisallowedEntriesPrivate<SessionStateCommandEntry>(state.Commands, allowedNames, commandEntry => commandEntry.Name);
            foreach (SessionStateCommandEntry entry2 in state.Commands)
            {
                if (entry2.GetType() == typeof(SessionStateAliasEntry))
                {
                    if (allowedAliases.Contains<string>(entry2.Name, StringComparer.OrdinalIgnoreCase))
                    {
                        entry2.Visibility = SessionStateEntryVisibility.Public;
                    }
                    else
                    {
                        entry2.Visibility = SessionStateEntryVisibility.Private;
                    }
                }
            }
            List<string> list2 = new List<string> { "Certificate.Format.ps1xml", "Event.format.ps1xml", "Diagnostics.format.ps1xml", "DotNetTypes.Format.ps1xml", "FileSystem.Format.ps1xml", "Help.Format.ps1xml", "HelpV3.format.ps1xml", "PowerShellCore.format.ps1xml", "PowerShellTrace.format.ps1xml", "Registry.format.ps1xml", "WSMan.format.ps1xml" };
            RemoveDisallowedEntries<SessionStateFormatEntry>(state.Formats, list2, formatEntry => Path.GetFileName(formatEntry.FileName));
            List<string> list3 = new List<string>();
            list3.AddRange(DefaultTypeFiles);
            RemoveDisallowedEntries<SessionStateTypeEntry>(state.Types, list3, typeEntry => Path.GetFileName(typeEntry.FileName));
            state.Variables.Clear();
            Hashtable hashtable2 = new Hashtable();
            hashtable2.Add("Get-Command:ListImported", true);
            Hashtable hashtable = hashtable2;
            state.Variables.Add(new SessionStateVariableEntry("PSDefaultParameterValues", hashtable, "Default Get-Command Action"));
            return state;
        }

        internal static string GetGetVerbText()
        {
            return "\r\nparam(\r\n    [Parameter(ValueFromPipeline=$true)]\r\n    [string[]]\r\n    $verb = '*'\r\n)\r\nbegin {\r\n    $allVerbs = [PSObject].Assembly.GetTypes() |\r\n        Where-Object {$_.Name -match '^Verbs.'} |\r\n        Get-Member -type Properties -static |\r\n        Select-Object @{\r\n            Name='Verb'\r\n            Expression = {$_.Name}\r\n        }, @{\r\n            Name='Group'\r\n            Expression = {\r\n                $str = \"$($_.TypeName)\"\r\n                $str.Substring($str.LastIndexOf('Verbs') + 5)\r\n            }\r\n        }\r\n}\r\nprocess {\r\n    foreach ($v in $verb) {\r\n        $allVerbs | Where-Object { $_.Verb -like $v }\r\n    }\r\n}\r\n# .Link\r\n# http://go.microsoft.com/fwlink/?LinkID=160712\r\n# .ExternalHelp System.Management.Automation.dll-help.xml\r\n";
        }

        internal static string GetHelpPagingFunctionText()
        {
            CommandMetadata metadata = new CommandMetadata(typeof(GetHelpCommand));
            return string.Format(CultureInfo.InvariantCulture, "\r\n<#\r\n.FORWARDHELPTARGETNAME Get-Help\r\n.FORWARDHELPCATEGORY Cmdlet\r\n#>\r\n{0}\r\nparam({1})\r\n\r\n      #Set the outputencoding to Console::OutputEncoding. More.com doesn't work well with Unicode.\r\n      $outputEncoding=[System.Console]::OutputEncoding\r\n\r\n      Get-Help @PSBoundParameters | more\r\n", new object[] { metadata.GetDecl(), metadata.GetParamBlock() });
        }

        internal static string GetMkdirFunctionText()
        {
            return "\r\n<#\r\n.FORWARDHELPTARGETNAME New-Item\r\n.FORWARDHELPCATEGORY Cmdlet\r\n#>\r\n[CmdletBinding(DefaultParameterSetName='pathSet',\r\n    SupportsShouldProcess=$true,\r\n    SupportsTransactions=$true,\r\n    ConfirmImpact='Medium')]\r\n    [OutputType([System.IO.DirectoryInfo])]\r\nparam(\r\n    [Parameter(ParameterSetName='nameSet', Position=0, ValueFromPipelineByPropertyName=$true)]\r\n    [Parameter(ParameterSetName='pathSet', Mandatory=$true, Position=0, ValueFromPipelineByPropertyName=$true)]\r\n    [System.String[]]\r\n    ${Path},\r\n\r\n    [Parameter(ParameterSetName='nameSet', Mandatory=$true, ValueFromPipelineByPropertyName=$true)]\r\n    [AllowNull()]\r\n    [AllowEmptyString()]\r\n    [System.String]\r\n    ${Name},\r\n\r\n    [Parameter(ValueFromPipeline=$true, ValueFromPipelineByPropertyName=$true)]\r\n    [System.Object]\r\n    ${Value},\r\n\r\n    [Switch]\r\n    ${Force},\r\n\r\n    [Parameter(ValueFromPipelineByPropertyName=$true)]\r\n    [System.Management.Automation.PSCredential]\r\n    ${Credential}\r\n)\r\n\r\nbegin {\r\n\r\n    try {\r\n        $wrappedCmd = $ExecutionContext.InvokeCommand.GetCommand('New-Item', [System.Management.Automation.CommandTypes]::Cmdlet)\r\n        $scriptCmd = {& $wrappedCmd -Type Directory @PSBoundParameters }\r\n        $steppablePipeline = $scriptCmd.GetSteppablePipeline()\r\n        $steppablePipeline.Begin($PSCmdlet)\r\n    } catch {\r\n        throw\r\n    }\r\n\r\n}\r\n\r\nprocess {\r\n\r\n    try {\r\n        $steppablePipeline.Process($_)\r\n    } catch {\r\n        throw\r\n    }\r\n\r\n}\r\n\r\nend {\r\n\r\n    try {\r\n        $steppablePipeline.End()\r\n    } catch {\r\n        throw\r\n    }\r\n\r\n}\r\n\r\n";
        }

        internal static string GetNestedModuleDllName(string moduleName)
        {
            string str = null;
            if (!EngineModuleNestedModuleMapping.TryGetValue(moduleName, out str))
            {
                str = string.Empty;
            }
            return str;
        }

        internal static string GetOSTFunctionText()
        {
            return "\r\n[CmdletBinding()]\r\nparam(\r\n    [ValidateRange(2, 2147483647)]\r\n    [int]\r\n    ${Width},\r\n\r\n    [Parameter(ValueFromPipeline=$true)]\r\n    [psobject]\r\n    ${InputObject})\r\n\r\nbegin\r\n{\r\n    try {\r\n        $PSBoundParameters['Stream'] = $true\r\n        $wrappedCmd = $ExecutionContext.InvokeCommand.GetCommand('Out-String',[System.Management.Automation.CommandTypes]::Cmdlet)\r\n        $scriptCmd = {& $wrappedCmd @PSBoundParameters }\r\n        $steppablePipeline = $scriptCmd.GetSteppablePipeline($myInvocation.CommandOrigin)\r\n        $steppablePipeline.Begin($PSCmdlet)\r\n    } catch {\r\n        throw\r\n    }\r\n}\r\n\r\nprocess\r\n{\r\n    try {\r\n        $steppablePipeline.Process($_)\r\n    } catch {\r\n        throw\r\n    }\r\n}\r\n\r\nend\r\n{\r\n    try {\r\n        $steppablePipeline.End()\r\n    } catch {\r\n        throw\r\n    }\r\n}\r\n<#\r\n.ForwardHelpTargetName Out-String\r\n.ForwardHelpCategory Cmdlet\r\n#>\r\n";
        }

        internal List<PSSnapInInfo> GetPSSnapIn(string psSnapinName)
        {
            List<PSSnapInInfo> list = null;
            foreach (PSSnapInInfo info in this.defaultSnapins)
            {
                if (info.Name.Equals(psSnapinName, StringComparison.OrdinalIgnoreCase))
                {
                    if (list == null)
                    {
                        list = new List<PSSnapInInfo>();
                    }
                    list.Add(info);
                }
            }
            PSSnapInInfo info2 = null;
            if (this._importedSnapins.TryGetValue(psSnapinName, out info2))
            {
                if (list == null)
                {
                    list = new List<PSSnapInInfo>();
                }
                list.Add(info2);
            }
            return list;
        }

        internal void ImportCmdletsFromAssembly(Assembly assembly, PSModuleInfo module)
        {
            if (assembly == null)
            {
                ArgumentNullException exception = new ArgumentNullException("assembly");
                throw exception;
            }
            Dictionary<string, SessionStateCmdletEntry> cmdlets = null;
            Dictionary<string, SessionStateProviderEntry> providers = null;
            string helpFile = null;
            PSSnapInHelpers.AnalyzePSSnapInAssembly(assembly, assembly.Location, null, module, true, out cmdlets, out providers, out helpFile);
            SessionStateAssemblyEntry item = new SessionStateAssemblyEntry(assembly.FullName, assembly.Location);
            this.Assemblies.Add(item);
            if (cmdlets != null)
            {
                foreach (SessionStateCmdletEntry entry2 in cmdlets.Values)
                {
                    this.Commands.Add(entry2);
                }
            }
            if (providers != null)
            {
                foreach (SessionStateProviderEntry entry3 in providers.Values)
                {
                    this.Providers.Add(entry3);
                }
            }
        }

        internal void ImportCmdletsFromAssembly(string fileName, out PSSnapInException warning)
        {
            if (fileName == null)
            {
                ArgumentNullException exception = new ArgumentNullException("fileName");
                throw exception;
            }
            Assembly assembly = LoadAssemblyFromFile(fileName);
            this.ImportCmdletsFromAssembly(assembly, null);
            warning = null;
        }

        internal PSSnapInInfo ImportCorePSSnapIn()
        {
            PSSnapInInfo item = PSSnapInReader.ReadCoreEngineSnapIn();
            this.defaultSnapins.Add(item);
            try
            {
                PSSnapInException exception;
                this.ImportPSSnapIn(item, out exception);
            }
            catch (PSSnapInException exception2)
            {
                throw exception2;
            }
            return item;
        }

        internal void ImportPSCoreModule(string[] name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            foreach (string str in name)
            {
                this._coreModulesToImport.Add(str);
            }
        }

        public void ImportPSModule(string[] name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            foreach (string str in name)
            {
                this._moduleSpecificationsToImport.Add(new ModuleSpecification(str));
            }
        }

        internal void ImportPSModule(IEnumerable<ModuleSpecification> modules)
        {
            foreach (ModuleSpecification specification in modules)
            {
                this._moduleSpecificationsToImport.Add(specification);
            }
        }

        public void ImportPSModulesFromPath(string path)
        {
            List<string> availableModuleFiles = new List<string>();
            string directory = Environment.ExpandEnvironmentVariables(path);
            List<string> modulePaths = new List<string> {
                directory
            };
            ModuleUtils.GetDefaultAvailableModuleFiles(directory, availableModuleFiles, modulePaths);
            foreach (string str2 in availableModuleFiles)
            {
                this.ImportPSModule(new string[] { str2 });
            }
        }

        internal PSSnapInInfo ImportPSSnapIn(PSSnapInInfo psSnapInInfo, out PSSnapInException warning)
        {
            bool flag = true;
            foreach (SessionStateAssemblyEntry entry in this.Assemblies)
            {
                if (entry.PSSnapIn != null)
                {
                    string assemblyName = entry.PSSnapIn.AssemblyName;
                    if (!string.IsNullOrEmpty(assemblyName) && string.Equals(assemblyName, psSnapInInfo.AssemblyName, StringComparison.OrdinalIgnoreCase))
                    {
                        warning = null;
                        flag = false;
                        break;
                    }
                }
            }
            Dictionary<string, SessionStateCmdletEntry> cmdlets = null;
            Dictionary<string, SessionStateProviderEntry> providers = null;
            if (psSnapInInfo == null)
            {
                ArgumentNullException exception = new ArgumentNullException("psSnapInInfo");
                throw exception;
            }
            if (!string.IsNullOrEmpty(psSnapInInfo.CustomPSSnapInType))
            {
                this.LoadCustomPSSnapIn(psSnapInInfo);
                warning = null;
                return psSnapInInfo;
            }
            Assembly assembly = null;
            string helpFile = null;
            if (flag)
            {
                _PSSnapInTracer.WriteLine("Loading assembly for psSnapIn {0}", new object[] { psSnapInInfo.Name });
                assembly = PSSnapInHelpers.LoadPSSnapInAssembly(psSnapInInfo, out cmdlets, out providers);
                if (assembly == null)
                {
                    _PSSnapInTracer.TraceError("Loading assembly for psSnapIn {0} failed", new object[] { psSnapInInfo.Name });
                    warning = null;
                    return null;
                }
                _PSSnapInTracer.WriteLine("Loading assembly for psSnapIn {0} succeeded", new object[] { psSnapInInfo.Name });
                PSSnapInHelpers.AnalyzePSSnapInAssembly(assembly, psSnapInInfo.Name, psSnapInInfo, null, true, out cmdlets, out providers, out helpFile);
            }
            foreach (string str3 in psSnapInInfo.Types)
            {
                SessionStateTypeEntry entry2 = new SessionStateTypeEntry(Path.Combine(psSnapInInfo.ApplicationBase, str3));
                entry2.SetPSSnapIn(psSnapInInfo);
                this.Types.Add(entry2);
            }
            foreach (string str5 in psSnapInInfo.Formats)
            {
                SessionStateFormatEntry entry3 = new SessionStateFormatEntry(Path.Combine(psSnapInInfo.ApplicationBase, str5));
                entry3.SetPSSnapIn(psSnapInInfo);
                this.Formats.Add(entry3);
            }
            SessionStateAssemblyEntry item = new SessionStateAssemblyEntry(psSnapInInfo.AssemblyName, psSnapInInfo.AbsoluteModulePath);
            item.SetPSSnapIn(psSnapInInfo);
            this.Assemblies.Add(item);
            if (psSnapInInfo.Name.Equals(CoreSnapin, StringComparison.OrdinalIgnoreCase))
            {
                item = new SessionStateAssemblyEntry("Microsoft.PowerShell.Security", null);
                this.Assemblies.Add(item);
            }
            if (cmdlets != null)
            {
                foreach (SessionStateCmdletEntry entry5 in cmdlets.Values)
                {
                    this.Commands.Add((SessionStateCmdletEntry) entry5.Clone());
                }
            }
            if (providers != null)
            {
                foreach (SessionStateProviderEntry entry6 in providers.Values)
                {
                    this.Providers.Add(entry6);
                }
            }
            warning = null;
            if (psSnapInInfo.Name.Equals(CoreSnapin, StringComparison.OrdinalIgnoreCase))
            {
                foreach (SessionStateFunctionEntry entry7 in BuiltInFunctions)
                {
                    Collection<SessionStateCommandEntry> collection = this.Commands[entry7.Name];
                    foreach (SessionStateCommandEntry entry8 in collection)
                    {
                        if (entry8 is SessionStateFunctionEntry)
                        {
                            ((SessionStateFunctionEntry) entry8).SetHelpFile(helpFile);
                        }
                    }
                }
            }
            return psSnapInInfo;
        }

        public PSSnapInInfo ImportPSSnapIn (string name, out PSSnapInException warning)
		{
			if (string.IsNullOrEmpty (name)) {
				PSTraceSource.NewArgumentNullException ("name");
			}
			warning = null;
			PSSnapInInfo psSnapInInfo = PSSnapInReader.Read ("3", name);
			if (psSnapInInfo != null) {
				if (!Utils.IsPSVersionSupported (psSnapInInfo.PSVersion.ToString ())) {
					_PSSnapInTracer.TraceError ("MshSnapin {0} and current monad engine's versions don't match.", new object[] { name });
					throw PSTraceSource.NewArgumentException ("mshSnapInID", "ConsoleInfoErrorStrings", "AddPSSnapInBadMonadVersion", new object[] {
						psSnapInInfo.PSVersion.ToString (),
						"3.0"
					});
				}
	            PSSnapInInfo info2 = this.ImportPSSnapIn(psSnapInInfo, out warning);
	            if (info2 != null)
	            {
	                this._importedSnapins.Add(info2.Name, info2);
	            }
	            return info2;
			}
			return psSnapInInfo;
        }

        internal static bool IsConstantEngineModule(string moduleName)
        {
            if (!ConstantEngineModules.Contains(moduleName))
            {
                return ConstantEngineNestedModules.Contains(moduleName);
            }
            return true;
        }

        internal static bool IsEngineModule(string moduleName)
        {
            if (!EngineModules.Contains(moduleName))
            {
                return NestedEngineModules.Contains(moduleName);
            }
            return true;
        }

        internal static bool IsNestedEngineModule(string moduleName)
        {
            return NestedEngineModules.Contains(moduleName);
        }

        internal static Assembly LoadAssemblyFromFile(string fileName)
        {
            _PSSnapInTracer.WriteLine("Loading assembly for psSnapIn {0}", new object[] { fileName });
            Assembly assembly = Assembly.LoadFrom(fileName);
            if (assembly == null)
            {
                _PSSnapInTracer.TraceError("Loading assembly for psSnapIn {0} failed", new object[] { fileName });
                return null;
            }
            _PSSnapInTracer.WriteLine("Loading assembly for psSnapIn {0} succeeded", new object[] { fileName });
            return assembly;
        }

        private void LoadCustomPSSnapIn(PSSnapInInfo psSnapInInfo)
        {
            if ((psSnapInInfo != null) && !string.IsNullOrEmpty(psSnapInInfo.CustomPSSnapInType))
            {
                Dictionary<string, SessionStateCmdletEntry> cmdlets = null;
                Dictionary<string, SessionStateProviderEntry> providers = null;
                Assembly assembly = null;
                _PSSnapInTracer.WriteLine("Loading assembly for mshsnapin {0}", new object[] { psSnapInInfo.Name });
                assembly = PSSnapInHelpers.LoadPSSnapInAssembly(psSnapInInfo, out cmdlets, out providers);
                if (assembly == null)
                {
                    _PSSnapInTracer.TraceError("Loading assembly for mshsnapin {0} failed", new object[] { psSnapInInfo.Name });
                }
                else
                {
                    CustomPSSnapIn customPSSnapIn = null;
                    try
                    {
                        if (assembly.GetType(psSnapInInfo.CustomPSSnapInType, true) != null)
                        {
                            customPSSnapIn = (CustomPSSnapIn) assembly.CreateInstance(psSnapInInfo.CustomPSSnapInType);
                        }
                        _PSSnapInTracer.WriteLine("Loading assembly for mshsnapin {0} succeeded", new object[] { psSnapInInfo.Name });
                    }
                    catch (TypeLoadException exception)
                    {
                        throw new PSSnapInException(psSnapInInfo.Name, exception.Message);
                    }
                    catch (ArgumentException exception2)
                    {
                        throw new PSSnapInException(psSnapInInfo.Name, exception2.Message);
                    }
                    catch (MissingMethodException exception3)
                    {
                        throw new PSSnapInException(psSnapInInfo.Name, exception3.Message);
                    }
                    catch (InvalidCastException exception4)
                    {
                        throw new PSSnapInException(psSnapInInfo.Name, exception4.Message);
                    }
                    catch (TargetInvocationException exception5)
                    {
                        if (exception5.InnerException != null)
                        {
                            throw new PSSnapInException(psSnapInInfo.Name, exception5.InnerException.Message);
                        }
                        throw new PSSnapInException(psSnapInInfo.Name, exception5.Message);
                    }
                    this.MergeCustomPSSnapIn(psSnapInInfo, customPSSnapIn);
                }
            }
        }

        private static void MakeDisallowedEntriesPrivate<T>(InitialSessionStateEntryCollection<T> list, List<string> allowedNames, Converter<T, string> nameGetter) where T: ConstrainedSessionStateEntry
        {
            foreach (T local in list)
            {
                string entryName = nameGetter(local);
                if (!allowedNames.Exists(allowedName => allowedName.Equals(entryName, StringComparison.OrdinalIgnoreCase)))
                {
                    local.Visibility = SessionStateEntryVisibility.Private;
                }
            }
        }

        private void MergeCustomPSSnapIn(PSSnapInInfo psSnapInInfo, CustomPSSnapIn customPSSnapIn)
        {
            if ((psSnapInInfo != null) && (customPSSnapIn != null))
            {
                _PSSnapInTracer.WriteLine("Merging configuration from custom mshsnapin {0}", new object[] { psSnapInInfo.Name });
                if (customPSSnapIn.Cmdlets != null)
                {
                    foreach (CmdletConfigurationEntry entry in customPSSnapIn.Cmdlets)
                    {
                        SessionStateCmdletEntry entry2 = new SessionStateCmdletEntry(entry.Name, entry.ImplementingType, entry.HelpFileName);
                        entry2.SetPSSnapIn(psSnapInInfo);
                        this.Commands.Add(entry2);
                    }
                }
                if (customPSSnapIn.Providers != null)
                {
                    foreach (ProviderConfigurationEntry entry3 in customPSSnapIn.Providers)
                    {
                        SessionStateProviderEntry entry4 = new SessionStateProviderEntry(entry3.Name, entry3.ImplementingType, entry3.HelpFileName);
                        entry4.SetPSSnapIn(psSnapInInfo);
                        this.Providers.Add(entry4);
                    }
                }
                if (customPSSnapIn.Types != null)
                {
                    foreach (TypeConfigurationEntry entry5 in customPSSnapIn.Types)
                    {
                        SessionStateTypeEntry entry6 = new SessionStateTypeEntry(Path.Combine(psSnapInInfo.ApplicationBase, entry5.FileName));
                        entry6.SetPSSnapIn(psSnapInInfo);
                        this.Types.Add(entry6);
                    }
                }
                if (customPSSnapIn.Formats != null)
                {
                    foreach (FormatConfigurationEntry entry7 in customPSSnapIn.Formats)
                    {
                        SessionStateFormatEntry entry8 = new SessionStateFormatEntry(Path.Combine(psSnapInInfo.ApplicationBase, entry7.FileName));
                        entry8.SetPSSnapIn(psSnapInInfo);
                        this.Formats.Add(entry8);
                    }
                }
                SessionStateAssemblyEntry item = new SessionStateAssemblyEntry(psSnapInInfo.AssemblyName, psSnapInInfo.AbsoluteModulePath);
                this.Assemblies.Add(item);
            }
        }

        internal static void RemoveAllDrivesForProvider(ProviderInfo pi, SessionStateInternal ssi)
        {
            foreach (PSDriveInfo info in ssi.GetDrivesForProvider(pi.FullName))
            {
                try
                {
                    ssi.RemoveDrive(info, true, null);
                }
                catch (Exception exception)
                {
                    CommandProcessorBase.CheckForSevereException(exception);
                }
            }
        }

        private static void RemoveDisallowedEntries<T>(InitialSessionStateEntryCollection<T> list, List<string> allowedNames, Converter<T, string> nameGetter) where T: InitialSessionStateEntry
        {
            List<string> list2 = new List<string>();
            foreach (T local in list)
            {
                string entryName = nameGetter(local);
                if (!allowedNames.Exists(allowedName => allowedName.Equals(entryName, StringComparison.OrdinalIgnoreCase)))
                {
                    list2.Add(local.Name);
                }
            }
            foreach (string str in list2)
            {
                list.Remove(str, null);
            }
        }

        internal static void RemoveTypesAndFormats(System.Management.Automation.ExecutionContext context, IEnumerable<string> formatFilesToRemove, IEnumerable<string> typeFilesToRemove)
        {
            if ((formatFilesToRemove != null) && (formatFilesToRemove.Count<string>() > 0))
            {
                InitialSessionStateEntryCollection<SessionStateFormatEntry> items = new InitialSessionStateEntryCollection<SessionStateFormatEntry>();
                HashSet<string> set = new HashSet<string>(formatFilesToRemove, StringComparer.OrdinalIgnoreCase);
                foreach (SessionStateFormatEntry entry in context.InitialSessionState.Formats)
                {
                    if (!set.Contains(entry.FileName))
                    {
                        items.Add(entry);
                    }
                }
                context.InitialSessionState.Formats.Clear();
                context.InitialSessionState.Formats.Add(items);
                context.InitialSessionState.UpdateFormats(context, false);
            }
            if (typeFilesToRemove != null)
            {
                InitialSessionStateEntryCollection<SessionStateTypeEntry> entrys2 = new InitialSessionStateEntryCollection<SessionStateTypeEntry>();
                List<string> list = new List<string>();
                foreach (string str in typeFilesToRemove)
                {
                    list.Add(ModuleCmdletBase.ResolveRootedFilePath(str, context) ?? str);
                }
                foreach (SessionStateTypeEntry entry2 in context.InitialSessionState.Types)
                {
                    if (entry2.FileName == null)
                    {
                        entrys2.Add(entry2);
                    }
                    else
                    {
                        string item = ModuleCmdletBase.ResolveRootedFilePath(entry2.FileName, context) ?? entry2.FileName;
                        if (!list.Contains(item))
                        {
                            entrys2.Add(entry2);
                        }
                    }
                }
                if (entrys2.Count > 0)
                {
                    context.InitialSessionState.Types.Clear();
                    context.InitialSessionState.Types.Add(entrys2);
                    context.InitialSessionState.UpdateTypes(context, false, false);
                }
                else
                {
                    context.TypeTable.Clear();
                }
            }
        }

        internal void ResetRunspaceState(System.Management.Automation.ExecutionContext context)
        {
            lock (this._syncObject)
            {
                SessionStateInternal engineSessionState = context.EngineSessionState;
                engineSessionState.InitializeSessionStateInternalSpecialVariables(true);
                foreach (SessionStateVariableEntry entry in BuiltInVariables)
                {
                    PSVariable variable = new PSVariable(entry.Name, entry.Value, entry.Options, entry.Attributes, entry.Description) {
                        Visibility = entry.Visibility
                    };
                    engineSessionState.GlobalScope.SetVariable(entry.Name, variable, false, true, engineSessionState, CommandOrigin.Internal, true);
                }
                engineSessionState.InitializeFixedVariables();
                foreach (SessionStateVariableEntry entry2 in this.Variables)
                {
                    PSVariable variable3 = new PSVariable(entry2.Name, entry2.Value, entry2.Options, entry2.Attributes, entry2.Description) {
                        Visibility = entry2.Visibility
                    };
                    engineSessionState.GlobalScope.SetVariable(entry2.Name, variable3, false, true, engineSessionState, CommandOrigin.Internal, true);
                }
                CreateQuestionVariable(context);
                SetSessionStateDrive(context, true);
                context.ResetManagers();
                context.PSDebugTraceLevel = 0;
                context.PSDebugTraceStep = false;
            }
        }

        internal void SaveAsConsoleFile(string path)
        {
            if (path == null)
            {
                throw PSTraceSource.NewArgumentNullException("path");
            }
            if (!path.EndsWith(".psc1", StringComparison.OrdinalIgnoreCase))
            {
                throw PSTraceSource.NewArgumentException("path", "ConsoleInfoErrorStrings", "BadConsoleExtension", new object[] { "" });
            }
            PSConsoleFileElement.WriteToFile(path, PSVersionInfo.PSVersion, this.ImportedSnapins.Values);
        }

        internal static void SetSessionStateDrive(System.Management.Automation.ExecutionContext context, bool setLocation)
        {
            try
            {
                bool flag = true;
                if (context.EngineSessionState.ProviderCount > 0)
                {
                    if (context.EngineSessionState.CurrentDrive == null)
                    {
                        bool flag2 = false;
                        try
                        {
                            Collection<PSDriveInfo> drives = context.EngineSessionState.GetSingleProvider(context.ProviderNames.FileSystem).Drives;
                            if ((drives != null) && (drives.Count > 0))
                            {
                                context.EngineSessionState.CurrentDrive = drives[0];
                                flag2 = true;
                            }
                        }
                        catch (ProviderNotFoundException)
                        {
                        }
                        if (!flag2)
                        {
                            Collection<PSDriveInfo> collection2 = context.EngineSessionState.Drives(null);
                            if ((collection2 != null) && (collection2.Count > 0))
                            {
                                context.EngineSessionState.CurrentDrive = collection2[0];
                            }
                            else
                            {
                                ItemNotFoundException e = new ItemNotFoundException(Environment.CurrentDirectory, "PathNotFound", SessionStateStrings.PathNotFound);
                                context.ReportEngineStartupError(e);
                                flag = false;
                            }
                        }
                    }
                    if (flag && setLocation)
                    {
                        CmdletProviderContext context2 = new CmdletProviderContext(context);
                        try
                        {
                            context.EngineSessionState.SetLocation(Environment.CurrentDirectory, context2);
                        }
                        catch (ItemNotFoundException)
                        {
                            string directoryName = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                            context.EngineSessionState.SetLocation(directoryName, context2);
                        }
                    }
                }
            }
            catch (Exception exception2)
            {
                CommandProcessorBase.CheckForSevereException(exception2);
            }
        }

        private static void ThrowTypeOrFormatErrors(string resourceString, string errorMsg, string errorId)
        {
            RuntimeException exception = new RuntimeException(StringUtil.Format(resourceString, errorMsg));
            exception.SetErrorId(errorId);
            throw exception;
        }

        internal void Unbind(System.Management.Automation.ExecutionContext context)
        {
            lock (this._syncObject)
            {
                SessionStateInternal engineSessionState = context.EngineSessionState;
                foreach (SessionStateAssemblyEntry entry in this.Assemblies)
                {
                    context.RemoveAssembly(entry.Name);
                }
                foreach (SessionStateCommandEntry entry2 in this.Commands)
                {
                    SessionStateCmdletEntry entry3 = entry2 as SessionStateCmdletEntry;
                    if ((entry3 != null) && context.TopLevelSessionState.GetCmdletTable().ContainsKey(entry3.Name))
                    {
                        List<CmdletInfo> list = context.TopLevelSessionState.GetCmdletTable()[entry3.Name];
                        for (int i = list.Count - 1; i >= 0; i--)
                        {
                            if (list[i].ModuleName.Equals(entry2.PSSnapIn.Name))
                            {
                                string name = list[i].Name;
                                list.RemoveAt(i);
                                context.TopLevelSessionState.RemoveCmdlet(name, i, true);
                            }
                        }
                        if (list.Count == 0)
                        {
                            context.TopLevelSessionState.RemoveCmdletEntry(entry3.Name, true);
                        }
                    }
                }
                if ((this._providers != null) && (this._providers.Count > 0))
                {
                    Dictionary<string, List<ProviderInfo>> providers = context.TopLevelSessionState.Providers;
                    foreach (SessionStateProviderEntry entry4 in this._providers)
                    {
                        if (providers.ContainsKey(entry4.Name))
                        {
                            List<ProviderInfo> list2 = providers[entry4.Name];
                            for (int j = list2.Count - 1; j >= 0; j--)
                            {
                                ProviderInfo pi = list2[j];
                                if (pi.ImplementingType == entry4.ImplementingType)
                                {
                                    RemoveAllDrivesForProvider(pi, context.TopLevelSessionState);
                                    list2.RemoveAt(j);
                                }
                            }
                            if (list2.Count == 0)
                            {
                                providers.Remove(entry4.Name);
                            }
                        }
                    }
                }
                List<string> formatFilesToRemove = new List<string>();
                if (this.Formats != null)
                {
                    formatFilesToRemove.AddRange(from f in this.Formats select f.FileName);
                }
                List<string> typeFilesToRemove = new List<string>();
                if (this.Types != null)
                {
                    typeFilesToRemove.AddRange(from t in this.Types select t.FileName);
                }
                RemoveTypesAndFormats(context, formatFilesToRemove, typeFilesToRemove);
            }
        }

        internal void UpdateFormats(System.Management.Automation.ExecutionContext context, bool update)
        {
            if (!this.DisableFormatUpdates && (this.Formats.Count != 0))
            {
                InitialSessionStateEntryCollection<SessionStateFormatEntry> formats;
                Collection<PSSnapInTypeAndFormatErrors> mshsnapins = new Collection<PSSnapInTypeAndFormatErrors>();
                if (update && (context.InitialSessionState != null))
                {
                    formats = context.InitialSessionState.Formats;
                    formats.Add(this.Formats);
                }
                else
                {
                    formats = this.Formats;
                }
                foreach (SessionStateFormatEntry entry in formats)
                {
                    string fileName = entry.FileName;
                    PSSnapInInfo pSSnapIn = entry.PSSnapIn;
                    if ((pSSnapIn != null) && !string.IsNullOrEmpty(pSSnapIn.Name))
                    {
                        fileName = pSSnapIn.Name;
                    }
                    if (entry.Formattable != null)
                    {
                        if (formats.Count != 1)
                        {
                            throw PSTraceSource.NewInvalidOperationException("FormatAndOutXmlLoadingStrings", "FormatTableCannotCoExist", new object[0]);
                        }
                        context.FormatDBManager = entry.Formattable.FormatDBManager;
                    }
                    else if (entry.FormatData != null)
                    {
                        mshsnapins.Add(new PSSnapInTypeAndFormatErrors(fileName, entry.FormatData));
                    }
                    else
                    {
                        mshsnapins.Add(new PSSnapInTypeAndFormatErrors(fileName, entry.FileName));
                    }
                }
                if (mshsnapins.Count > 0)
                {
                    context.FormatDBManager.UpdateDataBase(mshsnapins, context.AuthorizationManager, context.EngineHostInterface, true);
                    StringBuilder builder = new StringBuilder("\n");
                    bool flag = false;
                    foreach (PSSnapInTypeAndFormatErrors errors in mshsnapins)
                    {
                        if ((errors.Errors != null) && (errors.Errors.Count > 0))
                        {
                            foreach (string str2 in errors.Errors)
                            {
                                if (!string.IsNullOrEmpty(str2))
                                {
                                    flag = true;
                                    if (this.ThrowOnRunspaceOpenError || this.RefreshTypeAndFormatSetting)
                                    {
                                        builder.Append(str2);
                                        builder.Append('\n');
                                    }
                                    else
                                    {
                                        context.ReportEngineStartupError(FormatAndOutXmlLoadingStrings.FormatLoadingErrors, new object[] { str2 });
                                    }
                                }
                            }
                        }
                    }
                    if ((this.ThrowOnRunspaceOpenError || this.RefreshTypeAndFormatSetting) && flag)
                    {
                        ThrowTypeOrFormatErrors(FormatAndOutXmlLoadingStrings.FormatLoadingErrors, builder.ToString(), "ErrorsUpdatingFormats");
                    }
                }
            }
        }

        internal void UpdateTypes(System.Management.Automation.ExecutionContext context, bool updateOnly, bool preValidated)
        {
            bool clearTable = !updateOnly;
            bool flag2 = false;
            TypeTable typeTable = null;
            StringBuilder builder = new StringBuilder("\n");
            foreach (SessionStateTypeEntry entry in this.Types)
            {
                string moduleName = "";
                if ((entry.PSSnapIn != null) && !string.IsNullOrEmpty(entry.PSSnapIn.Name))
                {
                    moduleName = entry.PSSnapIn.Name;
                }
                Collection<string> errors = new Collection<string>();
                if (entry.TypeTable != null)
                {
                    if (!clearTable || (this.Types.Count != 1))
                    {
                        throw PSTraceSource.NewInvalidOperationException("TypesXml", "TypeTableCannotCoExist", new object[0]);
                    }
                    context.TypeTable = entry.TypeTable;
                    typeTable = entry.TypeTable;
                    break;
                }
                if (entry.FileName != null)
                {
                    bool flag3;
                    context.TypeTable.Update(moduleName, entry.FileName, errors, clearTable, context.AuthorizationManager, context.EngineHostInterface, preValidated, out flag3);
                }
                else
                {
                    context.TypeTable.Update(entry.TypeData, errors, entry.IsRemove, clearTable);
                }
                if (updateOnly && (context.InitialSessionState != null))
                {
                    context.InitialSessionState.Types.Add(entry);
                }
                clearTable = false;
                foreach (string str2 in errors)
                {
                    if (!string.IsNullOrEmpty(str2))
                    {
                        flag2 = true;
                        if (this.ThrowOnRunspaceOpenError || this.RefreshTypeAndFormatSetting)
                        {
                            builder.Append(str2);
                            builder.Append('\n');
                        }
                        else
                        {
                            context.ReportEngineStartupError(ExtendedTypeSystem.TypesXmlError, new object[] { str2 });
                        }
                    }
                }
                if (this.ThrowOnRunspaceOpenError && (errors.Count > 0))
                {
                    ThrowTypeOrFormatErrors(ExtendedTypeSystem.TypesXmlError, builder.ToString(), "ErrorsUpdatingTypes");
                }
            }
            if (typeTable != null)
            {
                this.Types.Clear();
                this.Types.Add(typeTable.typesInfo);
            }
            if (this.RefreshTypeAndFormatSetting && flag2)
            {
                ThrowTypeOrFormatErrors(ExtendedTypeSystem.TypesXmlError, builder.ToString(), "ErrorsUpdatingTypes");
            }
        }

        public System.Threading.ApartmentState ApartmentState
        {
            get
            {
                return this.apartmentState;
            }
            set
            {
                this.apartmentState = value;
            }
        }

        public virtual InitialSessionStateEntryCollection<SessionStateAssemblyEntry> Assemblies
        {
            get
            {
                lock (this._syncObject)
                {
                    if (this._assemblies == null)
                    {
                        this._assemblies = new InitialSessionStateEntryCollection<SessionStateAssemblyEntry>();
                    }
                }
                return this._assemblies;
            }
        }

        public virtual System.Management.Automation.AuthorizationManager AuthorizationManager
        {
            get
            {
                return this._authorizationManager;
            }
            set
            {
                this._authorizationManager = value;
            }
        }

        internal static SessionStateAliasEntry[] BuiltInAliases
        {
            get
            {
                return new SessionStateAliasEntry[] { 
                    new SessionStateAliasEntry("ac", "Add-Content", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("asnp", "Add-PSSnapIn", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("clc", "Clear-Content", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("cli", "Clear-Item", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("clp", "Clear-ItemProperty", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("clv", "Clear-Variable", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("compare", "Compare-Object", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("cpi", "Copy-Item", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("cpp", "Copy-ItemProperty", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("cvpa", "Convert-Path", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("dbp", "Disable-PSBreakpoint", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("diff", "Compare-Object", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("ebp", "Enable-PSBreakpoint", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("epal", "Export-Alias", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("epcsv", "Export-Csv", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("fc", "Format-Custom", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), 
                    new SessionStateAliasEntry("fl", "Format-List", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("foreach", "ForEach-Object", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("%", "ForEach-Object", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("ft", "Format-Table", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("fw", "Format-Wide", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("gal", "Get-Alias", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("gbp", "Get-PSBreakpoint", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("gc", "Get-Content", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("gci", "Get-ChildItem", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("gcm", "Get-Command", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("gdr", "Get-PSDrive", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("gcs", "Get-PSCallStack", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("ghy", "Get-History", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("gi", "Get-Item", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("gl", "Get-Location", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("gm", "Get-Member", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), 
                    new SessionStateAliasEntry("gmo", "Get-Module", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("gp", "Get-ItemProperty", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("gps", "Get-Process", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("group", "Group-Object", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("gsv", "Get-Service", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("gsnp", "Get-PSSnapIn", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("gu", "Get-Unique", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("gv", "Get-Variable", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("gwmi", "Get-WmiObject", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("iex", "Invoke-Expression", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("ihy", "Invoke-History", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("ii", "Invoke-Item", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("ipmo", "Import-Module", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("iwmi", "Invoke-WMIMethod", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("ipal", "Import-Alias", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("ipcsv", "Import-Csv", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), 
                    new SessionStateAliasEntry("measure", "Measure-Object", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("mi", "Move-Item", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("mp", "Move-ItemProperty", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("nal", "New-Alias", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("ndr", "New-PSDrive", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("ni", "New-Item", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("nv", "New-Variable", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("nmo", "New-Module", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("oh", "Out-Host", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("ogv", "Out-GridView", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("ise", "powershell_ise.exe", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("rbp", "Remove-PSBreakpoint", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("rdr", "Remove-PSDrive", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("ri", "Remove-Item", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("rni", "Rename-Item", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("rnp", "Rename-ItemProperty", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), 
                    new SessionStateAliasEntry("rp", "Remove-ItemProperty", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("rmo", "Remove-Module", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("rsnp", "Remove-PSSnapin", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("rv", "Remove-Variable", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("rwmi", "Remove-WMIObject", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("rvpa", "Resolve-Path", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("sal", "Set-Alias", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("sasv", "Start-Service", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("sbp", "Set-PSBreakpoint", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("sc", "Set-Content", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("select", "Select-Object", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("si", "Set-Item", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("sl", "Set-Location", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("swmi", "Set-WMIInstance", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("shcm", "Show-Command", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("sleep", "Start-Sleep", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), 
                    new SessionStateAliasEntry("sort", "Sort-Object", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("sp", "Set-ItemProperty", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("saps", "Start-Process", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("start", "Start-Process", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("spps", "Stop-Process", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("spsv", "Stop-Service", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("sv", "Set-Variable", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("tee", "Tee-Object", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("trcm", "Trace-Command", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("where", "Where-Object", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("?", "Where-Object", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("write", "Write-Output", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("rcsn", "Receive-PSSession", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("cnsn", "Connect-PSSession", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("dnsn", "Disconnect-PSSession", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("irm", "Invoke-RestMethod", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), 
                    new SessionStateAliasEntry("iwr", "Invoke-WebRequest", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("npssc", "New-PSSessionConfigurationFile", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("cat", "Get-Content", "", ScopedItemOptions.AllScope), new SessionStateAliasEntry("cd", "Set-Location", "", ScopedItemOptions.AllScope), new SessionStateAliasEntry("clear", "Clear-Host", "", ScopedItemOptions.AllScope), new SessionStateAliasEntry("cp", "Copy-Item", "", ScopedItemOptions.AllScope), new SessionStateAliasEntry("h", "Get-History", "", ScopedItemOptions.AllScope), new SessionStateAliasEntry("history", "Get-History", "", ScopedItemOptions.AllScope), new SessionStateAliasEntry("kill", "Stop-Process", "", ScopedItemOptions.AllScope), new SessionStateAliasEntry("lp", "Out-Printer", "", ScopedItemOptions.AllScope), new SessionStateAliasEntry("ls", "Get-ChildItem", "", ScopedItemOptions.AllScope), new SessionStateAliasEntry("man", "help", "", ScopedItemOptions.AllScope), new SessionStateAliasEntry("mount", "New-PSDrive", "", ScopedItemOptions.AllScope), new SessionStateAliasEntry("md", "mkdir", "", ScopedItemOptions.AllScope), new SessionStateAliasEntry("mv", "Move-Item", "", ScopedItemOptions.AllScope), new SessionStateAliasEntry("popd", "Pop-Location", "", ScopedItemOptions.AllScope), 
                    new SessionStateAliasEntry("ps", "Get-Process", "", ScopedItemOptions.AllScope), new SessionStateAliasEntry("pushd", "Push-Location", "", ScopedItemOptions.AllScope), new SessionStateAliasEntry("pwd", "Get-Location", "", ScopedItemOptions.AllScope), new SessionStateAliasEntry("r", "Invoke-History", "", ScopedItemOptions.AllScope), new SessionStateAliasEntry("rm", "Remove-Item", "", ScopedItemOptions.AllScope), new SessionStateAliasEntry("rmdir", "Remove-Item", "", ScopedItemOptions.AllScope), new SessionStateAliasEntry("echo", "Write-Output", "", ScopedItemOptions.AllScope), new SessionStateAliasEntry("cls", "Clear-Host", "", ScopedItemOptions.AllScope), new SessionStateAliasEntry("chdir", "Set-Location", "", ScopedItemOptions.AllScope), new SessionStateAliasEntry("copy", "Copy-Item", "", ScopedItemOptions.AllScope), new SessionStateAliasEntry("del", "Remove-Item", "", ScopedItemOptions.AllScope), new SessionStateAliasEntry("dir", "Get-ChildItem", "", ScopedItemOptions.AllScope), new SessionStateAliasEntry("erase", "Remove-Item", "", ScopedItemOptions.AllScope), new SessionStateAliasEntry("move", "Move-Item", "", ScopedItemOptions.AllScope), new SessionStateAliasEntry("rd", "Remove-Item", "", ScopedItemOptions.AllScope), new SessionStateAliasEntry("ren", "Rename-Item", "", ScopedItemOptions.AllScope), 
                    new SessionStateAliasEntry("set", "Set-Variable", "", ScopedItemOptions.AllScope), new SessionStateAliasEntry("type", "Get-Content", "", ScopedItemOptions.AllScope), new SessionStateAliasEntry("icm", "Invoke-Command", "", ScopedItemOptions.AllScope), new SessionStateAliasEntry("clhy", "Clear-History", "", ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly), new SessionStateAliasEntry("gjb", "Get-Job", "", ScopedItemOptions.AllScope), new SessionStateAliasEntry("rcjb", "Receive-Job", "", ScopedItemOptions.AllScope), new SessionStateAliasEntry("rjb", "Remove-Job", "", ScopedItemOptions.AllScope), new SessionStateAliasEntry("sajb", "Start-Job", "", ScopedItemOptions.AllScope), new SessionStateAliasEntry("spjb", "Stop-Job", "", ScopedItemOptions.AllScope), new SessionStateAliasEntry("wjb", "Wait-Job", "", ScopedItemOptions.AllScope), new SessionStateAliasEntry("sujb", "Suspend-Job", "", ScopedItemOptions.AllScope), new SessionStateAliasEntry("rujb", "Resume-Job", "", ScopedItemOptions.AllScope), new SessionStateAliasEntry("nsn", "New-PSSession", "", ScopedItemOptions.AllScope), new SessionStateAliasEntry("gsn", "Get-PSSession", "", ScopedItemOptions.AllScope), new SessionStateAliasEntry("rsn", "Remove-PSSession", "", ScopedItemOptions.AllScope), new SessionStateAliasEntry("ipsn", "Import-PSSession", "", ScopedItemOptions.AllScope), 
                    new SessionStateAliasEntry("epsn", "Export-PSSession", "", ScopedItemOptions.AllScope), new SessionStateAliasEntry("etsn", "Enter-PSSession", "", ScopedItemOptions.AllScope), new SessionStateAliasEntry("exsn", "Exit-PSSession", "", ScopedItemOptions.AllScope), new SessionStateAliasEntry("sls", "Select-String", "", ScopedItemOptions.None)
                 };
            }
        }

        public virtual InitialSessionStateEntryCollection<SessionStateCommandEntry> Commands
        {
            get
            {
                lock (this._syncObject)
                {
                    if (this._commands == null)
                    {
                        this._commands = new InitialSessionStateEntryCollection<SessionStateCommandEntry>();
                    }
                }
                return this._commands;
            }
        }

        internal HashSet<string> CoreModulesToImport
        {
            get
            {
                return this._coreModulesToImport;
            }
        }

        public bool DisableFormatUpdates { get; set; }

        public virtual InitialSessionStateEntryCollection<SessionStateFormatEntry> Formats
        {
            get
            {
                lock (this._syncObject)
                {
                    if (this._formats == null)
                    {
                        this._formats = new InitialSessionStateEntryCollection<SessionStateFormatEntry>();
                    }
                }
                return this._formats;
            }
        }

        internal Dictionary<string, PSSnapInInfo> ImportedSnapins
        {
            get
            {
                return this._importedSnapins;
            }
        }

        public PSLanguageMode LanguageMode
        {
            get
            {
                return this._languageMode;
            }
            set
            {
                this._languageMode = value;
            }
        }

        public ReadOnlyCollection<ModuleSpecification> Modules
        {
            get
            {
                return new ReadOnlyCollection<ModuleSpecification>(this._moduleSpecificationsToImport);
            }
        }

        internal Collection<ModuleSpecification> ModuleSpecificationsToImport
        {
            get
            {
                return this._moduleSpecificationsToImport;
            }
        }

        public virtual InitialSessionStateEntryCollection<SessionStateProviderEntry> Providers
        {
            get
            {
                lock (this._syncObject)
                {
                    if (this._providers == null)
                    {
                        this._providers = new InitialSessionStateEntryCollection<SessionStateProviderEntry>();
                    }
                }
                return this._providers;
            }
        }

        public PSThreadOptions ThreadOptions
        {
            get
            {
                return this.createThreadOptions;
            }
            set
            {
                this.createThreadOptions = value;
            }
        }

        public bool ThrowOnRunspaceOpenError
        {
            get
            {
                return this.throwOnRunspaceOpenError;
            }
            set
            {
                this.throwOnRunspaceOpenError = value;
            }
        }

        public virtual InitialSessionStateEntryCollection<SessionStateTypeEntry> Types
        {
            get
            {
                lock (this._syncObject)
                {
                    if (this._types == null)
                    {
                        this._types = new InitialSessionStateEntryCollection<SessionStateTypeEntry>();
                    }
                }
                return this._types;
            }
        }

        public bool UseFullLanguageModeInDebugger
        {
            get
            {
                return this._useFullLanguageModeInDebugger;
            }
            set
            {
                this._useFullLanguageModeInDebugger = value;
            }
        }

        public virtual InitialSessionStateEntryCollection<SessionStateVariableEntry> Variables
        {
            get
            {
                lock (this._syncObject)
                {
                    if (this._variables == null)
                    {
                        this._variables = new InitialSessionStateEntryCollection<SessionStateVariableEntry>();
                    }
                }
                return this._variables;
            }
        }

        internal bool WarmUpTabCompletionOnIdle { get; set; }
    }
}

