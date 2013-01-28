namespace System.Management.Automation
{
    using System;

    internal class PSInternalMemberSet : PSMemberSet
    {
        private PSObject psObject;
        private object syncObject;

        internal PSInternalMemberSet(string propertyName, PSObject psObject) : base(propertyName)
        {
            this.syncObject = new object();
            base.internalMembers = null;
            this.psObject = psObject;
        }

        private void GenerateInternalMembersFromBase()
        {
            if (this.psObject.isDeserialized)
            {
                if (this.psObject.clrMembers != null)
                {
                    foreach (PSMemberInfo info in this.psObject.clrMembers)
                    {
                        base.internalMembers.Add(info.Copy());
                    }
                }
            }
            else
            {
                foreach (PSMemberInfo info2 in PSObject.dotNetInstanceAdapter.BaseGetMembers<PSMemberInfo>(this.psObject.ImmediateBaseObject))
                {
                    base.internalMembers.Add(info2.Copy());
                }
            }
        }

        private void GenerateInternalMembersFromPSObject()
        {
            foreach (PSMemberInfo info in PSObject.dotNetInstanceAdapter.BaseGetMembers<PSMemberInfo>(this.psObject))
            {
                base.internalMembers.Add(info.Copy());
            }
        }

        private PSMemberInfoInternalCollection<PSMemberInfo> GetInternalMembersFromAdapted()
        {
            PSMemberInfoInternalCollection<PSMemberInfo> internals = new PSMemberInfoInternalCollection<PSMemberInfo>();
            if (this.psObject.isDeserialized)
            {
                if (this.psObject.adaptedMembers != null)
                {
                    foreach (PSMemberInfo info in this.psObject.adaptedMembers)
                    {
                        internals.Add(info.Copy());
                    }
                }
                return internals;
            }
            foreach (PSMemberInfo info2 in this.psObject.InternalAdapter.BaseGetMembers<PSMemberInfo>(this.psObject.ImmediateBaseObject))
            {
                internals.Add(info2.Copy());
            }
            return internals;
        }

        internal override PSMemberInfoInternalCollection<PSMemberInfo> InternalMembers
        {
            get
            {
                if (base.name.Equals("psadapted", StringComparison.OrdinalIgnoreCase))
                {
                    return this.GetInternalMembersFromAdapted();
                }
                if (base.internalMembers == null)
                {
                    lock (this.syncObject)
                    {
                        if (base.internalMembers == null)
                        {
                            base.internalMembers = new PSMemberInfoInternalCollection<PSMemberInfo>();
                            string str = base.name.ToLowerInvariant();
                            if (str != null)
                            {
                                if (!(str == "psbase"))
                                {
                                    if (str == "psobject")
                                    {
                                        goto Label_0079;
                                    }
                                }
                                else
                                {
                                    this.GenerateInternalMembersFromBase();
                                }
                            }
                        }
                        goto Label_008B;
                    Label_0079:
                        this.GenerateInternalMembersFromPSObject();
                    }
                }
            Label_008B:
                return base.internalMembers;
            }
        }
    }
}

