namespace System.Management.Automation.Tracing
{
    using System;

    public interface IEtwActivityReverter : IDisposable
    {
        void RevertCurrentActivityId();
    }
}

