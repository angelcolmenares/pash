namespace System.Management.Automation
{
    using System;

    public abstract class PSMemberInfo
    {
        internal object instance;
        internal bool isHidden;
        internal bool isInstance = true;
        internal bool isReservedMember;
        internal string name;
        internal bool shouldSerialize = true;

        protected PSMemberInfo()
        {
        }

        internal void CloneBaseProperties(PSMemberInfo destiny)
        {
            destiny.name = this.name;
            destiny.isHidden = this.isHidden;
            destiny.isReservedMember = this.isReservedMember;
            destiny.isInstance = this.isInstance;
            destiny.instance = this.instance;
            destiny.shouldSerialize = this.shouldSerialize;
        }

        public abstract PSMemberInfo Copy();
        internal bool MatchesOptions(MshMemberMatchOptions options)
        {
            if (this.IsHidden && ((options & MshMemberMatchOptions.IncludeHidden) == MshMemberMatchOptions.None))
            {
                return false;
            }
            if (!this.ShouldSerialize && ((options & MshMemberMatchOptions.OnlySerializable) != MshMemberMatchOptions.None))
            {
                return false;
            }
            return true;
        }

        internal virtual void ReplicateInstance(object particularInstance)
        {
            this.instance = particularInstance;
        }

        protected void SetMemberName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw PSTraceSource.NewArgumentException("name");
            }
            this.name = name;
        }

        internal void SetValueNoConversion(object setValue)
        {
            PSProperty property = this as PSProperty;
            if (property == null)
            {
                this.Value = setValue;
            }
            else
            {
                property.SetAdaptedValue(setValue, false);
            }
        }

        internal bool IsHidden
        {
            get
            {
                return this.isHidden;
            }
        }

        public bool IsInstance
        {
            get
            {
                return this.isInstance;
            }
        }

        internal bool IsReservedMember
        {
            get
            {
                return this.isReservedMember;
            }
        }

        public abstract PSMemberTypes MemberType { get; }

        public string Name
        {
            get
            {
                return this.name;
            }
        }

        internal bool ShouldSerialize
        {
            get
            {
                return this.shouldSerialize;
            }
            set
            {
                this.shouldSerialize = value;
            }
        }

        public abstract string TypeNameOfValue { get; }

        public abstract object Value { get; set; }
    }
}

