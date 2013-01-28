namespace System.Data.Services.Providers
{
    using System;

    internal interface IDataServiceInvokable
    {
        object GetResult();
        void Invoke();
    }
}

