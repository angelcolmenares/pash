namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Reflection;

    public sealed class CodePropertyData : TypeMemberData
    {
        private MethodInfo _getMethod;
        private bool _isHidden;
        private MethodInfo _setMethod;

        public CodePropertyData(string name, MethodInfo getMethod) : base(name)
        {
            this._getMethod = getMethod;
        }

        public CodePropertyData(string name, MethodInfo getMethod, MethodInfo setMethod) : base(name)
        {
            this._getMethod = getMethod;
            this._setMethod = setMethod;
        }

        internal override TypeMemberData Copy()
        {
            return new CodePropertyData(base.Name, this.GetCodeReference, this.SetCodeReference) { IsHidden = this.IsHidden };
        }

        public MethodInfo GetCodeReference
        {
            get
            {
                return this._getMethod;
            }
            set
            {
                this._getMethod = value;
            }
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

        public MethodInfo SetCodeReference
        {
            get
            {
                return this._setMethod;
            }
            set
            {
                this._setMethod = value;
            }
        }
    }
}

