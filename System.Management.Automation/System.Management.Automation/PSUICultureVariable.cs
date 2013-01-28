namespace System.Management.Automation
{
    using System;
    using System.Globalization;

    internal class PSUICultureVariable : PSVariable
    {
        internal PSUICultureVariable() : base("PSUICulture", true, ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly, RunspaceInit.DollarPSUICultureDescription)
        {
        }

        public override object Value
        {
            get
            {
                base.DebuggerCheckVariableRead();
                return CultureInfo.CurrentUICulture.Name;
            }
        }
    }
}

