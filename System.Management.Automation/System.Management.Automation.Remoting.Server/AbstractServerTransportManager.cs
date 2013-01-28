namespace System.Management.Automation.Remoting.Server
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Remoting;
    using System.Management.Automation.Tracing;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal abstract class AbstractServerTransportManager : BaseTransportManager
    {
        private Queue<Tuple<RemoteDataObject, bool, bool>> dataToBeSentQueue;
        private RemotingDataType dataType;
        private bool isSerializing;
        private SerializedDataStream.OnDataAvailableCallback onDataAvailable;
        private Guid powerShellInstanceId;
        private bool reportAsPending;
        private Guid runpacePoolInstanceId;
        private bool shouldFlushData;
        private object syncObject;
        private RemotingTargetInterface targetInterface;

        internal event EventHandler Closing;

        protected AbstractServerTransportManager(int fragmentSize, PSRemotingCryptoHelper cryptoHelper) : base(cryptoHelper)
        {
            this.syncObject = new object();
            base.Fragmentor.FragmentSize = fragmentSize;
            this.onDataAvailable = new SerializedDataStream.OnDataAvailableCallback(this.OnDataAvailable);
        }

        internal abstract void Close(Exception reasonForClose);
        private void OnDataAvailable(byte[] dataToSend, bool isEndFragment)
        {
            object[] args = new object[] { this.runpacePoolInstanceId.ToString(), this.powerShellInstanceId.ToString(), dataToSend.Length.ToString(CultureInfo.InvariantCulture), (int) this.dataType, (int) this.targetInterface };
            PSEtwLog.LogAnalyticInformational(PSEventId.ServerSendData, PSOpcode.Send, PSTask.None, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic, args);
            this.SendDataToClient(dataToSend, isEndFragment & this.shouldFlushData, this.reportAsPending, isEndFragment);
        }

        internal virtual void Prepare()
        {
            base.ReceivedDataCollection.AllowTwoThreadsToProcessRawData();
        }

        internal void RaiseClosingEvent()
        {
            this.Closing.SafeInvoke(this, EventArgs.Empty);
        }

        internal void ReportError(int errorCode, string methodName)
        {
            string generalError = RemotingErrorIdStrings.GeneralError;
            string message = string.Format(CultureInfo.InvariantCulture, generalError, new object[] { errorCode, methodName });
            PSRemotingTransportException e = new PSRemotingTransportException(message) {
                ErrorCode = errorCode
            };
            ThreadPool.QueueUserWorkItem(delegate (object state) {
                TransportErrorOccuredEventArgs eventArgs = new TransportErrorOccuredEventArgs(e, TransportMethodEnum.Unknown);
                this.RaiseErrorHandler(eventArgs);
            });
        }

        internal abstract void ReportExecutionStatusAsRunning();
        internal void SendDataToClient(RemoteDataObject psObjectData, bool flush, bool reportAsPending = false)
        {
            this.SendDataToClient<object>(psObjectData, flush, reportAsPending);
        }

        internal virtual void SendDataToClient<T>(RemoteDataObject<T> data, bool flush, bool reportPending = false)
        {
            lock (this.syncObject)
            {
                RemoteDataObject obj2 = RemoteDataObject.CreateFrom(data.Destination, data.DataType, data.RunspacePoolId, data.PowerShellId, data.Data);
                if (this.isSerializing)
                {
                    if (this.dataToBeSentQueue == null)
                    {
                        this.dataToBeSentQueue = new Queue<Tuple<RemoteDataObject, bool, bool>>();
                    }
                    this.dataToBeSentQueue.Enqueue(new Tuple<RemoteDataObject, bool, bool>(obj2, flush, reportPending));
                }
                else
                {
                    this.isSerializing = true;
                    try
                    {
                        do
                        {
                            using (SerializedDataStream stream = new SerializedDataStream(base.Fragmentor.FragmentSize, this.onDataAvailable))
                            {
                                this.shouldFlushData = flush;
                                this.reportAsPending = reportPending;
                                this.runpacePoolInstanceId = obj2.RunspacePoolId;
                                this.powerShellInstanceId = obj2.PowerShellId;
                                this.dataType = obj2.DataType;
                                this.targetInterface = obj2.TargetInterface;
                                base.Fragmentor.Fragment<object>(obj2, stream);
                            }
                            if ((this.dataToBeSentQueue != null) && (this.dataToBeSentQueue.Count > 0))
                            {
                                Tuple<RemoteDataObject, bool, bool> tuple = this.dataToBeSentQueue.Dequeue();
                                obj2 = tuple.Item1;
                                flush = tuple.Item2;
                                reportPending = tuple.Item3;
                            }
                            else
                            {
                                obj2 = null;
                            }
                        }
                        while (obj2 != null);
                    }
                    finally
                    {
                        this.isSerializing = false;
                    }
                }
            }
        }

		public virtual void CompleteProcessRawData ()
		{
			
		}

        protected abstract void SendDataToClient(byte[] data, bool flush, bool reportAsPending, bool reportAsDataBoundary);
    }
}

