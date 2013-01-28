namespace System.Management.Automation.Help
{
    using System;
    using System.Collections;
    using System.Management.Automation;

    internal class PositionalParameterComparer : IComparer
    {
        public int Compare(object x, object y)
        {
            CommandParameterInfo info = x as CommandParameterInfo;
            CommandParameterInfo info2 = y as CommandParameterInfo;
            return (info.Position - info2.Position);
        }
    }
}

