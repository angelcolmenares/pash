namespace System.Data.Services
{
    using System.Net;

    internal interface IDataServiceHost2 : IDataServiceHost
    {
        WebHeaderCollection RequestHeaders { get; }

        WebHeaderCollection ResponseHeaders { get; }
    }
}

