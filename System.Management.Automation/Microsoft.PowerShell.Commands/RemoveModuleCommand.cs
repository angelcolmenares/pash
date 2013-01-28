namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Runspaces;

    [Cmdlet("Remove", "Module", SupportsShouldProcess=true, HelpUri="http://go.microsoft.com/fwlink/?LinkID=141556")]
    public sealed class RemoveModuleCommand : ModuleCmdletBase
    {
        private PSModuleInfo[] _moduleInfo = new PSModuleInfo[0];
        private string[] _name = new string[0];
        private int _numberRemoved;

        protected override void EndProcessing()
        {
            if ((this._numberRemoved == 0) && !base.MyInvocation.BoundParameters.ContainsKey("WhatIf"))
            {
                bool flag = true;
                bool flag2 = true;
                foreach (string str in this._name)
                {
                    if (!InitialSessionState.IsEngineModule(str))
                    {
                        flag2 = false;
                    }
                    if (!WildcardPattern.ContainsWildcardCharacters(str))
                    {
                        flag = false;
                    }
                }
                if (!flag2 && (!flag || (this._moduleInfo.Length != 0)))
                {
                    InvalidOperationException exception = new InvalidOperationException(StringUtil.Format(Modules.NoModulesRemoved, new object[0]));
                    ErrorRecord errorRecord = new ErrorRecord(exception, "Modules_NoModulesRemoved", ErrorCategory.ResourceUnavailable, null);
                    base.WriteError(errorRecord);
                }
            }
        }

        private void GetAllNestedModules(PSModuleInfo module, ref List<PSModuleInfo> nestedModulesWithNoCircularReference)
        {
            List<PSModuleInfo> list = new List<PSModuleInfo>();
            if ((module.NestedModules != null) && (module.NestedModules.Count > 0))
            {
                foreach (PSModuleInfo info in module.NestedModules)
                {
                    if (!nestedModulesWithNoCircularReference.Contains(info))
                    {
                        nestedModulesWithNoCircularReference.Add(info);
                        list.Add(info);
                    }
                }
                foreach (PSModuleInfo info2 in list)
                {
                    this.GetAllNestedModules(info2, ref nestedModulesWithNoCircularReference);
                }
            }
        }

        private Dictionary<PSModuleInfo, List<PSModuleInfo>> GetRequiredDependencies()
        {
            Dictionary<PSModuleInfo, List<PSModuleInfo>> dictionary = new Dictionary<PSModuleInfo, List<PSModuleInfo>>();
            foreach (PSModuleInfo info in base.Context.Modules.GetModules(new string[] { "*" }, false))
            {
                foreach (PSModuleInfo info2 in info.RequiredModules)
                {
                    List<PSModuleInfo> list = null;
                    if (!dictionary.TryGetValue(info2, out list))
                    {
                        dictionary.Add(info2, list = new List<PSModuleInfo>());
                    }
                    list.Add(info);
                }
            }
            return dictionary;
        }

        private bool ModuleProvidesCurrentSessionDrive(PSModuleInfo module)
        {
            if (module.ModuleType == ModuleType.Binary)
            {
                foreach (KeyValuePair<string, List<ProviderInfo>> pair in base.Context.TopLevelSessionState.Providers)
                {
                    foreach (ProviderInfo info in pair.Value)
                    {
                        if (info.ImplementingType.Assembly.Location.Equals(module.Path, StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (PSDriveInfo info2 in base.Context.TopLevelSessionState.GetDrivesForProvider(info.FullName))
                            {
                                if (info2 == base.SessionState.Drive.Current)
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            return false;
        }

        protected override void ProcessRecord()
        {
            Dictionary<PSModuleInfo, List<PSModuleInfo>> dictionary = new Dictionary<PSModuleInfo, List<PSModuleInfo>>();
            foreach (PSModuleInfo info in base.Context.Modules.GetModules(this._name, false))
            {
                dictionary.Add(info, new List<PSModuleInfo> { info });
            }
            foreach (PSModuleInfo info2 in this._moduleInfo)
            {
                dictionary.Add(info2, new List<PSModuleInfo> { info2 });
            }
            Dictionary<PSModuleInfo, List<PSModuleInfo>> dictionary2 = new Dictionary<PSModuleInfo, List<PSModuleInfo>>();
            foreach (KeyValuePair<PSModuleInfo, List<PSModuleInfo>> pair in dictionary)
            {
                PSModuleInfo key = pair.Key;
                if ((key.NestedModules != null) && (key.NestedModules.Count > 0))
                {
                    List<PSModuleInfo> nestedModulesWithNoCircularReference = new List<PSModuleInfo>();
                    this.GetAllNestedModules(key, ref nestedModulesWithNoCircularReference);
                    dictionary2.Add(key, nestedModulesWithNoCircularReference);
                }
            }
            HashSet<PSModuleInfo> set = new HashSet<PSModuleInfo>(new PSModuleInfoComparer());
            if (dictionary2.Count > 0)
            {
                foreach (KeyValuePair<PSModuleInfo, List<PSModuleInfo>> pair2 in dictionary2)
                {
                    List<PSModuleInfo> list4 = null;
                    if (dictionary.TryGetValue(pair2.Key, out list4))
                    {
                        foreach (PSModuleInfo info4 in pair2.Value)
                        {
                            if (!set.Contains(info4))
                            {
                                list4.Add(info4);
                                set.Add(info4);
                            }
                        }
                    }
                }
            }
            Dictionary<PSModuleInfo, List<PSModuleInfo>> dictionary3 = new Dictionary<PSModuleInfo, List<PSModuleInfo>>();
            foreach (KeyValuePair<PSModuleInfo, List<PSModuleInfo>> pair3 in dictionary)
            {
                List<PSModuleInfo> list5 = new List<PSModuleInfo>();
                for (int i = pair3.Value.Count - 1; i >= 0; i--)
                {
                    PSModuleInfo targetObject = pair3.Value[i];
                    if (targetObject.AccessMode == ModuleAccessMode.Constant)
                    {
                        InvalidOperationException exception = new InvalidOperationException(StringUtil.Format(Modules.ModuleIsConstant, targetObject.Name));
                        ErrorRecord errorRecord = new ErrorRecord(exception, "Modules_ModuleIsConstant", ErrorCategory.PermissionDenied, targetObject);
                        base.WriteError(errorRecord);
                    }
                    else if ((targetObject.AccessMode == ModuleAccessMode.ReadOnly) && !base.BaseForce)
                    {
                        string text = StringUtil.Format(Modules.ModuleIsReadOnly, targetObject.Name);
                        if (InitialSessionState.IsConstantEngineModule(targetObject.Name))
                        {
                            base.WriteWarning(text);
                        }
                        else
                        {
                            InvalidOperationException exception2 = new InvalidOperationException(text);
                            ErrorRecord record2 = new ErrorRecord(exception2, "Modules_ModuleIsReadOnly", ErrorCategory.PermissionDenied, targetObject);
                            base.WriteError(record2);
                        }
                    }
                    else if (base.ShouldProcess(StringUtil.Format(Modules.ConfirmRemoveModule, targetObject.Name, targetObject.Path)))
                    {
                        if (this.ModuleProvidesCurrentSessionDrive(targetObject))
                        {
                            if (!InitialSessionState.IsEngineModule(targetObject.Name))
                            {
                                string str4 = (this._name.Length == 1) ? this._name[0] : targetObject.Name;
                                throw PSTraceSource.NewInvalidOperationException("Modules", "ModuleDriveInUse", new object[] { str4 });
                            }
                            if (!base.BaseForce)
                            {
                                string str3 = StringUtil.Format(Modules.CoreModuleCannotBeRemoved, targetObject.Name);
                                base.WriteWarning(str3);
                            }
                        }
                        else
                        {
                            list5.Add(targetObject);
                        }
                    }
                }
                dictionary3[pair3.Key] = list5;
            }
            Dictionary<PSModuleInfo, List<PSModuleInfo>> requiredDependencies = this.GetRequiredDependencies();
            foreach (KeyValuePair<PSModuleInfo, List<PSModuleInfo>> pair4 in dictionary3)
            {
                foreach (PSModuleInfo info6 in pair4.Value)
                {
                    if (!base.BaseForce)
                    {
                        List<PSModuleInfo> list6 = null;
                        if (requiredDependencies.TryGetValue(info6, out list6))
                        {
                            for (int j = list6.Count - 1; j >= 0; j--)
                            {
                                if (dictionary3.ContainsKey(list6[j]))
                                {
                                    list6.RemoveAt(j);
                                }
                            }
                            if (list6.Count > 0)
                            {
                                InvalidOperationException exception4 = new InvalidOperationException(StringUtil.Format(Modules.ModuleIsRequired, info6.Name, list6[0].Name));
                                ErrorRecord record3 = new ErrorRecord(exception4, "Modules_ModuleIsRequired", ErrorCategory.PermissionDenied, info6);
                                base.WriteError(record3);
                                continue;
                            }
                        }
                    }
                    this._numberRemoved++;
                    base.RemoveModule(info6, pair4.Key.Name);
                }
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

        [Parameter(Mandatory=true, ParameterSetName="ModuleInfo", ValueFromPipeline=true, Position=0)]
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

        [Parameter(Mandatory=true, ParameterSetName="name", ValueFromPipeline=true, Position=0)]
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
    }
}

