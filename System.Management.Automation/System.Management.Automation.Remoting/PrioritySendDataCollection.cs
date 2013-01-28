namespace System.Management.Automation.Remoting
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    internal class PrioritySendDataCollection
    {
        private SerializedDataStream[] dataToBeSent;
        private System.Management.Automation.Remoting.Fragmentor fragmentor;
        private bool isHandlingCallback;
        private OnDataAvailableCallback onDataAvailableCallback;
        private SerializedDataStream.OnDataAvailableCallback onSendCollectionDataAvailable;
        private object readSyncObject = new object();
        private object[] syncObjects;

        internal PrioritySendDataCollection()
        {
            this.onSendCollectionDataAvailable = new SerializedDataStream.OnDataAvailableCallback(this.OnDataAvailable);
        }

        internal void Add<T>(RemoteDataObject<T> data)
        {
            this.Add<T>(data, DataPriorityType.Default);
        }

        internal void Add<T>(RemoteDataObject<T> data, DataPriorityType priority)
        {
            lock (this.syncObjects[(int) priority])
            {
                this.fragmentor.Fragment<T>(data, this.dataToBeSent[(int) priority]);
            }
        }

        internal void Clear()
        {
            lock (this.syncObjects[1])
            {
                this.dataToBeSent[1].Dispose();
            }
            lock (this.syncObjects[0])
            {
                this.dataToBeSent[0].Dispose();
            }
        }

        private void OnDataAvailable(byte[] data, bool isEndFragment)
        {
            lock (this.readSyncObject)
            {
                if (this.isHandlingCallback)
                {
                    return;
                }
                this.isHandlingCallback = true;
            }
            if (this.onDataAvailableCallback != null)
            {
                DataPriorityType type;
                byte[] buffer = this.ReadOrRegisterCallback(this.onDataAvailableCallback, out type);
                if (buffer != null)
                {
                    OnDataAvailableCallback onDataAvailableCallback = this.onDataAvailableCallback;
                    this.onDataAvailableCallback = null;
                    onDataAvailableCallback(buffer, type);
                }
            }
            this.isHandlingCallback = false;
        }

        internal byte[] ReadOrRegisterCallback(OnDataAvailableCallback callback, out DataPriorityType priorityType)
        {
            lock (this.readSyncObject)
            {
                priorityType = DataPriorityType.Default;
                byte[] buffer = null;
                buffer = this.dataToBeSent[1].ReadOrRegisterCallback(this.onSendCollectionDataAvailable);
                priorityType = DataPriorityType.PromptResponse;
                if (buffer == null)
                {
                    buffer = this.dataToBeSent[0].ReadOrRegisterCallback(this.onSendCollectionDataAvailable);
                    priorityType = DataPriorityType.Default;
                }
                if (buffer == null)
                {
                    this.onDataAvailableCallback = callback;
                }
                return buffer;
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
                string[] names = Enum.GetNames(typeof(DataPriorityType));
                this.dataToBeSent = new SerializedDataStream[names.Length];
                this.syncObjects = new object[names.Length];
                for (int i = 0; i < names.Length; i++)
                {
                    this.dataToBeSent[i] = new SerializedDataStream(this.fragmentor.FragmentSize);
                    this.syncObjects[i] = new object();
                }
            }
        }

        internal delegate void OnDataAvailableCallback(byte[] data, DataPriorityType priorityType);
    }
}

