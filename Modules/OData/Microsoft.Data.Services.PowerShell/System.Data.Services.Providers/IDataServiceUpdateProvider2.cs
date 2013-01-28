namespace System.Data.Services.Providers
{
    using System;
    using System.Data.Services;

    internal interface IDataServiceUpdateProvider2 : IDataServiceUpdateProvider, IUpdatable
    {
        void ScheduleInvokable(IDataServiceInvokable invokable);
    }
}

