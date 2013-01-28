namespace System.Management.Automation
{
    using System;

    public class PSCustomObject
    {
        internal static PSCustomObject SelfInstance = new PSCustomObject();

        private PSCustomObject()
        {
        }

        public override string ToString()
        {
            return "";
        }
    }
}

