namespace System.Management.Automation
{
    using System;
    using System.Diagnostics.Eventing;
    using System.Management.Automation.Tracing;
    using System.Threading;

    public class BackgroundDispatcher : IBackgroundDispatcher
    {
        private readonly IMethodInvoker _etwActivityMethodInvoker;
        private readonly WaitCallback _invokerWaitCallback;

        internal BackgroundDispatcher(IMethodInvoker etwActivityMethodInvoker)
        {
            if (etwActivityMethodInvoker == null)
            {
                throw new ArgumentNullException("etwActivityMethodInvoker");
            }
            this._etwActivityMethodInvoker = etwActivityMethodInvoker;
            this._invokerWaitCallback = new WaitCallback(this.DoInvoker);
        }

        public BackgroundDispatcher(EventProvider transferProvider, EventDescriptor transferEvent) : this(new EtwActivityReverterMethodInvoker(new EtwEventCorrelator(transferProvider, transferEvent)))
        {
        }

        public IAsyncResult BeginInvoke(WaitCallback callback, object state, AsyncCallback completionCallback, object asyncState)
        {
            object[] objArray = this._etwActivityMethodInvoker.CreateInvokerArgs(callback, new object[] { state });
            return this._invokerWaitCallback.BeginInvoke(objArray, completionCallback, asyncState);
        }

        private void DoInvoker(object invokerArgs)
        {
            object[] args = (object[]) invokerArgs;
            this._etwActivityMethodInvoker.Invoker.DynamicInvoke(args);
        }

        public void EndInvoke(IAsyncResult asyncResult)
        {
            this._invokerWaitCallback.EndInvoke(asyncResult);
        }

        public bool QueueUserWorkItem(WaitCallback callback)
        {
            return this.QueueUserWorkItem(callback, null);
        }

        public bool QueueUserWorkItem(WaitCallback callback, object state)
        {
            object[] objArray = this._etwActivityMethodInvoker.CreateInvokerArgs(callback, new object[] { state });
            return ThreadPool.QueueUserWorkItem(this._invokerWaitCallback, objArray);
        }
    }
}

