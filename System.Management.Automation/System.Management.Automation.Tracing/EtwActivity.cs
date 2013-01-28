namespace System.Management.Automation.Tracing
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Eventing;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Timers;

    public abstract class EtwActivity
    {
        private static EventDescriptor _WriteTransferEvent = new EventDescriptor(0x1f05, 1, 0x11, 5, 20, 0, 0x4000000000000000L);
        private EventProvider currentProvider;
        private static Guid powerShellProviderId = Guid.Parse("A0C1853B-5C40-4b15-8766-3CF1C58F985A");
        private static Dictionary<Guid, EventProvider> providers = new Dictionary<Guid, EventProvider>();
        private static object syncLock = new object();

        public static  event EventHandler<EtwEventArgs> EventWritten;

        protected EtwActivity()
        {
        }

        public void Correlate()
        {
            Guid activityId = Trace.CorrelationManager.ActivityId;
            this.CorrelateWithActivity(activityId);
        }

        public AsyncCallback Correlate(AsyncCallback callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }
            return new AsyncCallback(new CorrelatedCallback(this, callback).Callback);
        }

        public CallbackNoParameter Correlate(CallbackNoParameter callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }
            return new CallbackNoParameter(new CorrelatedCallback(this, callback).Callback);
        }

        public CallbackWithState Correlate(CallbackWithState callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }
            return new CallbackWithState(new CorrelatedCallback(this, callback).Callback);
        }

        public CallbackWithStateAndArgs Correlate(CallbackWithStateAndArgs callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }
            return new CallbackWithStateAndArgs(new CorrelatedCallback(this, callback).Callback);
        }

        public void CorrelateWithActivity(Guid parentActivityId)
        {
            EventProvider provider = this.GetProvider();
            if (provider.IsEnabled())
            {
                Guid activityId = CreateActivityId();
                SetActivityId(activityId);
                if (parentActivityId != Guid.Empty)
                {
                    EventDescriptor transferEvent = this.TransferEvent;
                    provider.WriteTransferEvent(ref transferEvent, parentActivityId, new object[] { activityId, parentActivityId });
                }
            }
        }

        public static Guid CreateActivityId()
        {
            return EventProvider.CreateActivityId();
        }

        public static Guid GetActivityId()
        {
            Guid empty = Guid.Empty;
            UnsafeNativeMethods.EventActivityIdControl(UnsafeNativeMethods.ActivityControlCode.Get, ref empty);
            return empty;

        }

        private EventProvider GetProvider()
        {
            if (this.currentProvider == null)
            {
                lock (syncLock)
                {
                    if (this.currentProvider != null)
                    {
                        return this.currentProvider;
                    }
                    if (providers.ContainsKey(this.ProviderId))
                    {
                        this.currentProvider = providers[this.ProviderId];
                    }
                    else
                    {
                        this.currentProvider = new EventProvider(this.ProviderId);
                        providers[this.ProviderId] = this.currentProvider;
                    }
                }
            }
            return this.currentProvider;
        }

        public bool IsProviderEnabled(byte levels, long keywords)
        {
            return this.GetProvider().IsEnabled(levels, keywords);
        }

        public static bool SetActivityId(Guid activityId)
        {
            if (GetActivityId() != activityId)
            {
                EventProvider.SetActivityId(ref activityId);
                return true;
            }
            return false;
        }

        protected void WriteEvent(EventDescriptor ed, params object[] payload)
        {
            EventProvider provider = this.GetProvider();
            if (provider.IsEnabled())
            {
                if (payload != null)
                {
                    for (int i = 0; i < payload.Length; i++)
                    {
                        if (payload[i] == null)
                        {
                            payload[i] = string.Empty;
                        }
                    }
                }
                bool success = provider.WriteEvent(ref ed, payload);
                if (EventWritten != null)
                {
                    EventWritten(this, new EtwEventArgs(ed, success, payload));
                }
            }
        }

        public bool IsEnabled
        {
            get
            {
                return this.GetProvider().IsEnabled();
            }
        }

        protected virtual Guid ProviderId
        {
            get
            {
                return powerShellProviderId;
            }
        }

        protected virtual EventDescriptor TransferEvent
        {
            get
            {
                return _WriteTransferEvent;
            }
        }

        private class CorrelatedCallback
        {
            private AsyncCallback asyncCallback;
            private CallbackNoParameter callbackNoParam;
            private CallbackWithState callbackWithState;
            private CallbackWithStateAndArgs callbackWithStateAndArgs;
            protected readonly Guid parentActivityId;
            private readonly EtwActivity tracer;

            public CorrelatedCallback(EtwActivity tracer, AsyncCallback callback)
            {
                if (callback == null)
                {
                    throw new ArgumentNullException("callback");
                }
                if (tracer == null)
                {
                    throw new ArgumentNullException("tracer");
                }
                this.tracer = tracer;
                this.parentActivityId = EtwActivity.GetActivityId();
                this.asyncCallback = callback;
            }

            public CorrelatedCallback(EtwActivity tracer, CallbackNoParameter callback)
            {
                if (callback == null)
                {
                    throw new ArgumentNullException("callback");
                }
                if (tracer == null)
                {
                    throw new ArgumentNullException("tracer");
                }
                this.tracer = tracer;
                this.parentActivityId = EtwActivity.GetActivityId();
                this.callbackNoParam = callback;
            }

            public CorrelatedCallback(EtwActivity tracer, CallbackWithState callback)
            {
                if (callback == null)
                {
                    throw new ArgumentNullException("callback");
                }
                if (tracer == null)
                {
                    throw new ArgumentNullException("tracer");
                }
                this.tracer = tracer;
                this.parentActivityId = EtwActivity.GetActivityId();
                this.callbackWithState = callback;
            }

            public CorrelatedCallback(EtwActivity tracer, CallbackWithStateAndArgs callback)
            {
                if (callback == null)
                {
                    throw new ArgumentNullException("callback");
                }
                if (tracer == null)
                {
                    throw new ArgumentNullException("tracer");
                }
                this.tracer = tracer;
                this.parentActivityId = EtwActivity.GetActivityId();
                this.callbackWithStateAndArgs = callback;
            }

            public void Callback()
            {
                this.Correlate();
                this.callbackNoParam();
            }

            public void Callback(IAsyncResult asyncResult)
            {
                this.Correlate();
                this.asyncCallback(asyncResult);
            }

            public void Callback(object state)
            {
                this.Correlate();
                this.callbackWithState(state);
            }

            public void Callback(object state, ElapsedEventArgs args)
            {
                this.Correlate();
                this.callbackWithStateAndArgs(state, args);
            }

            private void Correlate()
            {
                this.tracer.CorrelateWithActivity(this.parentActivityId);
            }
        }

        private static class UnsafeNativeMethods
        {
            private const string ADVAPI32 = "advapi32.dll";

			/*
            [DllImport("advapi32.dll", CharSet=CharSet.Unicode, ExactSpelling=true)]
            internal static extern int EventActivityIdControl([In] ActivityControlCode controlCode, [In, Out] ref Guid activityId);
			*/

			internal static int EventActivityIdControl ([In] ActivityControlCode controlCode, [In, Out] ref Guid activityId)
			{
				activityId = Guid.NewGuid();
				return 0;
			}


            internal enum ActivityControlCode : int
            {
                Create = 3,
                CreateSet = 5,
                Get = 1,
                GetSet = 4,
                Set = 2
            }
        }
    }
}

