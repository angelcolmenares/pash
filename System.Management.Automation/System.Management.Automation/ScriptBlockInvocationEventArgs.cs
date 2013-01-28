namespace System.Management.Automation
{
    using System;
    using System.Management.Automation.Internal;
    using System.Runtime.CompilerServices;

    internal sealed class ScriptBlockInvocationEventArgs : EventArgs
    {
        internal ScriptBlockInvocationEventArgs(System.Management.Automation.ScriptBlock scriptBlock, bool useLocalScope, System.Management.Automation.ScriptBlock.ErrorHandlingBehavior errorHandlingBehavior, object dollarUnder, object input, object scriptThis, Pipe outputPipe, System.Management.Automation.InvocationInfo invocationInfo, params object[] args)
        {
            if (scriptBlock == null)
            {
                throw PSTraceSource.NewArgumentNullException("scriptBlock");
            }
            this.ScriptBlock = scriptBlock;
            this.OutputPipe = outputPipe;
            this.UseLocalScope = useLocalScope;
            this.ErrorHandlingBehavior = errorHandlingBehavior;
            this.DollarUnder = dollarUnder;
            this.Input = input;
            this.ScriptThis = scriptThis;
            this.InvocationInfo = invocationInfo;
            this.Args = args;
        }

        internal object[] Args { get; set; }

        internal object DollarUnder { get; set; }

        internal System.Management.Automation.ScriptBlock.ErrorHandlingBehavior ErrorHandlingBehavior { get; set; }

        internal System.Exception Exception { get; set; }

        internal object Input { get; set; }

        internal System.Management.Automation.InvocationInfo InvocationInfo { get; set; }

        internal Pipe OutputPipe { get; set; }

        internal System.Management.Automation.ScriptBlock ScriptBlock { get; set; }

        internal object ScriptThis { get; set; }

        internal bool UseLocalScope { get; set; }
    }
}

