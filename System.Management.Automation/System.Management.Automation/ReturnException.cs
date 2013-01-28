namespace System.Management.Automation
{
    using System;
    using System.Runtime.CompilerServices;

    internal class ReturnException : FlowControlException
    {
        internal ReturnException(object argument)
        {
            this.Argument = argument;
        }

        internal object Argument { get; set; }
    }
}

