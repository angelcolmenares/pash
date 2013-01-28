namespace System.Management.Automation.Tracing
{
    using System;

    internal class EtwActivityReverterMethodInvoker : IMethodInvoker
    {
        private readonly IEtwEventCorrelator _eventCorrelator;
        private readonly Func<Guid, Delegate, object[], object> _invoker;

        public EtwActivityReverterMethodInvoker(IEtwEventCorrelator eventCorrelator)
        {
            if (eventCorrelator == null)
            {
                throw new ArgumentNullException("eventCorrelator");
            }
            this._eventCorrelator = eventCorrelator;
            this._invoker = new Func<Guid, Delegate, object[], object>(this.DoInvoke);
        }

        public object[] CreateInvokerArgs(Delegate methodToInvoke, object[] methodToInvokeArgs)
        {
            return new object[] { this._eventCorrelator.CurrentActivityId, methodToInvoke, methodToInvokeArgs };
        }

        private object DoInvoke(Guid relatedActivityId, Delegate method, object[] methodArgs)
        {
            using (this._eventCorrelator.StartActivity(relatedActivityId))
            {
                return method.DynamicInvoke(methodArgs);
            }
        }

        public Delegate Invoker
        {
            get
            {
                return this._invoker;
            }
        }
    }
}

