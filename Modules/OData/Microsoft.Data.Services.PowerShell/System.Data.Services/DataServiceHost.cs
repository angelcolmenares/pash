namespace System.Data.Services
{
    using System;
    using System.ServiceModel.Web;

    [CLSCompliant(false)]
    internal class DataServiceHost : WebServiceHost
    {
        public DataServiceHost(Type serviceType, Uri[] baseAddresses) : base(serviceType, baseAddresses)
        {

        }

    }
}

