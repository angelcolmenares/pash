namespace System.Management.Automation
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    internal interface IBlockingEnumerator<out W> : IEnumerator<W>, IDisposable, IEnumerator
    {
        bool MoveNext(bool block);
    }
}

