namespace System.Data.Services.Client
{
    using System;
    using System.Diagnostics;
    using System.Xml.Linq;

    internal sealed class ReadingWritingEntityEventArgs : EventArgs
    {
        private Uri baseUri;
        private XElement data;
        private object entity;

        internal ReadingWritingEntityEventArgs(object entity, XElement data, Uri baseUri)
        {
            this.entity = entity;
            this.data = data;
            this.baseUri = baseUri;
        }

        public Uri BaseUri
        {
            [DebuggerStepThrough]
            get
            {
                return this.baseUri;
            }
        }

        public XElement Data
        {
            [DebuggerStepThrough]
            get
            {
                return this.data;
            }
        }

        public object Entity
        {
            get
            {
                return this.entity;
            }
        }
    }
}

