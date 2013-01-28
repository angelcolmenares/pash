namespace Microsoft.PowerShell.Commands
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Management.Automation;
    using System.Management.Automation.Provider;

    public class SessionStateProviderBaseContentReaderWriter : IContentReader, IContentWriter, IDisposable
    {
        private bool contentRead;
        private string path;
        private SessionStateProviderBase provider;

        internal SessionStateProviderBaseContentReaderWriter(string path, SessionStateProviderBase provider)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw PSTraceSource.NewArgumentException("path");
            }
            if (provider == null)
            {
                throw PSTraceSource.NewArgumentNullException("provider");
            }
            this.path = path;
            this.provider = provider;
        }

        public void Close()
        {
        }

        public void Dispose()
        {
            this.Close();
            GC.SuppressFinalize(this);
        }

        public IList Read(long readCount)
        {
            IList list = null;
            if (!this.contentRead)
            {
                object sessionStateItem = this.provider.GetSessionStateItem(this.path);
                if (sessionStateItem == null)
                {
                    return list;
                }
                object valueOfItem = this.provider.GetValueOfItem(sessionStateItem);
                if (valueOfItem != null)
                {
                    list = valueOfItem as IList;
                    if (list == null)
                    {
                        list = new object[] { valueOfItem };
                    }
                }
                this.contentRead = true;
            }
            return list;
        }

        public void Seek(long offset, SeekOrigin origin)
        {
            throw PSTraceSource.NewNotSupportedException("SessionStateStrings", "IContent_Seek_NotSupported", new object[0]);
        }

        public IList Write(IList content)
        {
            if (content == null)
            {
                throw PSTraceSource.NewArgumentNullException("content");
            }
            object obj2 = content;
            if (content.Count == 1)
            {
                obj2 = content[0];
            }
            this.provider.SetSessionStateItem(this.path, obj2, false);
            return content;
        }
    }
}

