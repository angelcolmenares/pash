namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Runspaces;

    [Cmdlet("Get", "Member", HelpUri="http://go.microsoft.com/fwlink/?LinkID=113322", RemotingCapability=RemotingCapability.None), OutputType(new Type[] { typeof(MemberDefinition) })]
    public class GetMemberCommand : PSCmdlet
    {
        private PSObject inputObject;
        private MshMemberMatchOptions matchOptions;
        private PSMemberTypes memberType = PSMemberTypes.All;
        private string[] name = new string[] { "*" };
        private bool staticParameter;
        private HybridDictionary typesAlreadyDisplayed = new HybridDictionary();
        private PSMemberViewTypes view = (PSMemberViewTypes.Adapted | PSMemberViewTypes.Extended);

        protected override void EndProcessing()
        {
            if (this.typesAlreadyDisplayed.Count == 0)
            {
                ErrorDetails details = new ErrorDetails(base.GetType().Assembly, "GetMember", "NoObjectSpecified", new object[0]);
                ErrorRecord errorRecord = new ErrorRecord(new InvalidOperationException(details.Message), "NoObjectInGetMember", ErrorCategory.CloseError, null);
                base.WriteError(errorRecord);
            }
        }

        protected override void ProcessRecord()
        {
            if ((this.InputObject != null) && (this.InputObject != AutomationNull.Value))
            {
                string fullName;
                Type type = null;
                Adapter dotNetStaticAdapter = null;
                if (this.Static == 1)
                {
                    dotNetStaticAdapter = PSObject.dotNetStaticAdapter;
                    object baseObject = this.InputObject.BaseObject;
                    type = baseObject as Type;
                    if (type == null)
                    {
                        type = baseObject.GetType();
                    }
                    fullName = type.FullName;
                }
                else
                {
                    ConsolidatedString internalTypeNames = this.InputObject.InternalTypeNames;
                    if (internalTypeNames.Count != 0)
                    {
                        fullName = internalTypeNames[0];
                    }
                    else
                    {
                        fullName = "<null>";
                    }
                }
                if (!this.typesAlreadyDisplayed.Contains(fullName))
                {
                    PSMemberInfoCollection<PSMemberInfo> infos;
                    this.typesAlreadyDisplayed.Add(fullName, "");
                    PSMemberTypes memberType = this.memberType;
                    PSMemberViewTypes view = this.view;
                    if (((this.view & PSMemberViewTypes.Extended) == 0) && !typeof(PSMemberSet).ToString().Equals(fullName, StringComparison.OrdinalIgnoreCase))
                    {
                        memberType ^= PSMemberTypes.MemberSet | PSMemberTypes.ScriptMethod | PSMemberTypes.CodeMethod | PSMemberTypes.PropertySet | PSMemberTypes.ScriptProperty | PSMemberTypes.NoteProperty | PSMemberTypes.CodeProperty | PSMemberTypes.AliasProperty;
                    }
                    if (((this.view & PSMemberViewTypes.Adapted) == 0) && ((this.view & PSMemberViewTypes.Base) == 0))
                    {
                        memberType ^= PSMemberTypes.ParameterizedProperty | PSMemberTypes.Method | PSMemberTypes.Property;
                    }
                    if (((this.view & PSMemberViewTypes.Base) == PSMemberViewTypes.Base) && (this.InputObject.InternalBaseDotNetAdapter == null))
                    {
                        view |= PSMemberViewTypes.Adapted;
                    }
                    if (this.Static == 1)
                    {
                        infos = dotNetStaticAdapter.BaseGetMembers<PSMemberInfo>(type);
                    }
                    else
                    {
                        Collection<CollectionEntry<PSMemberInfo>> memberCollection = PSObject.GetMemberCollection(view);
                        infos = new PSMemberInfoIntegratingCollection<PSMemberInfo>(this.InputObject, memberCollection);
                    }
                    foreach (string str3 in this.Name)
                    {
                        ReadOnlyPSMemberInfoCollection<PSMemberInfo> infos2 = infos.Match(str3, memberType, this.matchOptions);
                        MemberDefinition[] array = new MemberDefinition[infos2.Count];
                        int index = 0;
                        foreach (PSMemberInfo info in infos2)
                        {
                            if (this.Force == 0)
                            {
                                PSMethod method = info as PSMethod;
                                if ((method != null) && method.IsSpecial)
                                {
                                    continue;
                                }
                            }
                            array[index] = new MemberDefinition(fullName, info.Name, info.MemberType, info.ToString());
                            index++;
                        }
                        Array.Sort<MemberDefinition>(array, 0, index, new MemberComparer());
                        for (int i = 0; i < index; i++)
                        {
                            base.WriteObject(array[i]);
                        }
                    }
                }
            }
        }

        [Parameter]
        public SwitchParameter Force
        {
            get
            {
                return (this.matchOptions == MshMemberMatchOptions.IncludeHidden);
            }
            set
            {
                if (value != 0)
                {
                    this.matchOptions = MshMemberMatchOptions.IncludeHidden;
                }
                else
                {
                    this.matchOptions = MshMemberMatchOptions.None;
                }
            }
        }

        [Parameter(ValueFromPipeline=true)]
        public PSObject InputObject
        {
            get
            {
                return this.inputObject;
            }
            set
            {
                this.inputObject = value;
            }
        }

        [Alias(new string[] { "Type" }), Parameter]
        public PSMemberTypes MemberType
        {
            get
            {
                return this.memberType;
            }
            set
            {
                this.memberType = value;
            }
        }

        [ValidateNotNullOrEmpty, Parameter(Position=0)]
        public string[] Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        [Parameter]
        public SwitchParameter Static
        {
            get
            {
                return this.staticParameter;
            }
            set
            {
                this.staticParameter = (bool) value;
            }
        }

        [Parameter]
        public PSMemberViewTypes View
        {
            get
            {
                return this.view;
            }
            set
            {
                this.view = value;
            }
        }

        private class MemberComparer : IComparer<MemberDefinition>
        {
            public int Compare(MemberDefinition first, MemberDefinition second)
            {
                int num = string.Compare(first.MemberType.ToString(), second.MemberType.ToString(), StringComparison.OrdinalIgnoreCase);
                if (num != 0)
                {
                    return num;
                }
                return string.Compare(first.Name, second.Name, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}

