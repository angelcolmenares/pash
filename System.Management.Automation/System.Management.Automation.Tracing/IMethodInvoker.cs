namespace System.Management.Automation.Tracing
{
    using System;

    internal interface IMethodInvoker
    {
        object[] CreateInvokerArgs(Delegate methodToInvoke, object[] methodToInvokeArgs);

        Delegate Invoker { get; }
    }
}

