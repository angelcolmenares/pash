namespace System.Management.Automation.Provider
{
    using System;
    using System.Collections;
    using System.IO;

    public interface IContentWriter : IDisposable
    {
        void Close();
        void Seek(long offset, SeekOrigin origin);
        IList Write(IList content);
    }
}

