namespace System.Management.Automation
{
    using System;
    using System.Globalization;

    internal class PSCultureVariable : PSVariable
    {
        internal PSCultureVariable() : base("PSCulture", true, ScopedItemOptions.AllScope | ScopedItemOptions.ReadOnly, RunspaceInit.DollarPSCultureDescription)
        {
        }

        public override object Value
        {
            get
            {
                base.DebuggerCheckVariableRead();
                return CultureInfo.CurrentCulture.Name;
            }
        }
    }
}

