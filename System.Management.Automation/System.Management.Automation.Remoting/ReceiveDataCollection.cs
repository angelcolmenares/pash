namespace System.Management.Automation.Remoting
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Tracing;
    using System.Runtime.CompilerServices;

    internal class ReceiveDataCollection : IDisposable
    {
        [TraceSource("Transport", "Traces BaseWSManTransportManager")]
        private static PSTraceSource baseTracer = PSTraceSource.GetTracer("Transport", "Traces BaseWSManTransportManager");
        private bool canIgnoreOffSyncFragments;
        private long currentFrgId;
        private long currentObjectId;
        private MemoryStream dataToProcessStream;
        private Fragmentor defragmentor;
        private bool isCreateByClientTM;
        private bool isDisposed;
        private int maxNumberOfThreadsToAllowForProcessing = 1;
        private int? maxReceivedObjectSize;
        private int numberOfThreadsProcessing;
        private MemoryStream pendingDataStream = new MemoryStream();
        private object syncObject = new object();
        private int totalReceivedObjectSizeSoFar;

        internal ReceiveDataCollection(Fragmentor defragmentor, bool createdByClientTM)
        {
            this.defragmentor = defragmentor;
            this.isCreateByClientTM = createdByClientTM;
        }

        internal void AllowTwoThreadsToProcessRawData()
        {
            this.maxNumberOfThreadsToAllowForProcessing = 2;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal virtual void Dispose(bool isDisposing)
        {
            lock (this.syncObject)
            {
                this.isDisposed = true;
                if (this.numberOfThreadsProcessing == 0)
                {
                    this.ReleaseResources();
                }
            }
        }

        internal void PrepareForStreamConnect()
        {
            this.canIgnoreOffSyncFragments = true;
        }

        internal void ProcessRawData(byte[] data, OnDataAvailableCallback callback)
        {
            lock (this.syncObject)
            {
                if (this.isDisposed)
                {
                    return;
                }
                this.numberOfThreadsProcessing++;
                int maxNumberOfThreadsToAllowForProcessing = this.maxNumberOfThreadsToAllowForProcessing;
                int numberOfThreadsProcessing = this.numberOfThreadsProcessing;
            }
            try
            {
                this.pendingDataStream.Write(data, 0, data.Length);
            Label_005A:
                if (this.pendingDataStream.Length <= 0x15L)
                {
                    baseTracer.WriteLine(string.Format(CultureInfo.InvariantCulture, "Not enough data to process. Data is less than header length. Data length is {0}. Header Length {1}.", new object[] { this.pendingDataStream.Length, 0x15 }), new object[0]);
                }
                else
                {
                    byte[] fragmentBytes = this.pendingDataStream.GetBuffer();
                    long objectId = FragmentedRemoteObject.GetObjectId(fragmentBytes, 0);
                    if (objectId <= 0L)
                    {
                        throw new PSRemotingTransportException(RemotingErrorIdStrings.ObjectIdCannotBeLessThanZero);
                    }
                    long fragmentId = FragmentedRemoteObject.GetFragmentId(fragmentBytes, 0);
                    bool isStartFragment = FragmentedRemoteObject.GetIsStartFragment(fragmentBytes, 0);
                    bool isEndFragment = FragmentedRemoteObject.GetIsEndFragment(fragmentBytes, 0);
                    int blobLength = FragmentedRemoteObject.GetBlobLength(fragmentBytes, 0);
                    baseTracer.WriteLine(string.Format(CultureInfo.InvariantCulture, "Object Id: {0}", new object[] { objectId }), new object[0]);
                    baseTracer.WriteLine(string.Format(CultureInfo.InvariantCulture, "Fragment Id: {0}", new object[] { fragmentId }), new object[0]);
                    baseTracer.WriteLine(string.Format(CultureInfo.InvariantCulture, "Start Flag: {0}", new object[] { isStartFragment }), new object[0]);
                    baseTracer.WriteLine(string.Format(CultureInfo.InvariantCulture, "End Flag: {0}", new object[] { isEndFragment }), new object[0]);
                    baseTracer.WriteLine(string.Format(CultureInfo.InvariantCulture, "Blob Length: {0}", new object[] { blobLength }), new object[0]);
                    int count = 0;
                    try
                    {
                        count = 0x15 + blobLength;
                    }
                    catch (OverflowException)
                    {
                        baseTracer.WriteLine("Fragement too big.", new object[0]);
                        this.ResetRecieveData();
                        PSRemotingTransportException exception = new PSRemotingTransportException(RemotingErrorIdStrings.ObjectIsTooBig);
                        throw exception;
                    }
                    if (this.pendingDataStream.Length < count)
                    {
                        baseTracer.WriteLine(string.Format(CultureInfo.InvariantCulture, "Not enough data to process packet. Data is less than expected blob length. Data length {0}. Expected Length {1}.", new object[] { this.pendingDataStream.Length, count }), new object[0]);
                    }
                    else
                    {
                        if (this.maxReceivedObjectSize.HasValue)
                        {
                            this.totalReceivedObjectSizeSoFar += count;
                            if ((this.totalReceivedObjectSizeSoFar < 0) || (this.totalReceivedObjectSizeSoFar > this.maxReceivedObjectSize.Value))
                            {
                                baseTracer.WriteLine("ObjectSize > MaxReceivedObjectSize. ObjectSize is {0}. MaxReceivedObjectSize is {1}", new object[] { this.totalReceivedObjectSizeSoFar, this.maxReceivedObjectSize });
                                PSRemotingTransportException exception2 = null;
                                if (this.isCreateByClientTM)
                                {
                                    exception2 = new PSRemotingTransportException(PSRemotingErrorId.ReceivedObjectSizeExceededMaximumClient, RemotingErrorIdStrings.ReceivedObjectSizeExceededMaximumClient, new object[] { this.totalReceivedObjectSizeSoFar, this.maxReceivedObjectSize });
                                }
                                else
                                {
                                    exception2 = new PSRemotingTransportException(PSRemotingErrorId.ReceivedObjectSizeExceededMaximumServer, RemotingErrorIdStrings.ReceivedObjectSizeExceededMaximumServer, new object[] { this.totalReceivedObjectSizeSoFar, this.maxReceivedObjectSize });
                                }
                                this.ResetRecieveData();
                                throw exception2;
                            }
                        }
                        this.pendingDataStream.Seek(0L, SeekOrigin.Begin);
                        byte[] buffer = new byte[count];
                        this.pendingDataStream.Read(buffer, 0, count);
                        PSEtwLog.LogAnalyticVerbose(PSEventId.ReceivedRemotingFragment, PSOpcode.Receive, PSTask.None, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic, objectId, fragmentId, isStartFragment ? 1 : 0, isEndFragment ? 1 : 0, (int) blobLength, new PSETWBinaryBlob(buffer, 0x15, blobLength));
                        byte[] buffer3 = null;
                        if (count < this.pendingDataStream.Length)
                        {
                            buffer3 = new byte[this.pendingDataStream.Length - count];
                            this.pendingDataStream.Read(buffer3, 0, ((int) this.pendingDataStream.Length) - count);
                        }
                        this.pendingDataStream.Close();
                        this.pendingDataStream = new MemoryStream();
                        if (buffer3 != null)
                        {
                            this.pendingDataStream.Write(buffer3, 0, buffer3.Length);
                        }
                        if (isStartFragment)
                        {
                            this.canIgnoreOffSyncFragments = false;
                            this.currentObjectId = objectId;
                            this.dataToProcessStream = new MemoryStream();
                        }
                        else
                        {
                            if (objectId != this.currentObjectId)
                            {
                                baseTracer.WriteLine("ObjectId != CurrentObjectId", new object[0]);
                                this.ResetRecieveData();
                                if (!this.canIgnoreOffSyncFragments)
                                {
                                    PSRemotingTransportException exception3 = new PSRemotingTransportException(RemotingErrorIdStrings.ObjectIdsNotMatching);
                                    throw exception3;
                                }
                                baseTracer.WriteLine("Ignoring ObjectId != CurrentObjectId", new object[0]);
                                goto Label_005A;
                            }
                            if (fragmentId != (this.currentFrgId + 1L))
                            {
                                baseTracer.WriteLine("Fragment Id is not in sequence.", new object[0]);
                                this.ResetRecieveData();
                                if (!this.canIgnoreOffSyncFragments)
                                {
                                    PSRemotingTransportException exception4 = new PSRemotingTransportException(RemotingErrorIdStrings.FragmetIdsNotInSequence);
                                    throw exception4;
                                }
                                baseTracer.WriteLine("Ignoring Fragment Id is not in sequence.", new object[0]);
                                goto Label_005A;
                            }
                        }
                        this.currentFrgId = fragmentId;
                        this.dataToProcessStream.Write(buffer, 0x15, blobLength);
                        if (!isEndFragment)
                        {
                            goto Label_005A;
                        }
                        try
                        {
                            this.dataToProcessStream.Seek(0L, SeekOrigin.Begin);
                            RemoteDataObject<PSObject> obj2 = RemoteDataObject<PSObject>.CreateFrom(this.dataToProcessStream, this.defragmentor);
                            baseTracer.WriteLine(string.Format(CultureInfo.InvariantCulture, "Runspace Id: {0}", new object[] { obj2.RunspacePoolId }), new object[0]);
                            baseTracer.WriteLine(string.Format(CultureInfo.InvariantCulture, "PowerShell Id: {0}", new object[] { obj2.PowerShellId }), new object[0]);
                            callback(obj2);
                        }
                        finally
                        {
                            this.ResetRecieveData();
                        }
						if (!this.isDisposed && this.pendingDataStream.Length > 0x15L)
                        {
                            goto Label_005A;
                        }
                    }
                }
            }
            finally
            {
                lock (this.syncObject)
                {
                    if (this.isDisposed && (this.numberOfThreadsProcessing == 1))
                    {
                        this.ReleaseResources();
                    }
                    this.numberOfThreadsProcessing--;
                }
            }
        }

        private void ReleaseResources()
        {
            if (this.pendingDataStream != null)
            {
                this.pendingDataStream.Dispose();
                this.pendingDataStream = null;
            }
            if (this.dataToProcessStream != null)
            {
                this.dataToProcessStream.Dispose();
                this.dataToProcessStream = null;
            }
        }

        private void ResetRecieveData()
        {
            if (this.dataToProcessStream != null)
            {
                this.dataToProcessStream.Dispose();
            }
            this.currentObjectId = 0L;
            this.currentFrgId = 0L;
            this.totalReceivedObjectSizeSoFar = 0;
        }

        internal int? MaximumReceivedObjectSize
        {
            set
            {
                this.maxReceivedObjectSize = value;
            }
        }

        internal delegate void OnDataAvailableCallback(RemoteDataObject<PSObject> data);
    }
}

