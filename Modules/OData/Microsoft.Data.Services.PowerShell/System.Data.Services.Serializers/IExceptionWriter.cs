namespace System.Data.Services.Serializers
{
    using System;
    using System.Data.Services;

    internal interface IExceptionWriter
    {
        void WriteException(HandleExceptionArgs args);
    }
}

