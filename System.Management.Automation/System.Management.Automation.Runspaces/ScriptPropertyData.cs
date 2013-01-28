namespace System.Management.Automation.Runspaces
{
    using System;
    using System.Management.Automation;

    public sealed class ScriptPropertyData : TypeMemberData
    {
        private ScriptBlock _getScriptBlock;
        private bool _isHidden;
        private ScriptBlock _setScriptBlock;

        public ScriptPropertyData(string name, ScriptBlock getScriptBlock) : base(name)
        {
            this._getScriptBlock = getScriptBlock;
        }

        public ScriptPropertyData(string name, ScriptBlock getScriptBlock, ScriptBlock setScriptBlock) : base(name)
        {
            this._getScriptBlock = getScriptBlock;
            this._setScriptBlock = setScriptBlock;
        }

        internal override TypeMemberData Copy()
        {
            return new ScriptPropertyData(base.Name, this.GetScriptBlock, this.SetScriptBlock) { IsHidden = this.IsHidden };
        }

        public ScriptBlock GetScriptBlock
        {
            get
            {
                return this._getScriptBlock;
            }
            set
            {
                this._getScriptBlock = value;
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

        public ScriptBlock SetScriptBlock
        {
            get
            {
                return this._setScriptBlock;
            }
            set
            {
                this._setScriptBlock = value;
            }
        }
    }
}

