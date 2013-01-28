namespace System.Management.Automation.Remoting.Client
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Tracing;
    using System.Threading;

    internal abstract class BaseClientTransportManager : BaseTransportManager, IDisposable
    {
        private Queue<CallbackNotificationInformation> callbackNotificationQueue;
        protected PrioritySendDataCollection dataToBeSent;
        protected bool isClosed;
        private bool isServicingCallbacks;
        private ReceiveDataCollection.OnDataAvailableCallback onDataAvailableCallback;
        protected bool receiveDataInitiated;
        private Guid runspacePoolInstanceId;
        private bool suspendQueueServicing;
        protected object syncObject;
        [TraceSource("ClientTransport", "Traces ClientTransportManager")]
        protected static PSTraceSource tracer = PSTraceSource.GetTracer("ClientTransport", "Traces ClientTransportManager");

        internal event EventHandler<EventArgs> CloseCompleted;

        internal event EventHandler<EventArgs> ConnectCompleted;

        internal event EventHandler<CreateCompleteEventArgs> CreateCompleted;

        internal event EventHandler<EventArgs> DelayStreamRequestProcessed;

        internal event EventHandler<EventArgs> DisconnectCompleted;

        internal event EventHandler<EventArgs> ReadyForDisconnect;

        internal event EventHandler<EventArgs> ReconnectCompleted;

        internal event EventHandler<ConnectionStatusEventArgs> RobustConnectionNotification;

        protected BaseClientTransportManager(Guid runspaceId, PSRemotingCryptoHelper cryptoHelper) : base(cryptoHelper)
        {
            this.syncObject = new object();
            this.runspacePoolInstanceId = runspaceId;
            this.dataToBeSent = new PrioritySendDataCollection();
            this.onDataAvailableCallback = new ReceiveDataCollection.OnDataAvailableCallback(this.OnDataAvailableHandler);
            this.callbackNotificationQueue = new Queue<CallbackNotificationInformation>();
        }

        internal virtual void CloseAsync()
        {
            this.dataToBeSent.Clear();
        }

        internal abstract void ConnectAsync();
        internal abstract void CreateAsync();
        internal override void Dispose(bool isDisposing)
        {
            this.CreateCompleted = null;
            this.CloseCompleted = null;
            this.ConnectCompleted = null;
            this.DisconnectCompleted = null;
            this.ReconnectCompleted = null;
            base.Dispose(isDisposing);
        }

        internal void EnqueueAndStartProcessingThread(RemoteDataObject<PSObject> remoteObject, TransportErrorOccuredEventArgs transportErrorArgs, object privateData)
        {
            if (!this.isClosed)
            {
                lock (this.callbackNotificationQueue)
                {
                    if (((remoteObject != null) || (transportErrorArgs != null)) || (privateData != null))
                    {
                        CallbackNotificationInformation item = new CallbackNotificationInformation {
                            remoteObject = remoteObject,
                            transportError = transportErrorArgs,
                            privateData = privateData
                        };
                        if ((remoteObject != null) && (((remoteObject.DataType == RemotingDataType.PublicKey) || (remoteObject.DataType == RemotingDataType.EncryptedSessionKey)) || (remoteObject.DataType == RemotingDataType.PublicKeyRequest)))
                        {
                            base.CryptoHelper.Session.BaseSessionDataStructureHandler.RaiseKeyExchangeMessageReceived(remoteObject);
                        }
                        else
                        {
                            this.callbackNotificationQueue.Enqueue(item);
                        }
                    }
                    if ((!this.isServicingCallbacks && !this.suspendQueueServicing) && (this.callbackNotificationQueue.Count > 0))
                    {
                        this.isServicingCallbacks = true;
                        ThreadPool.QueueUserWorkItem(new WaitCallback(this.ServicePendingCallbacks));
                    }
                }
            }
        }

        ~BaseClientTransportManager()
        {
            EventHandler<EventArgs> handler = null;
            if (this.isClosed)
            {
                this.Dispose(false);
            }
            else
            {
                if (handler == null)
                {
                    handler = (source, args) => this.Dispose(false);
                }
                this.CloseCompleted += handler;
                try
                {
                    this.CloseAsync();
                }
                catch (ObjectDisposedException)
                {
                }
            }
        }

        private void OnDataAvailableHandler(RemoteDataObject<PSObject> remoteObject)
        {
            this.EnqueueAndStartProcessingThread(remoteObject, null, null);
        }

        internal virtual void PrepareForConnect()
        {
            throw new NotImplementedException();
        }

        internal virtual void PrepareForDisconnect()
        {
            throw new NotImplementedException();
        }

        internal virtual void ProcessPrivateData(object privateData)
        {
        }

        internal override void ProcessRawData(byte[] data, string stream)
        {
            if (!this.isClosed)
            {
                try
                {
                    base.ProcessRawData(data, stream, this.onDataAvailableCallback);
                }
                catch (PSRemotingTransportException exception)
                {
                    tracer.WriteLine(string.Format(CultureInfo.InvariantCulture, "Exception processing data. {0}", new object[] { exception.Message }), new object[0]);
                    TransportErrorOccuredEventArgs transportErrorArgs = new TransportErrorOccuredEventArgs(exception, TransportMethodEnum.ReceiveShellOutputEx);
                    this.EnqueueAndStartProcessingThread(null, transportErrorArgs, null);
                }
                catch (Exception exception2)
                {
                    CommandProcessorBase.CheckForSevereException(exception2);
                    tracer.WriteLine(string.Format(CultureInfo.InvariantCulture, "Exception processing data. {0}", new object[] { exception2.Message }), new object[0]);
                    PSRemotingTransportException e = new PSRemotingTransportException(exception2.Message);
                    TransportErrorOccuredEventArgs args2 = new TransportErrorOccuredEventArgs(e, TransportMethodEnum.ReceiveShellOutputEx);
                    this.EnqueueAndStartProcessingThread(null, args2, null);
                }
            }
        }

        internal void QueueRobustConnectionNotification(int flags)
        {
            ConnectionStatusEventArgs privateData = null;
            switch (flags)
            {
                case 0x400:
                    privateData = new ConnectionStatusEventArgs(ConnectionStatus.ConnectionRetrySucceeded);
                    break;

                case 0x800:
                    privateData = new ConnectionStatusEventArgs(ConnectionStatus.AutoDisconnectStarting);
                    break;

                case 0x1000:
                    privateData = new ConnectionStatusEventArgs(ConnectionStatus.InternalErrorAbort);
                    break;

                case 0x40:
                    privateData = new ConnectionStatusEventArgs(ConnectionStatus.AutoDisconnectSucceeded);
                    break;

                case 0x100:
                    privateData = new ConnectionStatusEventArgs(ConnectionStatus.NetworkFailureDetected);
                    break;

                case 0x200:
                    privateData = new ConnectionStatusEventArgs(ConnectionStatus.ConnectionRetryAttempt);
                    break;
            }
            this.EnqueueAndStartProcessingThread(null, null, privateData);
        }

        internal void RaiseCloseCompleted()
        {
            this.CloseCompleted.SafeInvoke<EventArgs>(this, EventArgs.Empty);
        }

        internal void RaiseConnectCompleted()
        {
            this.ConnectCompleted.SafeInvoke<EventArgs>(this, EventArgs.Empty);
        }

        internal void RaiseCreateCompleted(CreateCompleteEventArgs eventArgs)
        {
            this.CreateCompleted.SafeInvoke<CreateCompleteEventArgs>(this, eventArgs);
        }

        internal void RaiseDelayStreamProcessedEvent()
        {
            this.DelayStreamRequestProcessed.SafeInvoke<EventArgs>(this, EventArgs.Empty);
        }

        internal void RaiseDisconnectCompleted()
        {
            this.DisconnectCompleted.SafeInvoke<EventArgs>(this, EventArgs.Empty);
        }

        internal void RaiseReadyForDisconnect()
        {
            this.ReadyForDisconnect.SafeInvoke<EventArgs>(this, EventArgs.Empty);
        }

        internal void RaiseReconnectCompleted()
        {
            this.ReconnectCompleted.SafeInvoke<EventArgs>(this, EventArgs.Empty);
        }

        internal void RaiseRobustConnectionNotification(ConnectionStatusEventArgs args)
        {
            this.RobustConnectionNotification.SafeInvoke<ConnectionStatusEventArgs>(this, args);
        }

        internal void ResumeQueue()
        {
            lock (this.callbackNotificationQueue)
            {
                if (this.suspendQueueServicing)
                {
                    this.suspendQueueServicing = false;
                    this.EnqueueAndStartProcessingThread(null, null, null);
                }
            }
        }

        internal void ServicePendingCallbacks(object objectToProcess)
        {
            tracer.WriteLine("ServicePendingCallbacks thread is starting", new object[0]);
            PSEtwLog.ReplaceActivityIdForCurrentThread(this.runspacePoolInstanceId, PSEventId.OperationalTransferEventRunspacePool, PSEventId.AnalyticTransferEventRunspacePool, PSKeyword.Transport, PSTask.None);
            try
            {
                while (!this.isClosed)
                {
                    CallbackNotificationInformation information = null;
                    lock (this.callbackNotificationQueue)
                    {
                        if ((this.callbackNotificationQueue.Count <= 0) || this.suspendQueueServicing)
                        {
                            return;
                        }
                        information = this.callbackNotificationQueue.Dequeue();
                    }
                    if (information != null)
                    {
                        if (information.transportError != null)
                        {
                            this.RaiseErrorHandler(information.transportError);
                            return;
                        }
                        if (information.privateData != null)
                        {
                            this.ProcessPrivateData(information.privateData);
                        }
                        else
                        {
                            base.OnDataAvailableCallback(information.remoteObject);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                tracer.WriteLine(string.Format(CultureInfo.InvariantCulture, "Exception processing data. {0}", new object[] { exception.Message }), new object[0]);
                PSRemotingTransportException e = new PSRemotingTransportException(exception.Message, exception);
                TransportErrorOccuredEventArgs eventArgs = new TransportErrorOccuredEventArgs(e, TransportMethodEnum.ReceiveShellOutputEx);
                this.RaiseErrorHandler(eventArgs);
            }
            finally
            {
                lock (this.callbackNotificationQueue)
                {
                    tracer.WriteLine("ServicePendingCallbacks thread is exiting", new object[0]);
                    this.isServicingCallbacks = false;
                    this.EnqueueAndStartProcessingThread(null, null, null);
                }
            }
        }

        internal virtual void StartReceivingData()
        {
            throw new NotImplementedException();
        }

        internal void SuspendQueue()
        {
            lock (this.callbackNotificationQueue)
            {
                this.suspendQueueServicing = true;
            }
        }

        internal PrioritySendDataCollection DataToBeSentCollection
        {
            get
            {
                return this.dataToBeSent;
            }
        }

        internal Guid RunspacePoolInstanceId
        {
            get
            {
                return this.runspacePoolInstanceId;
            }
        }

        internal class CallbackNotificationInformation
        {
            internal object privateData;
            internal RemoteDataObject<PSObject> remoteObject;
            internal TransportErrorOccuredEventArgs transportError;
        }
    }
}

