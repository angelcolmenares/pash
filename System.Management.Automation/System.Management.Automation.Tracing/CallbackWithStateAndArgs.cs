namespace System.Management.Automation.Tracing
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Timers;

    public delegate void CallbackWithStateAndArgs(object state, ElapsedEventArgs args);
}

