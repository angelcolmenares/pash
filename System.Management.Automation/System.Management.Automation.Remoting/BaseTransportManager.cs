namespace System.Management.Automation.Remoting
{
    using System;
    using System.Globalization;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Remoting.Client;
    using System.Management.Automation.Runspaces;
    using System.Management.Automation.Tracing;
    using System.Threading;

    internal abstract class BaseTransportManager : IDisposable
    {
        [TraceSource("Transport", "Traces BaseWSManTransportManager")]
        private static PSTraceSource baseTracer = PSTraceSource.GetTracer("Transport", "Traces BaseWSManTransportManager");
        internal const int ClientCloseTimeoutMs = 0xea60;
        internal const int ClientDefaultOperationTimeoutMs = 0x2bf20;
        private PSRemotingCryptoHelper cryptoHelper;
        internal const int DefaultFragmentSize = 0x8000;
        private System.Management.Automation.Remoting.Fragmentor fragmentor;
        internal const string MAX_RECEIVED_DATA_PER_COMMAND_MB = "PSMaximumReceivedDataSizePerCommandMB";
        internal const string MAX_RECEIVED_OBJECT_SIZE_MB = "PSMaximumReceivedObjectSizeMB";
        internal const int MaximumReceivedDataSize = 0x3200000;
        internal const int MaximumReceivedObjectSize = 0xa00000;
        internal const int MinimumIdleTimeout = 0xea60;
        private System.Management.Automation.Remoting.ReceiveDataCollection.OnDataAvailableCallback onDataAvailableCallback;
        private PriorityReceiveDataCollection recvdData;
        internal const int ServerDefaultKeepAliveTimeoutMs = 0x3a980;
        internal const int UseServerDefaultIdleTimeout = -1;
        internal const int UseServerDefaultIdleTimeoutUInt = int.MaxValue;

        internal event EventHandler<RemoteDataEventArgs> DataReceived;

        public event EventHandler PowerShellGuidObserver;

        internal event EventHandler<TransportErrorOccuredEventArgs> WSManTransportErrorOccured;

        protected BaseTransportManager(PSRemotingCryptoHelper cryptoHelper)
        {
            this.cryptoHelper = cryptoHelper;
            this.fragmentor = new System.Management.Automation.Remoting.Fragmentor(0x8000, cryptoHelper);
            this.recvdData = new PriorityReceiveDataCollection(this.fragmentor, this is BaseClientTransportManager);
            this.onDataAvailableCallback = new System.Management.Automation.Remoting.ReceiveDataCollection.OnDataAvailableCallback(this.OnDataAvailableCallback);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                this.recvdData.Dispose();
            }
        }

        public void MigrateDataReadyEventHandlers(BaseTransportManager transportManager)
        {
            foreach (Delegate delegate2 in transportManager.DataReceived.GetInvocationList())
            {
                this.DataReceived += ((EventHandler<RemoteDataEventArgs>) delegate2);
            }
        }

		private static bool _slowed = false;

        internal void OnDataAvailableCallback (RemoteDataObject<PSObject> remoteObject)
		{
			PSEtwLog.LogAnalyticInformational (PSEventId.TransportReceivedObject, PSOpcode.Open, PSTask.None, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic, new object[] {
				remoteObject.RunspacePoolId.ToString (),
				remoteObject.PowerShellId.ToString (),
				(int)remoteObject.Destination,
				(int)remoteObject.DataType,
				(int)remoteObject.TargetInterface
			});
			this.PowerShellGuidObserver.SafeInvoke (remoteObject.PowerShellId, EventArgs.Empty);
			RemoteDataEventArgs eventArgs = new RemoteDataEventArgs (remoteObject);
			if (remoteObject.DataType == RemotingDataType.RunspacePoolStateInfo) {
				System.Diagnostics.Debug.WriteLine ("RunspacePool State Opened Received");
				Thread.Sleep (800); //HACK: Delay reception in local mode... TODO: Find why!!! cannot have a Wait somewhere
				_slowed = true;
			}
            this.DataReceived.SafeInvoke<RemoteDataEventArgs>(this, eventArgs);
        }

		internal virtual void ProcessRawDataAsync (byte[] data, string stream)
		{
			this.ProcessRawData (data, stream);
		}

        internal virtual void ProcessRawData(byte[] data, string stream)
        {
            try
            {
                this.ProcessRawData(data, stream, this.onDataAvailableCallback);
            }
            catch (Exception exception)
            {
                CommandProcessorBase.CheckForSevereException(exception);
                baseTracer.WriteLine(string.Format(CultureInfo.InvariantCulture, "Exception processing data. {0}", new object[] { exception.Message }), new object[0]);
                PSRemotingTransportException e = new PSRemotingTransportException(exception.Message, exception);
                TransportErrorOccuredEventArgs eventArgs = new TransportErrorOccuredEventArgs(e, TransportMethodEnum.ReceiveShellOutputEx);
                this.RaiseErrorHandler(eventArgs);
            }
        }

        internal void ProcessRawData(byte[] data, string stream, System.Management.Automation.Remoting.ReceiveDataCollection.OnDataAvailableCallback dataAvailableCallback)
        {
            baseTracer.WriteLine("Processing incoming data for stream {0}.", new object[] { stream });
            bool flag = false;
            DataPriorityType priorityType = DataPriorityType.Default;
            if (stream.Equals("stdin", StringComparison.OrdinalIgnoreCase) || stream.Equals("stdout", StringComparison.OrdinalIgnoreCase))
            {
                flag = true;
            }
            else if (stream.Equals("pr", StringComparison.OrdinalIgnoreCase))
            {
                priorityType = DataPriorityType.PromptResponse;
                flag = true;
            }
            if (!flag)
            {
                baseTracer.WriteLine("{0} is not a valid stream", new object[] { stream });
            }
            this.recvdData.ProcessRawData(data, priorityType, dataAvailableCallback);
        }

        internal virtual void RaiseErrorHandler(TransportErrorOccuredEventArgs eventArgs)
        {
            this.WSManTransportErrorOccured.SafeInvoke<TransportErrorOccuredEventArgs>(this, eventArgs);
        }

        internal PSRemotingCryptoHelper CryptoHelper
        {
            get
            {
                return this.cryptoHelper;
            }
            set
            {
                this.cryptoHelper = value;
            }
        }

        internal System.Management.Automation.Remoting.Fragmentor Fragmentor
        {
            get
            {
                return this.fragmentor;
            }
            set
            {
                this.fragmentor = value;
            }
        }

        internal PriorityReceiveDataCollection ReceivedDataCollection
        {
            get
            {
                return this.recvdData;
            }
        }

        internal System.Management.Automation.Runspaces.TypeTable TypeTable
        {
            get
            {
                return this.fragmentor.TypeTable;
            }
            set
            {
                this.fragmentor.TypeTable = value;
            }
        }
    }
}

