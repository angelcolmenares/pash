namespace System.Management.Automation.Remoting
{
    using System;
    using System.Xml;

    internal class PriorityReceiveDataCollection : IDisposable
    {
        private Fragmentor defragmentor;
        private bool isCreateByClientTM;
        private ReceiveDataCollection[] recvdData;

        internal PriorityReceiveDataCollection(Fragmentor defragmentor, bool createdByClientTM)
        {
            this.defragmentor = defragmentor;
            string[] names = Enum.GetNames(typeof(DataPriorityType));
            this.recvdData = new ReceiveDataCollection[names.Length];
            for (int i = 0; i < names.Length; i++)
            {
                this.recvdData[i] = new ReceiveDataCollection(defragmentor, createdByClientTM);
            }
            this.isCreateByClientTM = createdByClientTM;
        }

        internal void AllowTwoThreadsToProcessRawData()
        {
            for (int i = 0; i < this.recvdData.Length; i++)
            {
                this.recvdData[i].AllowTwoThreadsToProcessRawData();
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal virtual void Dispose(bool isDisposing)
        {
            if (this.recvdData != null)
            {
                for (int i = 0; i < this.recvdData.Length; i++)
                {
                    this.recvdData[i].Dispose();
                }
            }
        }

        internal void PrepareForStreamConnect()
        {
            for (int i = 0; i < this.recvdData.Length; i++)
            {
                this.recvdData[i].PrepareForStreamConnect();
            }
        }

        internal void ProcessRawData(byte[] data, DataPriorityType priorityType, ReceiveDataCollection.OnDataAvailableCallback callback)
        {
            try
            {
                this.defragmentor.DeserializationContext.LogExtraMemoryUsage(data.Length);
            }
            catch (XmlException)
            {
                PSRemotingTransportException exception = null;
                if (this.isCreateByClientTM)
                {
                    exception = new PSRemotingTransportException(PSRemotingErrorId.ReceivedDataSizeExceededMaximumClient, RemotingErrorIdStrings.ReceivedDataSizeExceededMaximumClient, new object[] { this.defragmentor.DeserializationContext.MaximumAllowedMemory.Value });
                }
                else
                {
                    exception = new PSRemotingTransportException(PSRemotingErrorId.ReceivedDataSizeExceededMaximumServer, RemotingErrorIdStrings.ReceivedDataSizeExceededMaximumServer, new object[] { this.defragmentor.DeserializationContext.MaximumAllowedMemory.Value });
                }
                throw exception;
            }
            this.recvdData[(int) priorityType].ProcessRawData(data, callback);
        }

        internal int? MaximumReceivedDataSize
        {
            set
            {
                this.defragmentor.DeserializationContext.MaximumAllowedMemory = value;
            }
        }

        internal int? MaximumReceivedObjectSize
        {
            set
            {
                foreach (ReceiveDataCollection datas in this.recvdData)
                {
                    datas.MaximumReceivedObjectSize = value;
                }
            }
        }
    }
}

