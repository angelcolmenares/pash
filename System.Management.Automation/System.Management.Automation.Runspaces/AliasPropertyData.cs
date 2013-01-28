namespace System.Management.Automation.Runspaces
{
    using System;

    public sealed class AliasPropertyData : TypeMemberData
    {
        private bool _isHidden;
        private string _referencedMemberName;
        private Type _type;

        public AliasPropertyData(string name, string referencedMemberName) : base(name)
        {
            this._referencedMemberName = referencedMemberName;
        }

        public AliasPropertyData(string name, string referencedMemberName, Type type) : base(name)
        {
            this._referencedMemberName = referencedMemberName;
            this._type = type;
        }

        internal override TypeMemberData Copy()
        {
            return new AliasPropertyData(base.Name, this.ReferencedMemberName, this.MemberType) { IsHidden = this.IsHidden };
        }

        public bool IsHidden
        {
            get
            {
                return this._isHidden;
            }
            set
            {
                this._isHidden = value;
            }
        }

        public Type MemberType
        {
            get
            {
                return this._type;
            }
            set
            {
                this._type = value;
            }
        }

        public string ReferencedMemberName
        {
            get
            {
                return this._referencedMemberName;
            }
            set
            {
                this._referencedMemberName = value;
            }
        }
    }
}

