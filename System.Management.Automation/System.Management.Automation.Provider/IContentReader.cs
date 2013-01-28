namespace System.Management.Automation.Provider
{
    using System;
    using System.Collections;
    using System.IO;

    public interface IContentReader : IDisposable
    {
        void Close();
        IList Read(long readCount);
        void Seek(long offset, SeekOrigin origin);
    }
}

