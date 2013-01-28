using Microsoft.Management.Odata.Tracing;
using System;
using System.Data.Services;
using System.ServiceModel;

namespace Microsoft.Management.Odata
{
	internal class ServiceHostFactory : DataServiceHostFactory
	{
		public ServiceHostFactory()
		{
			Tracer tracer = new Tracer();
			tracer.MethodCall0("ServiceHostFactory", "ServiceHostFactory");
		}

		protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
		{
			Tracer tracer = new Tracer();
			tracer.MethodCall1("ServiceHostFactory", "CreateServiceHost", serviceType.ToString());
			return base.CreateServiceHost(serviceType, baseAddresses);
		}
	}
}