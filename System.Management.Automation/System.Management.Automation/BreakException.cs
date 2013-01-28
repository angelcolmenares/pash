namespace System.Management.Automation
{
    using System;

    internal class BreakException : LoopFlowException
    {
        internal BreakException(string label) : base(label)
        {
        }
    }
}

