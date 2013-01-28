namespace System.Management.Automation.Runspaces
{
    using System;

    public sealed class NotePropertyData : TypeMemberData
    {
        private bool _isHidden;
        private object _value;

        public NotePropertyData(string name, object value) : base(name)
        {
            this._value = value;
        }

        internal override TypeMemberData Copy()
        {
            return new NotePropertyData(base.Name, this.Value) { IsHidden = this.IsHidden };
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

        public object Value
        {
            get
            {
                return this._value;
            }
            set
            {
                this._value = value;
            }
        }
    }
}

