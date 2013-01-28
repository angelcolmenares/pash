namespace System.Management.Automation.Remoting
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal class CmdletMethodInvoker<T>
    {
        internal Func<Cmdlet, T> Action { get; set; }

        internal Exception ExceptionThrownOnCmdletThread { get; set; }

        internal ManualResetEventSlim Finished { get; set; }

        internal T MethodResult { get; set; }

        internal object SyncObject { get; set; }
    }
}

