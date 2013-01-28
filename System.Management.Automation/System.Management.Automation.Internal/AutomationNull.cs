namespace System.Management.Automation.Internal
{
    using System;
    using System.Management.Automation;

    public static class AutomationNull
    {
        private static readonly PSObject value1 = new PSObject();

        public static PSObject Value
        {
            get
            {
                return value1;
            }
        }
    }
}

