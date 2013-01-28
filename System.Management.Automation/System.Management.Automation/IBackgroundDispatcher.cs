namespace System.Management.Automation
{
    using System;
    using System.Threading;

    public interface IBackgroundDispatcher
    {
        IAsyncResult BeginInvoke(WaitCallback callback, object state, AsyncCallback completionCallback, object asyncState);
        void EndInvoke(IAsyncResult asyncResult);
        bool QueueUserWorkItem(WaitCallback callback);
        bool QueueUserWorkItem(WaitCallback callback, object state);
    }
}

