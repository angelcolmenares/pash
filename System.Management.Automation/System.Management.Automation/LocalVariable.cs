namespace System.Management.Automation
{
    using System;

    internal class LocalVariable : PSVariable
    {
        private readonly MutableTuple _tuple;
        private readonly int _tupleSlot;

        public LocalVariable(string name, MutableTuple tuple, int tupleSlot) : base(name, false)
        {
            this._tuple = tuple;
            this._tupleSlot = tupleSlot;
        }

        internal override void SetValueRaw(object newValue, bool preserveValueTypeSemantics)
        {
            if (preserveValueTypeSemantics)
            {
                newValue = PSVariable.CopyMutableValues(newValue);
            }
            this.Value = newValue;
        }

        public override ScopedItemOptions Options
        {
            get
            {
                return base.Options;
            }
            set
            {
                if (value != base.Options)
                {
                    SessionStateUnauthorizedAccessException exception = new SessionStateUnauthorizedAccessException(base.Name, SessionStateCategory.Variable, "VariableOptionsNotSettable", SessionStateStrings.VariableOptionsNotSettable);
                    throw exception;
                }
            }
        }

        public override object Value
        {
            get
            {
                base.DebuggerCheckVariableRead();
                return this._tuple.GetValue(this._tupleSlot);
            }
            set
            {
                this._tuple.SetValue(this._tupleSlot, value);
                base.DebuggerCheckVariableWrite();
            }
        }
    }
}

