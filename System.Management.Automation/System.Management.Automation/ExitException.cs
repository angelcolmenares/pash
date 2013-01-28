namespace System.Management.Automation
{
    using System;
    using System.Runtime.CompilerServices;

    internal class ExitException : FlowControlException
    {
        public ExitException(object argument)
        {
            this.Argument = argument;
        }

        internal object Argument { get; set; }
    }
}

