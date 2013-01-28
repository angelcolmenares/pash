namespace System.Data.Services.Client
{
    using System;
    using System.IO;

    internal class DataServiceSaveStream
    {
        private readonly DataServiceRequestArgs args;
        private readonly bool close;
        private readonly System.IO.Stream stream;

        internal DataServiceSaveStream(System.IO.Stream stream, bool close, DataServiceRequestArgs args)
        {
            this.stream = stream;
            this.close = close;
            this.args = args;
        }

        internal void Close()
        {
            if ((this.stream != null) && this.close)
            {
                this.stream.Close();
            }
        }

        internal DataServiceRequestArgs Args
        {
            get
            {
                return this.args;
            }
        }

        internal System.IO.Stream Stream
        {
            get
            {
                return this.stream;
            }
        }
    }
}

