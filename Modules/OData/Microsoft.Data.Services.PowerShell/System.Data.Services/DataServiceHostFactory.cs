namespace System.Data.Services
{
    using System;
    using System.ServiceModel;
    using System.ServiceModel.Activation;

    public class DataServiceHostFactory : ServiceHostFactory
    {
        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
        {
            return new DataServiceHost(serviceType, baseAddresses);
        }
    }
}

