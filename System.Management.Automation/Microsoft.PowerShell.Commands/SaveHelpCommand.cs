namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Help;
    using System.Management.Automation.Internal;

    [Cmdlet("Save", "Help", DefaultParameterSetName="Path", HelpUri="http://go.microsoft.com/fwlink/?LinkID=210612")]
    public sealed class SaveHelpCommand : UpdatableHelpCommandBase
    {
        internal string[] _module;
        internal string[] _path;
        private bool alreadyCheckedOncePerDayPerModule;
        private bool isLiteralPath;
        private bool specifiedPath;

        public SaveHelpCommand() : base(UpdatableHelpCommandType.SaveHelpCommand)
        {
        }

        protected override void BeginProcessing()
        {
            base.BeginProcessing();
            if (!this.specifiedPath && !this.isLiteralPath)
            {
                PSArgumentException exception = new PSArgumentException(StringUtil.Format(HelpDisplayStrings.CannotSpecifyDestinationPathAndLiteralPath, new object[0]));
                base.ThrowTerminatingError(exception.ErrorRecord);
            }
            else if ((this.specifiedPath || this.isLiteralPath) && !(this.specifiedPath ^ this.isLiteralPath))
            {
                PSArgumentException exception2 = new PSArgumentException(StringUtil.Format(HelpDisplayStrings.CannotSpecifyDestinationPathAndLiteralPath, new object[0]));
                base.ThrowTerminatingError(exception2.ErrorRecord);
            }
        }

        internal override bool ProcessModuleWithCulture(UpdatableHelpModuleInfo module, string culture)
        {
            Collection<string> collection = new Collection<string>();
            foreach (string str in this._path)
            {
                UpdatableHelpSystemDrive drive = null;
                using (drive)
                {
                    if (string.IsNullOrEmpty(str))
                    {
                        PSArgumentException exception = new PSArgumentException(StringUtil.Format(HelpDisplayStrings.PathNullOrEmpty, new object[0]));
                        base.WriteError(exception.ErrorRecord);
                        return false;
                    }
                    string path = str;
                    if (base._credential != null)
                    {
                        if (str.Contains("*"))
                        {
                            int index = str.IndexOf("*", StringComparison.OrdinalIgnoreCase);
                            if (index == 0)
                            {
                                throw new UpdatableHelpSystemException("PathMustBeValidContainers", StringUtil.Format(HelpDisplayStrings.PathMustBeValidContainers, str), ErrorCategory.InvalidArgument, null, new ItemNotFoundException());
                            }
                            int length = index;
                            while (length >= 0)
                            {
                                char ch = str[length];
                                if (ch.Equals('/'))
                                {
                                    break;
                                }
                                char ch2 = str[length];
                                if (ch2.Equals('\\'))
                                {
                                    break;
                                }
                                length--;
                            }
                            if (length == 0)
                            {
                                throw new UpdatableHelpSystemException("PathMustBeValidContainers", StringUtil.Format(HelpDisplayStrings.PathMustBeValidContainers, str), ErrorCategory.InvalidArgument, null, new ItemNotFoundException());
                            }
                            drive = new UpdatableHelpSystemDrive(this, str.Substring(0, length), base._credential);
                            path = Path.Combine(drive.DriveName, str.Substring(length + 1, str.Length - (length + 1)));
                        }
                        else
                        {
                            drive = new UpdatableHelpSystemDrive(this, str, base._credential);
                            path = drive.DriveName;
                        }
                    }
                    if (this.isLiteralPath)
                    {
                        string unresolvedProviderPathFromPSPath = base.GetUnresolvedProviderPathFromPSPath(path);
                        if (!Directory.Exists(unresolvedProviderPathFromPSPath))
                        {
                            throw new UpdatableHelpSystemException("PathMustBeValidContainers", StringUtil.Format(HelpDisplayStrings.PathMustBeValidContainers, str), ErrorCategory.InvalidArgument, null, new ItemNotFoundException());
                        }
                        collection.Add(unresolvedProviderPathFromPSPath);
                    }
                    else
                    {
                        try
                        {
                            foreach (string str4 in base.ResolvePath(path, false, false))
                            {
                                collection.Add(str4);
                            }
                        }
                        catch (ItemNotFoundException exception2)
                        {
                            throw new UpdatableHelpSystemException("PathMustBeValidContainers", StringUtil.Format(HelpDisplayStrings.PathMustBeValidContainers, str), ErrorCategory.InvalidArgument, null, exception2);
                        }
                    }
                }
            }
            if (collection.Count == 0)
            {
                return true;
            }
            bool flag = false;
            foreach (string str5 in collection)
            {
                UpdatableHelpInfo currentHelpInfo = null;
                UpdatableHelpInfo newHelpInfo = null;
                string xml = UpdatableHelpSystem.LoadStringFromPath(this, base.SessionState.Path.Combine(str5, module.GetHelpInfoName()), base._credential);
                if (xml != null)
                {
                    currentHelpInfo = base._helpSystem.CreateHelpInfo(xml, module.ModuleName, module.ModuleGuid, null, null, false);
                }
                if (!this.alreadyCheckedOncePerDayPerModule && !base.CheckOncePerDayPerModule(module.ModuleName, str5, module.GetHelpInfoName(), DateTime.UtcNow, base._force))
                {
                    return true;
                }
                this.alreadyCheckedOncePerDayPerModule = true;
                string str8 = base._helpSystem.GetHelpInfoUri(module, null).ResolvedUri + module.GetHelpInfoName();
                newHelpInfo = base._helpSystem.GetHelpInfo(base._commandType, str8, module.ModuleName, module.ModuleGuid, culture);
                if (newHelpInfo == null)
                {
                    throw new UpdatableHelpSystemException("UnableToRetrieveHelpInfoXml", StringUtil.Format(HelpDisplayStrings.UnableToRetrieveHelpInfoXml, culture), ErrorCategory.ResourceUnavailable, null, null);
                }
                foreach (UpdatableHelpUri uri in newHelpInfo.HelpContentUriCollection)
                {
                    if (!base.IsUpdateNecessary(module, currentHelpInfo, newHelpInfo, uri.Culture, base._force))
                    {
                        base.WriteVerbose(StringUtil.Format(HelpDisplayStrings.SuccessfullyUpdatedHelpContent, new object[] { module.ModuleName, HelpDisplayStrings.NewestContentAlreadyDownloaded, uri.Culture.Name, newHelpInfo.GetCultureVersion(uri.Culture) }));
                        flag = true;
                    }
                    else
                    {
                        string resolvedUri = uri.ResolvedUri;
                        string helpContentName = module.GetHelpContentName(uri.Culture);
                        string str11 = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetTempFileName()));
                        UpdatableHelpSystemDrive drive2 = null;
                        using (drive2)
                        {
                            try
                            {
                                if (Directory.Exists(resolvedUri))
                                {
                                    File.Copy(base.SessionState.Path.Combine(resolvedUri, helpContentName), base.SessionState.Path.Combine(str5, helpContentName), true);
                                }
                                else
                                {
                                    if (base._credential != null)
                                    {
                                        try
                                        {
                                            drive2 = new UpdatableHelpSystemDrive(this, str5, base._credential);
                                            if (!base._helpSystem.DownloadHelpContent(base._commandType, str11, resolvedUri, helpContentName, culture))
                                            {
                                                flag = false;
                                                goto Label_0641;
                                            }
                                            base.InvokeProvider.Item.Copy(new string[] { str11 }, drive2.DriveName, true, CopyContainers.CopyChildrenOfTargetContainer, true, true);
                                            goto Label_04C2;
                                        }
                                        catch (Exception exception3)
                                        {
                                            CommandProcessorBase.CheckForSevereException(exception3);
                                            base.ProcessException(module.ModuleName, uri.Culture.Name, exception3);
                                            flag = false;
                                            goto Label_0641;
                                        }
                                    }
                                    if (!base._helpSystem.DownloadHelpContent(base._commandType, str5, resolvedUri, helpContentName, culture))
                                    {
                                        flag = false;
                                        goto Label_0641;
                                    }
                                }
                            Label_04C2:
                                if (base._credential != null)
                                {
                                    base._helpSystem.GenerateHelpInfo(module.ModuleName, module.ModuleGuid, newHelpInfo.UnresolvedUri, uri.Culture.Name, newHelpInfo.GetCultureVersion(uri.Culture), str11, module.GetHelpInfoName(), base._force);
                                    base.InvokeProvider.Item.Copy(new string[] { Path.Combine(str11, module.GetHelpInfoName()) }, Path.Combine(drive2.DriveName, module.GetHelpInfoName()), false, CopyContainers.CopyTargetContainer, true, true);
                                }
                                else
                                {
                                    base._helpSystem.GenerateHelpInfo(module.ModuleName, module.ModuleGuid, newHelpInfo.UnresolvedUri, uri.Culture.Name, newHelpInfo.GetCultureVersion(uri.Culture), str5, module.GetHelpInfoName(), base._force);
                                }
                                base.WriteVerbose(StringUtil.Format(HelpDisplayStrings.SuccessfullyUpdatedHelpContent, new object[] { module.ModuleName, StringUtil.Format(HelpDisplayStrings.SavedHelpContent, Path.Combine(str5, helpContentName)), uri.Culture.Name, newHelpInfo.GetCultureVersion(uri.Culture) }));
                                base.LogMessage(StringUtil.Format(HelpDisplayStrings.SaveHelpCompleted, str5));
                            }
                            catch (Exception exception4)
                            {
                                CommandProcessorBase.CheckForSevereException(exception4);
                                base.ProcessException(module.ModuleName, uri.Culture.Name, exception4);
                            }
                        }
                    Label_0641:;
                    }
                }
                flag = true;
            }
            return flag;
        }

        protected override void ProcessRecord()
        {
            try
            {
                base.Process(this._module);
            }
            finally
            {
                ProgressRecord progressRecord = new ProgressRecord(base.activityId, HelpDisplayStrings.SaveProgressActivityForModule, HelpDisplayStrings.UpdateProgressInstalling) {
                    PercentComplete = 100,
                    RecordType = ProgressRecordType.Completed
                };
                base.WriteProgress(progressRecord);
            }
        }

        [ValidateNotNull, Parameter(Mandatory=true, Position=0, ParameterSetName="Path")]
        public string[] DestinationPath
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

        [Alias(new string[] { "PSPath" }), Parameter(Mandatory=true, ParameterSetName="LiteralPath"), ValidateNotNull]
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

        [Alias(new string[] { "Name" }), ValidateNotNull, Parameter(Position=1, ParameterSetName="LiteralPath", ValueFromPipelineByPropertyName=true), Parameter(Position=1, ParameterSetName="Path", ValueFromPipelineByPropertyName=true)]
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
    }
}

