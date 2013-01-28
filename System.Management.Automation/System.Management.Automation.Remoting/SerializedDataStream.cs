namespace System.Management.Automation.Remoting
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Internal;
    using System.Management.Automation.Tracing;
    using System.Runtime.CompilerServices;
    using System.Threading;

    internal class SerializedDataStream : Stream, IDisposable
    {
        private static long _objectIdSequenceNumber = 0L;
        [TraceSource("SerializedDataStream", "SerializedDataStream")]
        private static PSTraceSource _trace = PSTraceSource.GetTracer("SerializedDataStream", "SerializedDataStream");
        private FragmentedRemoteObject currentFragment;
        private bool disposed;
        private long fragmentId;
        private int fragmentSize;
        private bool isDisposed;
        private bool isEntered;
        private long length;
        private bool notifyOnWriteFragmentImmediately;
        private OnDataAvailableCallback onDataAvailableCallback;
        private Queue<MemoryStream> queuedStreams;
        private int readOffSet;
        private MemoryStream readStream;
        private object syncObject;
        private int writeOffset;
        private MemoryStream writeStream;

        internal SerializedDataStream (int fragmentSize)
		{
			if (fragmentSize == 0) {
				_trace.WriteLine("Error: SerializedDataStream with fragmentsize : {0}", new object[] { fragmentSize });
			}
            _trace.WriteLine("Creating SerializedDataStream with fragmentsize : {0}", new object[] { fragmentSize });
            this.syncObject = new object();
            this.currentFragment = new FragmentedRemoteObject();
            this.queuedStreams = new Queue<MemoryStream>();
            this.fragmentSize = fragmentSize;
        }

        internal SerializedDataStream(int fragmentSize, OnDataAvailableCallback callbackToNotify) : this(fragmentSize)
        {
            if (callbackToNotify != null)
            {
                this.notifyOnWriteFragmentImmediately = true;
                this.onDataAvailableCallback = callbackToNotify;
            }
        }

        public void Dispose()
        {
            if (!this.disposed)
            {
                GC.SuppressFinalize(this);
                this.disposed = true;
            }
            base.Dispose();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (this.syncObject)
                {
                    foreach (MemoryStream stream in this.queuedStreams)
                    {
                        if (stream.CanRead)
                        {
                            stream.Dispose();
                        }
                    }
                    if ((this.readStream != null) && this.readStream.CanRead)
                    {
                        this.readStream.Dispose();
                    }
                    if ((this.writeStream != null) && this.writeStream.CanRead)
                    {
                        this.writeStream.Dispose();
                    }
                    this.isDisposed = true;
                }
            }
        }

        private void EnqueueWriteStream()
        {
            _trace.WriteLine("Queuing write stream: {0} Length: {1} Capacity: {2}", new object[] { this.writeStream.GetHashCode(), this.writeStream.Length, this.writeStream.Capacity });
            this.queuedStreams.Enqueue(this.writeStream);
            this.writeStream = new MemoryStream(this.fragmentSize);
            this.writeOffset = 0;
            _trace.WriteLine("Created write stream: {0}", new object[] { this.writeStream.GetHashCode() });
        }

        internal void Enter()
        {
            this.isEntered = true;
            this.fragmentId = 0L;
            this.currentFragment.ObjectId = GetObjectId();
            this.currentFragment.FragmentId = this.fragmentId;
            this.currentFragment.IsStartFragment = true;
            this.currentFragment.BlobLength = 0;
            this.currentFragment.Blob = new byte[this.fragmentSize];
        }

        internal void Exit()
        {
            this.isEntered = false;
            if (this.currentFragment.BlobLength > 0)
            {
                this.currentFragment.IsEndFragment = true;
                this.WriteCurrentFragmentAndReset();
            }
        }

        public override void Flush()
        {
        }

        private static long GetObjectId()
        {
            return Interlocked.Increment(ref _objectIdSequenceNumber);
        }

        internal byte[] Read()
        {
            lock (this.syncObject)
            {
                if (!this.isDisposed)
                {
                    int count = (this.length > this.fragmentSize) ? this.fragmentSize : ((int) this.length);
                    if (count > 0)
                    {
                        byte[] buffer = new byte[count];
                        this.Read(buffer, 0, count);
                        return buffer;
                    }
                }
                return null;
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int num = offset;
            int num2 = 0;
            Collection<MemoryStream> collection = new Collection<MemoryStream>();
            MemoryStream readStream = null;
            lock (this.syncObject)
            {
                if (!this.isDisposed)
                {
                    goto Label_017B;
                }
                return 0;
            Label_0032:
                if (this.readStream != null)
                {
                    goto Label_008E;
                }
                if (this.queuedStreams.Count > 0)
                {
                    this.readStream = this.queuedStreams.Dequeue();
                    if (this.readStream.CanRead && (readStream != this.readStream))
                    {
                        goto Label_0087;
                    }
                    this.readStream = null;
                    goto Label_017B;
                }
                this.readStream = this.writeStream;
            Label_0087:
                this.readOffSet = 0;
            Label_008E:
                this.readStream.Position = this.readOffSet;
                int num3 = this.readStream.Read(buffer, num, count - num2);
                _trace.WriteLine("Read {0} data from readstream: {1}", new object[] { num3, this.readStream.GetHashCode() });
                num2 += num3;
                num += num3;
                this.readOffSet += num3;
                this.length -= num3;
                if ((this.readStream.Capacity == this.readOffSet) && (this.readStream != this.writeStream))
                {
                    _trace.WriteLine("Adding readstream {0} to dispose collection.", new object[] { this.readStream.GetHashCode() });
                    collection.Add(this.readStream);
                    readStream = this.readStream;
                    this.readStream = null;
                }
            Label_017B:
                if (num2 < count)
                {
                    goto Label_0032;
                }
            }
            foreach (MemoryStream stream2 in collection)
            {
                _trace.WriteLine("Disposing stream: {0}", new object[] { stream2.GetHashCode() });
                stream2.Dispose();
            }
            return num2;
        }

        internal byte[] ReadOrRegisterCallback(OnDataAvailableCallback callback)
        {
            lock (this.syncObject)
			{
                if (this.length <= 0L)
                {
                    this.onDataAvailableCallback = callback;
                    return null;
                }
                int count = (this.length > this.fragmentSize) ? this.fragmentSize : ((int) this.length);
                byte[] buffer = new byte[count];
                this.Read(buffer, 0, count);
                return buffer;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write (byte[] buffer, int offset, int count)
		{
			int sourceIndex = offset;
			int num2 = count;
			/*
			if (this.currentFragment.BlobLength == 0) {
				return;
			}
			*/
            while (num2 > 0)
            {
                int num3 = (this.fragmentSize - 0x15) - this.currentFragment.BlobLength;
                if (num3 > 0)
                {
                    int length = (num2 > num3) ? num3 : num2;
                    num2 -= length;
                    Array.Copy(buffer, sourceIndex, this.currentFragment.Blob, this.currentFragment.BlobLength, length);
                    this.currentFragment.BlobLength += length;
                    sourceIndex += length;
                    if (num2 > 0)
                    {
                        this.WriteCurrentFragmentAndReset();
                    }
                }
                else
                {
                    this.WriteCurrentFragmentAndReset();
                }
            }
        }

        public override void WriteByte(byte value)
        {
            byte[] buffer = new byte[] { value };
            this.Write(buffer, 0, 1);
        }

        private void WriteCurrentFragmentAndReset()
        {
            PSEtwLog.LogAnalyticVerbose(PSEventId.SentRemotingFragment, PSOpcode.Send, PSTask.None, PSKeyword.Transport | PSKeyword.UseAlwaysAnalytic, this.currentFragment.ObjectId, this.currentFragment.FragmentId, this.currentFragment.IsStartFragment ? 1 : 0, this.currentFragment.IsEndFragment ? 1 : 0, (int) this.currentFragment.BlobLength, new PSETWBinaryBlob(this.currentFragment.Blob, 0, this.currentFragment.BlobLength));
            byte[] bytes = this.currentFragment.GetBytes();
            int length = bytes.Length;
			if (fragmentSize == 0) fragmentSize = length;
            int offset = 0;
            if (!this.notifyOnWriteFragmentImmediately)
            {
                lock (this.syncObject)
                {
                    if (this.isDisposed)
                    {
                        return;
                    }
                    if (this.writeStream == null)
                    {
                        this.writeStream = new MemoryStream(this.fragmentSize);
                        _trace.WriteLine("Created write stream: {0}", new object[] { this.writeStream.GetHashCode() });
                        this.writeOffset = 0;
                    }
                    while (length > 0)
                    {
                        int num3 = this.writeStream.Capacity - this.writeOffset;
                        if (num3 == 0)
                        {
                            this.EnqueueWriteStream();
                            num3 = this.writeStream.Capacity - this.writeOffset;
                        }
                        int count = (length > num3) ? num3 : length;
                        length -= count;
                        this.writeStream.Position = this.writeOffset;
                        this.writeStream.Write(bytes, offset, count);
                        offset += count;
                        this.writeOffset += count;
                        this.length += count;
                    }
                }
            }
            if (this.onDataAvailableCallback != null)
            {
                this.onDataAvailableCallback(bytes, this.currentFragment.IsEndFragment);
            }
            this.currentFragment.FragmentId = this.fragmentId += 1L;
            this.currentFragment.IsStartFragment = false;
            this.currentFragment.IsEndFragment = false;
            this.currentFragment.BlobLength = 0;
            this.currentFragment.Blob = new byte[this.fragmentSize];
        }

        public override bool CanRead
        {
            get
            {
                return true;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return false;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        public override long Length
        {
            get
            {
                return this.length;
            }
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        internal delegate void OnDataAvailableCallback(byte[] data, bool isEndFragment);
    }
}

