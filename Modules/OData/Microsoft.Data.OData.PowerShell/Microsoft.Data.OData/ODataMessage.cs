namespace Microsoft.Data.OData
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    internal abstract class ODataMessage
    {
        private Microsoft.Data.OData.BufferingReadStream bufferingReadStream;
        private readonly bool disableMessageStreamDisposal;
        private readonly long maxMessageSize;
        private bool? useBufferingReadStream;
        private readonly bool writing;

        protected ODataMessage(bool writing, bool disableMessageStreamDisposal, long maxMessageSize)
        {
            this.writing = writing;
            this.disableMessageStreamDisposal = disableMessageStreamDisposal;
            this.maxMessageSize = maxMessageSize;
        }

        public abstract string GetHeader(string headerName);
        public abstract Stream GetStream();
        protected internal Stream GetStream(Func<Stream> messageStreamFunc, bool isRequest)
        {
            if (!this.writing)
            {
                Microsoft.Data.OData.BufferingReadStream stream = this.TryGetBufferingReadStream();
                if (stream != null)
                {
                    return stream;
                }
            }
            Stream bufferingReadStream = messageStreamFunc();
            ValidateMessageStream(bufferingReadStream, isRequest);
            bool flag = !this.writing && (this.maxMessageSize > 0L);
            if (this.disableMessageStreamDisposal && flag)
            {
                bufferingReadStream = MessageStreamWrapper.CreateNonDisposingStreamWithMaxSize(bufferingReadStream, this.maxMessageSize);
            }
            else if (this.disableMessageStreamDisposal)
            {
                bufferingReadStream = MessageStreamWrapper.CreateNonDisposingStream(bufferingReadStream);
            }
            else if (flag)
            {
                bufferingReadStream = MessageStreamWrapper.CreateStreamWithMaxSize(bufferingReadStream, this.maxMessageSize);
            }
            if (!this.writing && (this.useBufferingReadStream == true))
            {
                this.bufferingReadStream = new Microsoft.Data.OData.BufferingReadStream(bufferingReadStream);
                bufferingReadStream = this.bufferingReadStream;
            }
            return bufferingReadStream;
        }

        public abstract Task<Stream> GetStreamAsync();
        protected internal Task<Stream> GetStreamAsync(Func<Task<Stream>> streamFuncAsync, bool isRequest)
        {
            Func<Task<Stream>, Stream> operation = null;
            if (!this.writing)
            {
                Stream stream = this.TryGetBufferingReadStream();
                if (stream != null)
                {
                    return TaskUtils.GetCompletedTask<Stream>(stream);
                }
            }
            Task<Stream> task = streamFuncAsync();
            ValidateMessageStreamTask(task, isRequest);
            task = task.FollowOnSuccessWith<Stream, Stream>(delegate (Task<Stream> streamTask) {
                Stream result = streamTask.Result;
                ValidateMessageStream(result, isRequest);
                bool flag = !this.writing && (this.maxMessageSize > 0L);
                if (this.disableMessageStreamDisposal && flag)
                {
                    return MessageStreamWrapper.CreateNonDisposingStreamWithMaxSize(result, this.maxMessageSize);
                }
                if (this.disableMessageStreamDisposal)
                {
                    return MessageStreamWrapper.CreateNonDisposingStream(result);
                }
                if (flag)
                {
                    result = MessageStreamWrapper.CreateStreamWithMaxSize(result, this.maxMessageSize);
                }
                return result;
            });
            if (this.writing)
            {
                return task;
            }
            task = task.FollowOnSuccessWithTask<Stream, BufferedReadStream>(streamTask => BufferedReadStream.BufferStreamAsync(streamTask.Result)).FollowOnSuccessWith<BufferedReadStream, Stream>(streamTask => streamTask.Result);
            if (this.useBufferingReadStream != true)
            {
                return task;
            }
            if (operation == null)
            {
                operation = delegate (Task<Stream> streamTask) {
                    Stream result = streamTask.Result;
                    this.bufferingReadStream = new Microsoft.Data.OData.BufferingReadStream(result);
                    return this.bufferingReadStream;
                };
            }
            return task.FollowOnSuccessWith<Stream, Stream>(operation);
        }

        public abstract void SetHeader(string headerName, string headerValue);
        private Microsoft.Data.OData.BufferingReadStream TryGetBufferingReadStream()
        {
            if (this.bufferingReadStream == null)
            {
                return null;
            }
            Microsoft.Data.OData.BufferingReadStream bufferingReadStream = this.bufferingReadStream;
            if (this.bufferingReadStream.IsBuffering)
            {
                this.bufferingReadStream.ResetStream();
                return bufferingReadStream;
            }
            this.bufferingReadStream = null;
            return bufferingReadStream;
        }

        private static void ValidateMessageStream(Stream stream, bool isRequest)
        {
            if (stream == null)
            {
                string message = isRequest ? Strings.ODataRequestMessage_MessageStreamIsNull : Strings.ODataResponseMessage_MessageStreamIsNull;
                throw new ODataException(message);
            }
        }

        private static void ValidateMessageStreamTask(Task<Stream> streamTask, bool isRequest)
        {
            if (streamTask == null)
            {
                string message = isRequest ? Strings.ODataRequestMessage_StreamTaskIsNull : Strings.ODataResponseMessage_StreamTaskIsNull;
                throw new ODataException(message);
            }
        }

        protected void VerifyCanSetHeader()
        {
            if (!this.writing)
            {
                throw new ODataException(Strings.ODataMessage_MustNotModifyMessage);
            }
        }

        protected internal Microsoft.Data.OData.BufferingReadStream BufferingReadStream
        {
            get
            {
                return this.bufferingReadStream;
            }
        }

        public abstract IEnumerable<KeyValuePair<string, string>> Headers { get; }

        protected internal bool? UseBufferingReadStream
        {
            get
            {
                return this.useBufferingReadStream;
            }
            set
            {
                this.useBufferingReadStream = value;
            }
        }
    }
}

