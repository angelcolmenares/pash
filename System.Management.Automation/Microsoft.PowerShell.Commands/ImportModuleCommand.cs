namespace Microsoft.PowerShell.Commands
{
    using Microsoft.Management.Infrastructure;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Language;
    using System.Management.Automation.Runspaces;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Threading;

    [OutputType(new Type[] { typeof(PSModuleInfo) }), Cmdlet("Import", "Module", DefaultParameterSetName="Name", HelpUri="http://go.microsoft.com/fwlink/?LinkID=141553")]
    public sealed class ImportModuleCommand : ModuleCmdletBase, IDisposable
    {
        private string[] _aliasExportList;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private string[] _cmdletImportList = new string[0];
        private bool _disposed;
        private string[] _functionImportList = new string[0];
        private bool _isScopeSpecified;
        private PSModuleInfo[] _moduleInfo = new PSModuleInfo[0];
        private string[] _name = new string[0];
        private string _scope = string.Empty;
        private string[] _variableExportList;
        private const string ParameterSet_Assembly = "Assembly";
        private const string ParameterSet_ModuleInfo = "ModuleInfo";
        private const string ParameterSet_Name = "Name";
        private const string ParameterSet_ViaCimSession = "CimSession";
        private const string ParameterSet_ViaPsrpSession = "PSSession";

        public ImportModuleCommand()
        {
            base.BaseDisableNameChecking = false;
        }

        protected override void BeginProcessing()
        {
            if (this.Global.IsPresent && this._isScopeSpecified)
            {
                InvalidOperationException exception = new InvalidOperationException(Modules.GlobalAndScopeParameterCannotBeSpecifiedTogether);
                ErrorRecord errorRecord = new ErrorRecord(exception, "Modules_GlobalAndScopeParameterCannotBeSpecifiedTogether", ErrorCategory.InvalidOperation, null);
                base.ThrowTerminatingError(errorRecord);
            }
            if (!string.IsNullOrEmpty(this.Scope) && this.Scope.Equals("GLOBAL", StringComparison.OrdinalIgnoreCase))
            {
                base.BaseGlobal = true;
            }
        }

        private IEnumerable<string> CreateCimModuleFiles(RemoteDiscoveryHelper.CimModule remoteCimModule, RemoteDiscoveryHelper.CimFileCode fileCode, Func<RemoteDiscoveryHelper.CimModuleFile, bool> filesFilter, string temporaryModuleDirectory)
        {
            string format = null;
            switch (fileCode)
            {
                case RemoteDiscoveryHelper.CimFileCode.TypesV1:
                    format = "{0}_{1}.types.ps1xml";
                    break;

                case RemoteDiscoveryHelper.CimFileCode.FormatV1:
                    format = "{0}_{1}.format.ps1xml";
                    break;

                case RemoteDiscoveryHelper.CimFileCode.CmdletizationV1:
                    format = "{0}_{1}.cdxml";
                    break;
            }
            List<string> list = new List<string>();
            foreach (RemoteDiscoveryHelper.CimModuleFile file in remoteCimModule.ModuleFiles)
            {
                if (filesFilter(file))
                {
                    string fileName = Path.GetFileName(file.FileName);
                    string item = string.Format(CultureInfo.InvariantCulture, format, new object[] { fileName.Substring(0, Math.Min(fileName.Length, 20)), Path.GetRandomFileName() });
                    list.Add(item);
                    string path = Path.Combine(temporaryModuleDirectory, item);
                    File.WriteAllBytes(path, file.RawFileData);
                    AlternateDataStreamUtilities.SetZoneOfOrigin(path, SecurityZone.Intranet);
                }
            }
            return list;
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

        private PSModuleInfo ImportModule_LocallyViaName(ModuleCmdletBase.ImportModuleOptions importModuleOptions, string name)
        {
            try
            {
                if (name.Equals("PSWorkflow", StringComparison.OrdinalIgnoreCase) && Utils.IsRunningFromSysWOW64())
                {
                    throw new NotSupportedException(AutomationExceptions.WorkflowDoesNotSupportWOW64);
                }
                bool found = false;
                PSModuleInfo module = null;
                string str = null;
                string str2 = null;
                if (((this.MinimumVersion == null) && (this.RequiredVersion == null)) && (PSModuleInfo.UseAppDomainLevelModuleCache && !base.BaseForce))
                {
                    str = PSModuleInfo.ResolveUsingAppDomainLevelModuleCache(name);
                }
                if (!string.IsNullOrEmpty(str))
                {
                    if (File.Exists(str))
                    {
                        str2 = str;
                    }
                    else
                    {
                        PSModuleInfo.RemoveFromAppDomainLevelCache(name);
                    }
                }
                if (str2 == null)
                {
                    str2 = ModuleCmdletBase.ResolveRootedFilePath(name, base.Context);
                }
                bool flag2 = false;
                if (!string.IsNullOrEmpty(str2))
                {
                    if (!base.BaseForce && base.Context.Modules.ModuleTable.ContainsKey(str2))
                    {
                        PSModuleInfo info2 = base.Context.Modules.ModuleTable[str2];
                        if (((this.RequiredVersion == null) || info2.Version.Equals(this.RequiredVersion)) || (((base.BaseMinimumVersion == null) || (info2.ModuleType != ModuleType.Manifest)) || (info2.Version >= base.BaseMinimumVersion)))
                        {
                            flag2 = true;
                            ModuleCmdletBase.AddModuleToModuleTables(base.Context, base.TargetSessionState.Internal, info2);
                            base.ImportModuleMembers(info2, base.BasePrefix, importModuleOptions);
                            if (base.BaseAsCustomObject)
                            {
                                if (info2.ModuleType != ModuleType.Script)
                                {
                                    InvalidOperationException exception = new InvalidOperationException(StringUtil.Format(Modules.CantUseAsCustomObjectWithBinaryModule, info2.Path));
                                    ErrorRecord errorRecord = new ErrorRecord(exception, "Modules_CantUseAsCustomObjectWithBinaryModule", ErrorCategory.PermissionDenied, null);
                                    base.WriteError(errorRecord);
                                }
                                else
                                {
                                    base.WriteObject(info2.AsCustomObject());
                                }
                            }
                            else if (base.BasePassThru)
                            {
                                base.WriteObject(info2);
                            }
                            found = true;
                            module = info2;
                        }
                    }
                    if (!flag2)
                    {
                        if (File.Exists(str2))
                        {
                            PSModuleInfo info3;
                            if (base.Context.Modules.ModuleTable.TryGetValue(str2, out info3))
                            {
                                base.RemoveModule(info3);
                            }
                            module = base.LoadModule(str2, null, base.BasePrefix, null, ref importModuleOptions, ModuleCmdletBase.ManifestProcessingFlags.LoadElements | ModuleCmdletBase.ManifestProcessingFlags.NullOnFirstError | ModuleCmdletBase.ManifestProcessingFlags.WriteErrors, out found);
                        }
                        else if (Directory.Exists(str2))
                        {
                            str2 = Path.Combine(str2, Path.GetFileName(str2));
                            module = base.LoadUsingExtensions(null, str2, str2, null, null, base.BasePrefix, null, importModuleOptions, ModuleCmdletBase.ManifestProcessingFlags.LoadElements | ModuleCmdletBase.ManifestProcessingFlags.NullOnFirstError | ModuleCmdletBase.ManifestProcessingFlags.WriteErrors, out found);
                        }
                    }
                }
                else
                {
                    if (InitialSessionState.IsEngineModule(name))
                    {
                        CmdletInfo cmdlet = base.Context.SessionState.InvokeCommand.GetCmdlet(@"Microsoft.PowerShell.Core\Get-PSSnapIn");
                        if ((cmdlet != null) && (cmdlet.Visibility == SessionStateEntryVisibility.Public))
                        {
                            CommandInfo commandInfo = new CmdletInfo("Get-PSSnapIn", typeof(GetPSSnapinCommand), null, null, base.Context);
                            Command command = new Command(commandInfo);
                            Collection<PSSnapInInfo> collection = null;
                            try
                            {
                                using (PowerShell shell = PowerShell.Create(RunspaceMode.CurrentRunspace))
                                {
                                    shell.AddCommand(command).AddParameter("Name", name).AddParameter("ErrorAction", ActionPreference.Ignore);
                                    collection = shell.Invoke<PSSnapInInfo>();
                                }
                            }
                            catch (Exception exception2)
                            {
                                CommandProcessorBase.CheckForSevereException(exception2);
                            }
                            if ((collection != null) && (collection.Count == 1))
                            {
                                string text = string.Format(CultureInfo.InvariantCulture, Modules.ModuleLoadedAsASnapin, new object[] { collection[0].Name });
                                base.WriteWarning(text);
                                found = true;
                                return module;
                            }
                        }
                    }
                    if (ModuleCmdletBase.IsRooted(name))
                    {
                        if (!string.IsNullOrEmpty(Path.GetExtension(name)))
                        {
                            module = base.LoadModule(name, null, base.BasePrefix, null, ref importModuleOptions, ModuleCmdletBase.ManifestProcessingFlags.LoadElements | ModuleCmdletBase.ManifestProcessingFlags.NullOnFirstError | ModuleCmdletBase.ManifestProcessingFlags.WriteErrors, out found);
                        }
                        else
                        {
                            module = base.LoadUsingExtensions(null, name, name, null, null, base.BasePrefix, null, importModuleOptions, ModuleCmdletBase.ManifestProcessingFlags.LoadElements | ModuleCmdletBase.ManifestProcessingFlags.NullOnFirstError | ModuleCmdletBase.ManifestProcessingFlags.WriteErrors, out found);
                        }
                    }
                    else
                    {
                        IEnumerable<string> modulePath = ModuleIntrinsics.GetModulePath(false, base.Context);
                        if ((this.MinimumVersion == null) && (this.RequiredVersion == null))
                        {
                            base.AddToAppDomainLevelCache = true;
                        }
                        found = base.LoadUsingModulePath(found, modulePath, name, null, importModuleOptions, ModuleCmdletBase.ManifestProcessingFlags.LoadElements | ModuleCmdletBase.ManifestProcessingFlags.NullOnFirstError | ModuleCmdletBase.ManifestProcessingFlags.WriteErrors, out module);
                    }
                }
                if (!found)
                {
                    ErrorRecord record2 = null;
                    string message = null;
                    if (base.BaseRequiredVersion != null)
                    {
                        message = StringUtil.Format(Modules.ModuleWithVersionNotFound, name, base.BaseRequiredVersion);
                    }
                    else if (base.BaseMinimumVersion != null)
                    {
                        message = StringUtil.Format(Modules.ModuleWithVersionNotFound, name, base.BaseMinimumVersion);
                    }
                    if ((base.BaseRequiredVersion != null) || (base.BaseMinimumVersion != null))
                    {
                        FileNotFoundException exception3 = new FileNotFoundException(message);
                        record2 = new ErrorRecord(exception3, "Modules_ModuleWithVersionNotFound", ErrorCategory.ResourceUnavailable, name);
                    }
                    else
                    {
                        FileNotFoundException exception4 = new FileNotFoundException(StringUtil.Format(Modules.ModuleNotFound, name));
                        record2 = new ErrorRecord(exception4, "Modules_ModuleNotFound", ErrorCategory.ResourceUnavailable, name);
                    }
                    base.WriteError(record2);
                }
                return module;
            }
            catch (PSInvalidOperationException exception5)
            {
                ErrorRecord record3 = new ErrorRecord(exception5.ErrorRecord, exception5);
                base.WriteError(record3);
            }
            return null;
        }

        private PSModuleInfo ImportModule_RemotelyViaCimModuleData(ModuleCmdletBase.ImportModuleOptions importModuleOptions, RemoteDiscoveryHelper.CimModule remoteCimModule, Microsoft.Management.Infrastructure.CimSession cimSession)
        {
            PSModuleInfo info5;
            try
            {
                Token[] tokenArray;
                ParseError[] errorArray;
                Version version;
                Func<RemoteDiscoveryHelper.CimModuleFile, bool> filesFilter = null;
                Func<RemoteDiscoveryHelper.CimModuleFile, bool> func2 = null;
                if (remoteCimModule.MainManifest == null)
                {
                    ArgumentException exception = new ArgumentException(string.Format(CultureInfo.InvariantCulture, Modules.EmptyModuleManifest, new object[] { remoteCimModule.ModuleName + ".psd1" }));
                    throw exception;
                }
                bool containedErrors = false;
                PSModuleInfo module = null;
                string fileName = Path.Combine(RemoteDiscoveryHelper.GetModulePath(remoteCimModule.ModuleName, null, cimSession.ComputerName, base.Context.CurrentRunspace), remoteCimModule.ModuleName + ".psd1");
                Hashtable data = null;
                Hashtable originalManifest = null;
                ScriptBlockAst ast = null;
                ast = Parser.ParseInput(remoteCimModule.MainManifest.FileData, fileName, out tokenArray, out errorArray);
                if ((ast == null) || ((errorArray != null) && (errorArray.Length > 0)))
                {
                    throw new ParseException(errorArray);
                }
                ScriptBlock scriptBlock = new ScriptBlock(ast, false);
                data = base.LoadModuleManifestData(fileName, scriptBlock, ModuleCmdletBase.ModuleManifestMembers, ModuleCmdletBase.ManifestProcessingFlags.NullOnFirstError | ModuleCmdletBase.ManifestProcessingFlags.WriteErrors, ref containedErrors);
                if ((data == null) || containedErrors)
                {
                    return null;
                }
                originalManifest = data;
                if (!base.GetScalarFromData<Version>(data, null, "ModuleVersion", 0, out version))
                {
                    version = null;
                }
                string str2 = RemoteDiscoveryHelper.GetModulePath(remoteCimModule.ModuleName, version, cimSession.ComputerName, base.Context.CurrentRunspace);
                fileName = Path.Combine(str2, remoteCimModule.ModuleName + ".psd1");
                PSModuleInfo info2 = base.IsModuleImportUnnecessaryBecauseModuleIsAlreadyLoaded(fileName, base.BasePrefix, importModuleOptions);
                if (info2 != null)
                {
                    info5 = info2;
                }
                else
                {
                    try
                    {
                        Directory.CreateDirectory(str2);
                        if (filesFilter == null)
                        {
                            filesFilter = cimModuleFile => this.IsTypesPs1XmlFile(cimModuleFile, data);
                        }
                        IEnumerable<string> typesToProcess = this.CreateCimModuleFiles(remoteCimModule, RemoteDiscoveryHelper.CimFileCode.TypesV1, filesFilter, str2);
                        if (func2 == null)
                        {
                            func2 = cimModuleFile => this.IsFormatPs1XmlFile(cimModuleFile, data);
                        }
                        IEnumerable<string> formatsToProcess = this.CreateCimModuleFiles(remoteCimModule, RemoteDiscoveryHelper.CimFileCode.FormatV1, func2, str2);
                        IEnumerable<string> nestedModules = this.CreateCimModuleFiles(remoteCimModule, RemoteDiscoveryHelper.CimFileCode.CmdletizationV1, new Func<RemoteDiscoveryHelper.CimModuleFile, bool>(ImportModuleCommand.IsCmdletizationFile), str2);
                        data = RemoteDiscoveryHelper.RewriteManifest(data, nestedModules, typesToProcess, formatsToProcess);
                        originalManifest = RemoteDiscoveryHelper.RewriteManifest(originalManifest);
                        module = base.LoadModuleManifest(fileName, null, data, originalManifest, ModuleCmdletBase.ManifestProcessingFlags.LoadElements | ModuleCmdletBase.ManifestProcessingFlags.NullOnFirstError | ModuleCmdletBase.ManifestProcessingFlags.WriteErrors, base.BaseMinimumVersion, base.BaseRequiredVersion, ref importModuleOptions, ref containedErrors);
                        if (module == null)
                        {
                            return null;
                        }
                        foreach (PSModuleInfo info3 in module.NestedModules)
                        {
                            Type type;
                            PSPrimitiveDictionary.TryPathGet<Type>(info3.PrivateData as IDictionary, out type, new string[] { "CmdletsOverObjects", "CmdletAdapter" });
                            if (!type.AssemblyQualifiedName.Equals("Microsoft.PowerShell.Cmdletization.Cim.CimCmdletAdapter, Microsoft.PowerShell.Commands.Management, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35", StringComparison.OrdinalIgnoreCase))
                            {
                                ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, CmdletizationCoreResources.ImportModule_UnsupportedCmdletAdapter, new object[] { type.FullName })), "UnsupportedCmdletAdapter", ErrorCategory.InvalidData, type);
                                base.ThrowTerminatingError(errorRecord);
                            }
                        }
                        if (this.IsMixedModePsCimModule(remoteCimModule))
                        {
                            string text = string.Format(CultureInfo.InvariantCulture, Modules.MixedModuleOverCimSessionWarning, new object[] { remoteCimModule.ModuleName });
                            base.WriteWarning(text);
                        }
                        foreach (PSModuleInfo info4 in module.NestedModules)
                        {
                            IDictionary dictionary;
                            PSPrimitiveDictionary.TryPathGet<IDictionary>(info4.PrivateData as IDictionary, out dictionary, new string[] { "CmdletsOverObjects" });
                            dictionary["DefaultSession"] = cimSession;
                        }
                        ScriptBlock newClosure = base.Context.Engine.ParseScriptBlock("\r\n                        Microsoft.PowerShell.Management\\Remove-Item `\r\n                            -LiteralPath $temporaryModulePath `\r\n                            -Force `\r\n                            -Recurse `\r\n                            -ErrorAction SilentlyContinue\r\n\r\n                        if ($previousOnRemoveScript -ne $null)\r\n                        {\r\n                            & $previousOnRemoveScript $args\r\n                        }\r\n                        ", false).GetNewClosure();
                        newClosure.Module.SessionState.PSVariable.Set("temporaryModulePath", str2);
                        newClosure.Module.SessionState.PSVariable.Set("previousOnRemoveScript", module.OnRemove);
                        module.OnRemove = newClosure;
                        ModuleCmdletBase.AddModuleToModuleTables(base.Context, base.TargetSessionState.Internal, module);
                        if (base.BasePassThru)
                        {
                            base.WriteObject(module);
                        }
                        info5 = module;
                    }
                    catch
                    {
                        if (Directory.Exists(str2))
                        {
                            Directory.Delete(str2, true);
                        }
                        throw;
                    }
                    finally
                    {
                        if ((module == null) && Directory.Exists(str2))
                        {
                            Directory.Delete(str2, true);
                        }
                    }
                }
            }
            catch (Exception exception2)
            {
                ErrorRecord errorRecordForProcessingOfCimModule = RemoteDiscoveryHelper.GetErrorRecordForProcessingOfCimModule(exception2, remoteCimModule.ModuleName);
                base.WriteError(errorRecordForProcessingOfCimModule);
                info5 = null;
            }
            return info5;
        }

        private void ImportModule_RemotelyViaCimSession(ModuleCmdletBase.ImportModuleOptions importModuleOptions, string[] moduleNames, Microsoft.Management.Infrastructure.CimSession cimSession, Uri resourceUri, string cimNamespace)
        {
            IEnumerable<RemoteDiscoveryHelper.CimModule> enumerable = RemoteDiscoveryHelper.GetCimModules(cimSession, resourceUri, cimNamespace, moduleNames, false, this, this.CancellationToken).ToList<RemoteDiscoveryHelper.CimModule>();
            IEnumerable<RemoteDiscoveryHelper.CimModule> enumerable2 = from cimModule in enumerable
                where cimModule.IsPsCimModule
                select cimModule;
            foreach (string str in from cimModule in enumerable
                where !cimModule.IsPsCimModule
                select cimModule.ModuleName)
            {
                ErrorRecord errorRecord = new ErrorRecord(new ArgumentException(string.Format(CultureInfo.InvariantCulture, Modules.PsModuleOverCimSessionError, new object[] { str })), "PsModuleOverCimSessionError", ErrorCategory.InvalidArgument, str);
                base.WriteError(errorRecord);
            }
            IEnumerable<string> source = (from cimModule in enumerable select cimModule.ModuleName).ToList<string>();
            foreach (string str3 in moduleNames)
            {
                WildcardPattern wildcardPattern = new WildcardPattern(str3, WildcardOptions.CultureInvariant | WildcardOptions.IgnoreCase);
                if (!source.Any<string>(foundModuleName => wildcardPattern.IsMatch(foundModuleName)))
                {
                    FileNotFoundException exception = new FileNotFoundException(StringUtil.Format(Modules.ModuleNotFound, str3));
                    ErrorRecord record2 = new ErrorRecord(exception, "Modules_ModuleNotFound", ErrorCategory.ResourceUnavailable, str3);
                    base.WriteError(record2);
                }
            }
            foreach (RemoteDiscoveryHelper.CimModule module in enumerable2)
            {
                this.ImportModule_RemotelyViaCimModuleData(importModuleOptions, module, cimSession);
            }
        }

        private IList<PSModuleInfo> ImportModule_RemotelyViaPsrpSession(ModuleCmdletBase.ImportModuleOptions importModuleOptions, IEnumerable<string> moduleNames, System.Management.Automation.Runspaces.PSSession psSession)
        {
            List<PSModuleInfo> list = new List<PSModuleInfo>();
            if (moduleNames != null)
            {
                foreach (string str in moduleNames)
                {
                    IList<PSModuleInfo> collection = this.ImportModule_RemotelyViaPsrpSession(importModuleOptions, str, psSession);
                    list.AddRange(collection);
                }
            }
            return list;
        }

        private IList<PSModuleInfo> ImportModule_RemotelyViaPsrpSession(ModuleCmdletBase.ImportModuleOptions importModuleOptions, string moduleName, System.Management.Automation.Runspaces.PSSession psSession)
        {
            List<PSObject> list;
            using (PowerShell shell = PowerShell.Create())
            {
                shell.Runspace = psSession.Runspace;
                shell.AddCommand("Import-Module");
                shell.AddParameter("Name", moduleName);
                shell.AddParameter("DisableNameChecking", this.DisableNameChecking);
                shell.AddParameter("PassThru", true);
                if (this.MinimumVersion != null)
                {
                    shell.AddParameter("Version", this.MinimumVersion);
                }
                if (this.RequiredVersion != null)
                {
                    shell.AddParameter("RequiredVersion", this.RequiredVersion);
                }
                if (this.ArgumentList != null)
                {
                    shell.AddParameter("ArgumentList", this.ArgumentList);
                }
                if (base.BaseForce)
                {
                    shell.AddParameter("Force", true);
                }
                string errorMessageTemplate = string.Format(CultureInfo.InvariantCulture, Modules.RemoteDiscoveryRemotePsrpCommandFailed, new object[] { string.Format(CultureInfo.InvariantCulture, "Import-Module -Name '{0}'", new object[] { moduleName }) });
                list = RemoteDiscoveryHelper.InvokePowerShell(shell, this.CancellationToken, this, errorMessageTemplate).ToList<PSObject>();
            }
            List<PSModuleInfo> list2 = new List<PSModuleInfo>();
            foreach (PSObject obj2 in list)
            {
                PSPropertyInfo info = obj2.Properties["Name"];
                if (info != null)
                {
                    Version version2;
                    string remoteModuleName = (string) LanguagePrimitives.ConvertTo(info.Value, typeof(string), CultureInfo.InvariantCulture);
                    PSPropertyInfo info2 = obj2.Properties["Version"];
                    Version remoteModuleVersion = null;
                    if ((info2 != null) && LanguagePrimitives.TryConvertTo<Version>(info2.Value, CultureInfo.InvariantCulture, out version2))
                    {
                        remoteModuleVersion = version2;
                    }
                    PSModuleInfo item = this.ImportModule_RemotelyViaPsrpSession_SinglePreimportedModule(importModuleOptions, remoteModuleName, remoteModuleVersion, psSession);
                    if (item != null)
                    {
                        list2.Add(item);
                    }
                }
            }
            return list2;
        }

        private PSModuleInfo ImportModule_RemotelyViaPsrpSession_SinglePreimportedModule(ModuleCmdletBase.ImportModuleOptions importModuleOptions, string remoteModuleName, Version remoteModuleVersion, System.Management.Automation.Runspaces.PSSession psSession)
        {
            PSModuleInfo info3;
            string pattern = RemoteDiscoveryHelper.GetModulePath(remoteModuleName, remoteModuleVersion, psSession.ComputerName, base.Context.CurrentRunspace);
            string str2 = WildcardPattern.Escape(pattern);
            try
            {
                PSModuleInfo info2;
                string modulePath = Path.Combine(pattern, Path.GetFileName(pattern) + ".psm1");
                PSModuleInfo info = base.IsModuleImportUnnecessaryBecauseModuleIsAlreadyLoaded(modulePath, base.BasePrefix, importModuleOptions);
                if (info != null)
                {
                    return info;
                }
                using (PowerShell shell = PowerShell.Create(RunspaceMode.CurrentRunspace))
                {
                    shell.AddCommand("Export-PSSession");
                    shell.AddParameter("OutputModule", str2);
                    shell.AddParameter("AllowClobber", true);
                    shell.AddParameter("Module", remoteModuleName);
                    shell.AddParameter("Force", true);
                    shell.AddParameter("FormatTypeName", "*");
                    shell.AddParameter("Session", psSession);
                    string errorMessageTemplate = string.Format(CultureInfo.InvariantCulture, Modules.RemoteDiscoveryFailedToGenerateProxyForRemoteModule, new object[] { remoteModuleName });
                    if (RemoteDiscoveryHelper.InvokePowerShell(shell, this.CancellationToken, this, errorMessageTemplate).Count<PSObject>() == 0)
                    {
                        return null;
                    }
                }
                string destFileName = Path.Combine(pattern, remoteModuleName + ".psd1");
                File.Move(Path.Combine(pattern, Path.GetFileName(pattern) + ".psd1"), destFileName);
                string name = WildcardPattern.Escape(destFileName);
                object[] argumentList = this.ArgumentList;
                try
                {
                    this.ArgumentList = new object[] { psSession };
                    this.ImportModule_LocallyViaName(importModuleOptions, name);
                }
                finally
                {
                    this.ArgumentList = argumentList;
                }
                Path.GetFileName(pattern);
                string key = Path.Combine(pattern, Path.GetFileName(pattern) + ".psm1");
                if (!base.Context.Modules.ModuleTable.TryGetValue(key, out info2))
                {
                    if (Directory.Exists(pattern))
                    {
                        Directory.Delete(pattern, true);
                    }
                    return null;
                }
                ScriptBlock newClosure = base.Context.Engine.ParseScriptBlock("\r\n                    Microsoft.PowerShell.Management\\Remove-Item `\r\n                        -LiteralPath $temporaryModulePath `\r\n                        -Force `\r\n                        -Recurse `\r\n                        -ErrorAction SilentlyContinue\r\n\r\n                    if ($previousOnRemoveScript -ne $null)\r\n                    {\r\n                        & $previousOnRemoveScript $args\r\n                    }\r\n                    ", false).GetNewClosure();
                newClosure.Module.SessionState.PSVariable.Set("temporaryModulePath", pattern);
                newClosure.Module.SessionState.PSVariable.Set("previousOnRemoveScript", info2.OnRemove);
                info2.OnRemove = newClosure;
                info3 = info2;
            }
            catch
            {
                if (Directory.Exists(pattern))
                {
                    Directory.Delete(pattern, true);
                }
                throw;
            }
            return info3;
        }

        private void ImportModule_ViaAssembly(ModuleCmdletBase.ImportModuleOptions importModuleOptions, System.Reflection.Assembly suppliedAssembly)
        {
            bool flag = false;
            if ((suppliedAssembly != null) && (base.Context.Modules.ModuleTable != null))
            {
                foreach (KeyValuePair<string, PSModuleInfo> pair in base.Context.Modules.ModuleTable)
                {
                    if (pair.Value.Path.Equals(suppliedAssembly.Location, StringComparison.OrdinalIgnoreCase))
                    {
                        flag = true;
                        break;
                    }
                }
            }
            if (!flag)
            {
                bool flag2;
                PSModuleInfo module = base.LoadBinaryModule(false, null, null, suppliedAssembly, null, null, importModuleOptions, ModuleCmdletBase.ManifestProcessingFlags.LoadElements | ModuleCmdletBase.ManifestProcessingFlags.NullOnFirstError | ModuleCmdletBase.ManifestProcessingFlags.WriteErrors, base.BasePrefix, false, false, out flag2);
                if (flag2 && (module != null))
                {
                    ModuleCmdletBase.AddModuleToModuleTables(base.Context, base.TargetSessionState.Internal, module);
                }
            }
        }

        private void ImportModule_ViaLocalModuleInfo(ModuleCmdletBase.ImportModuleOptions importModuleOptions, PSModuleInfo module)
        {
            try
            {
                PSModuleInfo info = null;
                base.Context.Modules.ModuleTable.TryGetValue(module.Path, out info);
                if (!base.BaseForce && base.IsModuleAlreadyLoaded(info))
                {
                    ModuleCmdletBase.AddModuleToModuleTables(base.Context, base.TargetSessionState.Internal, info);
                    base.ImportModuleMembers(info, base.BasePrefix, importModuleOptions);
                    if (base.BaseAsCustomObject)
                    {
                        if (info.ModuleType != ModuleType.Script)
                        {
                            InvalidOperationException exception = new InvalidOperationException(StringUtil.Format(Modules.CantUseAsCustomObjectWithBinaryModule, info.Path));
                            ErrorRecord errorRecord = new ErrorRecord(exception, "Modules_CantUseAsCustomObjectWithBinaryModule", ErrorCategory.PermissionDenied, null);
                            base.WriteError(errorRecord);
                        }
                        else
                        {
                            base.WriteObject(info.AsCustomObject());
                        }
                    }
                    else if (base.BasePassThru)
                    {
                        base.WriteObject(info);
                    }
                }
                else
                {
                    PSModuleInfo info2;
                    if (base.Context.Modules.ModuleTable.TryGetValue(module.Path, out info2))
                    {
                        base.RemoveModule(info2);
                    }
                    PSModuleInfo info3 = module;
                    try
                    {
                        if (module.SessionState == null)
                        {
                            if (File.Exists(module.Path))
                            {
                                bool flag;
                                info3 = base.LoadModule(module.Path, null, base.BasePrefix, null, ref importModuleOptions, ModuleCmdletBase.ManifestProcessingFlags.LoadElements | ModuleCmdletBase.ManifestProcessingFlags.NullOnFirstError | ModuleCmdletBase.ManifestProcessingFlags.WriteErrors, out flag);
                            }
                        }
                        else if (!string.IsNullOrEmpty(module.Name))
                        {
                            ModuleCmdletBase.AddModuleToModuleTables(base.Context, base.TargetSessionState.Internal, info3);
                            if (info3.SessionState != null)
                            {
                                base.ImportModuleMembers(info3, base.BasePrefix, importModuleOptions);
                            }
                            if (base.BaseAsCustomObject && (info3.SessionState != null))
                            {
                                base.WriteObject(module.AsCustomObject());
                            }
                            else if (base.BasePassThru)
                            {
                                base.WriteObject(info3);
                            }
                        }
                    }
                    catch (IOException)
                    {
                    }
                }
            }
            catch (PSInvalidOperationException exception2)
            {
                ErrorRecord record2 = new ErrorRecord(exception2.ErrorRecord, exception2);
                base.WriteError(record2);
            }
        }

        private static bool IsCmdletizationFile(RemoteDiscoveryHelper.CimModuleFile cimModuleFile)
        {
            return (cimModuleFile.FileCode == RemoteDiscoveryHelper.CimFileCode.CmdletizationV1);
        }

        private bool IsFormatPs1XmlFile(RemoteDiscoveryHelper.CimModuleFile cimModuleFile, Hashtable manifestData)
        {
            return this.IsPs1xmlFileHelper(cimModuleFile, manifestData, "FormatsToProcess", "TypesToProcess");
        }

        private bool IsMixedModePsCimModule(RemoteDiscoveryHelper.CimModule cimModule)
        {
            string temporaryModuleManifestPath = RemoteDiscoveryHelper.GetModulePath(cimModule.ModuleName, null, string.Empty, base.Context.CurrentRunspace);
            bool containedErrors = false;
            RemoteDiscoveryHelper.CimModuleFile mainManifest = cimModule.MainManifest;
            if (mainManifest == null)
            {
                return true;
            }
            Hashtable manifestData = RemoteDiscoveryHelper.ConvertCimModuleFileToManifestHashtable(mainManifest, temporaryModuleManifestPath, this, ref containedErrors);
            if (containedErrors || (manifestData == null))
            {
                return false;
            }
            if (IsNonEmptyManifestField(manifestData, "ScriptsToProcess") || IsNonEmptyManifestField(manifestData, "RequiredAssemblies"))
            {
                return true;
            }
            int num = 0;
            string[] result = null;
            if (LanguagePrimitives.TryConvertTo<string[]>(manifestData["NestedModules"], CultureInfo.InvariantCulture, out result) && (result != null))
            {
                num += result.Length;
            }
            if (manifestData.ContainsKey("RootModule") || manifestData.ContainsKey("ModuleToProcess"))
            {
                if (manifestData.ContainsKey("RootModule"))
                {
                    string str2;
                    if (LanguagePrimitives.TryConvertTo<string>(manifestData["RootModule"], CultureInfo.InvariantCulture, out str2) && !string.IsNullOrEmpty(str2))
                    {
                        num++;
                    }
                }
                else
                {
                    string str3;
                    if ((manifestData.ContainsKey("ModuleToProcess") && LanguagePrimitives.TryConvertTo<string>(manifestData["ModuleToProcess"], CultureInfo.InvariantCulture, out str3)) && !string.IsNullOrEmpty(str3))
                    {
                        num++;
                    }
                }
            }
            int num2 = (from moduleFile in cimModule.ModuleFiles
                where moduleFile.FileCode == RemoteDiscoveryHelper.CimFileCode.CmdletizationV1
                select moduleFile).Count<RemoteDiscoveryHelper.CimModuleFile>();
            return (num > num2);
        }

        private static bool IsNonEmptyManifestField(Hashtable manifestData, string key)
        {
            object[] objArray;
            if (!manifestData.ContainsKey(key))
            {
                return false;
            }
            object valueToConvert = manifestData[key];
            if (valueToConvert == null)
            {
                return false;
            }
            if (LanguagePrimitives.TryConvertTo<object[]>(valueToConvert, CultureInfo.InvariantCulture, out objArray))
            {
                return (objArray.Length != 0);
            }
            return true;
        }

        private bool IsPs1xmlFileHelper(RemoteDiscoveryHelper.CimModuleFile cimModuleFile, Hashtable manifestData, string goodKey, string badKey)
        {
            List<string> list;
            List<string> list2;
            if (!Path.GetExtension(cimModuleFile.FileName).Equals(".ps1xml", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
            if (!base.GetListOfStringsFromData(manifestData, null, goodKey, 0, out list))
            {
                list = new List<string>();
            }
            if (list == null)
            {
                list = new List<string>();
            }
            if (!base.GetListOfStringsFromData(manifestData, null, badKey, 0, out list2))
            {
                list2 = new List<string>();
            }
            if (list2 == null)
            {
                list2 = new List<string>();
            }
            bool flag = this.IsPs1xmlFileHelper_IsPresentInEntries(cimModuleFile, list);
            bool flag2 = this.IsPs1xmlFileHelper_IsPresentInEntries(cimModuleFile, list2);
            return (flag && !flag2);
        }

        private bool IsPs1xmlFileHelper_IsPresentInEntries(RemoteDiscoveryHelper.CimModuleFile cimModuleFile, IEnumerable<string> manifestEntries)
        {
            return (manifestEntries.Any<string>(s => s.EndsWith(cimModuleFile.FileName, StringComparison.OrdinalIgnoreCase)) || manifestEntries.Any<string>(s => this.FixupFileName("", s, ".ps1xml").EndsWith(cimModuleFile.FileName, StringComparison.OrdinalIgnoreCase)));
        }

        private bool IsTypesPs1XmlFile(RemoteDiscoveryHelper.CimModuleFile cimModuleFile, Hashtable manifestData)
        {
            return this.IsPs1xmlFileHelper(cimModuleFile, manifestData, "TypesToProcess", "FormatsToProcess");
        }

        protected override void ProcessRecord()
        {
            ModuleCmdletBase.ImportModuleOptions importModuleOptions = new ModuleCmdletBase.ImportModuleOptions {
                NoClobber = (bool) this.NoClobber
            };
            if (!string.IsNullOrEmpty(this.Scope) && this.Scope.Equals("LOCAL", StringComparison.OrdinalIgnoreCase))
            {
                importModuleOptions.Local = true;
            }
            if (base.ParameterSetName.Equals("ModuleInfo", StringComparison.OrdinalIgnoreCase))
            {
                PSModuleInfo[] infoArray = this._moduleInfo;
                for (int i = 0; i < infoArray.Length; i++)
                {
                    Action localAction = null;
                    Action<Microsoft.Management.Infrastructure.CimSession, Uri, string> cimSessionAction = null;
                    Action<System.Management.Automation.Runspaces.PSSession> psSessionAction = null;
                    PSModuleInfo module = infoArray[i];
                    if (localAction == null)
                    {
                        localAction = delegate {
                            this.ImportModule_ViaLocalModuleInfo(importModuleOptions, module);
                            this.SetModuleBaseForEngineModules(module.Name, this.Context);
                        };
                    }
                    if (cimSessionAction == null)
                    {
                        cimSessionAction = (cimSession, resourceUri, cimNamespace) => this.ImportModule_RemotelyViaCimSession(importModuleOptions, new string[] { module.Name }, cimSession, resourceUri, cimNamespace);
                    }
                    if (psSessionAction == null)
                    {
                        psSessionAction = psSession => this.ImportModule_RemotelyViaPsrpSession(importModuleOptions, new string[] { module.Path }, psSession);
                    }
                    RemoteDiscoveryHelper.DispatchModuleInfoProcessing(module, localAction, cimSessionAction, psSessionAction);
                }
            }
            else if (base.ParameterSetName.Equals("Assembly", StringComparison.OrdinalIgnoreCase))
            {
                if (this.Assembly != null)
                {
                    foreach (System.Reflection.Assembly assembly in this.Assembly)
                    {
                        this.ImportModule_ViaAssembly(importModuleOptions, assembly);
                    }
                }
            }
            else if (base.ParameterSetName.Equals("Name", StringComparison.OrdinalIgnoreCase))
            {
                foreach (string str in this.Name)
                {
                    PSModuleInfo info = this.ImportModule_LocallyViaName(importModuleOptions, str);
                    if (info != null)
                    {
                        this.SetModuleBaseForEngineModules(info.Name, base.Context);
                    }
                }
            }
            else if (base.ParameterSetName.Equals("PSSession", StringComparison.OrdinalIgnoreCase))
            {
                this.ImportModule_RemotelyViaPsrpSession(importModuleOptions, this.Name, this.PSSession);
            }
            else if (base.ParameterSetName.Equals("CimSession", StringComparison.OrdinalIgnoreCase))
            {
                this.ImportModule_RemotelyViaCimSession(importModuleOptions, this.Name, this.CimSession, this.CimResourceUri, this.CimNamespace);
            }
        }

        private void SetModuleBaseForEngineModules(string moduleName, System.Management.Automation.ExecutionContext context)
        {
            if (InitialSessionState.IsEngineModule(moduleName))
            {
                foreach (PSModuleInfo info in context.EngineSessionState.ModuleTable.Values)
                {
                    if (info.Name.Equals(moduleName, StringComparison.OrdinalIgnoreCase))
                    {
                        info.SetModuleBase(Utils.GetApplicationBase(Utils.DefaultPowerShellShellID));
                        foreach (PSModuleInfo info2 in info.NestedModules)
                        {
                            info2.SetModuleBase(Utils.GetApplicationBase(Utils.DefaultPowerShellShellID));
                        }
                    }
                }
                foreach (PSModuleInfo info3 in context.Modules.ModuleTable.Values)
                {
                    if (info3.Name.Equals(moduleName, StringComparison.OrdinalIgnoreCase))
                    {
                        info3.SetModuleBase(Utils.GetApplicationBase(Utils.DefaultPowerShellShellID));
                        foreach (PSModuleInfo info4 in info3.NestedModules)
                        {
                            info4.SetModuleBase(Utils.GetApplicationBase(Utils.DefaultPowerShellShellID));
                        }
                    }
                }
            }
        }

        protected override void StopProcessing()
        {
            this._cancellationTokenSource.Cancel();
        }

        [ValidateNotNull, Parameter]
        public string[] Alias
        {
            get
            {
                return this._aliasExportList;
            }
            set
            {
                if (value != null)
                {
                    this._aliasExportList = value;
                    base.BaseAliasPatterns = new List<WildcardPattern>();
                    foreach (string str in this._aliasExportList)
                    {
                        base.BaseAliasPatterns.Add(new WildcardPattern(str, WildcardOptions.IgnoreCase));
                    }
                }
            }
        }

        [Alias(new string[] { "Args" }), Parameter]
        public object[] ArgumentList
        {
            get
            {
                return base.BaseArgumentList;
            }
            set
            {
                base.BaseArgumentList = value;
            }
        }

        [Parameter]
        public SwitchParameter AsCustomObject
        {
            get
            {
                return base.BaseAsCustomObject;
            }
            set
            {
                base.BaseAsCustomObject = (bool) value;
            }
        }

        [Parameter(ParameterSetName="Assembly", Mandatory=true, ValueFromPipeline=true, Position=0)]
        public System.Reflection.Assembly[] Assembly { get; set; }

        private System.Threading.CancellationToken CancellationToken
        {
            get
            {
                return this._cancellationTokenSource.Token;
            }
        }

        [Parameter(ParameterSetName="CimSession", Mandatory=false), ValidateNotNullOrEmpty]
        public string CimNamespace { get; set; }

        [Parameter(ParameterSetName="CimSession", Mandatory=false), ValidateNotNull]
        public Uri CimResourceUri { get; set; }

        [Parameter(ParameterSetName="CimSession", Mandatory=true), ValidateNotNull]
        public Microsoft.Management.Infrastructure.CimSession CimSession { get; set; }

        [ValidateNotNull, Parameter]
        public string[] Cmdlet
        {
            get
            {
                return this._cmdletImportList;
            }
            set
            {
                if (value != null)
                {
                    this._cmdletImportList = value;
                    base.BaseCmdletPatterns = new List<WildcardPattern>();
                    foreach (string str in this._cmdletImportList)
                    {
                        base.BaseCmdletPatterns.Add(new WildcardPattern(str, WildcardOptions.IgnoreCase));
                    }
                }
            }
        }

        [Parameter]
        public SwitchParameter DisableNameChecking
        {
            get
            {
                return base.BaseDisableNameChecking;
            }
            set
            {
                base.BaseDisableNameChecking = (bool) value;
            }
        }

        [Parameter]
        public SwitchParameter Force
        {
            get
            {
                return base.BaseForce;
            }
            set
            {
                base.BaseForce = (bool) value;
            }
        }

        [ValidateNotNull, Parameter]
        public string[] Function
        {
            get
            {
                return this._functionImportList;
            }
            set
            {
                if (value != null)
                {
                    this._functionImportList = value;
                    base.BaseFunctionPatterns = new List<WildcardPattern>();
                    foreach (string str in this._functionImportList)
                    {
                        base.BaseFunctionPatterns.Add(new WildcardPattern(str, WildcardOptions.IgnoreCase));
                    }
                }
            }
        }

        [Parameter]
        public SwitchParameter Global
        {
            get
            {
                return base.BaseGlobal;
            }
            set
            {
                base.BaseGlobal = (bool) value;
            }
        }

        [Parameter(ParameterSetName="Name"), Parameter(ParameterSetName="CimSession"), Alias(new string[] { "Version" }), Parameter(ParameterSetName="PSSession")]
        public Version MinimumVersion
        {
            get
            {
                return base.BaseMinimumVersion;
            }
            set
            {
                base.BaseMinimumVersion = value;
            }
        }

        [Parameter(ParameterSetName="ModuleInfo", Mandatory=true, ValueFromPipeline=true, Position=0)]
        public PSModuleInfo[] ModuleInfo
        {
            get
            {
                return this._moduleInfo;
            }
            set
            {
                this._moduleInfo = value;
            }
        }

        [Parameter(ParameterSetName="CimSession", Mandatory=true, ValueFromPipeline=true, Position=0), Parameter(ParameterSetName="Name", Mandatory=true, ValueFromPipeline=true, Position=0), Parameter(ParameterSetName="PSSession", Mandatory=true, ValueFromPipeline=true, Position=0)]
        public string[] Name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
            }
        }

        [Parameter, Alias(new string[] { "NoOverwrite" })]
        public SwitchParameter NoClobber { get; set; }

        [Parameter]
        public SwitchParameter PassThru
        {
            get
            {
                return base.BasePassThru;
            }
            set
            {
                base.BasePassThru = (bool) value;
            }
        }

        [Parameter, ValidateNotNull]
        public string Prefix
        {
            get
            {
                return base.BasePrefix;
            }
            set
            {
                base.BasePrefix = value;
            }
        }

        [Parameter(ParameterSetName="PSSession", Mandatory=true), ValidateNotNull]
        public System.Management.Automation.Runspaces.PSSession PSSession { get; set; }

        [Parameter(ParameterSetName="PSSession"), Parameter(ParameterSetName="Name"), Parameter(ParameterSetName="CimSession")]
        public Version RequiredVersion
        {
            get
            {
                return base.BaseRequiredVersion;
            }
            set
            {
                base.BaseRequiredVersion = value;
            }
        }

        [Parameter, ValidateSet(new string[] { "Local", "Global" })]
        public string Scope
        {
            get
            {
                return this._scope;
            }
            set
            {
                this._scope = value;
                this._isScopeSpecified = true;
            }
        }

        [ValidateNotNull, Parameter]
        public string[] Variable
        {
            get
            {
                return this._variableExportList;
            }
            set
            {
                if (value != null)
                {
                    this._variableExportList = value;
                    base.BaseVariablePatterns = new List<WildcardPattern>();
                    foreach (string str in this._variableExportList)
                    {
                        base.BaseVariablePatterns.Add(new WildcardPattern(str, WildcardOptions.IgnoreCase));
                    }
                }
            }
        }
    }
}

