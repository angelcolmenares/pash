namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Help;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Runspaces;

    [Cmdlet("Update", "Help", DefaultParameterSetName="Path", HelpUri="http://go.microsoft.com/fwlink/?LinkID=210614")]
    public sealed class UpdateHelpCommand : UpdatableHelpCommandBase
    {
        internal string[] _module;
        internal string[] _path;
        private bool _recurse;
        private bool alreadyCheckedOncePerDayPerModule;
        private bool isInitialized;
        private bool isLiteralPath;
        private bool specifiedPath;

        public UpdateHelpCommand() : base(UpdatableHelpCommandType.UpdateHelpCommand)
        {
        }

        protected override void BeginProcessing()
        {
            UpdatableHelpSystem.SetDisablePromptToUpdateHelp();
            if (this._path == null)
            {
                string defaultSourcePath = base._helpSystem.GetDefaultSourcePath();
                if (defaultSourcePath != null)
                {
                    this._path = new string[] { defaultSourcePath };
                }
            }
        }

        internal override bool ProcessModuleWithCulture(UpdatableHelpModuleInfo module, string culture)
        {
            if (((InitialSessionState.IsEngineModule(module.ModuleName) || InitialSessionState.IsNestedEngineModule(module.ModuleName)) || module.ModuleName.Equals(InitialSessionState.CoreSnapin, StringComparison.OrdinalIgnoreCase)) && !UpdatableHelpSystem.IsAdministrator())
            {
                string message = StringUtil.Format(HelpErrors.UpdatableHelpRequiresElevation, new object[0]);
                base.ProcessException(module.ModuleName, null, new UpdatableHelpSystemException("UpdatableHelpSystemRequiresElevation", message, ErrorCategory.InvalidOperation, null, null));
                return false;
            }
            UpdatableHelpInfo currentHelpInfo = null;
            UpdatableHelpInfo newHelpInfo = null;
            string str2 = null;
            string xml = UpdatableHelpSystem.LoadStringFromPath(this, base.SessionState.Path.Combine(module.ModuleBase, module.GetHelpInfoName()), null);
            if (xml != null)
            {
                currentHelpInfo = base._helpSystem.CreateHelpInfo(xml, module.ModuleName, module.ModuleGuid, null, null, false);
            }
            if (!this.alreadyCheckedOncePerDayPerModule && !base.CheckOncePerDayPerModule(module.ModuleName, module.ModuleBase, module.GetHelpInfoName(), DateTime.UtcNow, base._force))
            {
                return true;
            }
            this.alreadyCheckedOncePerDayPerModule = true;
            if (this._path != null)
            {
                //using (null)
                {
                    try
                    {
                        Collection<string> collection = new Collection<string>();
                        foreach (string str4 in this._path)
                        {
                            if (string.IsNullOrEmpty(str4))
                            {
                                PSArgumentException exception = new PSArgumentException(StringUtil.Format(HelpDisplayStrings.PathNullOrEmpty, new object[0]));
                                base.WriteError(exception.ErrorRecord);
                                return false;
                            }
                            try
                            {
                                string path = str4;
                                if (base._credential != null)
                                {
                                    UpdatableHelpSystemDrive drive2 = new UpdatableHelpSystemDrive(this, str4, base._credential);
                                    path = drive2.DriveName;
                                }
                                foreach (string str6 in base.ResolvePath(path, this._recurse, this.isLiteralPath))
                                {
                                    collection.Add(str6);
                                }
                            }
                            catch (System.Management.Automation.DriveNotFoundException exception2)
                            {
                                this.ThrowPathMustBeValidContainersException(str4, exception2);
                            }
                            catch (ItemNotFoundException exception3)
                            {
                                this.ThrowPathMustBeValidContainersException(str4, exception3);
                            }
                        }
                        if (collection.Count == 0)
                        {
                            return true;
                        }
                        foreach (string str7 in collection)
                        {
                            string str8 = base.SessionState.Path.Combine(str7, module.GetHelpInfoName());
                            xml = UpdatableHelpSystem.LoadStringFromPath(this, str8, base._credential);
                            if (xml != null)
                            {
                                newHelpInfo = base._helpSystem.CreateHelpInfo(xml, module.ModuleName, module.ModuleGuid, culture, str7, false);
                                str2 = str7;
                                goto Label_02DD;
                            }
                        }
                    }
                    catch (Exception exception4)
                    {
                        CommandProcessorBase.CheckForSevereException(exception4);
                        throw new UpdatableHelpSystemException("UnableToRetrieveHelpInfoXml", StringUtil.Format(HelpDisplayStrings.UnableToRetrieveHelpInfoXml, culture), ErrorCategory.ResourceUnavailable, null, exception4);
                    }
                    goto Label_02DD;
                }
            }
            string str9 = base._helpSystem.GetHelpInfoUri(module, null).ResolvedUri + module.GetHelpInfoName();
            newHelpInfo = base._helpSystem.GetHelpInfo(UpdatableHelpCommandType.UpdateHelpCommand, str9, module.ModuleName, module.ModuleGuid, culture);
        Label_02DD:
            if (newHelpInfo == null)
            {
                throw new UpdatableHelpSystemException("UnableToRetrieveHelpInfoXml", StringUtil.Format(HelpDisplayStrings.UnableToRetrieveHelpInfoXml, culture), ErrorCategory.ResourceUnavailable, null, null);
            }
            bool flag = false;
            foreach (UpdatableHelpUri uri in newHelpInfo.HelpContentUriCollection)
            {
                if (!base.IsUpdateNecessary(module, currentHelpInfo, newHelpInfo, uri.Culture, base._force))
                {
                    base.WriteVerbose(StringUtil.Format(HelpDisplayStrings.SuccessfullyUpdatedHelpContent, new object[] { module.ModuleName, HelpDisplayStrings.NewestContentAlreadyInstalled, uri.Culture.Name, newHelpInfo.GetCultureVersion(uri.Culture) }));
                    flag = true;
                }
                else
                {
                    try
                    {
                        Collection<string> collection3;
                        string resolvedUri = uri.ResolvedUri;
                        string xsdPath = base.SessionState.Path.Combine(Utils.GetApplicationBase(base.Context.ShellID), @"Schemas\PSMaml\maml.xsd");
                        Collection<string> destPaths = new Collection<string> {
                            module.ModuleBase
                        };
                        if (UpdatableHelpCommandBase.IsSystemModule(module.ModuleName) && Environment.Is64BitOperatingSystem)
                        {
                            string item = Utils.GetApplicationBase(Utils.DefaultPowerShellShellID).Replace("System32", "SysWOW64");
                            destPaths.Add(item);
                        }
                        if (Directory.Exists(resolvedUri))
                        {
                            if (base._credential != null)
                            {
                                string helpContentName = module.GetHelpContentName(uri.Culture);
                                string str14 = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));
                                try
                                {
                                    using (UpdatableHelpSystemDrive drive3 = new UpdatableHelpSystemDrive(this, resolvedUri, base._credential))
                                    {
                                        if (!Directory.Exists(str14))
                                        {
                                            Directory.CreateDirectory(str14);
                                        }
                                        base.InvokeProvider.Item.Copy(new string[] { Path.Combine(drive3.DriveName, helpContentName) }, Path.Combine(str14, helpContentName), false, CopyContainers.CopyTargetContainer, true, true);
                                        base._helpSystem.InstallHelpContent(UpdatableHelpCommandType.UpdateHelpCommand, base.Context, str14, destPaths, module.GetHelpContentName(uri.Culture), Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName())), uri.Culture, xsdPath, out collection3);
                                    }
                                    goto Label_0593;
                                }
                                catch (Exception exception5)
                                {
                                    CommandProcessorBase.CheckForSevereException(exception5);
                                    throw new UpdatableHelpSystemException("HelpContentNotFound", StringUtil.Format(HelpDisplayStrings.HelpContentNotFound, new object[0]), ErrorCategory.ResourceUnavailable, null, exception5);
                                }
                            }
                            base._helpSystem.InstallHelpContent(UpdatableHelpCommandType.UpdateHelpCommand, base.Context, resolvedUri, destPaths, module.GetHelpContentName(uri.Culture), Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName())), uri.Culture, xsdPath, out collection3);
                        }
                        else if (!base._helpSystem.DownloadAndInstallHelpContent(UpdatableHelpCommandType.UpdateHelpCommand, base.Context, destPaths, module.GetHelpContentName(uri.Culture), uri.Culture, resolvedUri, xsdPath, out collection3))
                        {
                            flag = false;
                            goto Label_069B;
                        }
                    Label_0593:
                        base._helpSystem.GenerateHelpInfo(module.ModuleName, module.ModuleGuid, newHelpInfo.UnresolvedUri, uri.Culture.Name, newHelpInfo.GetCultureVersion(uri.Culture), module.ModuleBase, module.GetHelpInfoName(), base._force);
                        foreach (string str15 in collection3)
                        {
                            base.WriteVerbose(StringUtil.Format(HelpDisplayStrings.SuccessfullyUpdatedHelpContent, new object[] { module.ModuleName, StringUtil.Format(HelpDisplayStrings.UpdatedHelpContent, str15), uri.Culture.Name, newHelpInfo.GetCultureVersion(uri.Culture) }));
                        }
                        base.LogMessage(StringUtil.Format(HelpDisplayStrings.UpdateHelpCompleted, new object[0]));
                        flag = true;
                    }
                    catch (Exception exception6)
                    {
                        CommandProcessorBase.CheckForSevereException(exception6);
                        base.ProcessException(module.ModuleName, uri.Culture.Name, exception6);
                    }
                Label_069B:;
                }
            }
            return flag;
        }

        protected override void ProcessRecord()
        {
            try
            {
                if (!this.isInitialized)
                {
                    if ((this.specifiedPath || this.isLiteralPath) && !(this.specifiedPath ^ this.isLiteralPath))
                    {
                        PSArgumentException exception = new PSArgumentException(StringUtil.Format(HelpDisplayStrings.CannotSpecifySourcePathAndLiteralPath, new object[0]));
                        base.ThrowTerminatingError(exception.ErrorRecord);
                    }
                    if ((this._path == null) && this.Recurse.IsPresent)
                    {
                        PSArgumentException exception2 = new PSArgumentException(StringUtil.Format(HelpDisplayStrings.CannotSpecifyRecurseWithoutPath, new object[0]));
                        base.ThrowTerminatingError(exception2.ErrorRecord);
                    }
                    this.isInitialized = true;
                }
                base.Process(this._module);
                foreach (HelpProvider provider in base.Context.HelpSystem.HelpProviders)
                {
                    if (base._stopping)
                    {
                        return;
                    }
                    provider.Reset();
                }
            }
            finally
            {
                ProgressRecord progressRecord = new ProgressRecord(base.activityId, HelpDisplayStrings.UpdateProgressActivityForModule, HelpDisplayStrings.UpdateProgressInstalling) {
                    PercentComplete = 100,
                    RecordType = ProgressRecordType.Completed
                };
                base.WriteProgress(progressRecord);
            }
        }

        private void ThrowPathMustBeValidContainersException(string path, Exception e)
        {
            throw new UpdatableHelpSystemException("PathMustBeValidContainers", StringUtil.Format(HelpDisplayStrings.PathMustBeValidContainers, path), ErrorCategory.InvalidArgument, null, e);
        }

        [Alias(new string[] { "PSPath" }), Parameter(ParameterSetName="LiteralPath", ValueFromPipelineByPropertyName=true), ValidateNotNull]
        public string[] LiteralPath
        {
            get
            {
                return this._path;
            }
            set
            {
                this._path = value;
                this.isLiteralPath = true;
            }
        }

        [ValidateNotNull, Parameter(Position=0, ParameterSetName="LiteralPath", ValueFromPipelineByPropertyName=true), Parameter(Position=0, ParameterSetName="Path", ValueFromPipelineByPropertyName=true), Alias(new string[] { "Name" })]
        public string[] Module
        {
            get
            {
                return this._module;
            }
            set
            {
                this._module = value;
            }
        }

        [Parameter]
        public SwitchParameter Recurse
        {
            get
            {
                return this._recurse;
            }
            set
            {
                this._recurse = (bool) value;
            }
        }

        [ValidateNotNull, Parameter(Position=1, ParameterSetName="Path")]
        public string[] SourcePath
        {
            get
            {
                return this._path;
            }
            set
            {
                this._path = value;
                this.specifiedPath = true;
            }
        }
    }
}

