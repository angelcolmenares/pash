namespace System.Management.Automation
{
    using System;

    internal class QuestionMarkVariable : PSVariable
    {
        private readonly ExecutionContext _context;

        internal QuestionMarkVariable(ExecutionContext context) : base("?", true, ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly, RunspaceInit.DollarHookDescription)
        {
            this._context = context;
        }

        public override object Value
        {
            get
            {
                base.DebuggerCheckVariableRead();
                return this._context.QuestionMarkVariableValue;
            }
            set
            {
                base.Value = value;
            }
        }
    }
}

