namespace System.Management.Automation
{
    using System;

    internal class ContinueException : LoopFlowException
    {
        internal ContinueException(string label) : base(label)
        {
        }
    }
}

